using Beamable.Editor.Toolbox.Components;
using Beamable.Editor.UI.Components;
using System;

namespace Beamable.Editor.Toolbox.Models
{
	public class SurveyAnnouncementModel : AnnouncementModelBase
	{
		public string TitleLabelText => "HOW ARE YOU ENJOYING WITH BEAMABLE?";

		public Action<SurveyResult> OnOpinionSelected;
		public Action OnIgnore;

		public override BeamableVisualElement CreateVisualElement()
		{
			return new SurveyAnnouncementVisualElement()
			{
				SurveyAnnouncementModel = this
			};
		}
	}

	public enum SurveyResult
	{
		Positive = 0,
		Neutral = 1,
		Negative = 2
	}
}
