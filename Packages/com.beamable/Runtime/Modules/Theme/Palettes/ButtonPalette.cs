using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Theme.Palettes
{

	[System.Serializable]
	public class ButtonStyle : PaletteStyle
	{
		public ColorBinding ImageColor, TextColor;
		public ImageBinding Image;
		public SelectableBinding Selectable;
		public TextBinding Text;
		public GradientBinding Gradient;
		public SoundBinding ClickSound;
		public bool OverrideAlignment = false;
		public TextAlignmentOptions Alignment;

		public override PaletteStyle Clone()
		{
			return new ButtonStyle
			{
				Name = Name,
				Enabled = Enabled,
				ImageColor = ImageColor.Clone(),
				TextColor = TextColor.Clone(),
				Image = Image.Clone(),
				Selectable = Selectable.Clone(),
				Text = Text.Clone(),
				Gradient = Gradient.Clone(),
				ClickSound = ClickSound.Clone(),
				OverrideAlignment = OverrideAlignment,
				Alignment = Alignment
			};
		}
	}

	[System.Serializable]
	public class ButtonStyleData
	{
		public StyledColorBlock Colors = new StyledColorBlock();
		public Selectable.Transition Transition;
		public AnimationTriggers AnimationTriggers;
		public SpriteState SpriteState;
	}

	[Serializable]
	public struct StyledColorBlock
	{
		public ColorBinding NormalColor;
		public ColorBinding HighlightedColor;
		public ColorBinding PressedColor;
		public ColorBinding SelectedColor;
		public ColorBinding DisabledColor;

		[Range(1f, 5f)]
		public float ColorMultiplier;
		public float FadeDuration;
	}

	[System.Serializable]
	public class ButtonPalette : Palette<ButtonStyle>
	{
		public override ButtonStyle DefaultValue()
		{
			return new ButtonStyle
			{
				Name = "default",
				Enabled = true
			};
		}
	}

	[System.Serializable]
	public class ButtonBinding : ButtonPalette.PaletteBinding { }
}
