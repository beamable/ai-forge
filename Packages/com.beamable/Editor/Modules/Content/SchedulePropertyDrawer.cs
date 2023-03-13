using Beamable.Common.Content;
using Beamable.Common.Shop;
using Beamable.Content.Utility;
using Beamable.CronExpression;
using Beamable.Editor.UI.Components;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using static Beamable.Common.Constants.Features.Schedules;

namespace Beamable.Editor.Content
{

	[CustomPropertyDrawer(typeof(ListingSchedule))]
	public class ListingSchedulePropertyDrawer : SchedulePropertyDrawer<ListingContent, ListingScheduleWindow>
	{
		protected override ListingContent GetDataObject(SerializedProperty property)
		{
			return ContentRefPropertyDrawer.GetTargetParentObjectOfProperty(property, 2) as ListingContent;
		}
	}

	[CustomPropertyDrawer(typeof(EventSchedule))]
	public class EventSchedulePropertyDrawer : SchedulePropertyDrawer<EventContent, EventScheduleWindow>
	{
		protected override EventContent GetDataObject(SerializedProperty property)
		{
			return ContentRefPropertyDrawer.GetTargetParentObjectOfProperty(property, 2) as EventContent;
		}

		protected override void UpdateSchedule(SerializedProperty property, EventContent evtContent, Schedule schedule, Schedule nextSchedule)
		{
			base.UpdateSchedule(property, evtContent, schedule, nextSchedule);
		}

		protected override void UpdateStartDate(EventContent data)
		{
			var date = data.startDate.ParseEventStartDate(out var _);
			data.startDate = $"{date.Year}-{date.Month:00}-{date.Day:00}T{_window.StartTimeComponent.SelectedHour}";
		}
	}


	public abstract class SchedulePropertyDrawer<TData, TWindow> : PropertyDrawer

	   where TWindow : BeamableVisualElement, IScheduleWindow<TData>, new()
	{
		private Schedule _schedule;
		protected TWindow _window;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight * 4 +
				   EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(Schedule.description))) + 2 +
				   EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(Schedule.definitions))) + 2 +
				   EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(Schedule.activeTo)));
		}
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var topRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
			EditorGUI.LabelField(topRect, label);

			var indent = 20;

			var buttonRect = new Rect(position.x + indent, position.y + 20, position.width - indent * 2, 20);

			_schedule = ContentRefPropertyDrawer.GetTargetObjectOfProperty(property) as Schedule;
			_schedule.activeFrom = DateTime.UtcNow.ToString(DateUtility.ISO_FORMAT);

			var requestEdit = GUI.Button(buttonRect, "Edit Schedule");

			var nextY = buttonRect.y + 20;
			buttonRect = new Rect(buttonRect.x, nextY, buttonRect.width, 20);
			nextY = buttonRect.y + 20;

			for (var index = 0; index < _schedule.definitions.Count; index++)
			{
				var scheduleDefinition = _schedule.definitions[index];
				scheduleDefinition.index = index;
				scheduleDefinition.OnCronRawSaveButtonPressed -= HandleCronRawUpdate;
				scheduleDefinition.OnCronRawSaveButtonPressed += HandleCronRawUpdate;
				scheduleDefinition.OnScheduleModified -= SetDefinitions;
				scheduleDefinition.OnScheduleModified += SetDefinitions;
			}

			void RenderProperty(SerializedProperty prop)
			{
				var height = EditorGUI.GetPropertyHeight(prop);
				var rect = new Rect(buttonRect.x, nextY, buttonRect.width, height);
				nextY += height + 2;
				EditorGUI.PropertyField(rect, prop, true);
			}

			GUI.enabled = false;
			RenderProperty(property.FindPropertyRelative(nameof(Schedule.description)));
			RenderProperty(property.FindPropertyRelative(nameof(Schedule.activeTo)));
			RenderProperty(property.FindPropertyRelative(nameof(Schedule.definitions)));
			GUI.enabled = true;

			if (requestEdit)
			{
				OpenWindow(property, _schedule);
			}
		}

		protected void OpenWindow(SerializedProperty property, Schedule schedule)
		{
			var data = GetDataObject(property);
			if (data == null)
			{
				Debug.LogWarning("No data object exists for " + property);
			}

			_window = new TWindow();
			BeamablePopupWindow popupWindow =
				(Resources.FindObjectsOfTypeAll(typeof(BeamablePopupWindow)) as BeamablePopupWindow[]).FirstOrDefault(
					w => w.titleContent.text == SCHEDULES_WINDOW_HEADER);
			if (popupWindow != null)
			{
				var oldElement = popupWindow.GetRootVisualContainer().Q<TWindow>();
				if (oldElement != null)
				{
					oldElement.Destroy();
				}
				popupWindow.SwapContent(_window);
			}
			else
			{
				popupWindow = BeamablePopupWindow.ShowUtility(SCHEDULES_WINDOW_HEADER,
														  _window, null, SCHEDULES_WINDOW_SIZE);
			}

			_window.Set(schedule, data);
			_window.OnScheduleUpdated += nextSchedule =>
			{
				_window.ApplyDataTransforms(data);
				UpdateSchedule(property, data, schedule, nextSchedule);
				popupWindow.Close();
			};
			_window.OnCancelled += () => popupWindow.Close();

		}

		private void HandleCronRawUpdate(ScheduleDefinition scheduleDefinition)
		{
			var newDefinition = ExpressionDescriptor.CronToScheduleDefinition(scheduleDefinition.cronRawFormat);
			newDefinition.cronRawFormat = scheduleDefinition.cronRawFormat;
			newDefinition.cronHumanFormat = ExpressionDescriptor.GetDescription(newDefinition.cronRawFormat, out _);
			_schedule.definitions[scheduleDefinition.index] = newDefinition;
		}

		protected abstract TData GetDataObject(SerializedProperty property);

		protected virtual void UpdateSchedule(SerializedProperty property, TData data, Schedule schedule, Schedule nextSchedule)
		{
			schedule.description = nextSchedule.description;
			schedule.activeTo = nextSchedule.activeTo;
			schedule.definitions = nextSchedule.definitions;
			UpdateStartDate(data);
			SetDefinitions(schedule);
		}

		protected virtual void UpdateStartDate(TData data) { }

		private void SetDefinitions(Schedule schedule)
		{
			foreach (var definition in schedule.definitions)
			{
				definition.cronRawFormat = ExpressionDescriptor.ScheduleDefinitionToCron(definition);
				definition.cronHumanFormat = ExpressionDescriptor.GetDescription(definition.cronRawFormat, out _);
			}
		}
	}
}
