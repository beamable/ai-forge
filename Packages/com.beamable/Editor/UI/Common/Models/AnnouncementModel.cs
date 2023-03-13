using Beamable.Editor.Toolbox.Components;
using Beamable.Editor.UI.Components;
using System;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Toolbox.Models
{
	public class AnnouncementModel : AnnouncementModelBase
	{
		public void SetTitle(string title)
		{
			TitleElement = new Label(title);
		}

		public void SetDescription(string desc)
		{
			DescriptionElement = new Label(desc);
			DescriptionElement.AddTextWrapStyle();
		}

		public string ActionText;
		public Texture2D CustomIcon;
		public Action Action;
		public override BeamableVisualElement CreateVisualElement()
		{
			return new AnnouncementVisualElement()
			{
				AnnouncementModel = this
			};
		}
	}
}
