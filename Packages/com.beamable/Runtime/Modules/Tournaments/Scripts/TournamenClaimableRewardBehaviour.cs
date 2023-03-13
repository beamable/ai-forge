using Beamable.Common;
using Beamable.UI.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Tournaments
{
	public class TournamenClaimableRewardBehaviour : MonoBehaviour
	{
		private const float PopTime = .3f;
		public TextReference AmountText;
		public Image IconImage;

		public Promise<Unit> Grow()
		{
			return this.RunAnimation(
				i => { transform.localScale = Vector3.one * i; },
				PopTime,
				15, BeamableAnimationUtil.EaseInCubic);
		}

		public Promise<Unit> Delay(float t)
		{
			return this.RunAnimation(_ => { }, t);
		}

		public void Set(int amount, Sprite sprite, int index)
		{
			transform.localScale = Vector3.zero;
			Delay(index * PopTime).Then(_ => Grow());

			AmountText.Value = "" + TournamentScoreUtil.GetShortScore((ulong)amount);
			IconImage.sprite = sprite;
		}
	}

}
