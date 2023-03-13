using Beamable.Common.Content;
using Beamable.Common.Player;
using System;
using System.Collections.Generic;

namespace Beamable.Experimental.Api.Parties
{
	[Serializable]
	public class Party : DefaultObservable
	{
		/// <summary>
		/// The id of the lobby. Use this id when making requests for a particular lobby via <see cref="IPartyApi"/>
		/// </summary>
		public string id;

		/// <summary>
		/// String version of the `Access` property.
		/// </summary>
		public string restriction;

		/// <summary>
		/// PlayerId of a party leader.
		/// </summary>
		public string leader;

		/// <summary>
		/// List of ids of players who are currently active in the party.
		/// </summary>
		public List<string> members;

		/// <summary>
		/// Either "Private" of "Public" representing who can join the <see cref="Party"/>
		/// </summary>
		public PartyRestriction Restriction => (PartyRestriction)Enum.Parse(typeof(PartyRestriction), restriction);

		/// <summary>
		/// Maximum allowed number of players in the party.
		/// </summary>
		public int maxSize;

		/// <summary>
		/// Update the state of the current party with the data from another party instance.
		/// This will trigger the observable callbacks.
		/// </summary>
		/// <param name="updatedState">The latest copy of the party</param>
		public void Set(Party updatedState)
		{
			id = updatedState?.id;
			restriction = updatedState?.restriction;
			leader = updatedState?.leader;
			members = updatedState?.members;
			maxSize = updatedState?.maxSize ?? 0;
			TriggerUpdate();
		}
	}
}
