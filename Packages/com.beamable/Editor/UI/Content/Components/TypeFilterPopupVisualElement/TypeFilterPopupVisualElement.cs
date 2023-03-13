using Beamable.Editor.Content.Models;
using Beamable.Editor.UI.Components;
using System.Collections.Generic;
using System.Linq;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Content.Components
{
	public class TypeFilterPopupVisualElement : ContentManagerComponent
	{
		public ContentDataModel Model { get; set; }

		public TypeFilterPopupVisualElement() : base(nameof(TypeFilterPopupVisualElement))
		{

		}

		public override void Refresh()
		{
			base.Refresh();
			// add two rows as testing
			var listRoot = Root.Q<VisualElement>("typeFilterList");
			var allTypes = Model.GetContentTypes().ToList();
			var searchbar = Root.Q<SearchBarVisualElement>();
			searchbar.DoFocus();
			searchbar.OnSearchChanged += filter =>
			{
				SetTypeList(allTypes, listRoot, filter.ToLower());
			};
			SetTypeList(allTypes, listRoot);

		}

		private void SetTypeList(List<ContentTypeDescriptor> allTypes, VisualElement listRoot, string filter = null)
		{
			listRoot.Clear();
			foreach (var typeDescriptor in allTypes)
			{
				if (!string.IsNullOrEmpty(filter) && !typeDescriptor.TypeName.ToLower().Contains(filter)) continue;

				var shouldBeChecked = Model.Filter?.TypeConstraints?.Contains(typeDescriptor.ContentType) ?? false;
				var row1 = new FilterRowVisualElement();
				row1.OnValueChanged += nextValue => { Model.ToggleTypeFilter(typeDescriptor, nextValue); };

				row1.FilterName = typeDescriptor.TypeName;
				row1.Refresh();
				row1.SetValue(shouldBeChecked);
				listRoot.Add(row1);
			}
		}
	}
}

