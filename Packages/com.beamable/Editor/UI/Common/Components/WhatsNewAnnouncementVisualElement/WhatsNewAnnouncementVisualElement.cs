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
	public class WhatsNewAnnouncementVisualElement : BeamableVisualElement
	{
		public WhatsNewAnnouncementModel WhatsNewAnnouncementModel { get; set; }
		public WhatsNewAnnouncementVisualElement() : base(
		   $"{Directories.COMMON_COMPONENTS_PATH}/{nameof(WhatsNewAnnouncementVisualElement)}/{nameof(WhatsNewAnnouncementVisualElement)}")
		{
		}

		public override void Refresh()
		{
			base.Refresh();

			var titleLabel = Root.Q<Label>("announcement-title");
			titleLabel.text = WhatsNewAnnouncementModel.TitleLabelText;
			titleLabel.AddTextWrapStyle();

			var descriptionLabel = Root.Q<Label>("announcement-description");
			descriptionLabel.text = WhatsNewAnnouncementModel.DescriptionLabelText;
			descriptionLabel.AddTextWrapStyle();

			var ignoreButton = Root.Q<Button>("announcement-ignore");
			ignoreButton.clickable.clicked += () => WhatsNewAnnouncementModel.OnIgnore?.Invoke();

			var whatsnewButton = Root.Q<Button>("announcement-whatsnew");
			whatsnewButton.text = WhatsNewAnnouncementModel.WhatsNewButtonText;
			whatsnewButton.clickable.clicked += () => WhatsNewAnnouncementModel.OnWhatsNew?.Invoke();
		}
	}
}
