using Beamable.Content;
using Beamable.Editor.Content.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Content.Components
{
	public class StatusBarVisualElement : ContentManagerComponent
	{
		public new class UxmlFactory : UxmlFactory<StatusBarVisualElement, UxmlTraits> { }
		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			UxmlStringAttributeDescription customText = new UxmlStringAttributeDescription { name = "custom-text", defaultValue = "nada" };

			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get { yield break; }
			}
			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				var self = ve as StatusBarVisualElement;
			}
		}
		private Button _validationSwitchBtn, _modificationSwitchBtn;

		private VisualElement _statusIcon;
		private Label _statusDespLabel, _updateDateLabel, _remoteModeWarningLabel;
		private Button _statusDespBtn1, _statusDespBtn2, _statusDespBtn3, _statusDespBtn4;
		private CountVisualElement _countElement1, _countElement2, _countElement3, _countElement4;

		private string _statusClassName = "modified"; // current, modified
		private ContentCounts _counts = new ContentCounts();
		private const string CSS_HIDE_ELEMENT = "hide";

		public string Text { set; get; }

		public ContentDataModel Model { get; set; }

		public StatusBarVisualElement() : base(nameof(StatusBarVisualElement))
		{

		}

		public override void Refresh()
		{
			base.Refresh();

			_statusIcon = Root.Q<VisualElement>("status-icon");
			_statusDespLabel = Root.Q<Label>("Description");
			_updateDateLabel = Root.Q<Label>("UpdateDate");

			_statusDespBtn1 = Root.Q<Button>("description1");
			_statusDespBtn1.clickable.clicked += HandleInvalidOnClick;
			_statusDespBtn2 = Root.Q<Button>("description2");
			_statusDespBtn2.clickable.clicked += HandleCreatedOnClick;
			_statusDespBtn3 = Root.Q<Button>("description3");
			_statusDespBtn3.clickable.clicked += HandleModifiedOnClick;
			_statusDespBtn4 = Root.Q<Button>("description4");
			_statusDespBtn4.clickable.clicked += HandleDeletedOnClick;
			_countElement1 = Root.Q<CountVisualElement>("count1");
			_countElement2 = Root.Q<CountVisualElement>("count2");
			_countElement3 = Root.Q<CountVisualElement>("count3");
			_countElement4 = Root.Q<CountVisualElement>("count4");

			_remoteModeWarningLabel = Root.Q<Label>("remoteModeWarning");


			Model.OnItemEnriched += Model_OnItemEnriched;
			Model.OnContentDeleted += Model_OnItemEnriched;
			Model.OnSoftReset += GetNewCounts;
			GetNewCounts();


		}

		private void Model_OnItemEnriched(ContentItemDescriptor _)
		{
			EditorDebouncer.Debounce("content-item-enrich", GetNewCounts);
		}

		private void GetNewCounts()
		{
			_counts = Model.GetCounts();
			RefreshStatus();
		}

		public void RefreshStatus()
		{
			_statusIcon.RemoveFromClassList(_statusClassName);

			// check any difference
			var anyInvalid = _counts.Invalid > 0;
			var anyDeleted = _counts.Deleted > 0;
			var anyCreated = _counts.Created > 0;
			var anyModified = _counts.Modified > 0;
			var timestamp = Model?.LastManifestTimestamp;

			if (timestamp > 0)
			{
				var date = DateTimeOffset.FromUnixTimeMilliseconds(timestamp.Value).UtcDateTime.ToLocalTime();
				_updateDateLabel.text = $"Last publish: {date.ToString("F", CultureInfo.CurrentCulture)}";
			}
			else
			{
				_updateDateLabel.text = "Content unpublished";
			}

			var isLocalMode = ContentConfiguration.Instance.EnableLocalContentInEditor;

			_remoteModeWarningLabel.text = isLocalMode
				? "[Local Mode]"
				: "[REMOTE MODE]";
			_remoteModeWarningLabel.tooltip = isLocalMode
				? "Runtime content from the Unity Editor will use your local content"
				: "Runtime content will be fetched from your remote realm, making your local changes appear to have no effect.";
			_remoteModeWarningLabel.EnableInClassList("remote", !isLocalMode);
			_remoteModeWarningLabel.RegisterCallback<MouseDownEvent>(evt =>
			{
				SettingsService.OpenProjectSettings($"Project/Beamable/Content");
			});

			if (anyInvalid || anyCreated || anyModified || anyDeleted)
			{
				_statusDespLabel.text = "";
				if (anyInvalid)
				{
					_countElement1.RemoveFromClassList(CSS_HIDE_ELEMENT);
					_countElement1.SetValue(_counts.Invalid);
					_statusDespBtn1.RemoveFromClassList(CSS_HIDE_ELEMENT);
				}
				else
				{
					_countElement1.AddToClassList(CSS_HIDE_ELEMENT);
					_statusDespBtn1.AddToClassList(CSS_HIDE_ELEMENT);
				}

				if (anyCreated)
				{
					_countElement2.RemoveFromClassList(CSS_HIDE_ELEMENT);
					_countElement2.SetValue(_counts.Created);
					_statusDespBtn2.RemoveFromClassList(CSS_HIDE_ELEMENT);
				}
				else
				{
					_countElement2.AddToClassList(CSS_HIDE_ELEMENT);
					_statusDespBtn2.AddToClassList(CSS_HIDE_ELEMENT);
				}

				if (anyModified)
				{
					_countElement3.RemoveFromClassList(CSS_HIDE_ELEMENT);
					_countElement3.SetValue(_counts.Modified);
					_statusDespBtn3.RemoveFromClassList(CSS_HIDE_ELEMENT);
				}
				else
				{
					_countElement3.AddToClassList(CSS_HIDE_ELEMENT);
					_statusDespBtn3.AddToClassList(CSS_HIDE_ELEMENT);
				}

				if (anyDeleted)
				{
					_countElement4.RemoveFromClassList(CSS_HIDE_ELEMENT);
					_countElement4.SetValue(_counts.Deleted);
					_statusDespBtn4.RemoveFromClassList(CSS_HIDE_ELEMENT);
				}
				else
				{
					_countElement4.AddToClassList(CSS_HIDE_ELEMENT);
					_statusDespBtn4.AddToClassList(CSS_HIDE_ELEMENT);
				}
				_statusClassName = "modified";
			}
			else
			{
				_statusDespLabel.text = "All data has synced with server";
				_countElement1.AddToClassList(CSS_HIDE_ELEMENT);
				_statusDespBtn1.AddToClassList(CSS_HIDE_ELEMENT);
				_countElement2.AddToClassList(CSS_HIDE_ELEMENT);
				_statusDespBtn2.AddToClassList(CSS_HIDE_ELEMENT);
				_countElement3.AddToClassList(CSS_HIDE_ELEMENT);
				_statusDespBtn3.AddToClassList(CSS_HIDE_ELEMENT);
				_countElement4.AddToClassList(CSS_HIDE_ELEMENT);
				_statusDespBtn4.AddToClassList(CSS_HIDE_ELEMENT);
				_statusClassName = "current";
			}

			_statusIcon.AddToClassList(_statusClassName);
		}

		private void HandleInvalidOnClick()
		{
			// Show invalid
			Model.ToggleValidationFilter(ContentValidationStatus.VALID, false);
			Model.ToggleValidationFilter(ContentValidationStatus.INVALID, true);

			Model.ToggleStatusFilter(ContentModificationStatus.LOCAL_ONLY, true);
			Model.ToggleStatusFilter(ContentModificationStatus.MODIFIED, true);
			Model.ToggleStatusFilter(ContentModificationStatus.SERVER_ONLY, true);
		}

		private void HandleCreatedOnClick()
		{
			Model.ToggleValidationFilter(ContentValidationStatus.VALID, false);
			Model.ToggleValidationFilter(ContentValidationStatus.INVALID, false);
			// Show Created
			Model.ToggleStatusFilter(ContentModificationStatus.LOCAL_ONLY, true);
			Model.ToggleStatusFilter(ContentModificationStatus.MODIFIED, false);
			Model.ToggleStatusFilter(ContentModificationStatus.SERVER_ONLY, false);
		}

		private void HandleModifiedOnClick()
		{
			Model.ToggleValidationFilter(ContentValidationStatus.VALID, false);
			Model.ToggleValidationFilter(ContentValidationStatus.INVALID, false);
			// Show Modified
			Model.ToggleStatusFilter(ContentModificationStatus.LOCAL_ONLY, false);
			Model.ToggleStatusFilter(ContentModificationStatus.MODIFIED, true);
			Model.ToggleStatusFilter(ContentModificationStatus.SERVER_ONLY, false);
		}

		private void HandleDeletedOnClick()
		{
			Model.ToggleValidationFilter(ContentValidationStatus.VALID, false);
			Model.ToggleValidationFilter(ContentValidationStatus.INVALID, false);
			// Show Deleted
			Model.ToggleStatusFilter(ContentModificationStatus.LOCAL_ONLY, false);
			Model.ToggleStatusFilter(ContentModificationStatus.MODIFIED, false);
			Model.ToggleStatusFilter(ContentModificationStatus.SERVER_ONLY, true);
		}
	}
}
