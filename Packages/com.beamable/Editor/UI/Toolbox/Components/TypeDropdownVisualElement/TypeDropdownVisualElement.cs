using Beamable.Editor.Toolbox.Models;
using Beamable.Editor.Toolbox.UI.Components;
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
	public class TypeDropdownVisualElement : ToolboxComponent
	{
		public IToolboxViewService Model { get; set; }

		public TypeDropdownVisualElement() : base(nameof(TypeDropdownVisualElement))
		{

		}

		public override void Refresh()
		{
			base.Refresh();
			// add two rows as testing
			var listRoot = Root.Q<VisualElement>("typeList");
			var allTypes = Enum.GetValues(typeof(WidgetOrientationSupport)).Cast<WidgetOrientationSupport>().ToList();

			SetTypesList(allTypes, listRoot);
		}

		private void SetTypesList(IEnumerable<WidgetOrientationSupport> allTypes, VisualElement listRoot)
		{
			listRoot.Clear();
			foreach (var orientation in allTypes)
			{
				var typeName = orientation.Serialize();


				var row = new FilterRowVisualElement();
				row.OnValueChanged += nextValue =>
				{
					Model.SetOrientationSupport(orientation, nextValue);
				};

				row.FilterName = typeName;
				row.Refresh();
				var isOrientationSupported = (Model.Query?.HasOrientationConstraint ?? false)
											 && Model.Query.FilterIncludes(orientation);
				row.SetValue(isOrientationSupported);

				listRoot.Add(row);
			}
		}

	}
}
