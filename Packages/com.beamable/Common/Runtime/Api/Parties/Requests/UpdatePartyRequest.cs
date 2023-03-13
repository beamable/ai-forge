using Beamable.Common.Content;
using Beamable.Serialization;
using System;

namespace Beamable.Experimental.Api.Parties
{
	[Serializable]
	public class UpdatePartyRequest : JsonSerializable.ISerializable
	{
		/// <summary>
		/// Stringified version of the <see cref="PartyRestriction"/>
		/// </summary>
		public string restriction;

		/// <summary>
		/// Maximum allowed number of players in the party.
		/// </summary>
		public int maxSize;

		public UpdatePartyRequest(string restriction, int maxSize = 0)
		{
			this.restriction = restriction;
			this.maxSize = maxSize;
		}

		public void Serialize(JsonSerializable.IStreamSerializer s)
		{
			s.Serialize("restriction", ref restriction);
			if (maxSize > 0)
			{
				s.Serialize("maxSize", ref maxSize);
			}
		}
	}
}
