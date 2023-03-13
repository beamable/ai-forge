using Beamable.Editor.UI.Components;
using static Beamable.Common.Constants.Features.Toolbox;

namespace Beamable.Editor.Toolbox.UI.Components
{
	public class ToolboxComponent : BeamableVisualElement
	{
		public ToolboxComponent(string name) : base($"{COMPONENTS_PATH}/{name}/{name}")
		{

		}
	}
}
