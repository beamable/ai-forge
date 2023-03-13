using TMPro;

namespace Beamable.UI.Scripts
{
	public class TextReference : TextReferenceBase
	{
		public TextMeshProUGUI Text;
		public override string Value
		{
			get => Text.text.Replace("\u200B", "");
			set => Text.text = value;
		}

	}
}
