using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Tournaments
{
	public enum TournamentColorClass
	{
		BACKGROUND,
		ENTRY,
		STAGE
	}
	public class TournamentColorConstraint : MonoBehaviour
	{
		// pick a semantic meaning for this color component
		public TournamentColorClass Color;
		public List<Image> BoundImages;

		private bool _useOverride = false;

		public float RedCoef = 1, GreenCoef = 1, BlueCoef = 1, AlphaCoef = 1;

		// Start is called before the first frame update
		void Start()
		{
			Refresh();
		}

		public void ReleaseOverride()
		{
			if (!_useOverride) return;
			_useOverride = false;
			Refresh();
		}
		public void OverrideColor(Color color)
		{
			_useOverride = true;
			foreach (var image in BoundImages)
			{
				image.color = color;
			}
		}

		public void Refresh(TournamentsBehaviour master = null)
		{
			if (_useOverride) return;

			if (master == null)
			{
				master = GetComponentInParent<TournamentsBehaviour>();
			}
			var colorData = master.GetColorForClass(Color);

			var modColor = new Color(colorData.r * RedCoef, colorData.g * GreenCoef, colorData.b * BlueCoef,
				colorData.a * AlphaCoef);
			foreach (var image in BoundImages)
			{
				image.color = modColor;
			}
		}
	}
}
