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
	public class FeedbackAnnouncementModel : AnnouncementModelBase
	{
		public string TitleLabelText => "WOULD YOU LIKE TO SHARE SOME FEEDBACK?";
		public string DescriptionLabelText => "You'll get a prize!";
		public string ShareButtonText => "Share";

		public Action OnIgnore;
		public Action OnShare;

		public override BeamableVisualElement CreateVisualElement()
		{
			return new FeedbackAnnouncementVisualElement
			{
				FeedbackAnnouncementModel = this
			};
		}
	}
}
