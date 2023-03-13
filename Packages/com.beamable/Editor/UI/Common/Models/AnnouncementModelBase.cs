using Beamable.Editor.UI.Components;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Toolbox.Models
{
	public abstract class AnnouncementModelBase
	{
		public abstract BeamableVisualElement CreateVisualElement();

		public VisualElement TitleElement;
		public VisualElement DescriptionElement;
		public ToolboxAnnouncementStatus Status;
	}
	public enum ToolboxAnnouncementStatus
	{
		INFO,
		WARNING,
		DANGER
	}
}
