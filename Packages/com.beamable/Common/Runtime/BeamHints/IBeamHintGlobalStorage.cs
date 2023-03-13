using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Beamable.Common.Assistant
{
	/// <summary>
	/// Interface for the Global Storage --- only exists to enable mocking for automated testing purposes so it'll acknowledge implementation details of the
	/// <see cref="BeamHintGlobalStorage"/> which is our implementation of this interface.
	/// <para/>
	/// Internally, we have one <see cref="IBeamHintStorage"/> for each User's hints and another one for Beamable's hints.
	/// As the number of generated hints a domain can produce grows, we split these macro-storages into one for each domain generated with
	/// <see cref="BeamHintDomains.GenerateSubDomain"/> and/or <see cref="BeamHintDomains.GenerateBeamableDomain"/>.
	/// <para/>
	/// This approach allows us to ensure we can move our internal data around to avoid slow-editor performance.
	/// </summary>
	public interface IBeamHintGlobalStorage : IBeamHintStorage
	{
		/// <summary>
		/// The combined hints of all internal <see cref="IBeamHintStorage"/>s.
		/// </summary>
		IEnumerable<BeamHint> All
		{
			get;
		}

		/// <summary>
		/// User-defined domains go into this storage (see <see cref="BeamHintDomains.IsUserDomain"/>).
		/// Our Beamable Assistant UI continuously detects hints added to this storage automatically and displays it in a special section for User domains.
		/// </summary>
		IBeamHintStorage UserDefinedStorage
		{
			get;
		}

		/// <summary>
		/// Beamable-defined hints are stored here.
		/// </summary>
		IBeamHintStorage BeamableStorage
		{
			get;
		}

		/// <summary>
		/// More performant version of <see cref="IBeamHintStorage.AddOrReplaceHints(System.Collections.Generic.IEnumerable{Common.Runtime.BeamHints.BeamHintHeader},System.Collections.Generic.IEnumerable{object})"/>
		/// for the global case. Call this if you know that all hints are either <see cref="BeamHintDomains.IsBeamableDomain"/> or <see cref="BeamHintDomains.IsUserDomain"/>.
		/// </summary>
		/// <param name="domainOwner">Either <see cref="BeamHintDomains.BEAM_DOMAIN_PREFIX"/> or <see cref="BeamHintDomains.USER_DOMAIN_PREFIX"/>.</param>
		/// <param name="headers">List of headers to add. Parallel to <paramref name="hintContextObj"/>.</param>
		/// <param name="hintContextObj">List of context objects to add. Parallel to <paramref name="headers"/>.</param>
		void BatchAddBeamHints(string domainOwner, IEnumerable<BeamHintHeader> headers, IEnumerable<object> hintContextObj);

		/// <summary>
		/// More performant version of <see cref="IBeamHintStorage.AddOrReplaceHints(System.Collections.Generic.IEnumerable{Common.Runtime.BeamHints.BeamHint})"/>
		/// for the global case. Call this if you know that all hints are either <see cref="BeamHintDomains.IsBeamableDomain"/> or <see cref="BeamHintDomains.IsUserDomain"/>.
		/// </summary>
		/// <param name="domainOwner">Either <see cref="BeamHintDomains.BEAM_DOMAIN_PREFIX"/> or <see cref="BeamHintDomains.USER_DOMAIN_PREFIX"/>.</param>
		/// <param name="hints">The <see cref="BeamHint"/>s to add.</param>
		void BatchAddBeamHints(string domainOwner, IEnumerable<BeamHint> hints);


		#region Per-Domain Beamable Storages

		/// <summary>
		/// Contains the <see cref="BeamHint"/> for the entire <see cref="BeamHintDomains.BEAM_REFLECTION_CACHE"/> domain.
		/// </summary>
		IEnumerable<BeamHint> ReflectionCacheHints
		{
			get;
		}

		/// <summary>
		/// Contains the <see cref="BeamHint"/>s for the entire <see cref="BeamHintDomains.BEAM_CSHARP_MICROSERVICES"/> domain.
		/// </summary>
		IEnumerable<BeamHint> CSharpMSHints
		{
			get;
		}

		/// <summary>
		/// Contains the <see cref="BeamHint"/>s for the entire <see cref="BeamHintDomains.BEAM_CONTENT"/> domain.
		/// </summary>
		IEnumerable<BeamHint> ContentHints
		{
			get;
		}

		/// <summary>
		/// Contains the <see cref="BeamHint"/>s for the entire <see cref="BeamHintDomains.BEAM_ASSISTANT"/> domain.
		/// </summary>
		IEnumerable<BeamHint> AssistantHints
		{
			get;
		}


		#endregion
	}

	public class BeamHintGlobalStorage : IBeamHintGlobalStorage
	{
		public IBeamHintStorage UserDefinedStorage
		{
			get;
		}

		public IBeamHintStorage BeamableStorage
		{
			get;
		}



		public IEnumerable<BeamHint> All => BeamableStorage.Union(UserDefinedStorage);

		#region Per-Domain Beamable Storages

		public IEnumerable<BeamHint> ReflectionCacheHints => BeamableStorage.Where(hint => BeamHintDomains.IsReflectionCacheDomain(hint.Header.Domain));
		public IEnumerable<BeamHint> CSharpMSHints => BeamableStorage.Where(hint => BeamHintDomains.IsCSharpMSDomain(hint.Header.Domain));
		public IEnumerable<BeamHint> ContentHints => BeamableStorage.Where(hint => BeamHintDomains.IsContentDomain(hint.Header.Domain));
		public IEnumerable<BeamHint> AssistantHints => BeamableStorage.Where(hint => BeamHintDomains.IsAssistantDomain(hint.Header.Domain));

		#endregion

		public BeamHintGlobalStorage()
		{
			UserDefinedStorage = new BeamHintStorage();
			BeamableStorage = new BeamHintStorage();
		}

		#region IEnumerable Implementation

		public IEnumerator<BeamHint> GetEnumerator()
		{
			return All.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		public void AddOrReplaceHint(BeamHintType type, string hintDomain, string uniqueId, object hintContextObj = null)
		{
			AddOrReplaceHint(new BeamHintHeader(type, hintDomain, uniqueId), hintContextObj);
		}

		public void AddOrReplaceHint(BeamHintHeader header, object hintContextObj = null)
		{
			if (BeamHintDomains.IsBeamableDomain(header.Domain))
				BeamableStorage.AddOrReplaceHint(header, hintContextObj);

			if (BeamHintDomains.IsUserDomain(header.Domain))
				UserDefinedStorage.AddOrReplaceHint(header, hintContextObj);
		}

		public void AddOrReplaceHints(IEnumerable<BeamHintHeader> headers, IEnumerable<object> hintContextObjs)
		{
			AddOrReplaceHints(headers.Zip(hintContextObjs, (header, o) => new BeamHint(header, o)));
		}

		public void AddOrReplaceHints(IEnumerable<BeamHint> bakedHints)
		{
			foreach (BeamHint hint in bakedHints)
			{
				var header = hint.Header;
				var hintContextObj = hint.ContextObject;

				if (BeamHintDomains.IsBeamableDomain(header.Domain))
					BeamableStorage.AddOrReplaceHint(header, hintContextObj);

				if (BeamHintDomains.IsUserDomain(header.Domain))
					UserDefinedStorage.AddOrReplaceHint(header, hintContextObj);
			}
		}

		public void RemoveHint(BeamHintHeader header)
		{
			if (BeamHintDomains.IsBeamableDomain(header.Domain))
				BeamableStorage.RemoveHint(header);

			if (BeamHintDomains.IsUserDomain(header.Domain))
				UserDefinedStorage.RemoveHint(header);
		}

		public void RemoveHint(BeamHint hint)
		{
			RemoveHint(hint.Header);
		}

		public void RemoveHints(IEnumerable<BeamHintHeader> headers)
		{
			foreach (BeamHintHeader header in headers)
			{
				if (BeamHintDomains.IsBeamableDomain(header.Domain))
					BeamableStorage.RemoveHint(header);

				if (BeamHintDomains.IsUserDomain(header.Domain))
					UserDefinedStorage.RemoveHint(header);
			}
		}

		public void RemoveHints(IEnumerable<BeamHint> hints)
		{
			RemoveHints(hints.Select(h => h.Header));
		}

		public int RemoveAllHints(IEnumerable<string> hintDomains, IEnumerable<string> hintIds)
		{
			var hintDomainRegexStr = string.Join("|", hintDomains);
			var hintIdRegexStr = string.Join("|", hintIds);

			return RemoveAllHints(hintDomainRegexStr, hintIdRegexStr);
		}

		public int RemoveAllHints(BeamHintType type)
		{
			var removedFromBeamableCount = BeamableStorage.RemoveAllHints(type);
			var removedFromUserCount = UserDefinedStorage.RemoveAllHints(type);
			return removedFromBeamableCount + removedFromUserCount;
		}

		public int RemoveAllHints(string hintDomainRegex = ".*", string idRegex = ".*")
		{
			var removedFromBeamableCount = BeamableStorage.RemoveAllHints(hintDomainRegex, idRegex);
			var removedFromUserCount = UserDefinedStorage.RemoveAllHints(hintDomainRegex, idRegex);
			return removedFromBeamableCount + removedFromUserCount;
		}

		public BeamHint GetHint(BeamHintHeader header)
		{
			if (BeamHintDomains.IsBeamableDomain(header.Domain))
				return BeamableStorage.GetHint(header);

			if (BeamHintDomains.IsUserDomain(header.Domain))
				return UserDefinedStorage.GetHint(header);

			return default;
		}

		public void BatchAddBeamHints(string domainOwner, IEnumerable<BeamHintHeader> headers, IEnumerable<object> hintContextObj)
		{
			BatchAddBeamHints(domainOwner, headers.Zip(hintContextObj, (header, o) => new BeamHint(header, o)));
		}

		public void BatchAddBeamHints(string domainOwner, IEnumerable<BeamHint> hints)
		{
			var beamHints = hints.ToList();

			if (BeamHintDomains.IsBeamableDomain(domainOwner))
			{
				foreach (BeamHint beamHint in beamHints)
				{
					BeamableStorage.AddOrReplaceHint(beamHint.Header, beamHint.ContextObject);
				}
			}

			if (BeamHintDomains.IsUserDomain(domainOwner))
			{
				foreach (BeamHint beamHint in beamHints)
				{
					UserDefinedStorage.AddOrReplaceHint(beamHint.Header, beamHint.ContextObject);
				}
			}
		}

	}
}
