using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Theme.Objects
{
	[System.Serializable]
	public class TextStyleObject : StyleObject<TextMeshProUGUI>
	{
		public Color Color = Color.white;
		public TMP_FontAsset FontAsset;
		public float FontSize = 24f;
		public FontStyles FontStyle;
		public TextAlignmentOptions Alignment;

		protected override void Apply(TextMeshProUGUI target)
		{
			target.font = FontAsset;
			target.fontSize = FontSize;
			target.fontStyle = FontStyle;
			target.color = Color;
			target.alignment = Alignment;

			target.SetAllDirty();

			var parentLayout = target.GetComponentInParent<LayoutGroup>();

			if (parentLayout != null)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(parentLayout.GetComponent<RectTransform>());
			}
		}
	}
}
