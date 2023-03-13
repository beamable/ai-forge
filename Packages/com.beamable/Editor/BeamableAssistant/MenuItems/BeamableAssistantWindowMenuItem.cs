using Beamable.Editor.Assistant;
using System.Linq;
using UnityEngine;

namespace Beamable.Editor.ToolbarExtender
{
	/// <summary>
	/// Menu Item that opens the <see cref="BeamableAssistantWindow"/> when clicked.
	/// </summary>
#if BEAMABLE_DEVELOPER
	[CreateAssetMenu(fileName = "OpenAssistantWindowMenuItem", menuName = "Beamable/Assistant/Toolbar Menu Items/Assistant Window", order = BeamableMenuItemScriptableObjectCreationOrder)]
#endif
	public class BeamableAssistantWindowMenuItem : BeamableAssistantMenuItem
	{
		public override GUIContent RenderLabel(BeamEditorContext beamableApi)
		{
			var _hintNotificationManager = default(BeamHintNotificationManager);
			BeamEditor.GetBeamHintSystem(ref _hintNotificationManager);


			var label = $"{Text}";
			if (_hintNotificationManager != null)
			{
				var numNotifications = _hintNotificationManager.AllPendingNotifications.Count();
				label += numNotifications > 0 ? $" - ({numNotifications})" : "";
			}
			return new GUIContent(label);
		}

		public override async void OnItemClicked(BeamEditorContext beamableApi)
		{
			await BeamableAssistantWindow.Init();
		}
	}
}
