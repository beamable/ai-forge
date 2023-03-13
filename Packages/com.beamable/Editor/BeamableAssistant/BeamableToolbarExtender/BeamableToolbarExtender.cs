#if !DISABLE_BEAMABLE_TOOLBAR_EXTENDER

using Beamable.Common;
using Beamable.Editor.Assistant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
#if UNITY_2019_4_OR_NEWER
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
#endif

namespace Beamable.Editor.ToolbarExtender
{
	public static class BeamableToolbarExtender
	{
		const string GET_ALL_METHOD_NAME = "GetAll";

		private static int _toolCount;
		private static GUIStyle _commandStyle = null;

		public static readonly List<Action> LeftToolbarGUI = new List<Action>();
		public static readonly List<Action> RightToolbarGUI = new List<Action>();

		private static BeamEditorContext _editorAPI;
		private static List<BeamableAssistantMenuItem> _assistantMenuItems;
		private static List<BeamableToolbarButton> _leftButtons;
		private static List<BeamableToolbarButton> _rightButtons;

		private static Texture _noHintsTexture;
		private static Texture _hintsTexture;
		private static Texture _validationTexture;

#if UNITY_2019_4_OR_NEWER
		private static bool _hasPreviewPackages = false;
#endif

		private static Action _repaint;

		public static void Repaint() => _repaint?.Invoke();

