using Beamable.Editor.Toolbox.Models;
using Beamable.Editor.Toolbox.UI.Components;
using Beamable.Editor.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Toolbox.Components
{
	public class CategoryDropdownVisualElement : ToolboxComponent
	{
		public IToolboxViewService Model { get; set; }

		public CategoryDropdownVisualElement() : base(nameof(CategoryDropdownVisualElement))
		{

		}

		public override void Refresh()
		{
			base.Refresh();
			// add two rows as testing
			var listRoot = Root.Q<VisualElement>("tagList");
			var searchBar = Root.Q<SearchBarVisualElement>();
			var allTypes = Enum.GetValues(typeof(WidgetTags)).Cast<WidgetTags>().ToList();

			searchBar.OnSearchChanged += filter =>
			{
				SetTypesList(allTypes, listRoot, filter);
			};

			SetTypesList(allTypes, listRoot);
		}

		private void SetTypesList(IEnumerable<WidgetTags> allTypes, VisualElement listRoot, string filter = null)
		{
			listRoot.Clear();
			foreach (var type in allTypes)
			{
				var typeName = type.Serialize();

				if (!string.IsNullOrEmpty(filter) && !typeName.ToLower().Contains(filter)) continue;

				var row = new FilterRowVisualElement();
				row.OnValueChanged += nextValue =>
				{
					Model.SetQueryTag(type, nextValue);
				};

				row.FilterName = typeName;
				row.Refresh();
				var hasTag = (Model.Query?.HasTagConstraint ?? false)
											 && Model.Query.FilterIncludes(type);
				row.SetValue(hasTag);

				listRoot.Add(row);
			}
		}
	}
}

