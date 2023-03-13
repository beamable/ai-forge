using TMPro;

namespace Beamable.UI.Scripts
{
	public class InputReference : TextReferenceBase
	{
		public TMP_InputField Field;
		private string _originalText;

		public void OnApplicationFocus(bool hasFocus)
		{
			if (hasFocus)
			{
				Field.DeactivateInputField();
			}
		}

		public override string Value
		{
			get => Field.text.Replace("\u200B", "");
			set => Field.text = value;
		}
	}
}
