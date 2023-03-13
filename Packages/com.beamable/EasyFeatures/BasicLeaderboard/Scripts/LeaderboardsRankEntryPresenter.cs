using Beamable.AccountManagement;
using Beamable.Avatars;
using Beamable.Common.Api.Leaderboards;
using Beamable.Stats;
using Beamable.UI.Buss;
using Beamable.UI.Scripts;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Beamable.Common.Constants.Features.Leaderboards;

namespace Beamable.EasyFeatures.BasicLeaderboard
{
	public class LeaderboardsRankEntryPresenter : MonoBehaviour
	{
		public TextMeshProUGUI Rank;
		public TextMeshProUGUI Name;
		public TextMeshProUGUI Score;
		public Image Avatar;

		public SdfImageBussElement MainBussElement;
		public SdfImageBussElement RankBussElement;

		public GameObjectToggler LoadingIndicator;

		private string _entryName;
		private Sprite _entryAvatar;
		private long _entryRank;
		private double _entryScore;

		private long _currentPlayerRank;

		public void Enrich(string name, Sprite avatar, long rank, double score, long currentPlayerRank)
		{
			_entryName = name;
			_entryAvatar = avatar;
			_entryRank = rank;
			_entryScore = score;


			_currentPlayerRank = currentPlayerRank;
		}

		public void RebuildRankEntry()
		{
			Rank.text = _entryRank.ToString();
			Name.text = _entryName;
			Score.text = _entryScore.ToString(CultureInfo.InvariantCulture);
			Avatar.sprite = _entryAvatar;

			if (_currentPlayerRank == _entryRank)
			{
				MainBussElement.AddClass(BUSS_CLASS_CURRENT_PLAYER);
			}

			switch (_entryRank)
			{
				case 1:
					RankBussElement.AddClass(BUSS_CLASS_FIRST_PLACE);
					break;
				case 2:
					RankBussElement.AddClass(BUSS_CLASS_SECOND_PLACE);
					break;
				case 3:
					RankBussElement.AddClass(BUSS_CLASS_THIRD_PLACE);
					break;
			}

			LoadingIndicator.Toggle(false);
		}

		private Sprite GetAvatar(string id)
		{
			List<AccountAvatar> accountAvatars = AvatarConfiguration.Instance.Avatars;
			AccountAvatar accountAvatar = accountAvatars.Find(avatar => avatar.Name == id);
			return accountAvatar.Sprite;
		}

		public class PoolData : PoolableScrollView.IItem
		{
			public string Name;
			public Sprite Avatar;
			public double Score;
			public long Rank;

			public float Height { get; set; }
		}
	}
}
