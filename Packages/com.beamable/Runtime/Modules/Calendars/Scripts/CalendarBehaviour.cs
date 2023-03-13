using Beamable.Experimental.Common.Api.Calendars;
using Beamable.Experimental.Common.Calendars;
using Beamable.Tournaments;
using Beamable.UI.Scripts;
using UnityEngine;
using UnityEngine.UI;
using static Beamable.Common.Constants.URLs;

namespace Beamable.Experimental.Calendars
{
	public class CalendarBehaviour : MonoBehaviour
	{
		public MenuManagementBehaviour MenuManager;
		public CalendarRef Calendar;
		public GameObject ContentContainer;
		public CalendarRewardsDay CalendarRewardsPrefab;
		public Button ClaimButton;
		public CountdownTextBehaviour countdownText;

		private CalendarView view;

		public async void Start()
		{
			var beam = await API.Instance;
#pragma warning disable CS0618
			beam.Experimental.CalendarService.Subscribe(Calendar.Id, resp =>
#pragma warning restore CS0618
			{
				view = resp;
				updateUI();
			});
			countdownText.enabled = false;
		}

		private void updateUI()
		{
			// Clear the children
			var trans = ContentContainer.transform;
			foreach (Transform child in trans)
			{
				Destroy(child.gameObject);
			}

			for (var i = 0; i < view.days.Count; i++)
			{
				var day = view.days[i];
				var childPrefab = Instantiate(CalendarRewardsPrefab, ContentContainer.transform);

				var claimable = i == view.nextIndex && view.nextClaimSeconds <= 0;

				var claimStatus = ClaimStatus.TOBECLAIMED;
				if (i < view.nextIndex)
				{
					claimStatus = ClaimStatus.CLAIMED;
				}
				else if (claimable)
				{
					claimStatus = ClaimStatus.CLAIMABLE;
				}

				childPrefab.setRewardForDay(day, claimStatus);
			}

			if (view.nextClaimSeconds > 0)
			{
				ClaimButton.interactable = false;
				countdownText.SetSecondsLeft(view.nextClaimSeconds);
				countdownText.enabled = true;
			}
			else
			{
				ClaimButton.interactable = true;
				countdownText.enabled = false;
			}
		}

		public async void onClaim()
		{
			var beam = await API.Instance;
#pragma warning disable CS0618
			await beam.Experimental.CalendarService.Claim(Calendar.Id);
#pragma warning restore CS0618
		}
	}
}
