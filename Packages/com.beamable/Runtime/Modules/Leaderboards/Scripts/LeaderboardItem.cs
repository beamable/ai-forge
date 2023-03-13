using Beamable.Common.Api.Leaderboards;
using TMPro;
using UnityEngine;

namespace Beamable.Leaderboards
{
	public class LeaderboardItem : MonoBehaviour
	{
		public TextMeshProUGUI TxtAlias;
		public TextMeshProUGUI TxtRank;
		public TextMeshProUGUI TxtScore;

		public async void Apply(RankEntry entry)
		{
			var de = await API.Instance;
			var stats = await de.StatsService.GetStats("client", "public", "player", entry.gt);
			string alias;
			stats.TryGetValue("alias", out alias);
			TxtAlias.text = alias;
			TxtRank.text = entry.rank.ToString();
			TxtScore.text = entry.score.ToString();
		}
	}
}
