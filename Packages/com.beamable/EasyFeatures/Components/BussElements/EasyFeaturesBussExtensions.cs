using Beamable.UI.Buss;
using System.Collections.Generic;
using System.Linq;

namespace EasyFeatures.Components
{
	public static class EasyFeaturesBussExtensions
	{
		public static void SetButtonPrimary(this BussElement element)
		{
			element.UpdateClasses(new List<string> { "button", "primary" });

			BussElement labelBussElement = element.Children.First(child => child is TextMeshBussElement tmp);
			if (labelBussElement != null)
			{
				labelBussElement.UpdateClasses(new List<string> { "button", "primary", "label" });
			}
		}

		public static void SetButtonDisabled(this BussElement element)
		{
			element.UpdateClasses(new List<string> { "button", "disable" });

			BussElement labelBussElement = element.Children.First(child => child is TextMeshBussElement tmp);
			if (labelBussElement != null)
			{
				labelBussElement.UpdateClasses(new List<string> { "button", "disable", "label" });
			}
		}

		public static void SetSelected(this BussElement element, bool value)
		{
			if (value)
			{
				element.AddClass("selected");
			}
			else
			{
				element.RemoveClass("selected");
			}
		}
	}
}
