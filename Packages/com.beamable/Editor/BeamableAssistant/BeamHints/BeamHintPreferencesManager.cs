using Beamable.Common.Assistant;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Beamable.Editor.Assistant
{
	/// <summary>
	/// Manages and persists <see cref="BeamHint"/> preferences. Can decide to display/ignore hints, play mode warnings and/or notifications.
	/// It persists this configuration in a per-session or permanent level. 
	/// </summary>
	public class BeamHintPreferencesManager : IBeamHintPreferencesManager
	{
		/// <summary>
		/// Different levels of persistence of any single <see cref="BeamHint"/>'s <see cref="BeamHintVisibilityPreference"/> that the <see cref="BeamHintPreferencesManager"/> supports.
		/// </summary>
		private enum PersistenceLevel
		{
			Permanent,
			Session,
		}

		/// <summary>
		/// Key into both <see cref="SessionState"/> and <see cref="EditorPrefs"/> to store number of <see cref="BeamHint"/> states persisted in each of these.
		/// Used to allocate once at startup and avoid continuous resize allocations mid-loop.
		/// </summary>
		private const string VISIBILITY_HIDDEN_SAVED_COUNT = "BEAM_HINT_" + nameof(VISIBILITY_HIDDEN_SAVED_COUNT);

		/// <summary>
		/// Key into both <see cref="SessionState"/> and <see cref="EditorPrefs"/> to store the <see cref="BEAM_HINT_PREFERENCES_SEPARATOR"/>-separated list
		/// of <see cref="BeamHintHeaders"/> states persisted in each prefs.
		/// </summary>
		private const string VISIBILITY_HIDDEN_SAVED = "BEAM_HINT_" + nameof(VISIBILITY_HIDDEN_SAVED);

		/// <summary>
		/// Key into both <see cref="SessionState"/> and <see cref="EditorPrefs"/> to store number of <see cref="BeamHint"/> states persisted in each of these.
		/// Used to allocate once at startup and avoid continuous resize allocations mid-loop.
		/// </summary>
		private const string PLAY_MODE_WARNING_ENABLED_SAVED_COUNT = "BEAM_HINT_" + nameof(PLAY_MODE_WARNING_ENABLED_SAVED_COUNT);

		/// <summary>
		/// Key into both <see cref="SessionState"/> and <see cref="EditorPrefs"/> to store the <see cref="BEAM_HINT_PREFERENCES_SEPARATOR"/>-separated list
		/// of <see cref="BeamHintHeaders"/> states persisted in each prefs.
		/// </summary>
		private const string PLAY_MODE_WARNING_ENABLED_SAVED = "BEAM_HINT_" + nameof(PLAY_MODE_WARNING_ENABLED_SAVED);

		/// <summary>
		/// Key into both <see cref="SessionState"/> and <see cref="EditorPrefs"/> to store number of <see cref="BeamHint"/> states persisted in each of these.
		/// Used to allocate once at startup and avoid continuous resize allocations mid-loop.
		/// </summary>
		private const string PLAY_MODE_WARNING_MANUALLY_DISABLED_SAVED_COUNT = "BEAM_HINT_" + nameof(PLAY_MODE_WARNING_MANUALLY_DISABLED_SAVED_COUNT);

		/// <summary>
		/// Key into both <see cref="SessionState"/> and <see cref="EditorPrefs"/> to store the <see cref="BEAM_HINT_PREFERENCES_SEPARATOR"/>-separated list
		/// of <see cref="BeamHintHeaders"/> states persisted in each prefs.
		/// </summary>
		private const string PLAY_MODE_WARNING_MANUALLY_DISABLED_SAVED = "BEAM_HINT_" + nameof(PLAY_MODE_WARNING_MANUALLY_DISABLED_SAVED);

		/// <summary>
		/// Key into <see cref="EditorPrefs"/> to store number of <see cref="BeamHint"/> states persisted to always notify.
		/// Used to allocate once at startup and avoid continuous resize allocations mid-loop.
		/// </summary>
		private const string NOTIFICATION_NEVER_SAVED_COUNT = "BEAM_HINT_" + nameof(NOTIFICATION_NEVER_SAVED_COUNT);

		/// <summary>
		/// Key into <see cref="EditorPrefs"/> to store the <see cref="BEAM_HINT_PREFERENCES_SEPARATOR"/>-separated list of hints that are configured to always notify.
		/// </summary>
		private const string NOTIFICATION_NEVER_SAVED = "BEAM_HINT_" + nameof(NOTIFICATION_NEVER_SAVED);


		/// <summary>
		/// Current state of each <see cref="BeamHint"/>. Mapped by the <see cref="BeamHintHeader"/>.
		/// Any not found <see cref="BeamHintHeader"/> is presumed to be with the following state: <see cref="VisibBeamHintVisibilityPreferencelay"/>.
		/// </summary>
		private readonly Dictionary<BeamHintHeader, BeamHintVisibilityPreference> _perHintVisibilityStates;

		/// <summary>
		/// List of all header's currently ignored in this <see cref="SessionState"/>. Helper list to make the code for managing this easier.
		/// </summary>
		private readonly List<BeamHintHeader> _sessionVisibilityIgnoredHints;

		/// <summary>
		/// List of all header's currently ignored in this <see cref="EditorPrefs"/>. Helper list to make the code for managing this easier.
		/// </summary>
		private readonly List<BeamHintHeader> _permanentlyVisibilityIgnoredHints;


		/// <summary>
		/// Current state of each <see cref="BeamHint"/>. Mapped by the <see cref="BeamHintHeader"/>.
		/// Any not found <see cref="BeamHintHeader"/> is presumed to be with the following state: <see cref="PlayMBeamHintPlayModeWarningPreferenceled"/>.
		/// </summary>
		private readonly Dictionary<BeamHintHeader, BeamHintPlayModeWarningPreference> _perHintPlayModeWarningStates;

		/// <summary>
		/// List of all header's currently disabled <see cref="BeamHintPlayModeWarningPreference"/> in this <see cref="SessionState"/>. Helper list to make the code for managing this easier.
		/// </summary>
		private readonly List<BeamHintHeader> _sessionPlayModeWarningEnabledHints;

		/// <summary>
		/// List of all header's currently disabled <see cref="BeamHintPlayModeWarningPreference"/> in this <see cref="EditorPrefs"/>. Helper list to make the code for managing this easier.
		/// </summary>
		private readonly List<BeamHintHeader> _permanentlyPlayModeWarningEnabledHints;

		/// <summary>
		/// List of all header's currently disabled <see cref="BeamHintPlayModeWarningPreference"/> in this <see cref="EditorPrefs"/>. Helper list to make the code for managing this easier.
		/// </summary>
		private readonly List<BeamHintHeader> _permanentlyPlayModeWarningManuallyDisabledHints;


		/// <summary>
		/// Current state of each <see cref="BeamHint"/>. Mapped by the <see cref="BeamHintHeader"/>.
		/// Any not found <see cref="BeamHintHeader"/> is presumed to be with the following states:
		/// <para>
		///  - If <see cref="BeamHintType.Hint"/>, the default is <see cref="BeamHintNotificationPreference.NotifyOncePerSession"/>.
		/// </para>
		/// <para>
		///  - If <see cref="BeamHintType.Validation"/>, the default is <see cref="BeamHintNotificationPreference.NotifyOnContextObjectChanged"/>. This assumes that validation hints
		/// change their context objects if they ever update a hint. 
		/// </para>
		/// </summary>
		private readonly Dictionary<BeamHintHeader, BeamHintNotificationPreference> _perHintNotificationStates;

		/// <summary>
		/// List of all headers currently set to <see cref="BeamHintNotificationPreference.NotifyNever"/>. Helper list to make the code for managing this easier.
		/// </summary>
		private readonly List<BeamHintHeader> _neverNotifyHints;

		/// <summary>
		/// List of all <see cref="BeamHintHeader"/>s for hint types that should have their play-mode-warnings Enabled-by-default. 
		/// </summary>
		private readonly List<BeamHintHeader> _hintsToPlayModeWarningByDefault;

		/// <summary>
		/// Creates a new <see cref="BeamHintPreferencesManager"/> instance you can use to manage <see cref="BeamHint"/> display/ignore preferences.
		/// </summary>
		public BeamHintPreferencesManager(List<BeamHintHeader> playModeWarningByDefaultHints = null)
		{
			var sessionVisibilityPrefsCount = SessionState.GetInt(VISIBILITY_HIDDEN_SAVED_COUNT, 0);
			var hintVisibilityPrefsCount = EditorPrefs.GetInt(VISIBILITY_HIDDEN_SAVED_COUNT, 0);
			_perHintVisibilityStates = new Dictionary<BeamHintHeader, BeamHintVisibilityPreference>(sessionVisibilityPrefsCount + hintVisibilityPrefsCount);

			_sessionVisibilityIgnoredHints = new List<BeamHintHeader>(sessionVisibilityPrefsCount);
			_permanentlyVisibilityIgnoredHints = new List<BeamHintHeader>(hintVisibilityPrefsCount);


			var sessionPlayModeWarningPrefsCount = SessionState.GetInt(PLAY_MODE_WARNING_ENABLED_SAVED_COUNT, 0);
			var hintPlayModeWarningPrefsCount = EditorPrefs.GetInt(PLAY_MODE_WARNING_ENABLED_SAVED_COUNT, 0);
			var hintPlayModeWarningManuallyDisabledPrefsCount = EditorPrefs.GetInt(PLAY_MODE_WARNING_MANUALLY_DISABLED_SAVED_COUNT, 0);
			_perHintPlayModeWarningStates = new Dictionary<BeamHintHeader, BeamHintPlayModeWarningPreference>(sessionPlayModeWarningPrefsCount + hintPlayModeWarningPrefsCount + hintPlayModeWarningManuallyDisabledPrefsCount);

			_sessionPlayModeWarningEnabledHints = new List<BeamHintHeader>(sessionPlayModeWarningPrefsCount);
			_permanentlyPlayModeWarningEnabledHints = new List<BeamHintHeader>(hintPlayModeWarningPrefsCount);
			_permanentlyPlayModeWarningManuallyDisabledHints = new List<BeamHintHeader>(hintPlayModeWarningManuallyDisabledPrefsCount);


			var neverNotifyCount = EditorPrefs.GetInt(NOTIFICATION_NEVER_SAVED_COUNT, 0);
			_perHintNotificationStates = new Dictionary<BeamHintHeader, BeamHintNotificationPreference>(neverNotifyCount);
			_neverNotifyHints = new List<BeamHintHeader>(neverNotifyCount);

			_hintsToPlayModeWarningByDefault = new List<BeamHintHeader>();
			_hintsToPlayModeWarningByDefault.AddRange(playModeWarningByDefaultHints ?? new List<BeamHintHeader>());
		}

		public void RebuildPerHintPreferences()
		{
			// Rebuild Visibility preferences
			_perHintVisibilityStates.Clear();
			_sessionVisibilityIgnoredHints.Clear();
			_permanentlyVisibilityIgnoredHints.Clear();

			// Go through editor prefs to get all permanently silenced hints
			var permanentSilencedHints = EditorPrefs.GetString(VISIBILITY_HIDDEN_SAVED, "");
			ApplyStoredHintPreferences(permanentSilencedHints, BeamHintVisibilityPreference.Hidden, _perHintVisibilityStates, _permanentlyVisibilityIgnoredHints);

			// Go through session state to get all silenced hints for this session
			var sessionSilencedHints = SessionState.GetString(VISIBILITY_HIDDEN_SAVED, "");
			ApplyStoredHintPreferences(sessionSilencedHints, BeamHintVisibilityPreference.Hidden, _perHintVisibilityStates, _sessionVisibilityIgnoredHints);


			// Rebuild Play-Mode-Warning preferences
			_perHintPlayModeWarningStates.Clear();
			_sessionPlayModeWarningEnabledHints.Clear();
			_permanentlyPlayModeWarningEnabledHints.Clear();
			_permanentlyPlayModeWarningManuallyDisabledHints.Clear();

			// Go through editor prefs to get all permanently play-mode-warning enabled hints
			var permanentEnabledPlayModeWarningHints = EditorPrefs.GetString(PLAY_MODE_WARNING_ENABLED_SAVED, "");
			ApplyStoredHintPreferences(permanentEnabledPlayModeWarningHints, BeamHintPlayModeWarningPreference.Enabled, _perHintPlayModeWarningStates, _permanentlyPlayModeWarningEnabledHints);

			// Go through session state to get all play-mode-warning enabled hints for this session
			var sessionEnabledPlayModeWarningHints = SessionState.GetString(PLAY_MODE_WARNING_ENABLED_SAVED, "");
			ApplyStoredHintPreferences(sessionEnabledPlayModeWarningHints, BeamHintPlayModeWarningPreference.EnabledDuringSession, _perHintPlayModeWarningStates, _sessionPlayModeWarningEnabledHints);

			// Go through manually disabled play-mode-warning hints
			var manuallyDisabledPlayModeWarningHints = EditorPrefs.GetString(PLAY_MODE_WARNING_MANUALLY_DISABLED_SAVED, "");
			ApplyStoredHintPreferences(manuallyDisabledPlayModeWarningHints, BeamHintPlayModeWarningPreference.Disabled, _perHintPlayModeWarningStates, _permanentlyPlayModeWarningManuallyDisabledHints);

			// Goes through list of enabled-by-default beam hint headers and, if they were not manually disabled, add them to the dictionary as enabled.
			foreach (var header in _hintsToPlayModeWarningByDefault)
			{
				// Don't add to the helper list as we don't need this value to be serialized and persisted since this always sets up the default value as Enabled for these hints.
				// Only happens, when the game maker hasn't explicitly chosen a setup -- if they did, that would've been serialized and therefore, the header would already be in the dictionary.
				if (!_perHintPlayModeWarningStates.TryGetValue(header, out _))
					_perHintPlayModeWarningStates.Add(header, BeamHintPlayModeWarningPreference.Enabled);
			}

			// Rebuild Notification preferences
			_perHintNotificationStates.Clear();
			_neverNotifyHints.Clear();

			// Go through stored notification preferences set as NotifyNever. 
			var neverNotificationHints = EditorPrefs.GetString(NOTIFICATION_NEVER_SAVED, "");
			ApplyStoredHintPreferences(neverNotificationHints, BeamHintNotificationPreference.NotifyNever, _perHintNotificationStates, _neverNotifyHints);


		}

		/// <summary>
		/// /// <summary>
		/// Deserializes and stores <see cref="BeamHintHeader"/> and <typeparamref name="T"/> from a serialized string of <see cref="BEAM_HINT_PREFERENCES_SEPARATOR"/>-separated
		/// <see cref="BeamHintHeader"/>s and the given state for it. Stores the results both in <paramref name="outHintStateStore"/> and <paramref name="outPerStateList"/>.
		/// </summary>
		///
		/// </summary>
		/// <param name="savedSerializedHeaders">
		/// The <see cref="BEAM_HINT_PREFERENCES_SEPARATOR"/>-separated list of
		/// <see cref="BeamHintHeader"/>s (via <see cref="BeamHintHeader.AsKey"/>).
		/// </param>
		/// 
		/// <param name="stateToRestore">
		/// The state to apply to all deserialized <paramref name="savedSerializedHeaders"/>.
		/// </param>
		///
		/// <param name="outHintStateStore">
		/// A dictionary to store the combination of deserialized <paramref name="savedSerializedHeaders"/> and <paramref name="stateToRestore"/>. 
		/// </param>
		/// <param name="outPerStateList">
		/// A list to add to the deserialized <paramref name="savedSerializedHeaders"/> into. 
		/// </param>
		/// <typeparam name="T">An enum defining the state of preferences for a given hint.</typeparam>
		private void ApplyStoredHintPreferences<T>(string savedSerializedHeaders,
												T stateToRestore,
												Dictionary<BeamHintHeader, T> outHintStateStore,
												List<BeamHintHeader> outPerStateList) where T : Enum
		{
			var savedSerializedHeadersArray = savedSerializedHeaders.Split(new[] { BeamHintSharedConstants.BEAM_HINT_PREFERENCES_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string serializedHeader in savedSerializedHeadersArray)
			{
				var header = BeamHintHeader.DeserializeBeamHintHeader(serializedHeader);

				// Skip existing values when deserializing --- this means that the order in which we deserialize can be used to create behaviour.
				// Ex: For Play-Mode-Warnings, we load Enabled in Session hints before manually disabled hints. This means that if you manually disable a hint or set it up as in this session only,
				// it'll be loaded as EnabledInSession for the duration of this session and then revert to a Manually disabled hint.
				if (!outHintStateStore.TryGetValue(header, out _)) outHintStateStore.Add(header, stateToRestore);

				outPerStateList.Add(header);
			}
		}


		public BeamHintVisibilityPreference GetHintVisibilityPreferences(BeamHint hint)
		{
			if (_perHintVisibilityStates.TryGetValue(hint.Header, out var visibilityState))
				return visibilityState;

			return BeamHintVisibilityPreference.Display;
		}

		public void SetHintVisibilityPreferences(BeamHint hint, BeamHintVisibilityPreference newBeamHintVisibilityPreference)
		{
			if (_perHintVisibilityStates.TryGetValue(hint.Header, out var currState))
				currState = _perHintVisibilityStates[hint.Header] = newBeamHintVisibilityPreference;
			else
			{
				_perHintVisibilityStates.Add(hint.Header, newBeamHintVisibilityPreference);
				currState = newBeamHintVisibilityPreference;
			}

			if (currState == BeamHintVisibilityPreference.Display)
			{
				RemoveHintPreferenceState(hint,
										  VISIBILITY_HIDDEN_SAVED,
										  VISIBILITY_HIDDEN_SAVED_COUNT,
										  PersistenceLevel.Permanent,
										  _permanentlyVisibilityIgnoredHints);

				RemoveHintPreferenceState(hint,
										  VISIBILITY_HIDDEN_SAVED,
										  VISIBILITY_HIDDEN_SAVED_COUNT,
										  PersistenceLevel.Session,
										  _sessionVisibilityIgnoredHints);
			}

			if (currState == BeamHintVisibilityPreference.HiddenDuringSession)
			{
				RemoveHintPreferenceState(hint,
										  VISIBILITY_HIDDEN_SAVED,
										  VISIBILITY_HIDDEN_SAVED_COUNT,
										  PersistenceLevel.Permanent,
										  _permanentlyVisibilityIgnoredHints);

				SetSerializedHintPreference(hint,
											VISIBILITY_HIDDEN_SAVED,
											VISIBILITY_HIDDEN_SAVED_COUNT,
											PersistenceLevel.Session,
											_sessionVisibilityIgnoredHints);
			}

			if (currState == BeamHintVisibilityPreference.Hidden)
			{
				RemoveHintPreferenceState(hint,
										  VISIBILITY_HIDDEN_SAVED,
										  VISIBILITY_HIDDEN_SAVED_COUNT,
										  PersistenceLevel.Session,
										  _sessionVisibilityIgnoredHints);

				SetSerializedHintPreference(hint,
											VISIBILITY_HIDDEN_SAVED,
											VISIBILITY_HIDDEN_SAVED_COUNT,
											PersistenceLevel.Permanent,
											_permanentlyVisibilityIgnoredHints);

			}
		}

		public void SplitHintsByVisibilityPreferences(IEnumerable<BeamHint> hints, out IEnumerable<BeamHint> outToDisplayHints, out IEnumerable<BeamHint> outToIgnoreHints)
		{
			var groups = hints.GroupBy(h =>
			{
				if (!_perHintVisibilityStates.TryGetValue(h.Header, out var state))
					state = BeamHintVisibilityPreference.Display;

				return state;
			}).ToList();

			outToDisplayHints = groups.Where(h => h.Key == BeamHintVisibilityPreference.Display)
									  .SelectMany(h => h);

			outToIgnoreHints = groups.Where(h => h.Key == BeamHintVisibilityPreference.Hidden || h.Key == BeamHintVisibilityPreference.HiddenDuringSession)
									 .SelectMany(h => h);
		}



		public BeamHintPlayModeWarningPreference GetHintPlayModeWarningPreferences(BeamHint hint)
		{
			if (_perHintPlayModeWarningStates.TryGetValue(hint.Header, out var playModeWarningState))
				return playModeWarningState;

			return BeamHintPlayModeWarningPreference.Disabled;
		}

		public void SetHintPlayModeWarningPreferences(BeamHint hint, BeamHintPlayModeWarningPreference newBeamHintPlayModeWarningPreference)
		{
			if (_perHintPlayModeWarningStates.TryGetValue(hint.Header, out var currState))
				currState = _perHintPlayModeWarningStates[hint.Header] = newBeamHintPlayModeWarningPreference;
			else
			{
				_perHintPlayModeWarningStates.Add(hint.Header, newBeamHintPlayModeWarningPreference);
				currState = newBeamHintPlayModeWarningPreference;
			}

			if (currState == BeamHintPlayModeWarningPreference.Disabled)
			{
				RemoveHintPreferenceState(hint,
										  PLAY_MODE_WARNING_ENABLED_SAVED,
										  PLAY_MODE_WARNING_ENABLED_SAVED_COUNT,
										  PersistenceLevel.Permanent,
										  _permanentlyPlayModeWarningEnabledHints);

				RemoveHintPreferenceState(hint,
										  PLAY_MODE_WARNING_ENABLED_SAVED,
										  PLAY_MODE_WARNING_ENABLED_SAVED_COUNT,
										  PersistenceLevel.Session,
										  _sessionPlayModeWarningEnabledHints);

				SetSerializedHintPreference(hint,
										  PLAY_MODE_WARNING_MANUALLY_DISABLED_SAVED,
										  PLAY_MODE_WARNING_MANUALLY_DISABLED_SAVED_COUNT,
										  PersistenceLevel.Permanent,
										  _permanentlyPlayModeWarningManuallyDisabledHints);
			}

			if (currState == BeamHintPlayModeWarningPreference.EnabledDuringSession)
			{
				RemoveHintPreferenceState(hint,
										  PLAY_MODE_WARNING_ENABLED_SAVED,
										  PLAY_MODE_WARNING_ENABLED_SAVED_COUNT,
										  PersistenceLevel.Permanent,
										  _permanentlyPlayModeWarningEnabledHints);

				// Sets this as manually disabled, so it correctly reverts to disabled after this session ends.
				// See ApplyStoredHintPreferences() for more details on how this behaviour is enforced via this.
				SetSerializedHintPreference(hint,
											PLAY_MODE_WARNING_MANUALLY_DISABLED_SAVED,
											PLAY_MODE_WARNING_MANUALLY_DISABLED_SAVED_COUNT,
											PersistenceLevel.Permanent,
											_permanentlyPlayModeWarningManuallyDisabledHints);

				SetSerializedHintPreference(hint,
											PLAY_MODE_WARNING_ENABLED_SAVED,
											PLAY_MODE_WARNING_ENABLED_SAVED_COUNT,
											PersistenceLevel.Session,
											_sessionPlayModeWarningEnabledHints);
			}

			if (currState == BeamHintPlayModeWarningPreference.Enabled)
			{
				RemoveHintPreferenceState(hint,
										  PLAY_MODE_WARNING_MANUALLY_DISABLED_SAVED,
										  PLAY_MODE_WARNING_MANUALLY_DISABLED_SAVED_COUNT,
										  PersistenceLevel.Permanent,
										  _permanentlyPlayModeWarningManuallyDisabledHints);

				RemoveHintPreferenceState(hint,
										  PLAY_MODE_WARNING_ENABLED_SAVED,
										  PLAY_MODE_WARNING_ENABLED_SAVED_COUNT,
										  PersistenceLevel.Session,
										  _sessionPlayModeWarningEnabledHints);

				SetSerializedHintPreference(hint,
											PLAY_MODE_WARNING_ENABLED_SAVED,
											PLAY_MODE_WARNING_ENABLED_SAVED_COUNT,
											PersistenceLevel.Permanent,
											_permanentlyPlayModeWarningEnabledHints);

			}
		}

		public void SplitHintsByPlayModeWarningPreferences(IEnumerable<BeamHint> hints, out IEnumerable<BeamHint> outToWarnHints, out IEnumerable<BeamHint> outToIgnoreHints)
		{
			var groups = hints.GroupBy(h =>
			{

				if (!_perHintPlayModeWarningStates.TryGetValue(h.Header, out var state))
				{
					state = BeamHintPlayModeWarningPreference.Disabled;
				}

				return state;
			}).ToList();

			outToWarnHints = groups.Where(h => h.Key == BeamHintPlayModeWarningPreference.Enabled || h.Key == BeamHintPlayModeWarningPreference.EnabledDuringSession)
								   .SelectMany(h => h);

			outToIgnoreHints = groups.Where(h => h.Key == BeamHintPlayModeWarningPreference.Disabled)
									 .SelectMany(h => h);
		}




		public BeamHintNotificationPreference GetHintNotificationPreferences(BeamHint hint)
		{
			if (_perHintNotificationStates.TryGetValue(hint.Header, out var state)) return state;
			return hint.Header.Type == BeamHintType.Validation ? BeamHintNotificationPreference.NotifyOnContextObjectChanged : BeamHintNotificationPreference.NotifyOncePerSession;
		}

		public void SetHintNotificationPreferences(BeamHint hint, BeamHintNotificationPreference newBeamHintNotificationPreference)
		{
			if (_perHintNotificationStates.TryGetValue(hint.Header, out var currState))
				currState = _perHintNotificationStates[hint.Header] = newBeamHintNotificationPreference;
			else
			{
				_perHintNotificationStates.Add(hint.Header, newBeamHintNotificationPreference);
				currState = newBeamHintNotificationPreference;
			}

			switch (currState)
			{
				case BeamHintNotificationPreference.NotifyOncePerSession:
				case BeamHintNotificationPreference.NotifyOnContextObjectChanged:
				{
					RemoveHintPreferenceState(hint, NOTIFICATION_NEVER_SAVED, NOTIFICATION_NEVER_SAVED_COUNT, PersistenceLevel.Permanent, _neverNotifyHints);
					break;
				}
				case BeamHintNotificationPreference.NotifyNever:
				{
					SetSerializedHintPreference(hint, NOTIFICATION_NEVER_SAVED, NOTIFICATION_NEVER_SAVED_COUNT, PersistenceLevel.Permanent, _neverNotifyHints);
					break;
				}
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void SplitHintsByNotificationPreferences(IEnumerable<BeamHint> hints,
														out List<BeamHint> outToNotifyNever,
														out List<BeamHint> outToNotifyOncePerSession,
														out List<BeamHint> outToNotifyOnContextObjectChange)
		{
			var groups = hints.GroupBy(h =>
			{

				if (!_perHintNotificationStates.TryGetValue(h.Header, out var state))
				{
					switch (h.Header.Type)
					{
						case BeamHintType.Hint:
						{
							state = BeamHintNotificationPreference.NotifyOncePerSession;
							break;
						}
						case BeamHintType.Validation:
						{
							state = BeamHintNotificationPreference.NotifyOnContextObjectChanged;
							break;
						}
						default:
							throw new ArgumentOutOfRangeException();
					}
				}

				return state;
			}).ToList();


			outToNotifyNever = groups.Where(h => h.Key == BeamHintNotificationPreference.NotifyNever)
									 .SelectMany(h => h).ToList();

			outToNotifyOncePerSession = groups.Where(h => h.Key == BeamHintNotificationPreference.NotifyOncePerSession)
											  .SelectMany(h => h).ToList();

			outToNotifyOnContextObjectChange = groups.Where(h => h.Key == BeamHintNotificationPreference.NotifyOnContextObjectChanged)
													 .SelectMany(h => h).ToList();
		}



		/// <summary>
		/// Removes the given <paramref name="hint"/> from it's given <paramref name="persistenceLevel"/> while updating a helper per-state lists to make it easier to manage the
		/// string-based <see cref="EditorPrefs"/> and <see cref="SessionState"/>.
		/// <param name="hint">The hint to remove the preference for.</param>
		/// <param name="preferencesKey">The key to store the preferences state in.</param>
		/// <param name="preferencesCountKey">The key to store the preferences state count in.</param>
		/// <param name="persistenceLevel">Whether to save the updated preferences using <see cref="SessionState"/> or <see cref="EditorPrefs"/>.</param>
		/// <param name="outHints">
		/// Helper list of headers for the preference you are persisting.
		/// Caller should pass correct list based on <paramref name="persistenceLevel"/>.
		/// </param>
		private void RemoveHintPreferenceState(BeamHint hint,
											   string preferencesKey,
											   string preferencesCountKey,
											   PersistenceLevel persistenceLevel,
											   List<BeamHintHeader> outHints)
		{
			outHints.Remove(hint.Header);
			outHints = outHints.Distinct().ToList();
			var keys = outHints.Select(header => header.AsKey()).ToList();
			var serializedPreferences = string.Join(BeamHintSharedConstants.BEAM_HINT_PREFERENCES_SEPARATOR, keys);

			switch (persistenceLevel)
			{
				case PersistenceLevel.Permanent:
				{
					EditorPrefs.SetString(preferencesKey, serializedPreferences);
					EditorPrefs.SetInt(preferencesCountKey, outHints.Count);
					break;
				}
				case PersistenceLevel.Session:
				{
					SessionState.SetString(preferencesKey, serializedPreferences);
					SessionState.SetInt(preferencesCountKey, outHints.Count);
					break;
				}
				default:
					throw new ArgumentOutOfRangeException(nameof(persistenceLevel), persistenceLevel, null);
			}
		}


		/// <summary>
		/// Adds the given <paramref name="hint"/> to it's given <paramref name="persistenceLevel"/> while updating the helper per-state list to make it easier to manage the
		/// string-based <see cref="EditorPrefs"/> and <see cref="SessionState"/>.
		/// </summary>
		/// <param name="hint">The hint to serialize the preferences for.</param>
		/// <param name="preferencesKey">The key to store the preferences state in.</param>
		/// <param name="preferencesCountKey">The key to store the preferences state count in.</param>
		/// <param name="persistenceLevel">Whether to save the preferences using <see cref="SessionState"/> or <see cref="EditorPrefs"/>.</param>
		/// <param name="outHints">
		/// Helper list of headers for the preference you are persisting.
		/// Caller should pass correct list based on <paramref name="persistenceLevel"/>.
		/// </param>
		private void SetSerializedHintPreference(BeamHint hint,
												  string preferencesKey,
												  string preferencesCountKey,
												  PersistenceLevel persistenceLevel, List<BeamHintHeader> outHints)
		{
			outHints.Add(hint.Header);
			outHints = outHints.Distinct().ToList();
			var keys = outHints.Select(header => header.AsKey()).ToList();

			switch (persistenceLevel)
			{
				case PersistenceLevel.Permanent:
				{
					EditorPrefs.SetString(preferencesKey, string.Join(BeamHintSharedConstants.BEAM_HINT_PREFERENCES_SEPARATOR, keys));
					EditorPrefs.SetInt(preferencesCountKey, keys.Count);
					break;
				}
				case PersistenceLevel.Session:
				{
					SessionState.SetString(preferencesKey, string.Join(BeamHintSharedConstants.BEAM_HINT_PREFERENCES_SEPARATOR, keys));
					SessionState.SetInt(preferencesCountKey, keys.Count);
					break;
				}
				default:
					throw new ArgumentOutOfRangeException(nameof(persistenceLevel), persistenceLevel, null);
			}
		}


		/// <summary>
		/// Discards all persisted <see cref="BeamHintVisibilityPreference"/>s, <see cref="BeamHintPlayModeWarningPreference"/>s and <see cref="BeamHintNotificationPreference"/>s of all hints.
		/// </summary>
		public void ClearAllPreferences()
		{
			EditorPrefs.SetString(VISIBILITY_HIDDEN_SAVED, "");
			SessionState.SetString(VISIBILITY_HIDDEN_SAVED, "");

			EditorPrefs.SetInt(VISIBILITY_HIDDEN_SAVED_COUNT, 0);
			SessionState.SetInt(VISIBILITY_HIDDEN_SAVED_COUNT, 0);

			EditorPrefs.SetString(PLAY_MODE_WARNING_ENABLED_SAVED, "");
			SessionState.SetString(PLAY_MODE_WARNING_ENABLED_SAVED, "");

			EditorPrefs.SetInt(PLAY_MODE_WARNING_ENABLED_SAVED_COUNT, 0);
			SessionState.SetInt(PLAY_MODE_WARNING_ENABLED_SAVED_COUNT, 0);

			EditorPrefs.SetInt(NOTIFICATION_NEVER_SAVED_COUNT, 0);
			EditorPrefs.SetString(NOTIFICATION_NEVER_SAVED, "");

			RebuildPerHintPreferences();
		}
	}
}
