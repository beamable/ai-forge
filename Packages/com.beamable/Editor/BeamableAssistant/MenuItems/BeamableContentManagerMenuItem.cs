using Beamable.Editor.Assistant;
using Beamable.Editor.Content;
using UnityEngine;

namespace Beamable.Editor.ToolbarExtender
{
	/// <summary>
	/// Menu Item that opens the <see cref="BeamableAssistantWindow"/> when clicked.
	/// </summary>
#if BEAMABLE_DEVELOPER
	[CreateAssetMenu(fileName = "OpenContentManagerMenuItem", menuName = "Beamable/Assistant/Toolbar Menu Items/Content Manager Window", order = BeamableMenuItemScriptableObjectCreationOrder)]
#endif
	public class BeamableContentManagerMenuItem : BeamableAssistantMenuItem
	{
		public override async void OnItemClicked(BeamEditorContext beamableApi)
		{
			await ContentManagerWindow.Init();
		}
	}
}
