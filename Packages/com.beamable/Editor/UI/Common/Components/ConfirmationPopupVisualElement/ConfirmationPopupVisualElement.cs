using System;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using static Beamable.Common.Constants;

namespace Beamable.Editor.UI.Components
{
	public class ConfirmationPopupVisualElement : BeamableVisualElement
	{
		private Label _bodyLabel;
		private PrimaryButtonVisualElement _okButton;
		private GenericButtonVisualElement _cancelButton;

		private readonly string _contentText;
		private readonly Action _onConfirm;
		private readonly Action _onClose;
		private readonly bool _showCancelButton;

		public ConfirmationPopupVisualElement(string contentText, Action onConfirm, Action onClose, bool showCancelButton = true) : base(
			$"{Directories.COMMON_COMPONENTS_PATH}/{nameof(ConfirmationPopupVisualElement)}/{nameof(ConfirmationPopupVisualElement)}")
		{
			_contentText = contentText;
			_onConfirm = onConfirm;
			_onClose = onClose;
			_showCancelButton = showCancelButton;
		}

		public override void Refresh()
		{
			base.Refresh();

			_bodyLabel = Root.Q<Label>("contentLabel");
			_bodyLabel.text = _contentText;

			_okButton = Root.Q<PrimaryButtonVisualElement>("okButton");
			_okButton.Button.clickable.clicked += HandleOkButtonClicked;

			_cancelButton = Root.Q<GenericButtonVisualElement>("cancelButton");

			if (_showCancelButton)
			{
				_cancelButton.OnClick += HandleCancelButtonClicked;
			}
			else
			{
				_cancelButton.RemoveFromHierarchy();
			}
		}

		public void SetCancelButtonText(string text)
		{
			_cancelButton.SetText(text);
		}

		public void SetConfirmButtonText(string text)
		{
			_okButton.SetText(text);
		}

		private void HandleOkButtonClicked()
		{
			_onConfirm?.Invoke();
			_onClose?.Invoke();
		}

		private void HandleCancelButtonClicked()
		{
			_onClose?.Invoke();
		}
	}
}
