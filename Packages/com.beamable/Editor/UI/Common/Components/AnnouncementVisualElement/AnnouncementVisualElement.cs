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
	public class AnnouncementVisualElement : BeamableVisualElement
	{
		public AnnouncementModel AnnouncementModel { get; set; }
		public AnnouncementVisualElement() : base(
		   $"{Directories.COMMON_COMPONENTS_PATH}/{nameof(AnnouncementVisualElement)}/{nameof(AnnouncementVisualElement)}")
		{
		}

		public override void Refresh()
		{
			base.Refresh();

			var titleLabel = Root.Q<VisualElement>("title");
			var descLabel = Root.Q<VisualElement>("desc");
			var actionButton = Root.Q<Button>();

			titleLabel.Add(AnnouncementModel.TitleElement);
			descLabel.Add(AnnouncementModel.DescriptionElement);

			actionButton.text = AnnouncementModel.ActionText;
			actionButton.clickable.clicked += OnActionClicked;

			if (AnnouncementModel.CustomIcon != null)
			{
				var icon = Root.Q<VisualElement>("icon");
				icon.style.backgroundImage = AnnouncementModel.CustomIcon;
			}

			switch (AnnouncementModel.Status)
			{
				case ToolboxAnnouncementStatus.INFO:
					AddToClassList("info");
					break;
				case ToolboxAnnouncementStatus.WARNING:
					AddToClassList("warning");
					break;
				case ToolboxAnnouncementStatus.DANGER:
					AddToClassList("danger");
					break;
			}
		}

		private void OnActionClicked()
		{
			// TODO: maybe do something flashy?
			AnnouncementModel?.Action?.Invoke();
		}
	}
}
