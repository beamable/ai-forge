using Beamable.AccountManagement;
using Beamable.Avatars;
using Beamable.Common.Api.Leaderboards;
using Beamable.Modules.Generics;
using Beamable.Stats;
using Beamable.UI.Buss;
using Beamable.UI.Scripts;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Beamable.Common.Constants.Features.Leaderboards;

namespace Beamable.Modules.Leaderboards
{
	public class LeaderboardsRankEntryPresenter : DataPresenter<RankEntry>
	{
#pragma warning disable CS0649
		[SerializeField] private TextMeshProUGUI _rank;
		[SerializeField] private TextMeshProUGUI _name;
		[SerializeField] private TextMeshProUGUI _score;
		[SerializeField] private Image _avatar;

		[SerializeField] private SdfImageBussElement _mainBussElement;
		[SerializeField] private SdfImageBussElement _rankBussElement;
#pragma warning restore CS0649

		private long _currentPlayerRank;
		private StatObject _aliasStatObject;
		private StatObject _avatarStatObject;

		public override void Setup(RankEntry data, params object[] additionalParams)
		{
			_currentPlayerRank = (long)additionalParams[0];
			_aliasStatObject = AccountManagementConfiguration.Instance.DisplayNameStat;
			_avatarStatObject = AccountManagementConfiguration.Instance.AvatarStat;
			base.Setup(data, additionalParams);
		}

		protected override void Refresh()
		{
			_rank.text = Data.rank.ToString();
			_name.text = Data.GetStat(_aliasStatObject.StatKey) ?? _aliasStatObject.DefaultValue;
			_score.text = Data.score.ToString(CultureInfo.InvariantCulture);

			string spriteId = Data.GetStat(_avatarStatObject.StatKey);

			_avatar.sprite = !string.IsNullOrWhiteSpace(spriteId)
				? GetAvatar(spriteId)
				: AvatarConfiguration.Instance.Default.Sprite;

			if (_currentPlayerRank == Data.rank)
			{
				_mainBussElement?.AddClass(BUSS_CLASS_CURRENT_PLAYER);
			}

			switch (Data.rank)
			{
				case 1:
					_rankBussElement?.AddClass(BUSS_CLASS_FIRST_PLACE);
					break;
				case 2:
					_rankBussElement?.AddClass(BUSS_CLASS_SECOND_PLACE);
					break;
				case 3:
					_rankBussElement?.AddClass(BUSS_CLASS_THIRD_PLACE);
					break;
			}
		}

		private Sprite GetAvatar(string id)
		{
			List<AccountAvatar> accountAvatars = AvatarConfiguration.Instance.Avatars;
			AccountAvatar accountAvatar = accountAvatars.Find(avatar => avatar.Name == id);
			return accountAvatar.Sprite;
		}

		public class PoolData : PoolableScrollView.IItem
		{
			public RankEntry RankEntry;

			public float Height
			{
				get;
				set;
			}
		}
	}
}
