using Beamable.Editor.Content.Helpers;
using Beamable.Editor.Content.Models;
using System;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace Beamable.Editor.Content.Components
{
	public class ContentSorterDropdownVisualElement : ContentManagerComponent
	{
		public event Action<ContentSortType, string> OnSortingChanged;

		private VisualElement _popupContent;
		private ContentDataModel _model;

		public ContentSorterDropdownVisualElement(ContentDataModel model) : base(nameof(ContentSorterDropdownVisualElement))
		{
			_model = model;
		}

		public override void Refresh()
		{
			base.Refresh();
			_popupContent = Root.Q<VisualElement>("popupContent");
			_popupContent.Clear();
			SetupSorters();
		}

		private void SetupSorters()
		{
			foreach (var sorterOption in ContentSorterHelper.GetAllSorterOptions)
			{
				var value = ContentSorterHelper.GetContentSorterTitle(sorterOption);
				var button = new Button { text = value };
				button.clickable.clicked += () => OnSortingChanged?.Invoke(sorterOption, value);
				button.SetEnabled(_model.CurrentSorter != sorterOption);
				_popupContent.Add(button);
			}
		}
	}
}
