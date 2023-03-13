using Beamable.CronExpression;
using Beamable.Editor.UI.Components;
using System;
using UnityEditor;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using static Beamable.Common.Constants.Features.Schedules;

namespace Beamable.Editor.Content
{
	public class CronEditorVisualElement : BeamableVisualElement
	{
		private string _cronRawFormat;
		private Action<string> _result;

		private Label _humanFormatPreview;
		private TextField _rawFormatInput;
		private PrimaryButtonVisualElement _confirmButton;

		public CronEditorVisualElement(string cronRawFormat, Action<string> result) :
			base($"{SCHEDULES_PATH}/CronEditor/{nameof(CronEditorVisualElement)}/{nameof(CronEditorVisualElement)}")
		{
			_cronRawFormat = cronRawFormat;
			_result = result;
		}

		public override void Refresh()
		{
			base.Refresh();
			Root.parent.parent.style.flexGrow = 1;

			_confirmButton = Root.Q<PrimaryButtonVisualElement>("confirmButton");
			_confirmButton.Button.clickable.clicked += HandleConfirmButton;

			Root.Q<GenericButtonVisualElement>("cancelButton").OnClick += HandleCloseButton;

			_humanFormatPreview = Root.Q<Label>("humanFormatPreview");
			_humanFormatPreview.AddTextWrapStyle();

			_rawFormatInput = Root.Q<TextField>("rawFormatInput");
			_rawFormatInput.RegisterCallback<ChangeEvent<string>>(HandleValueChange);
			_rawFormatInput.value = _cronRawFormat;
			_rawFormatInput.AddTextWrapStyle();
		}

		private void HandleValueChange(ChangeEvent<string> evt)
		{
			_humanFormatPreview.text = ExpressionDescriptor.GetDescription(evt.newValue, out var errorData);
			_humanFormatPreview.EnableInClassList("validationFailed", errorData.IsError);

			if (errorData.IsError)
			{
				_confirmButton.tooltip = "Invalid CRON string";
				_confirmButton.Disable();
			}
			else
			{
				_confirmButton.tooltip = string.Empty;
				_confirmButton.Enable();
			}
		}

		private void HandleConfirmButton()
		{
			_result?.Invoke(_rawFormatInput.value);
			CronEditorWindow.CloseWindow();
		}

		private void HandleCloseButton()
		{
			if (_cronRawFormat == _rawFormatInput.value)
			{
				CronEditorWindow.CloseWindow();
				return;
			}

			var closeWindow = EditorUtility.DisplayDialog(
			"Confirmation",
			"Are you sure you want to discard the changes and close the window?",
			"Discard",
			"Cancel");

			if (closeWindow)
				CronEditorWindow.CloseWindow();
		}
	}
}
