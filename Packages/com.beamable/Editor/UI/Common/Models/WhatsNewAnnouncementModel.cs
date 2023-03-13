using Beamable.Editor.Toolbox.Components;
using Beamable.Editor.UI.Components;
using System;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
namespace Beamable.Editor.Toolbox.Models
{
	public class WhatsNewAnnouncementModel : AnnouncementModelBase
	{
		public string WhatsNewButtonText = "What's New";
		public string TitleLabelText => "BEAMABLE PACKAGE IS UPDATED";
		public string DescriptionLabelText => "Check out the new features on the official blog";

		public Action OnIgnore;
		public Action OnWhatsNew;

		public override BeamableVisualElement CreateVisualElement()
		{
			return new WhatsNewAnnouncementVisualElement
			{
				WhatsNewAnnouncementModel = this
			};
		}
	}
}
