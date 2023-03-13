using Beamable.Api;
using Beamable.Common;
using Beamable.Common.Api.Leaderboards;
using Beamable.UI.Scripts;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Beamable.Leaderboards
{
	public class LeaderboardMainMenu : MenuBase
	{
		public LeaderboardItem LeaderboardItemPrefab;
		public LeaderboardBehavior LeaderboardBehavior;
		public Transform LeaderboardEntries;

		private const int maxRetries = 5;

		protected async void OnEnable()
		{
			var de = await API.Instance;

			// There is a timing issue the first time a leaderboard is accessed for a particular realm.
			// This is a small bandage to fix that issue.
			var retry = 0;
			while (retry < maxRetries)
			{
				try
				{
					await FetchBoard(de);
					return;
				}
				catch (PlatformRequesterException e)
				{
					if (e.Status == 404)
					{
						await Task.Delay(500);
						retry++;
						if (retry == maxRetries)
						{
							Debug.LogError(e.Message);
						}
					}
					else
					{
						throw;
					}
				}
			}

			Debug.LogError("Unable to load. Please check LeaderboardFlow GameObject has Leaderboard field set.");
		}

		private Promise<LeaderBoardView> FetchBoard(IBeamableAPI de)
		{
			return de.LeaderboardService.GetBoard(LeaderboardBehavior.Leaderboard.Id, 0, 50)
			   .Error(err =>
			   {
				   // Explicitly do nothing to prevent an error from being logged.
			   })
			   .Then(HandleBoard);
		}

		private void HandleBoard(LeaderBoardView board)
		{
			// Clear all data
			for (var i = 0; i < LeaderboardEntries.childCount; i++)
			{
				Destroy(LeaderboardEntries.GetChild(i).gameObject);
			}

			// Populate lines
			foreach (var rank in board.rankings)
			{
				var leaderboardItem = Instantiate(LeaderboardItemPrefab, LeaderboardEntries);
				leaderboardItem.Apply(rank);
			}
		}
	}
}
