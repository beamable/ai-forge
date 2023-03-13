using Beamable.UI.Scripts;
using UnityEngine;

namespace Beamable.Tournaments
{
	public class TournamentRewardPanelBehaviour : MonoBehaviour
	{
		public RectTransform RewardContainer;
		public TournamenClaimableRewardBehaviour Prefab;
		public TextReference ClaimButtonText;

		public TournamentsBehaviour TournamentsBehaviour;

		private TournamentPlayerRewards _rewards;
		private const int PAGE_SIZE = 6;
		private int _currentPage = 0;

		public void HandleClick()
		{
			var totalPages = _rewards.UnclaimedRewards.Count / PAGE_SIZE;
			if (_currentPage == totalPages)
			{
				TournamentsBehaviour.ClaimAllRewards();
			}
			else
			{
				ShowPage(_currentPage + 1);
			}
		}

		void ClearChildren()
		{
			for (var i = 0; i < RewardContainer.childCount; i++)
			{
				Destroy(RewardContainer.GetChild(i).gameObject);
			}
		}


		public void ShowPage(int requestedPageNumber = 0)
		{
			ClearChildren();
			var lastPageIndex = _rewards.UnclaimedRewards.Count / PAGE_SIZE;
			var pageNumber = Mathf.Clamp(requestedPageNumber, 0, lastPageIndex);
			var startElement = pageNumber * PAGE_SIZE;
			var endElement = Mathf.Min(startElement + PAGE_SIZE, _rewards.UnclaimedRewards.Count);

			if (pageNumber == lastPageIndex)
			{
				ClaimButtonText.Value = "Claim";
			}
			else
			{
				ClaimButtonText.Value = $"Next {pageNumber + 1}/{lastPageIndex + 1}";
			}

			_rewards.ResolveAllIcons().Then(lookup =>
			{
				for (var i = startElement; i < endElement; i++)
				{
					var reward = _rewards.UnclaimedRewards[i];
					var instance = Instantiate(Prefab, RewardContainer);
					var sprite = lookup[reward.symbol.Id];
					instance.Set(reward.amount, sprite, (i - startElement));
				}
			});
			_currentPage = pageNumber;
		}

		public void Set(TournamentPlayerRewards rewards)
		{
			ClearChildren();
			_rewards = rewards;
			//ShowPage(0);
		}
	}

}
