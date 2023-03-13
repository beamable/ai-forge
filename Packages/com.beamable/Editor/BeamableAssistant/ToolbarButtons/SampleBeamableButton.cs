using Beamable.Common;
using Beamable.Editor.Assistant;
using Beamable.Editor.Content;
using UnityEditor;
using UnityEngine;
using static Beamable.Common.Constants.MenuItems.Assets.Orders;

namespace Beamable.Editor.ToolbarExtender
{
	/// <summary>
	/// A sample button that can be used as a reference to understand how to extend the Unity editor's toolbar.  
	/// </summary>
	[CreateAssetMenu(menuName = "Beamable/Assistant/Sample Toolbar Button", fileName = "SampleBeamableButton", order = MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_LAST)]
	public class SampleBeamableButton : BeamableToolbarButton
	{
		public override bool ShouldDisplayButton(BeamEditorContext editorAPI) => editorAPI.HasCustomer;
		public override void OnButtonClicked(BeamEditorContext editorAPI) => BeamableLogger.Log($"I'm a working beamable button for customer: {editorAPI.CurrentUser.email}");

		public override GenericMenu GetDropdownOptions(BeamEditorContext editorAPI)
		{
			GenericMenu menu = new GenericMenu();
			menu.AddItem(new GUIContent("Open Beamable Assistant 😃"), false, async () => await BeamableAssistantWindow.Init());
			menu.AddItem(new GUIContent("Open Beamable Content 💼"), false, async () => await ContentManagerWindow.Init());

			return menu;
		}
	}
}
