using Beamable.Common.Assistant;
using Beamable.Editor.ToolbarExtender;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Beamable.Editor.Assistant
{
	/// <summary>
	/// Manager that handles the state of notifications for each detected <see cref="BeamHint"/>.
	/// <para/>
	/// We keep track of each individual <see cref="BeamHintHeader"/> and the notification preferences set for them (via <see cref="IBeamHintPreferencesManager"/>).
	/// Based on these, we track which hints have been displayed this session and/or which <see cref="BeamHint.ContextObject"/>s have a changed.
	/// <para/>
	/// We use these to keep an updated list of pending <see cref="BeamHintType.Hint"/> and <see cref="BeamHintType.Validation"/> notifications. These are cleared
	/// whenever the <see cref="BeamableAssistantWindow"/> is opened or focused. 
	/// </summary>
	// ReSharper disable once ClassNeverInstantiated.Global
	[BeamHintSystem()]
	public class BeamHintNotificationManager : IBeamHintSystem
	{
		private const string NOTIFICATION_SESSION_KEY = "BEAM_HINT_NOTIFICATION_SESSION_KEY";

		private IBeamHintGlobalStorage _hintStorage;
		private IBeamHintPreferencesManager _hintPreferences;

		private List<BeamHintHeader> _hintsDisplayedThisSession;
		private Dictionary<BeamHintHeader, object> _lastDetectedContextObjects;

		private List<BeamHintHeader> _pendingNotificationHints;
		private List<BeamHintHeader> _pendingNotificationValidations;

		private double _lastTickTime;

		/// <summary>
		/// Current amount of seconds to wait before checking for new notifications.
		/// </summary>
		public double TickRate
		{
			get;
			set;
		}

		/// <summary>
		/// Latest list of pending notifications that were found.
		/// </summary>
		public IEnumerable<BeamHintHeader> AllPendingNotifications => _pendingNotificationHints.Union(_pendingNotificationValidations);

		/// <summary>
		/// Latest list of pending notifications of hints of type <see cref="BeamHintType.Hint"/> that were found.
		/// </summary>
		public IEnumerable<BeamHintHeader> PendingHintNotifications => _pendingNotificationHints;

		/// <summary>
		/// Latest list of pending notifications of hints of type <see cref="BeamHintType.Validation"/> that were found.
		/// </summary>
		public IEnumerable<BeamHintHeader> PendingValidationNotifications => _pendingNotificationValidations;

		public BeamHintNotificationManager()
		{
			// Pick up hints that have already been notified this session.
			_hintsDisplayedThisSession = SessionState.GetString(NOTIFICATION_SESSION_KEY, "")
													 .Split(new[] { BeamHintSharedConstants.BEAM_HINT_PREFERENCES_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries)
													 .Select(BeamHintHeader.DeserializeBeamHintHeader)
													 .ToList();

			_lastDetectedContextObjects = new Dictionary<BeamHintHeader, object>();

			_pendingNotificationHints = new List<BeamHintHeader>();
			_pendingNotificationValidations = new List<BeamHintHeader>();

			TickRate = 1;
			_lastTickTime = 0;
		}

		/// <summary>
		/// False, since this is a globally accessible hint system. It does not get injected into <see cref="BeamContext"/>.
		/// </summary>
		public bool IsBeamContextSystem => false;

		/// <summary>
		/// Update the reference to the <see cref="IBeamHintPreferencesManager"/> used by this <see cref="BeamHintNotificationManager"/>.
		/// </summary>
		public void SetPreferencesManager(IBeamHintPreferencesManager preferencesManager)
		{
			_hintPreferences = preferencesManager;
		}

		/// <summary>
		/// Update the reference to the <see cref="IBeamHintGlobalStorage"/> used by this <see cref="BeamHintNotificationManager"/>.
		/// </summary>
		public void SetStorage(IBeamHintGlobalStorage hintGlobalStorage)
		{
			_hintStorage = hintGlobalStorage;
		}

		/// <summary>
		/// Initializes the update callback for this <see cref="IBeamHintSystem"/>.
		/// </summary>
		public void OnInitialized()
		{
			EditorApplication.update -= Update;
			EditorApplication.update += Update;
		}

		/// <summary>
		/// Call this to clear a set of pending hint notifications. This will clear them according to their established preferences in
		/// <see cref="BeamHintPreferencesManager.GetHintNotificationPreferences"/>.
		/// </summary>
		/// <param name="hintsToMarkAsSeen">When null, clears all hints. Otherwise, clears only the specified hints.</param>
		public void ClearPendingNotifications(IEnumerable<BeamHintHeader> hintsToMarkAsSeen = null)
		{
			// Update in-memory and SessionState of notifications for hint BeamHints that have been seen already.
			_hintsDisplayedThisSession.AddRange(_pendingNotificationHints);
			_hintsDisplayedThisSession = _hintsDisplayedThisSession.Distinct().ToList();
			var serializedHeaders = string.Join(BeamHintSharedConstants.BEAM_HINT_PREFERENCES_SEPARATOR, _hintsDisplayedThisSession.Select(h => h.AsKey()));
			SessionState.SetString(NOTIFICATION_SESSION_KEY, serializedHeaders);

			// Update in-memory caches of state of notifications for validation BeamHints.
			foreach (var hint in _pendingNotificationValidations.Select(_hintStorage.GetHint))
			{
				if (!_lastDetectedContextObjects.ContainsKey(hint.Header))
					_lastDetectedContextObjects.Add(hint.Header, hint.ContextObject);
				else
					_lastDetectedContextObjects[hint.Header] = hint.ContextObject;
			}

			if (hintsToMarkAsSeen == null)
			{
				_pendingNotificationHints.Clear();
				_pendingNotificationValidations.Clear();
			}
			else
			{
				var toMarkAsSeen = hintsToMarkAsSeen.ToList();
				var markHintAsSeen = toMarkAsSeen.Where(h => (h.Type & BeamHintType.Hint) != 0);
				var markValidationAsSeen = toMarkAsSeen.Where(h => (h.Type & BeamHintType.Validation) != 0);

				_pendingNotificationHints = _pendingNotificationHints.Except(markHintAsSeen).ToList();
				_pendingNotificationValidations = _pendingNotificationValidations.Except(markValidationAsSeen).ToList();
			}
		}

		/// <summary>
		/// Detects whether or not we should take another pass to identify new notifications or not.
		/// This is added to <see cref="EditorApplication.update"/> during <see cref="BeamEditor.Initialize"/>.
		/// </summary>
		public void Update()
		{
			var currTickTime = EditorApplication.timeSinceStartup;

			// Do nothing if it's not time to check for notifications again.
			if (_lastTickTime != 0 && !(currTickTime - _lastTickTime >= TickRate)) return;

			// Update the last tick time and 
			_lastTickTime = currTickTime;
			_hintPreferences.RebuildPerHintPreferences();
			CheckNotifications();
#if !DISABLE_BEAMABLE_TOOLBAR_EXTENDER
			BeamableToolbarExtender.Repaint();
#endif
		}

		private void CheckNotifications()
		{
			if (_hintStorage == null) return;

			UpdateNotifications(_hintPreferences,
								_hintStorage.All.ToList(),
								_hintsDisplayedThisSession,
								_lastDetectedContextObjects,
								_pendingNotificationHints,
								_pendingNotificationValidations);

			// NOTE: Commented out block below is useful when you want to update only a specific sub-set of hints based on domain and log them.

			// UpdateNotifications(_hintPreferences,
			//                     _hintStorage.ReflectionCacheHints.ToList(),
			//                     _hintsDisplayedThisSession,
			//                     _lastDetectedContextObjects,
			//                     _pendingNotificationHints,
			//                     _pendingNotificationValidations);
			//
			// // var reflectionCacheHints = AllPendingNotifications.Select(hint => hint.ToString()).ToList();
			// // BeamableLogger.Log($"ReflectionCache Hints -- Count: {reflectionCacheHints.Count}\n" +
			// //                    $"{string.Join("\n", reflectionCacheHints)}");
			//
			// UpdateNotifications(_hintPreferences,
			//                     _hintStorage.CSharpMSHints.ToList(),
			//                     _hintsDisplayedThisSession,
			//                     _lastDetectedContextObjects,
			//                     _pendingNotificationHints,
			//                     _pendingNotificationValidations);
			//
			// // var cSharpHints = AllPendingNotifications.Select(hint => hint.ToString()).ToList();
			// // BeamableLogger.Log($"C# Microservice Hints -- Count: {cSharpHints.Count}\n" +
			// //                    $"{string.Join("\n", cSharpHints)}");
			//
			// UpdateNotifications(_hintPreferences,
			//                     _hintStorage.ContentHints.ToList(),
			//                     _hintsDisplayedThisSession,
			//                     _lastDetectedContextObjects,
			//                     _pendingNotificationHints,
			//                     _pendingNotificationValidations);
			//
			// // var contentHints = AllPendingNotifications.Select(hint => hint.ToString()).ToList();
			// // BeamableLogger.Log($"Beamable Content Hints -- Count: {contentHints.Count}\n" +
			// //                    $"{string.Join("\n", contentHints)}");
			//
			// UpdateNotifications(_hintPreferences,
			//                     _hintStorage.UserDefinedStorage.ToList(),
			//                     _hintsDisplayedThisSession,
			//                     _lastDetectedContextObjects,
			//                     _pendingNotificationHints,
			//                     _pendingNotificationValidations);
			//
			// // var userDefinedHints = AllPendingNotifications.Select(hint => hint.ToString()).ToList();
			// // BeamableLogger.Log($"User Defined Hints -- Count: {userDefinedHints.Count}\n" +
			// //                    $"{string.Join("\n", userDefinedHints)}");
			//
			// UpdateNotifications(_hintPreferences,
			//                     _hintStorage.AssistantHints.ToList(),
			//                     _hintsDisplayedThisSession,
			//                     _lastDetectedContextObjects,
			//                     _pendingNotificationHints,
			//                     _pendingNotificationValidations);
			//
			// // var assistant = AllPendingNotifications.Select(hint => hint.ToString()).ToList();
			// // BeamableLogger.Log($"Beamable Assistant Hints -- Count: {assistant.Count}\n" +
			// //                    $"{string.Join("\n", assistant)}");
		}

		/// <summary>
		/// Updates <paramref name="outPendingNotificationHints"/> and <paramref name="outPendingNotificationValidations"/> with the given <paramref name="hintsToUpdate"/> based on
		/// their notification preferences and the current state of hint notifications (see <paramref name="hintsClearedThisSession"/> and <see cref="lastDetectedContextObjects"/>). 
		/// </summary>
		/// <param name="beamHintPreferencesManager">A <see cref="IBeamHintPreferencesManager"/> holding serialized preferences for the hints.</param>
		/// <param name="hintsToUpdate">The list of <see cref="BeamHint"/>s to check for notifications over.</param>
		/// <param name="hintsClearedThisSession">The list of hints that have already been cleared this session.</param>
		/// <param name="lastDetectedContextObjects">A mapping of headers to context object instances that have already been seen.</param>
		/// <param name="outPendingNotificationHints">The output list of detected <see cref="BeamHintType.Hint"/> type notifications.</param>
		/// <param name="outPendingNotificationValidations">The output list of detected <see cref="BeamHintType.Validation"/> type notifications.</param>
		private static void UpdateNotifications(IBeamHintPreferencesManager beamHintPreferencesManager,
												IReadOnlyCollection<BeamHint> hintsToUpdate,
												List<BeamHintHeader> hintsClearedThisSession,
												Dictionary<BeamHintHeader, object> lastDetectedContextObjects,
												List<BeamHintHeader> outPendingNotificationHints,
												List<BeamHintHeader> outPendingNotificationValidations)
		{
			beamHintPreferencesManager.SplitHintsByNotificationPreferences(hintsToUpdate,
																		   out var toNotifyNever,
																		   out var sessionNotify,
																		   out var contextObjectChangeNotify);

			var notYetNotifiedThisSession = sessionNotify.Where(hint => !hintsClearedThisSession.Contains(hint.Header)).ToList();
			var notYetNotifiedWithCurrentContextObj = contextObjectChangeNotify.Where(hint =>
			{
				if (lastDetectedContextObjects.TryGetValue(hint.Header, out var ctxObject)) return ctxObject != hint.ContextObject;
				return true;
			}).ToList();

			var toNotify = notYetNotifiedThisSession
						   .Union(notYetNotifiedWithCurrentContextObj)
						   .Except(toNotifyNever)
						   .ToList();

			outPendingNotificationHints.AddRange(toNotify.Where(h => (h.Header.Type & BeamHintType.Hint) != 0).Select(h => h.Header).Distinct());
			outPendingNotificationValidations.AddRange(toNotify.Where(h => (h.Header.Type & BeamHintType.Validation) != 0).Select(h => h.Header).Distinct());
		}
	}
}
