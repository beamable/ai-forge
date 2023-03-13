using Beamable.Editor.Content.Models;
using Beamable.Editor.UI.Components;
using System.Collections.Generic;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Content.Components
{
	public class TagFilterPopupVisualElement : ContentManagerComponent
	{
		public ContentDataModel Model { get; set; }

		public TagFilterPopupVisualElement() : base(nameof(TagFilterPopupVisualElement))
		{

		}

		public override void Refresh()
		{
			base.Refresh();
			// add two rows as testing
			var listRoot = Root.Q<VisualElement>("tagFilterList");
			Model.RebuildTagSet();
			var allTags = Model.GetAllTagDescriptors();

			var searchBar = Root.Q<SearchBarVisualElement>();
			searchBar.OnSearchChanged += filter =>
			{
				SetTagList(allTags, listRoot, filter.ToLower());
			};

			searchBar.DoFocus();
			SetTagList(allTags, listRoot);

		}

		private void SetTagList(IEnumerable<ContentTagDescriptor> allTags, VisualElement listRoot, string filter = null)
		{
			listRoot.Clear();
			foreach (var tagDescriptor in allTags)
			{
				if (!string.IsNullOrEmpty(filter) && !tagDescriptor.Tag.ToLower().Contains(filter)) continue;

				var shouldBeChecked = Model.Filter?.TagConstraints?.Contains(tagDescriptor.Tag) ?? false;
				var row1 = new FilterRowVisualElement();
				row1.OnValueChanged += nextValue => { Model.ToggleTagFilter(tagDescriptor.Tag, nextValue); };

				row1.FilterName = tagDescriptor.Tag;
				row1.Refresh();
				row1.SetValue(shouldBeChecked);
				listRoot.Add(row1);
			}
		}
	}
}

