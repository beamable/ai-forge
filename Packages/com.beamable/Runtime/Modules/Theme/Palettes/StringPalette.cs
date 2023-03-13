namespace Beamable.Theme.Palettes
{
	[System.Serializable]
	public class StringStyle : PaletteStyle
	{
		public string DefaultContent;
	}

	[System.Serializable]
	public class StringPalette : Palette<StringStyle>
	{
		public override StringStyle DefaultValue()
		{
			return new StringStyle
			{
				DefaultContent = ""
			};
		}
	}

	[System.Serializable]
	public class StringBinding : StringPalette.PaletteBinding
	{

	}

	public static class StringBindingExtensions
	{
		public static string Localize(this StringBinding binding)
		{
			var style = binding.Resolve();
			// TODO: add whatever localization hooks here.
			return style.DefaultContent;
		}
	}

}
