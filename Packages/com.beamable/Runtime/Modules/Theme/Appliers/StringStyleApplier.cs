using Beamable.Theme.Palettes;
using TMPro;

namespace Beamable.Theme.Appliers
{
	[System.Serializable]
	public class StringStyleApplier : StyleApplier<TextMeshProUGUI>
	{
		public StringBinding Binding;
		public override void Apply(ThemeObject theme, TextMeshProUGUI component)
		{
			if (!Binding.Exists())
			{
				return;
			}

			component.text = Binding.Localize();
		}
	}
}
