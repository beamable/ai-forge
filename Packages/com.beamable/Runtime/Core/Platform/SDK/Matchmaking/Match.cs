using Beamable.Common.Content;
using System;
using System.Collections.Generic;

namespace Beamable.Experimental.Api.Matchmaking
{
	/// <summary>
	/// This type defines the %Match for the %MatchmakingService.
	/// 
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	/// 
	/// #### Related Links
	/// - See Beamable.Experimental.Api.Matchmaking.MatchmakingService script reference
	/// 
	/// ![img beamable-logo]
	/// 
	/// </summary>
	[Serializable]
	public class Match
	{
		public string matchId;
		public string status;
		public string created;
		public SimGameType matchType;
		public List<Team> teams;
		public bool IsRunning => status == "Running";
	}

	/// <summary>
	/// This type defines the %Team for the %MatchmakingService.
	/// 
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	/// 
	/// #### Related Links
	/// - See Beamable.Experimental.Api.Sim.SimClient script reference
	/// 
	/// ![img beamable-logo]
	/// 
	/// </summary>
	[Serializable]
	public class Team
	{
		public string name;
		public List<string> players;
	}
}
