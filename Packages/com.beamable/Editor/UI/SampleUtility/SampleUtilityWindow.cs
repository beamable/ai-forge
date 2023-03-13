using Beamable.Common;
using Beamable.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Beamable.Editor.UI.SampleUtility
{
	public class SampleUtilityWindow : BeamEditorWindow<SampleUtilityWindow>
	{
		static SampleUtilityWindow()
		{
			WindowDefaultConfig = new BeamEditorWindowInitConfig()
			{
				Title = Constants.MenuItems.Windows.Names.SAMPLE_UTILITY,
				DockPreferenceTypeName = null,
				FocusOnShow = true,
				RequireLoggedUser = false,
			};
		}

#if BEAMABLE_DEVELOPER
		[MenuItem(
			Constants.MenuItems.Windows.Paths.MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES_SAMPLE,
			priority = Constants.MenuItems.Windows.Orders.MENU_ITEM_PATH_WINDOW_PRIORITY_1
		)]
#endif
		public static async void Init() => await GetFullyInitializedWindow();

		public static async void Init(BeamEditorWindowInitConfig initParameters) =>
			await GetFullyInitializedWindow(initParameters);

		private string currentTypeToAddOrRemove = "";
		private HashSet<string> currentlySelectedTypes = new HashSet<string>();

		protected override void Build()
		{
			this.rootVisualElement.Clear();

			currentlySelectedTypes = new HashSet<string>(
				EditorPrefs.GetString(Constants.EditorPrefKeys.ALLOWED_SAMPLES_REGISTER_FUNCTIONS, "").Split(';')
			);

			var registry = BeamEditor.GetReflectionSystem<BeamReflectionCache.Registry>();
			var listOfPossibleTypes = registry.SampleTypesContainingDependencyFunctions.ToList();
			listOfPossibleTypes.Add("");

			var popup = new PopupField<string>("RegisterBeamableDependency in Samples", listOfPossibleTypes,
											   currentTypeToAddOrRemove);
			popup.RegisterValueChangedCallback(evt => currentTypeToAddOrRemove = evt.newValue);

			var addBtn = new Button(() =>
			{
				if (string.IsNullOrEmpty(currentTypeToAddOrRemove)) return;

				currentlySelectedTypes.Add(currentTypeToAddOrRemove);
				EditorPrefs.SetString(Constants.EditorPrefKeys.ALLOWED_SAMPLES_REGISTER_FUNCTIONS,
									  string.Join(";", currentlySelectedTypes));

				BuildWithContext(ActiveContext);
			})
			{ text = "+" };

			var removeBtn = new Button(() =>
			{
				if (string.IsNullOrEmpty(currentTypeToAddOrRemove)) return;

				currentlySelectedTypes.Remove(currentTypeToAddOrRemove);
				EditorPrefs.SetString(Constants.EditorPrefKeys.ALLOWED_SAMPLES_REGISTER_FUNCTIONS,
									  string.Join(";", currentlySelectedTypes));

				BuildWithContext(ActiveContext);
			})
			{ text = "-" };

			var text = new Label(
				$"Selected Types (in Samples) whose RegisterBeamableDependencies Functions will run: {string.Join("\n", currentlySelectedTypes)}");

			this.rootVisualElement.Add(popup);
			this.rootVisualElement.Add(addBtn);
			this.rootVisualElement.Add(removeBtn);
			this.rootVisualElement.Add(text);
		}
	}
}
