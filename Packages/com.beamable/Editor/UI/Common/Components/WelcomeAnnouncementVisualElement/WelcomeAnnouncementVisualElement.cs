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
	public class WelcomeAnnouncementVisualElement : BeamableVisualElement
	{
		public WelcomeAnnouncementModel WelcomeAnnouncementModel { get; set; }
		public WelcomeAnnouncementVisualElement() : base(
		   $"{Directories.COMMON_COMPONENTS_PATH}/{nameof(WelcomeAnnouncementVisualElement)}/{nameof(WelcomeAnnouncementVisualElement)}")
		{
		}

		public override void Refresh()
		{
			base.Refresh();

			var titleLabel = Root.Q<Label>("announcement-title");
			titleLabel.text = WelcomeAnnouncementModel.TitleLabelText;

			var textsSection = Root.Q<VisualElement>("announcement-textsSection");
			textsSection.Add(WelcomeAnnouncementModel.DescriptionElement);

			var importButton = Root.Q<Button>("announcement-import");
			importButton.text = WelcomeAnnouncementModel.ImportButtonText;
			importButton.clickable.clicked += () => WelcomeAnnouncementModel.OnImport?.Invoke();
		}
	}
}
