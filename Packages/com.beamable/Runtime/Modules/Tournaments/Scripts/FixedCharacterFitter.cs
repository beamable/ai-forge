using TMPro;
using UnityEngine;

namespace Beamable.Tournaments
{
	public class FixedCharacterFitter : MonoBehaviour
	{
		public TextMeshProUGUI TextElement;
		public RectTransform Target;

		[Tooltip("The interval at which the fitter will snap the width to. Always snaps up, never down.")]
		public float SnapCellSize = 15f;
		public float Padding = 0;
		public float WidthPerCharacter = 15;
		public float FontCoef = .9f;
		public float BaseFontSize = 23;
		public float MinWidth = 0;
		public float MaxWidth = 100;
		public float FontSizeSnapInterval = .1f;

		private float _startSize;

		void OnEnable()
		{
			_startSize = TextElement?.fontSize ?? 12;
			Refresh();
		}

		public void Refresh()
		{
			if (!TextElement || !Target) return;

			TextElement.ForceMeshUpdate();
			var charCount = TextElement.text.Length;

			var currentWidth = charCount * WidthPerCharacter;
			var snappedUp = Padding + Mathf.CeilToInt((currentWidth + SnapCellSize * .5f) / SnapCellSize) * SnapCellSize;
			var clamped = Mathf.Clamp(snappedUp, MinWidth, MaxWidth);

			var fontSize = BaseFontSize + charCount * FontCoef;
			var fontSnap = Mathf.RoundToInt(fontSize / FontSizeSnapInterval) * FontSizeSnapInterval;
			TextElement.fontSize = fontSnap;

			Target.sizeDelta = new Vector2(clamped, Target.sizeDelta.y);
			TextElement.ForceMeshUpdate();

		}
	}
}
