using Beamable.Common.Content.Validation;
using Beamable.Editor.Content.Models;
using System;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

using static Beamable.Common.Constants;

namespace Beamable.Editor.Content.Components
{
	public class ContentVisualElement : ContentManagerComponent
	{
		public event Action<ContentItemDescriptor> OnRightMouseButtonClicked;

		// public StatusIconVisualElement _statusIconVisualElement;
		public VisualElement _statusIcon;

		private ContentItemDescriptor _contentItemDescriptor;
		public ContentItemDescriptor ContentItemDescriptor
		{
			get => _contentItemDescriptor;
			set
			{
				if (_contentItemDescriptor != null)
				{
					_contentItemDescriptor.OnEnriched -= ContentItemDescriptor_OnEnriched;
					_contentItemDescriptor.OnRenameRequested -= ContentItemDescriptor_OnRenameRequested;
				}
				_contentItemDescriptor = value;
			}
		}
		private string _statusClassName;
		private string _previousStatusClassName;

		private TextField _nameTextField;
		private Label _pathLabel;
		private TagListVisualElement _tagListVisualElement;
		private Label _lastChanged;
		private object _tagsLabel;
		private string _nameBackup;
		private bool _isContentNameInEditMode;

		public ContentVisualElement() : base(nameof(ContentVisualElement)) { }

		protected override void OnDetach()
		{
			ContentItemDescriptor = null; // trigger the cleanup.
		}

		public override void Refresh()
		{
			base.Refresh();

			RegisterCallback<MouseDownEvent>(OnMouseDownEvent,
			   TrickleDown.TrickleDown);

			_pathLabel = Root.Q<Label>("pathLabel");

			// _statusIconVisualElement = Root.Q<StatusIconVisualElement>("statusIconVisualElement");
			_statusIcon = Root.Q<VisualElement>("statusIcon");
			_nameTextField = Root.Q<TextField>("nameTextField");
			_nameTextField.SetEnabled(false);

			_lastChanged = Root.Q<Label>("lastChanged");

			// Update status icon based on states
			UpdateStatusIcon();

			_nameTextField.RegisterCallback<FocusEvent>(NameLabel_OnFocus,
			   TrickleDown.TrickleDown);
			_nameTextField.RegisterCallback<BlurEvent>(NameLabel_OnBlur,
			   TrickleDown.TrickleDown);
			_nameTextField.RegisterCallback<KeyDownEvent>(NameLabel_OnKeydown,
			   TrickleDown.TrickleDown);
			_nameTextField.RegisterCallback<KeyUpEvent>(NameLabel_OnKeyup,
			   TrickleDown.TrickleDown);

			_tagListVisualElement = Root.Q<TagListVisualElement>("tagListVisualElement");

			if (ContentItemDescriptor != null)
			{
				ContentItemDescriptor.OnEnriched += ContentItemDescriptor_OnEnriched;
				ContentItemDescriptor.OnRenameRequested += ContentItemDescriptor_OnRenameRequested;
				Update();

			}
			else
			{
				//Placeholders
				_nameTextField.value = "{Name}";
				_pathLabel.text = "{Path}";
			}
		}

		private void ContentItemDescriptor_OnEnriched(ContentItemDescriptor obj)
		{
			Update();
			UpdateLastChanged();
		}

		private void ContentItemDescriptor_OnRenameRequested()
		{
			RenameGestureBegin();
		}

		private void Update()
		{
			_nameTextField.value = ContentItemDescriptor.Name;
			_pathLabel.text = ContentItemDescriptor.ContentType.ShortName;
			_tagListVisualElement.TagDescriptors = ContentItemDescriptor.GetAllTags().ToList();
			_lastChanged.text = ContentItemDescriptor.GetFormattedLastChanged + (ContentItemDescriptor.IsCorrupted ? " (corrupted)" : string.Empty);
			// _tagListVisualElement.ContentItemDescriptor = _contentItemDescriptor;
			_tagListVisualElement.Refresh();


			if (ContentItemDescriptor.ValidationStatus == ContentValidationStatus.INVALID)
			{
				Root.AddToClassList("validationError");
			}
			else
			{
				Root.RemoveFromClassList("validationError");
			}
			UpdateStatusIcon();
		}

