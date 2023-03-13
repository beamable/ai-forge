using Beamable.Common.Reflection;
using BeamableReflection;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Beamable.Common.Assistant
{
	[Flags]
	public enum BeamHintType
	{
		Invalid = 0,
		Validation = 1 << 0,
		Hint = 1 << 1,

		All = Validation | Hint
	}

	/// <summary>
	/// Constants that are used across all Beamable Packages by the BeamHint system.
	/// </summary>
	public static class BeamHintSharedConstants
	{
		/// <summary> 
		/// Separator used to split the stored string of serialized <see cref="BeamHintHeader"/>s (via <see cref="BeamHintHeader.AsKey"/>).
		/// Used by <see cref="BeamHintPreferencesManager"/>.
		/// </summary>
		public const string BEAM_HINT_PREFERENCES_SEPARATOR = "₢";
	}

	/// <summary>
	/// A compound-key identifying each hint.
	/// It is a string-based compound key whose individual fields cannot have <see cref="AS_KEY_SEPARATOR"/> or <see cref="BeamHintSharedConstants.BEAM_HINT_PREFERENCES_SEPARATOR"/>.
	/// These are reserved by our internal systems that manage hints.
	/// See <see cref="BeamHintDomains"/> and <see cref="BeamHintIds"/> for a better understanding of how to generate hint domains and ids.
	/// </summary>
	[System.Serializable]
	public struct BeamHintHeader : IEquatable<BeamHintHeader>
	{
		public const string AS_KEY_SEPARATOR = "¬¬";

		/// <summary>
		/// Type of the <see cref="BeamHint"/>.
		/// </summary>
		public BeamHintType Type;

		/// <summary>
		/// Domain this hint belongs to. See <see cref="BeamHintDomains"/> for more details.
		/// </summary>
		public string Domain;

		/// <summary>
		/// Unique Id, within <see cref="Domain"/> and <see cref="Type"/>, that represents these hints.
		/// Cannot have "₢" character as it is reserved by the system. 
		/// </summary>
		public string Id;

		/// <summary>
		/// Creates a new header with the given <paramref name="type"/>, <paramref name="domain"/> and <see cref="id"/>.
		/// See <see cref="BeamHintDomains"/> and <see cref="BeamHintIds"/> for a better understanding of how these are generated.
		/// </summary>
		public BeamHintHeader(BeamHintType type, string domain, string id = "")
		{
			System.Diagnostics.Debug.Assert(!(domain.Contains(AS_KEY_SEPARATOR) || domain.Contains(BeamHintSharedConstants.BEAM_HINT_PREFERENCES_SEPARATOR)),
											$"Domain [{domain}] cannot contain: '{AS_KEY_SEPARATOR}' or '{BeamHintSharedConstants.BEAM_HINT_PREFERENCES_SEPARATOR}'");
			System.Diagnostics.Debug.Assert(
				!(id.Contains(AS_KEY_SEPARATOR) || id.Contains(BeamHintDomains.SUB_DOMAIN_SEPARATOR) || id.Contains(BeamHintSharedConstants.BEAM_HINT_PREFERENCES_SEPARATOR)),
				$"Id [{id}] cannot contain: '{AS_KEY_SEPARATOR}', '{BeamHintDomains.SUB_DOMAIN_SEPARATOR}' or '{BeamHintSharedConstants.BEAM_HINT_PREFERENCES_SEPARATOR}'");

			Type = type;
			Domain = domain;
			Id = id;
		}

		public bool Equals(BeamHintHeader other) => Type == other.Type && Domain == other.Domain && Id == other.Id;

		public override bool Equals(object obj) => obj is BeamHintHeader other && Equals(other);

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (int)Type;
				hashCode = (hashCode * 397) ^ (Domain != null ? Domain.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Id != null ? Id.GetHashCode() : 0);
				return hashCode;
			}
		}

		/// <summary>
		/// Returns the header in it's "key" string format. This is used to interface with EditorPrefs/SessionState in multiple places.
		/// </summary>
		public string AsKey() => $"{Type}{AS_KEY_SEPARATOR}{Domain}{AS_KEY_SEPARATOR}{Id}";

		/// <summary>
		/// Deserializes a single <see cref="BeamHintHeader"/> in the format provided by <see cref="BeamHintHeader.AsKey"/>.
		/// </summary>
		public static BeamHintHeader DeserializeBeamHintHeader(string serializedHint)
		{
			var typeDomainId = serializedHint.Split(new[] { BeamHintHeader.AS_KEY_SEPARATOR }, StringSplitOptions.None);
			var type = (BeamHintType)Enum.Parse(typeof(BeamHintType), typeDomainId[0]);
			var domain = typeDomainId[1];
			var id = typeDomainId[2];

			return new BeamHintHeader(type, domain, id);
		}

		public override string ToString() => $"{nameof(Type)}: {Type}, {nameof(Domain)}: {Domain}, {nameof(Id)}: {Id}";
	}

	/// <summary>
	/// Helper struct to deal with an individual BeamHint. Contains it's <see cref="BeamHintHeader"/> (unique key identifying the BeamHint) and a read-only reference to the hint's object.  
	/// </summary>
	public readonly struct BeamHint : IEquatable<BeamHint>, IEquatable<BeamHintHeader>
	{
		/// <summary>
		/// The unique <see cref="BeamHintHeader"/> identifying this hint.
		/// </summary>
		public readonly BeamHintHeader Header;

		/// <summary>
		/// An object associated with this hint to provide some context to it (normally used to render hint details via HintDetailsProvider).
		/// </summary>
		public readonly object ContextObject;

		public BeamHint(BeamHintHeader header, object contextObject)
		{
			this.Header = header;
			ContextObject = contextObject;
		}

		public bool Equals(BeamHint other) => other.Header.Equals(Header);
		public bool Equals(BeamHintHeader other) => other.Equals(Header);
		public override string ToString() => $"{nameof(Header)}: {Header}, {nameof(ContextObject)}: {ContextObject}";
	}

	/// <summary>
	/// Manages and persists <see cref="BeamHint"/> preferences. Can decide to display/ignore hints, play mode warnings and/or notifications.
	/// It persists this configuration in a per-session or permanent level. 
	/// </summary>
	public interface IBeamHintPreferencesManager
	{
		/// <summary>
		/// Restores the current in-memory state of the <see cref="BeamHintPreferencesManager"/> to match what is stored in its persistent storages.
		/// </summary>
		void RebuildPerHintPreferences();

		/// <summary>
		/// Gets the current <see cref="BeamHintVisibilityPreference"/> for the given hint.  
		/// </summary>
		BeamHintVisibilityPreference GetHintVisibilityPreferences(BeamHint hint);

		/// <summary>
		/// Sets, for the given <paramref name="hint"/>, the given <paramref name="newBeamHintVisibilityPreference"/>.
		/// </summary>
		void SetHintVisibilityPreferences(BeamHint hint, BeamHintVisibilityPreference newBeamHintVisibilityPreference);

		/// <summary>
		/// Splits all given hints by their <see cref="BeamHintVisibilityPreference"/>s.
		/// </summary>
		/// <param name="hints">The hints to split by.</param>
		/// <param name="outToDisplayHints">The resulting list of <see cref="BeamHint"/>s that should be displayed.</param>
		/// <param name="outToIgnoreHints">The resulting list of <see cref="BeamHint"/>s that should be ignored.</param>
		void SplitHintsByVisibilityPreferences(IEnumerable<BeamHint> hints, out IEnumerable<BeamHint> outToDisplayHints, out IEnumerable<BeamHint> outToIgnoreHints);

		/// <summary>
		/// Gets the current <see cref="BeamHintPlayModeWarningPreference"/> for the given hint.  
		/// </summary>
		BeamHintPlayModeWarningPreference GetHintPlayModeWarningPreferences(BeamHint hint);

		/// <summary>
		/// Sets, for the given <paramref name="hint"/>, the given <paramref name="newBeamHintPlayModeWarningPreference"/>.
		/// </summary>
		void SetHintPlayModeWarningPreferences(BeamHint hint, BeamHintPlayModeWarningPreference newBeamHintPlayModeWarningPreference);

		/// <summary>
		/// Splits all given hints by their <see cref="BeamHintPlayModeWarningPreference"/>s.
		/// </summary>
		/// <param name="hints">The hints to split by.</param>
		/// <param name="outToWarnHints">The resulting list of <see cref="BeamHint"/>s that should cause a play-mode-warning.</param>
		/// <param name="outToIgnoreHints">The resulting list of <see cref="BeamHint"/>s that should cause a play-mode-warning.</param>
		void SplitHintsByPlayModeWarningPreferences(IEnumerable<BeamHint> hints, out IEnumerable<BeamHint> outToWarnHints, out IEnumerable<BeamHint> outToIgnoreHints);

		/// <summary>
		/// Gets the current <see cref="BeamHintNotificationPreference"/> for the given hint.
		/// </summary>
		BeamHintNotificationPreference GetHintNotificationPreferences(BeamHint hint);

		/// <summary>
		/// Update the <see cref="BeamHintNotificationPreference"/> for a given hint. 
		/// </summary>
		void SetHintNotificationPreferences(BeamHint hint, BeamHintNotificationPreference newBeamHintNotificationPreference);

		/// <summary>
		/// Splits all given hints by their <see cref="BeamHintPlayModeWarningPreference"/>s.
		/// </summary>
		/// <param name="hints">The hints to split by.</param>
		/// <param name="outToNotifyNever">The resulting list of <see cref="BeamHint"/>s that should never notify.</param>
		/// <param name="outToNotifyOncePerSession">The resulting list of <see cref="BeamHint"/>s that should notify only once per session.</param>
		/// <param name="outToNotifyOnContextObjectChange">The resulting list of <see cref="BeamHint"/>s that should notify whenever the context object changed.</param>
		void SplitHintsByNotificationPreferences(IEnumerable<BeamHint> hints,
												 out List<BeamHint> outToNotifyNever,
												 out List<BeamHint> outToNotifyOncePerSession,
												 out List<BeamHint> outToNotifyOnContextObjectChange);

		/// <summary>
		/// Discards all persisted <see cref="BeamHintVisibilityPreference"/>s, <see cref="BeamHintPlayModeWarningPreference"/>s and <see cref="BeamHintNotificationPreference"/>s of all hints.
		/// </summary>
		void ClearAllPreferences();
	}

	/// <summary>
	/// Current State of display tied to any specific <see cref="BeamHintHeader"/>.
	/// </summary>
	public enum BeamHintVisibilityPreference
	{
		Display,
		HiddenDuringSession,
		Hidden,
	}

	/// <summary>
	/// Current State of the play mode warning tied to any specific <see cref="BeamHintHeader"/>.
	/// </summary>
	public enum BeamHintPlayModeWarningPreference
	{
		Enabled,
		EnabledDuringSession,
		Disabled,
	}

	/// <summary>
	/// Current state of the Notification preference setting tied to any specific <see cref="BeamHintHeader"/>s.
	/// </summary>
	public enum BeamHintNotificationPreference
	{
		NotifyOncePerSession, // Default for hints ----------------> Stores which hints were already notified in SessionState
		NotifyOnContextObjectChanged, // Default for validations --> Stores ContextObject of each hint in internal state, compares when bumps into a hint --- if not same reference, notify. Assumes that validations will change the Hint's context object when they run again and therefore should be notified again.
		NotifyNever, // Only if user explicitly asks for these ----> Never notifies the user
	}

	/// <summary>
	/// These are <see cref="UnityEditor"/> only systems. You should assume they'll only exist "#if UNITY_EDITOR" is true.
	/// You can use these to <see cref="IBeamHintSystem"/>s read, filter, clear and arrange data logically in relation to <see cref="BeamHintHeader"/>s to be read by UI and other systems.
	/// Keep in mind:
	/// <list type="bullet">
	/// <item>These only work in editor --- their instances won't ever be initialized outside of it.</item>
	/// <item>Place <see cref="System.Diagnostics.ConditionalAttribute"/>("UNITY_EDITOR") on all functions you plan to call from non-editor code.</item>
	/// <item>For use with non-editor only systems, write pure functions that take in ALL the necessary data from the non-editor systems and does the processing and addition of hints. </item>
	/// <item>Use "void" functions whenever interacting with non-editor code as we'll strip these calls in non-editor environments.</item>
	/// <item>If you need state to decide to show hints, aggregate and store this state in the <see cref="IBeamHintSystem"/>'s member fields via calls to methods that are <see cref="System.Diagnostics.ConditionalAttribute"/>. </item> 
	/// </list>
	/// 
	/// Keep the usage of these as simple as you can.
	/// The point of these systems is to provide a simple way for teams to bake their assumptions and established conventions into their editor workflow.
	/// If complicate the process of doing this, it defeats its very purpose.
	/// </summary>
	public interface IBeamHintSystem
	{
		void SetPreferencesManager(IBeamHintPreferencesManager preferencesManager);
		void SetStorage(IBeamHintGlobalStorage hintGlobalStorage);

		void OnInitialized();
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class BeamHintSystemAttribute : PreserveAttribute, IReflectionAttribute
	{
		public bool IsBeamContextSystem
		{
			get;
		}

		public BeamHintSystemAttribute(bool isBeamContextSystem = false)
		{
			IsBeamContextSystem = isBeamContextSystem;
		}

		public AttributeValidationResult IsAllowedOnMember(MemberInfo member)
		{
			var type = (Type)member;
			if (!typeof(IBeamHintSystem).IsAssignableFrom(type))
			{
				return new AttributeValidationResult(this, type, ReflectionCache.ValidationResultType.Error, $"BeamHintSystemAttribute cannot be over type [{member.Name}] " +
																											 $"since [{member.Name}] does not implement [{nameof(IBeamHintSystem)}].");
			}

			return new AttributeValidationResult(this, type, ReflectionCache.ValidationResultType.Valid, "");
		}
	}
}
