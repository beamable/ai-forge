using Beamable.Common.Leaderboards;
using Beamable.UI.Scripts;
using UnityEngine;
using static Beamable.Common.Constants.URLs;

namespace Beamable.Leaderboards
{


	[HelpURL(Documentations.URL_DOC_LEADERBOARD_FLOW)]
	public class LeaderboardBehavior : MonoBehaviour
	{

		public MenuManagementBehaviour MenuManager;
		public LeaderboardRef Leaderboard;

		public void Toggle(bool leaderboardDesiredState)
		{

			if (!leaderboardDesiredState && MenuManager.IsOpen)
			{

				MenuManager.CloseAll();
			}
			else if (leaderboardDesiredState && !MenuManager.IsOpen)
			{

				MenuManager.Show<LeaderboardMainMenu>();
			}
		}
	}
}