		private void UpdateStatusIcon()
		{
			_pathLabel.RemoveFromClassList("pathDeleted");

			_previousStatusClassName = _statusClassName;
			if (!string.IsNullOrEmpty(_statusClassName))
			{
				_statusIcon.RemoveFromClassList(_statusClassName);
			}
			switch (ContentItemDescriptor.Status)
			{
				case ContentModificationStatus.MODIFIED:
					_statusClassName = "modified";
					_statusIcon.tooltip = Tooltips.ContentManager.MODIFIED;
					break;
				case ContentModificationStatus.LOCAL_ONLY:
					_statusClassName = "localNew";
					_statusIcon.tooltip = Tooltips.ContentManager.NEW_ADD;
					break;
				case ContentModificationStatus.SERVER_ONLY:
					_statusClassName = "localDeleted";
					_pathLabel.AddToClassList("pathDeleted");
					_statusIcon.tooltip = Tooltips.ContentManager.DELETED;
					break;
				case ContentModificationStatus.NOT_MODIFIED:
					_statusClassName = "inSync";
					_statusIcon.tooltip = Tooltips.ContentManager.SYNCED;
					break;
				default:
					_statusClassName = "modified";
					_statusIcon.tooltip = Tooltips.ContentManager.MODIFIED;
					break;
			}
			_statusIcon.AddToClassList(_statusClassName);
		}

		private void UpdateLastChanged()
		{
			if (string.IsNullOrWhiteSpace(_previousStatusClassName))
				return;

			if (_statusClassName.Equals("inSync"))
			{
				_contentItemDescriptor.RefreshLatestUpdate(true);
			}
			else if (!_previousStatusClassName.Equals(_statusClassName) ||
					 _previousStatusClassName.Equals(_statusClassName) && _statusClassName.Equals("modified"))
			{
				_contentItemDescriptor.RefreshLatestUpdate();
			}
			_lastChanged.text = ContentItemDescriptor.GetFormattedLastChanged;
		}

		/// <summary>
		/// Focus the user input to allow for free typing to rename
		/// the <see cref="ContentVisualElement"/>. Activated via double-click
		/// or via right-click menu
		/// </summary>
		public void RenameGestureBegin()
		{
			if (ContentItemDescriptor == null) return;
			// can only rename if we have local data.
			if (ContentItemDescriptor.LocalStatus != HostStatus.AVAILABLE) return;

			_nameBackup = _nameTextField.value;
			_nameTextField.SetEnabled(true);
			_nameTextField.BeamableFocus();
		}


		private void NameLabel_OnFocus(FocusEvent evt)
		{
			_nameTextField.SelectAll();
		}

		private void NameLabel_OnKeydown(KeyDownEvent evt)
		{
			evt.StopPropagation();
			switch (evt.keyCode)
			{
				case KeyCode.Escape:
					CancelName();
					break;
				case KeyCode.Return:
					CommitName();
					break;
			}
		}

		private void NameLabel_OnKeyup(KeyUpEvent evt)
		{
			CheckName();
		}

		private void NameLabel_OnBlur(BlurEvent evt)
		{
			CommitName();
		}

		private void CommitName()
		{
			if (_isContentNameInEditMode)
				return;
			_isContentNameInEditMode = true;

			_nameTextField.SelectRange(0, 0);
			_nameTextField.SetEnabled(false);
			//Invokes internal event
			try
			{
				ContentItemDescriptor.Name = _nameTextField.value;
			}
			catch (Exception ex)
			{
				Debug.LogWarning($"Cannot assign name. message=[{ex.Message}]");
				CancelName();
			}
			finally
			{
				_nameTextField.Blur();
				_isContentNameInEditMode = false;
			}
		}

		private void CancelName()
		{
			_nameTextField.SetValueWithoutNotify(_nameBackup);
			_nameTextField.Blur();
		}

		public void CheckName()
		{
			var name = _nameTextField.value;
			var content = ContentItemDescriptor?.GetContent();
			if (content != null && ContentNameValidationException.HasNameValidationErrors(content, name, out var errors))
			{
				foreach (var error in errors)
				{
					var replaceWith = error.InvalidChar == ' ' ? "_" : "";
					name = name.Replace(error.InvalidChar.ToString(), replaceWith);
				}
			}

			_nameTextField.value = name;
		}

		private void OnMouseDownEvent(MouseDownEvent evt)
		{
			//Double click
			if (evt.clickCount == 2)
			{
				RenameGestureBegin();
			}

			//Right click
			if (evt.button == 1)
			{
				OnRightMouseButtonClicked?.Invoke(ContentItemDescriptor);
			}
		}
	}
}