		public static void LoadToolbarExtender()
		{
			Type toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");

#if UNITY_2019_1_OR_NEWER
			string fieldName = "k_ToolCount";
#else
			string fieldName = "s_ShownToolIcons";
#endif

			FieldInfo toolIcons = toolbarType.GetField(fieldName,
													   BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

			_repaint = () =>
			{
				BeamableToolbarCallbacks.m_toolbarType.GetMethod("RepaintToolbar", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
			};

#if UNITY_2019_3_OR_NEWER
			_toolCount = toolIcons != null ? ((int) toolIcons.GetValue(null)) : 8;
#elif UNITY_2019_1_OR_NEWER
			_toolCount = toolIcons != null ? ((int) toolIcons.GetValue(null)) : 7;
#elif UNITY_2018_1_OR_NEWER
			_toolCount = toolIcons != null ? ((Array)toolIcons.GetValue(null)).Length : 6;
#else
			_toolCount = toolIcons != null ? ((Array)toolIcons.GetValue(null)).Length : 5;
#endif

			BeamableToolbarCallbacks.OnToolbarGUI = OnGUI;


			if (!BeamEditor.IsInitialized)
				return;

			var api = BeamEditorContext.Default;
			_editorAPI = api;

			// Load and inject Beamable Menu Items (necessary due to multiple package split of SDK) --- sort them by specified order, and alphabetically when tied.
			var menuItemsSearchInFolders = BeamEditor.CoreConfiguration.BeamableAssistantMenuItemsPath.Where(Directory.Exists).ToArray();
			var menuItemsGuids = BeamableAssetDatabase.FindAssets<BeamableAssistantMenuItem>(menuItemsSearchInFolders);
			_assistantMenuItems = menuItemsGuids.Select(guid => AssetDatabase.LoadAssetAtPath<BeamableAssistantMenuItem>(AssetDatabase.GUIDToAssetPath(guid))).ToList();
			_assistantMenuItems.Sort((mi1, mi2) =>
			{
				var orderComp = mi1.Order.CompareTo(mi2.Order);
				var labelComp = string.Compare(mi1.RenderLabel(_editorAPI).text, mi2.RenderLabel(_editorAPI).text, StringComparison.Ordinal);

				return orderComp == 0 ? labelComp : orderComp;
			});

			var toolbarButtonsSearchInFolders = BeamEditor.CoreConfiguration.BeamableAssistantToolbarButtonsPaths.Where(Directory.Exists).ToArray();
			var toolbarButtonsGuids = BeamableAssetDatabase.FindAssets<BeamableToolbarButton>(toolbarButtonsSearchInFolders);
			var toolbarButtons = toolbarButtonsGuids.Select(guid => AssetDatabase.LoadAssetAtPath<BeamableToolbarButton>(AssetDatabase.GUIDToAssetPath(guid))).ToList();

			var groupedBySide = toolbarButtons.GroupBy(btn => btn.GetButtonSide(api)).ToList();
			_leftButtons = groupedBySide.Where(g => g.Key == BeamableToolbarButton.Side.Left)
										.SelectMany(g => g)
										.ToList();

			_rightButtons = groupedBySide.Where(g => g.Key == BeamableToolbarButton.Side.Right)
										 .SelectMany(g => g)
										 .ToList();

			_leftButtons.Sort((b1, b2) =>
			{
				var orderComp = b1.GetButtonOrder(_editorAPI).CompareTo(b2.GetButtonOrder(_editorAPI));
				var labelComp = string.Compare(b1.GetButtonText(_editorAPI), b2.GetButtonText(_editorAPI), StringComparison.Ordinal);
				var textureComp = string.Compare(b1.GetButtonTexture(_editorAPI).name, b2.GetButtonTexture(_editorAPI).name, StringComparison.Ordinal);

				return orderComp == 0 ? (labelComp == 0 ? textureComp : labelComp) : orderComp;
			});

			_rightButtons.Sort((b1, b2) =>
			{
				var orderComp = b1.GetButtonOrder(_editorAPI).CompareTo(b2.GetButtonOrder(_editorAPI));
				var labelComp = string.Compare(b1.GetButtonText(_editorAPI), b2.GetButtonText(_editorAPI), StringComparison.Ordinal);
				var textureComp = string.Compare(b1.GetButtonTexture(_editorAPI).name, b2.GetButtonTexture(_editorAPI).name, StringComparison.Ordinal);

				return orderComp == 0 ? (labelComp == 0 ? textureComp : labelComp) : orderComp;
			});

			_noHintsTexture = AssetDatabase.LoadAssetAtPath<Texture>("Packages/com.beamable/Editor/UI/BeamableAssistant/Icons/info.png");
			_hintsTexture = AssetDatabase.LoadAssetAtPath<Texture>("Packages/com.beamable/Editor/UI/BeamableAssistant/Icons/info hit.png");
			_validationTexture = AssetDatabase.LoadAssetAtPath<Texture>("Packages/com.beamable/Editor/UI/BeamableAssistant/Icons/info valu.png");
		}

#if UNITY_2019_3_OR_NEWER
		public const float space = 8;
#else
		public const float space = 10;
#endif
		public const float largeSpace = 20;
		public const float buttonWidth = 32;
		public const float dropdownWidth = 80;

#if UNITY_2019_1_OR_NEWER
		public const float dropdownHeight = 21;
#else
		public const float dropdownHeight = 18;
#endif

		public const float beamableAssistantWidth = 145;
#if UNITY_2019_1_OR_NEWER
		public const float playPauseStopWidth = 140;
#else
		public const float playPauseStopWidth = 100;
#endif

#if UNITY_2020_1_OR_NEWER
		public const float versionControlWidth = 60;
#elif UNITY_2019_1_OR_NEWER
		public const float versionControlWidth = 100;
#else
		public const float versionControlWidth = 78;
#endif


#if UNITY_2021_2_OR_NEWER
		public const float previewPackagesWarningWidth = 190;
#elif UNITY_2020_1_OR_NEWER
		public const float previewPackagesWarningWidth = 175;
#elif UNITY_2019_4_OR_NEWER
		public const float previewPackagesWarningWidth = 0;
#else
		public const float previewPackagesWarningWidth = 0;
#endif

		static void OnGUI()
		{
			if (_editorAPI == null) return;
			// Create two containers, left and right
			// Screen is whole toolbar

			if (_commandStyle == null)
			{
				_commandStyle = new GUIStyle("CommandLeft");
			}

			var screenWidth = EditorGUIUtility.currentViewWidth;

			// Following calculations match code reflected from Toolbar.OldOnGUI()
			float playButtonsPosition = Mathf.RoundToInt((screenWidth - playPauseStopWidth) / 2);

			Rect leftRect = new Rect(0, 0, screenWidth, Screen.height);
			leftRect.xMin += space; // Spacing left

#if !UNITY_2021_2_OR_NEWER
			leftRect.xMin += buttonWidth * _toolCount; // Tool buttons
#if UNITY_2019_3_OR_NEWER
			leftRect.xMin += space; // Spacing between tools and pivot
#else
			leftRect.xMin += largeSpace; // Spacing between tools and pivot
#endif
			leftRect.xMin += 64 * 2; // Pivot buttons
#if UNITY_2019_3_OR_NEWER
			leftRect.xMin += buttonWidth; // Spacing grid snapping tool
#endif
#else
			leftRect.xMin += 125; // Login, Services and Plastic SCM buttons
#endif
			leftRect.xMax = playButtonsPosition;



			Rect rightRect = new Rect(0, 0, screenWidth, Screen.height);
			rightRect.xMin = playButtonsPosition;
			rightRect.xMin += _commandStyle.fixedWidth * 3; // Play buttons
			rightRect.xMax = screenWidth;
			rightRect.xMax -= space; // Spacing right
			rightRect.xMax -= dropdownWidth; // Layout
			rightRect.xMax -= space; // Spacing between layout and layers
			rightRect.xMax -= dropdownWidth; // Layers
#if UNITY_2019_3_OR_NEWER
			rightRect.xMax -= space; // Spacing between layers and account
#else
			rightRect.xMax -= largeSpace; // Spacing between layers and account
#endif
#if UNITY_2021_2_OR_NEWER
			rightRect.xMax -= buttonWidth; // Account
#else
			rightRect.xMax -= dropdownWidth; // Account
			rightRect.xMax -= space; // Spacing between account and cloud
			rightRect.xMax -= buttonWidth; // Cloud
			rightRect.xMax -= space; // Spacing between cloud and collab
			rightRect.xMax -= versionControlWidth; // Colab/PlasticSCM button
#endif


#if UNITY_2019_4_OR_NEWER // Handling of preview packages
			Type type = typeof(UnityEditor.PackageManager.PackageInfo);
			MethodInfo methodInfo = type?.GetMethod(GET_ALL_METHOD_NAME, BindingFlags.NonPublic | BindingFlags.Static);
			var result =  methodInfo?.Invoke(null, null);

			if (result != null)
			{
				var allPackages = ((Array)result).Cast<PackageInfo>().ToArray();

				// Parse package list only if we haven't detected that there are preview packages.
				var foundPreviewPackages = allPackages.Any(package =>
				{
					// referencing https://docs.unity3d.com/Manual/upm-lifecycle.html
					if (package.registry == null)
						return false; // no registry implies a local package, which won't trigger.
					if (!PackageVersion.TryFromSemanticVersionString(package.version, out var version))
					{
						return true; // this isn't a valid package version, so we'll assume its a preview.
					}

					var isPreview = version.IsExperimental || version.IsPreview;
					return isPreview;
				});

				// var foundPreviewPackages = _packageListRequest.Result.Any(pck => pck.version.ToLower().Contains("preview") || string.IsNullOrEmpty(pck.versions.verified));
				_hasPreviewPackages = _hasPreviewPackages || foundPreviewPackages;
				if (_hasPreviewPackages)
				{
					rightRect.xMax -= space;
					rightRect.xMax -= previewPackagesWarningWidth;
				}
				
			}
#endif

#if UNITY_2021_2_OR_NEWER
			rightRect.xMax -= buttonWidth; // Cloud
			rightRect.xMax -= space; // Spacing between cloud and collab
#endif

			var beamableAssistantEnd = rightRect.xMax -= space; // Space between collab and Beamable Assistant
			var beamableAssistantStart = rightRect.xMax -= beamableAssistantWidth; // Beamable Assistant Button

			// Add spacing around existing controls
			leftRect.xMin += space;
			leftRect.xMax -= space;
			rightRect.xMin += space;
			rightRect.xMax -= space;

			// Add top and bottom margins
#if !UNITY_2021_2_OR_NEWER
#if UNITY_2019_3_OR_NEWER
			leftRect.y = 4;
			leftRect.height = 26;
			rightRect.y = 2;
			rightRect.height = 22;
#else
			leftRect.y = 5;
			leftRect.height = 24;
			rightRect.y = 5;
			rightRect.height = 24;
#endif
#else
			leftRect.y = 5;
			leftRect.height = 24;
			rightRect.y = 3;
			rightRect.height = 24;
#endif


			var beamableAssistantButtonRect = new Rect(beamableAssistantStart, rightRect.y + 2, beamableAssistantEnd - beamableAssistantStart, dropdownHeight);
			var btnTexture = _noHintsTexture;

			// Gets notification manager and evaluate if there are pending notifications
			BeamHintNotificationManager notificationManager = null;
			BeamEditor.GetBeamHintSystem(ref notificationManager);
			if (notificationManager != null && notificationManager.PendingHintNotifications.Any())
				btnTexture = _hintsTexture;

			if (notificationManager != null && notificationManager.PendingValidationNotifications.Any())
				btnTexture = _validationTexture;


			GUILayout.BeginArea(beamableAssistantButtonRect);
			if (GUILayout.Button(new GUIContent(" Beamable", btnTexture), GUILayout.Width(beamableAssistantEnd - beamableAssistantStart), GUILayout.Height(dropdownHeight)))
			{
				// create the menu and add items to it
				var menu = new GenericMenu();

				_assistantMenuItems
					.ForEach(item =>
					{
						menu.AddItem(item.RenderLabel(_editorAPI), false, data => item.OnItemClicked((BeamEditorContext)data), _editorAPI);
					});

				menu.ShowAsContext();
			}

			GUILayout.EndArea();

			GUILayout.BeginArea(leftRect);
			GUILayout.BeginHorizontal();

			RenderBeamableToolbarButtons(_leftButtons, _editorAPI);

			foreach (var handler in LeftToolbarGUI)
			{
				handler();
			}

			GUILayout.EndHorizontal();
			GUILayout.EndArea();

			GUILayout.BeginArea(rightRect);
			GUILayout.BeginHorizontal();

			foreach (var handler in RightToolbarGUI)
			{
				handler();
			}

			RenderBeamableToolbarButtons(_rightButtons, _editorAPI);

			GUILayout.EndHorizontal();
			GUILayout.EndArea();

			void RenderBeamableToolbarButtons(List<BeamableToolbarButton> beamableToolbarButtons, BeamEditorContext editorAPI)
			{
				foreach (var button in beamableToolbarButtons.Where(button => button.ShouldDisplayButton(editorAPI)))
				{
					var size = button.GetButtonSize(editorAPI);
					var texture = button.GetButtonTexture(editorAPI);
					var text = button.GetButtonText(editorAPI);

					if (GUILayout.Button(new GUIContent(text, texture), GUILayout.Width(size.x), GUILayout.Height(size.y)))
					{
						// Show dropdown context
						var genMenu = button.GetDropdownOptions(editorAPI);
						genMenu?.ShowAsContext();

						// Runs the configured action. The implementer can simply make this do nothing if they simply want the dropdown options to do stuff.
						button.OnButtonClicked(editorAPI);
					}
				}
			}
		}
	}
}

#endif
