using UnityEngine.UI;

namespace Beamable.Theme.Palettes
{

	[System.Serializable]
	public class SelectableStyle : PaletteStyle
	{
		public ButtonStyleData SelectionData;
	}


	[System.Serializable]
	public class SelectablePalette : Palette<SelectableStyle>
	{
		public override SelectableStyle DefaultValue()
		{
			return new SelectableStyle
			{
				Name = "default",
				Enabled = true,
				SelectionData = new ButtonStyleData
				{
					Transition = Selectable.Transition.None,
					Colors = new StyledColorBlock(),
					AnimationTriggers = new AnimationTriggers(),
					SpriteState = new SpriteState()
				}
			};
		}
	}

	[System.Serializable]
	public class SelectableBinding : SelectablePalette.PaletteBinding { }

}
