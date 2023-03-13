using Beamable.Editor.Toolbox.Models;
using Beamable.Editor.UI.Components;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using static Beamable.Common.Constants;

namespace Beamable.Editor.Toolbox.Components
{
	public class SurveyAnnouncementVisualElement : BeamableVisualElement
	{
		public SurveyAnnouncementModel SurveyAnnouncementModel { get; set; }
		public SurveyAnnouncementVisualElement() : base(
		   $"{Directories.COMMON_COMPONENTS_PATH}/{nameof(SurveyAnnouncementVisualElement)}/{nameof(SurveyAnnouncementVisualElement)}")
		{
		}

		public override void Refresh()
		{
			base.Refresh();

			var titleLabel = Root.Q<Label>("announcement-title");
			titleLabel.text = SurveyAnnouncementModel.TitleLabelText;
			titleLabel.AddTextWrapStyle();

			var surveyPositiveButton = Root.Q<Button>("announcement-surveyPositive");
			surveyPositiveButton.clickable.clicked += () => SurveyAnnouncementModel.OnOpinionSelected?.Invoke(SurveyResult.Positive);
			surveyPositiveButton.tooltip = "Happy with Beamable";

			var surveyNeutralButton = Root.Q<Button>("announcement-surveyNeutral");
			surveyNeutralButton.clickable.clicked += () => SurveyAnnouncementModel.OnOpinionSelected?.Invoke(SurveyResult.Neutral);
			surveyNeutralButton.tooltip = "Ok with Beamable";

			var surveyNegativeButton = Root.Q<Button>("announcement-surveyNegative");
			surveyNegativeButton.clickable.clicked += () => SurveyAnnouncementModel.OnOpinionSelected?.Invoke(SurveyResult.Negative);
			surveyNegativeButton.tooltip = "Not enjoy at all with Beamable";

			var ignoreButton = Root.Q<Button>("announcement-ignore");
			ignoreButton.clickable.clicked += () => SurveyAnnouncementModel.OnIgnore?.Invoke();
		}
	}
}
