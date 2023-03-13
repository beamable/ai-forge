using Beamable.Editor.UI.Components;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

using static Beamable.Common.Constants.Features.Buss.ThemeManager;

namespace Beamable.Editor.UI.Buss
{
	public class ThemeManagerComponent : BeamableVisualElement
	{
		public ThemeManagerComponent(string name) : base($"{BUSS_THEME_MANAGER_PATH}/{name}/{name}") { }
	}
}
