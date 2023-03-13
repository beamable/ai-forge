using Beamable.Common.Runtime;
using Beamable.Content;
using Beamable.Editor.UI.Common.Models;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using static Beamable.Common.Constants;
using static Beamable.Common.Constants.Features.ContentManager;

namespace Beamable.Editor.UI.Components
{
	public class ManifestButtonVisualElement : BeamableVisualElement
	{
		public new class UxmlFactory : UxmlFactory<ManifestButtonVisualElement, UxmlTraits>
		{
		}
		public new class UxmlTraits : VisualElement.UxmlTraits
		{

			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get { yield break; }
			}

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				var self = ve as ManifestButtonVisualElement;
			}
		}
		private ManifestModel Model { get; set; }
		private Button _manifestButton;
		private Label _manifestLabel;
		private bool _manyManifests;
		private bool _nonDefaultManifest;

		public ManifestButtonVisualElement() : base(
			$"{Directories.COMMON_COMPONENTS_PATH}/{nameof(ManifestButtonVisualElement)}/{nameof(ManifestButtonVisualElement)}")
		{

		}

		public override void Refresh()
		{
			base.Refresh();
			Model = new ManifestModel();
			Model.OnAvailableElementsChanged -= HandleAvailableManifestsChanged;
			Model.OnAvailableElementsChanged += HandleAvailableManifestsChanged;
			visible = false;
			Model.Initialize();
			_manifestButton = Root.Q<Button>("manifestButton");
			_manifestButton.clickable.clicked += () => { OnButtonClicked(_manifestButton.worldBound); };

			_manifestLabel = _manifestButton.Q<Label>();
			if (Model.Current == null || Model.Current.DisplayName == null)
			{
				_manifestLabel.text = "Select manifest ID";
			}
			else
			{
				_manifestLabel.text = Model.Current?.DisplayName;
			}
			Model.OnElementChanged -= HandleManifestChanged;
			Model.OnElementChanged += HandleManifestChanged;
		}

		private void HandleAvailableManifestsChanged(List<ISearchableElement> ids)
		{
			var idsAmount = ids?.Count ?? 0;
			_manyManifests = idsAmount > 1;
			_nonDefaultManifest = idsAmount == 1 && ids[0].DisplayName != DEFAULT_MANIFEST_ID;

			RefreshButtonVisibility();
		}

		public void RefreshButtonVisibility()
		{
			visible = (_manyManifests && ContentConfiguration.Instance.EnableMultipleContentNamespaces) || _nonDefaultManifest;
			EnableInClassList("hidden", !visible);
			ContentConfiguration.Instance.multipleContentNamespacesSettingLocked = ContentConfiguration.Instance.EditorManifestID != DEFAULT_MANIFEST_ID;
		}

		private void HandleManifestChanged(ISearchableElement manifest)
		{
			_manifestLabel.text = Model.Current?.DisplayName;
		}

		private void OnButtonClicked(Rect visualElementBounds)
		{
			var popupWindowRect = BeamablePopupWindow.GetLowerLeftOfBounds(visualElementBounds);

			var content = new SearchabledDropdownVisualElement(string.Empty)
			{
				Model = Model
			};

			var wnd = BeamablePopupWindow.ShowDropdown("Select Manifest", popupWindowRect, new Vector2(200, 300), content);

			content.OnElementDelete += (manifest) =>
			{
				if (manifest != null)
				{
					var deleteManifestDecision = EditorUtility.DisplayDialog(
							"Deleting manifest version",
							$"Are you sure you want to archive manifest named '{manifest.DisplayName}'\n" +
							$"This operation will archive it permanently for all users!",
							"Yes", "No");

					if (deleteManifestDecision)
					{
						var api = BeamEditorContext.Default;
						api.ContentIO.ArchiveManifests(manifest.DisplayName);
					}
				}
			};

			content.OnElementSelected += (manifest) =>
			{
				var api = BeamEditorContext.Default;
				if (manifest != null) api.ContentIO.SwitchManifest(manifest.DisplayName);
				wnd.Close();
			};
			content.Refresh();
		}

	}
}
