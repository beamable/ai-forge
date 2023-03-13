using Beamable.Serialization;
using System;

namespace Beamable.Experimental.Api.Parties
{
	[Serializable]
	public class CreatePartyRequest : JsonSerializable.ISerializable
	{
		/// <summary>
		/// Stringified version of the <see cref="PartyRestriction"/>
		/// </summary>
		public string restriction;

		/// <summary>
		/// Player id of a party leader.
		/// </summary>
		public string leader;

		/// <summary>
		/// Maximum allowed number of players in the party.
		/// </summary>
		public int maxSize;

		public CreatePartyRequest(string restriction, string leader, int maxSize = 0)
		{
			this.restriction = restriction;
			this.leader = leader;
			this.maxSize = maxSize;
		}

		public void Serialize(JsonSerializable.IStreamSerializer s)
		{
			s.Serialize("restriction", ref restriction);
			s.Serialize("leader", ref leader);
			if (maxSize > 0)
			{
				s.Serialize("maxSize", ref maxSize);
			}
		}
	}
}
