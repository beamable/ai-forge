using Beamable.Editor.Toolbox.Components;
using Beamable.Editor.UI.Components;
using System;

namespace Beamable.Editor.Toolbox.Models
{
	public class WelcomeAnnouncementModel : AnnouncementModelBase
	{
		public string TitleLabelText => "BEAMABLE + TEXTMESHPRO + ADDRESSABLES = ♥";
		public string ImportButtonText => "Import";

		public Action OnImport;

		public override BeamableVisualElement CreateVisualElement()
		{
			return new WelcomeAnnouncementVisualElement()
			{
				WelcomeAnnouncementModel = this
			};
		}
	}
}
