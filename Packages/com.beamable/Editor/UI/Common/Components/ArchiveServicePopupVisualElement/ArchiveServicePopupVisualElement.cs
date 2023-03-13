using System;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using static Beamable.Common.Constants;
using static Beamable.Common.Constants.Features.Archive;

namespace Beamable.Editor.UI.Components
{
	public class ArchiveServicePopupVisualElement : BeamableVisualElement
	{
		private Label _contentLabelTop;
		private Label _contentLabelBottom;
		private PrimaryButtonVisualElement _okButton;
		private GenericButtonVisualElement _cancelButton;
		private LabeledCheckboxVisualElement _checkbox;

		public Action<bool> onConfirm;
		public Action onClose;

		public bool ShowDeleteOption { get; set; }

		public ArchiveServicePopupVisualElement() : base(
			$"{Directories.COMMON_COMPONENTS_PATH}/{nameof(ArchiveServicePopupVisualElement)}/{nameof(ArchiveServicePopupVisualElement)}")
		{
		}

		public override void Refresh()
		{
			base.Refresh();

			_contentLabelTop = Root.Q("content").Q<Label>();
			_contentLabelTop.text = ARCHIVE_WINDOW_TEXT;

			_okButton = Root.Q<PrimaryButtonVisualElement>("okButton");
			_okButton.Button.clickable.clicked += HandleOkButtonClicked;

			_checkbox = Root.Q<LabeledCheckboxVisualElement>("checkbox");
			_checkbox.Refresh();
			_checkbox.SetText(DELETE_ALL_FILES_TEXT);
			_checkbox.Q<Label>().RegisterCallback<MouseDownEvent>(evt => _checkbox.SetWithoutNotify(!_checkbox.Value));

			if (!ShowDeleteOption)
			{
				_checkbox.RemoveFromHierarchy();
			}

			_cancelButton = Root.Q<GenericButtonVisualElement>("cancelButton");
			_cancelButton.OnClick += HandleCancelButtonClicked;

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
			onConfirm?.Invoke(_checkbox.Value);
			onClose?.Invoke();
		}

		private void HandleCancelButtonClicked()
		{
			onClose?.Invoke();
		}
	}
}
