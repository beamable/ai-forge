using Beamable.Editor.Environment;
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
	public class UpdateAvailableAnnouncementVisualElement : BeamableVisualElement
	{
		public UpdateAvailableAnnouncementModel UpdateAvailableAnnouncementModel { get; set; }
		public UpdateAvailableAnnouncementVisualElement() : base(
		   $"{Directories.COMMON_COMPONENTS_PATH}/{nameof(UpdateAvailableAnnouncementVisualElement)}/{nameof(UpdateAvailableAnnouncementVisualElement)}")
		{
		}

		public override void Refresh()
		{
			base.Refresh();

			var titleLabel = Root.Q<Label>("announcement-title");
			titleLabel.text = UpdateAvailableAnnouncementModel.TitleLabelText;
			titleLabel.AddTextWrapStyle();

			var descriptionLabel = Root.Q<Label>("announcement-description");
			descriptionLabel.text = UpdateAvailableAnnouncementModel.DescriptionLabelText;
			descriptionLabel.AddTextWrapStyle();

			var ignoreButton = Root.Q<Button>("announcement-ignore");
			ignoreButton.clickable.clicked += () => UpdateAvailableAnnouncementModel.OnIgnore?.Invoke();

			var whatsnewButton = Root.Q<Button>("announcement-whatsnew");
			whatsnewButton.text = UpdateAvailableAnnouncementModel.WhatsNewButtonText;
			whatsnewButton.clickable.clicked += () => UpdateAvailableAnnouncementModel.OnWhatsNew?.Invoke();

			var installButton = Root.Q<Button>("announcement-install");
			installButton.text = UpdateAvailableAnnouncementModel.InstallButtonText;
			installButton.clickable.clicked += () =>
			{
				installButton.SetEnabled(false);
				UpdateAvailableAnnouncementModel.OnInstall?.Invoke();
			};
		}
	}
}
