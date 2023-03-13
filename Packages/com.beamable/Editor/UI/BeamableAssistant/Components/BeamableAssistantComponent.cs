using Beamable.Editor.UI.Components;
using static Beamable.Common.Constants.Features.Assistant;

namespace Beamable.Editor.Assistant
{
	public class BeamableAssistantComponent : BeamableVisualElement
	{
		public BeamableAssistantComponent(string name) : base($"{COMPONENTS_PATH}/{name}/{name}") { }
	}
}
