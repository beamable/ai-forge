using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Beamable.UI.Buttons
{

	[ExecuteInEditMode]
	public class DeButtonBehaviour : MonoBehaviour
	{
		public UnityEvent OnClick;
		public string Text;

		public TextMeshProUGUI ButtonLabel;

		private string _oldText = "";

		// Update is called once per frame
		void Update()
		{
			if (!_oldText.Equals(Text))
			{
				ButtonLabel.text = Text;
				_oldText = Text;
			}
		}

		public void Trigger()
		{
			OnClick?.Invoke();
		}
	}
}
