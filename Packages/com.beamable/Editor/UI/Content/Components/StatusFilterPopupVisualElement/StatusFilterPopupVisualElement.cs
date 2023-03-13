using Beamable.Editor.Content.Models;
using System;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Content.Components
{
	public class StatusFilterPopupVisualElement : ContentManagerComponent
	{
		private ScrollView _scroller;
		public ContentDataModel Model { get; set; }

		public StatusFilterPopupVisualElement() : base(nameof(StatusFilterPopupVisualElement))
		{

		}

		public override void Refresh()
		{
			base.Refresh();
			_scroller = Root.Q<ScrollView>();


			foreach (var value in Enum.GetValues(typeof(ContentValidationStatus)))
			{
				var validationValue = (ContentValidationStatus)value;
				var filterRow = new FilterRowVisualElement();
				filterRow.FilterName = validationValue.ToString().ToLower();
				filterRow.Refresh();
				filterRow.OnValueChanged += next =>
				{
					Model.ToggleValidationFilter(validationValue, next);
					Refresh();
				};
				filterRow.SetValue((Model?.Filter?.HasValidationConstraint ?? false) && Model.Filter.ValidationConstraint == validationValue);
				_scroller.contentContainer.Add(filterRow);
			}

			// TODO: add spacer

			foreach (var value in Enum.GetValues(typeof(ContentModificationStatus)))
			{
				var statusValue = (ContentModificationStatus)value;

				if (statusValue == ContentModificationStatus.NOT_AVAILABLE_ANYWHERE) continue;

				var filterRow = new FilterRowVisualElement();
				filterRow.FilterName = statusValue.Serialize();
				filterRow.Refresh();
				filterRow.OnValueChanged += next =>
				{
					Model.ToggleStatusFilter(statusValue, next);
				};

				var hasFilter = Model?.Filter?.HasStatusConstraint ?? false;
				var included = hasFilter && (statusValue == (Model.Filter.StatusConstraint & statusValue));

				filterRow.SetValue(included);
				_scroller.contentContainer.Add(filterRow);
			}

		}
	}
}

