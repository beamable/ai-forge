using Beamable.EasyFeatures.Components;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.EasyFeatures.BasicLobby
{
	public class OverlayedLobbySettingsWindow : MonoBehaviour, IOverlayComponent
	{
		public TMP_InputField _nameField;
		public TMP_InputField _descriptionField;
		public TextMeshProUGUI _passwordLabel;
		public GameObject _passwordGroup;

		public Button CancelButton;
		public Button ConfirmButton;

		public void Show(string lobbyName, string description, Action<string, string, string> confirmAction, Action closeAction, string password)
		{
			_nameField.SetTextWithoutNotify(lobbyName);
			_descriptionField.SetTextWithoutNotify(description);

			_passwordGroup.SetActive(password != string.Empty);
			_passwordLabel.text = password;

			CancelButton.onClick.ReplaceOrAddListener(closeAction.Invoke);
			ConfirmButton.onClick.ReplaceOrAddListener(() =>
			{
				confirmAction?.Invoke(_nameField.text, _descriptionField.text, BeamContext.Default.PlayerId.ToString());
			});

			gameObject.SetActive(true);
		}

		public void Hide()
		{
			gameObject.SetActive(false);
			CancelButton.onClick.RemoveAllListeners();
			ConfirmButton.onClick.RemoveAllListeners();
		}
	}
}
