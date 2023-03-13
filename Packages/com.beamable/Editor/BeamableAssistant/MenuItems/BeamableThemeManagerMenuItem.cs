using Beamable.Editor.Assistant;
using Beamable.Editor.UI.Buss;
using UnityEngine;

namespace Beamable.Editor.ToolbarExtender
{
	/// <summary>
	/// Menu Item that opens the <see cref="BeamableAssistantWindow"/> when clicked.
	/// </summary>
#if BEAMABLE_DEVELOPER
	[CreateAssetMenu(fileName = "OpenThemeManagerMenuItem", menuName = "Beamable/Assistant/Toolbar Menu Items/Theme Manager Window", order = BeamableMenuItemScriptableObjectCreationOrder)]
#endif
	public class BeamableThemeManagerMenuItem : BeamableAssistantMenuItem
	{
		public override void OnItemClicked(BeamEditorContext beamableApi)
		{
			ThemeManager.Init();
		}
	}
}
