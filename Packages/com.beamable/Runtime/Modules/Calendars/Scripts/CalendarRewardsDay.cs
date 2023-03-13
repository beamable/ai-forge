
using Beamable.Experimental.Common.Api.Calendars;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Experimental.Calendars
{
	public class CalendarRewardsDay : MonoBehaviour
	{
		public Image RewardImage;
		public TextMeshProUGUI Name;
		private Image Background;

		public void Awake()
		{
			Background = gameObject.GetComponent<Image>();
		}

		public void setRewardForDay(RewardCalendarDay day, ClaimStatus claimStatus)
		{
			// TODO: At some point this whole thing should be replaced with something much better
		}
	}
}

public enum ClaimStatus
{
	CLAIMED,
	CLAIMABLE,
	TOBECLAIMED
}
