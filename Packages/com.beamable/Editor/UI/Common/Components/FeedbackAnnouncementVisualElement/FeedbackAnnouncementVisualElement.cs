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
	public class FeedbackAnnouncementVisualElement : BeamableVisualElement
	{
		public FeedbackAnnouncementModel FeedbackAnnouncementModel { get; set; }
		public FeedbackAnnouncementVisualElement() : base(
		   $"{Directories.COMMON_COMPONENTS_PATH}/{nameof(FeedbackAnnouncementVisualElement)}/{nameof(FeedbackAnnouncementVisualElement)}")
		{
		}

		public override void Refresh()
		{
			base.Refresh();

			var titleLabel = Root.Q<Label>("announcement-title");
			titleLabel.text = FeedbackAnnouncementModel.TitleLabelText;
			titleLabel.AddTextWrapStyle();

			var descriptionLabel = Root.Q<Label>("announcement-description");
			descriptionLabel.text = FeedbackAnnouncementModel.DescriptionLabelText;
			descriptionLabel.AddTextWrapStyle();

			var ignoreButton = Root.Q<Button>("announcement-ignore");
			ignoreButton.clickable.clicked += () => FeedbackAnnouncementModel.OnIgnore?.Invoke();

			var shareButton = Root.Q<Button>("announcement-share");
			shareButton.text = FeedbackAnnouncementModel.ShareButtonText;
			shareButton.clickable.clicked += () => FeedbackAnnouncementModel.OnShare?.Invoke();
		}
	}
}
