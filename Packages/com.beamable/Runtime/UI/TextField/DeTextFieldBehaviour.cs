using TMPro;
using UnityEngine;

namespace Beamable.UI.TextField
{
	[ExecuteInEditMode]
	public class DeTextFieldBehaviour : MonoBehaviour
	{
		public string LabelText = "Label";
		public string InitialText;
		public string PlaceholderText = "enter text...";
		public string Text => InputField.text;

		public TextMeshProUGUI PlaceholderField;
		public TextMeshProUGUI LabelField;
		public TMP_InputField InputField;

		private string _oldText;
		private string _oldPlaceholderText = "";
		private string _oldLabelText = "Label";

		// Start is called before the first frame update
		void Start()
		{
			if (InputField != null)
			{
				InputField.text = InitialText;
			}
		}

		// Update is called once per frame
		void Update()
		{
			if (PlaceholderField != null && !PlaceholderText.Equals(_oldPlaceholderText))
			{
				PlaceholderField.text = PlaceholderText;
				_oldPlaceholderText = PlaceholderText;
			}

			if (LabelField != null && !LabelText.Equals(_oldLabelText))
			{
				LabelField.text = LabelText;
				_oldLabelText = LabelText;
			}
		}

	}
}
