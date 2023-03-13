using Beamable.UI.Scripts;
using TMPro;
using UnityEngine;

namespace Beamable.AccountManagement
{
	public class LoadingPopup : MenuBase
	{
		public Transform _spinnerTfm;
		public TextMeshProUGUI _messageTxt;

		//   public static void Show(string message)
		//   {
		//      LoadingPopup.Open();
		//      Instance._messageTxt.text = message;
		//   }
		//
		//   public static void Hide()
		//   {
		//      LoadingPopup.Close();
		//   }

		public string Message
		{
			get => _messageTxt.text;
			set => _messageTxt.text = value;
		}

		void Update()
		{
			_spinnerTfm.Rotate(0, 0, 75 * Time.deltaTime);
		}
	}
}
