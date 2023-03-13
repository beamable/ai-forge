using Beamable.Common.Reflection;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Beamable.Server
{
	[AttributeUsage(AttributeTargets.Class)]
	public class MicroserviceAttribute : Attribute, INamingAttribute
	{
		public string MicroserviceName { get; }
		public string SourcePath { get; }

		[Obsolete(
		   "Any new client build of your game won't require the payload string. Unless you've deployed a client build using Beamable before version 0.11.0, you shouldn't set this")]
		public bool UseLegacySerialization { get; set; } = false;

		/// <summary>
		/// <b>Danger!</b>, if you enable this field, you will not be able to rely on content loaded from the Microservice code.
		/// Normally, when a Beamable Microservice starts, it registers an event subscription with Beamable's internal platform,
		/// so that it can receive a stream of Beamable events. The primary use case is to receive events about Content Publication.
		/// When a Content Publication event is received, the Microservice flushes the in-memory content cache. After the cache is flushed,
		/// all future content requests download new content. Without the event, the content downloaded on the Microservice will
		/// always be the same as it was the first time the content was requested.
		///
		/// When this flag is enabled, the Microservice will not register an event subscription. This may be useful to prevent
		/// the Microservice from receiving other unrelated Beamable platform events such as inventory-change events.
		/// </summary>
		public bool DisableAllBeamableEvents { get; set; } = false;

		/// <summary>
		/// When enabled, the Microservice will download the latest content manifest before declaring itself ready
		/// to accept traffic. This will add time to your Microservice startup time, but will reduce the time
		/// it takes for the first request that requires content to complete. It is enabled by default.
		/// </summary>
		public bool EnableEagerContentLoading { get; set; } = true;

		public MicroserviceAttribute(string microserviceName, [CallerFilePath] string sourcePath = "")
		{
			MicroserviceName = microserviceName;
			SourcePath = sourcePath;
		}

		public string GetSourcePath()
		{
			return SourcePath;
		}

		public AttributeValidationResult IsAllowedOnMember(MemberInfo member)
		{
			// Guaranteed to be a type, due to AttributeUsage attribute being set to Class.

			if (!typeof(Microservice).IsAssignableFrom((Type)member))
			{
				return new AttributeValidationResult(this,
																			member,
																			ReflectionCache.ValidationResultType.Error,
																			$"Microservice Attribute [{MicroserviceName}] cannot be over type [{member.Name}] " +
																			$"since [{member.Name}] does not inherit from [{nameof(Microservice)}].");
			}

			return new AttributeValidationResult(this, member, ReflectionCache.ValidationResultType.Valid, "");
		}

		public string[] Names => new[] { MicroserviceName };

		public AttributeValidationResult AreValidNameForType(MemberInfo member, string[] potentialNames)
		{
			// TODO: Validate no invalid characters are in the C#MS/Storage Object name
			return new AttributeValidationResult(this, member, ReflectionCache.ValidationResultType.Valid, "");
		}
	}
}
