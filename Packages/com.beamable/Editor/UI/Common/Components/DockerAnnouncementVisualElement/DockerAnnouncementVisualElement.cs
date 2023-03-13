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
	public class DockerAnnouncementVisualElement : BeamableVisualElement
	{
		public DockerAnnouncementModel DockerAnnouncementModel { get; set; }
		public DockerAnnouncementVisualElement() : base(
		   $"{Directories.COMMON_COMPONENTS_PATH}/{nameof(DockerAnnouncementVisualElement)}/{nameof(DockerAnnouncementVisualElement)}")
		{
		}

		public override void Refresh()
		{
			base.Refresh();

			var titleLabel = Root.Q<Label>("announcement-title");
			titleLabel.text = DockerAnnouncementModel.TitleLabelText;
			titleLabel.AddTextWrapStyle();

			var descriptionLabel = Root.Q<Label>("announcement-description");
			descriptionLabel.text = DockerAnnouncementModel.DescriptionLabelText;
			descriptionLabel.AddTextWrapStyle();

			var installButton = Root.Q<Button>("announcement-install");
			installButton.text = DockerAnnouncementModel.InstallButtonText;
			installButton.clickable.clicked += () => DockerAnnouncementModel.OnInstall?.Invoke();
		}
	}
}
