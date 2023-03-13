using Beamable.Editor.UI.Common;
using Beamable.Editor.UI.Common.Models;
using Beamable.Editor.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using static Beamable.Common.Constants.Features.ContentManager;

namespace Beamable.Editor.Content.Components
{
	public class ArchiveManifestsVisualElement : ContentManagerComponent
	{
		public event Action OnCancelled;
		public event Action OnCompleted;

		private ManifestModel _model = new ManifestModel();
		private VisualElement _listRoot;
		private List<Entry> _entries = new List<Entry>();
		private PrimaryButtonVisualElement _archiveBtn;
		private FormConstraint _buttonGatekeeper;

		private ArchiveManifestsVisualElement() : base(nameof(ArchiveManifestsVisualElement)) { }

		public static BeamablePopupWindow OpenAsUtilityWindow() => OpenAsUtilityWindow(null, out var _);

		public static BeamablePopupWindow OpenAsUtilityWindow(EditorWindow parent, out ArchiveManifestsVisualElement content)
		{
			content = new ArchiveManifestsVisualElement();
			var window = BeamablePopupWindow.ShowUtility(ActionNames.ARCHIVE_MANIFESTS, content, parent, WindowSizeMinimum, (callbackWindow) =>
			{
				callbackWindow?.Close();
				OpenAsUtilityWindow();
			});
			window.minSize = WindowSizeMinimum;
			content.OnCompleted += window.Close;
			content.OnCancelled += window.Close;
			return window;
		}

		public override void Refresh()
		{
			base.Refresh();
			_listRoot = Root.Q("listRoot");
			_listRoot.Clear();
			_entries.Clear();
			Root.Q<Label>("manifestWarningMessage")?.AddTextWrapStyle();
			_model.RefreshAvailableManifests().Then(manifests =>
			{
				if (manifests.manifests.Count < 2)
				{
					_listRoot.Add(new Label("No manifest namespaces to archive."));
					return;
				}
				foreach (var manifest in manifests.manifests.OrderBy(x => x.id))
				{
					if (manifest.id == DEFAULT_MANIFEST_ID) continue;
					var enabled = manifest.id != _model.Current?.DisplayName;
					_entries.Add(new Entry(manifest, _listRoot, enabled, UpdateArchiveButtonInteractivity));
				}
			});

			Root.Q<Label>("manifestWarningMessage").AddTextWrapStyle();

			_archiveBtn = Root.Q<PrimaryButtonVisualElement>("archiveBtn");
			_archiveBtn.Button.clickable.clicked += ArchiveButton_OnClicked;
			_buttonGatekeeper = FormConstraint.Logical("No namespace selected.", () => _entries.Count(e => e.IsSelected) == 0);
			_archiveBtn.AddGateKeeper(_buttonGatekeeper);
			UpdateArchiveButtonInteractivity();

			var cancelBtn = Root.Q<GenericButtonVisualElement>("cancelBtn");
			cancelBtn.OnClick += CancelButton_OnClicked;
		}

		private void UpdateArchiveButtonInteractivity()
		{
			_buttonGatekeeper.Check();
		}

		private void CancelButton_OnClicked()
		{
			OnCancelled?.Invoke();
		}

		private void ArchiveButton_OnClicked()
		{
			var manifests = _entries.Where(e => e.IsSelected).Select(e => e.manifestId).ToArray();
			var api = BeamEditorContext.Default;
			var promise = api.ContentIO.ArchiveManifests(manifests);
			_archiveBtn.Load(promise);
			promise.Then(_ =>
			{
				_archiveBtn.SetText("Okay");
				_archiveBtn.Button.clickable.clicked -= ArchiveButton_OnClicked;
				_archiveBtn.Button.clickable.clicked += OkButton_OnClicked;
				_listRoot.Clear();
				_listRoot.Add(new Label("Selected manifest namespaces removed."));
			});
		}

		private void OkButton_OnClicked()
		{

			OnCompleted?.Invoke();
		}

		private class Entry
		{
			public readonly LabeledCheckboxVisualElement visualElement;
			public readonly string manifestId;
			public bool IsSelected => visualElement.Value;

			public Entry(AvailableManifestModel model, VisualElement listRoot, bool enabled, Action onValueChange)
			{
				visualElement = new LabeledCheckboxVisualElement();
				listRoot.Add(visualElement);
				manifestId = model.id;
				visualElement.SetEnabled(enabled);
				visualElement.EnableInClassList("disabled", !enabled);
				visualElement.SetFlipState(true);
				visualElement.Refresh();
				visualElement.Checkbox.Button.pickingMode = enabled ? PickingMode.Position : PickingMode.Ignore;
				visualElement.DisableIcon();
				visualElement.SetText(model.id);
				visualElement.OnValueChanged += _ => onValueChange();
			}
		}
	}
}
