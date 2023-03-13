using System;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Theme.Objects
{
	[Serializable]
	public class ImageStyleObject : StyleObject<Image>
	{
		public Color Color = Color.white;
		public Sprite SourceImage;
		public Image.Type Type;
		public Material Material;
		public bool OverrideImage = false;

		protected override void Apply(Image target)
		{
			target.color = Color;
			if (OverrideImage)
			{
				target.sprite = SourceImage;
			}
			target.type = Type;
			target.material = Material;
		}
	}
}
