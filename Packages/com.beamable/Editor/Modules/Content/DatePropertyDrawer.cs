using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using Beamable.Content.Utility;
using System;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.Content
{
#if !BEAMABLE_NO_DATE_STRING_DRAWERS
	[CustomPropertyDrawer(typeof(MustBeDateString))]
#endif
	public class DatePropertyDrawer : PropertyDrawer
	{
		private const string TextFieldPref = "DateDrawer_useTextField";
		private const float SingleLineWidth = 590f;
		private const float CalendarButtonWidth = 24f;
		private const float SpaceWidth = 2f;

		private const string CalendarIconDark = "d_GridLayoutGroup Icon";
		private const string CalendarIconLight = "GridLayoutGroup Icon";

		#region Reuseable arrays

		private static int _monthNum = 1;
		private static readonly GUIContent[] monthNames =
			DateTimeFormatInfo.InvariantInfo.MonthNames.Select(s => new GUIContent($"{(_monthNum++).ToString("00")} - {s}")).ToArray();

		private static readonly int[] monthsArray = GetIncrementalArray(1, 12);
		private static readonly int[] days28Array = GetIncrementalArray(1, 28);
		private static readonly int[] days29Array = GetIncrementalArray(1, 29);
		private static readonly int[] days30Array = GetIncrementalArray(1, 30);
		private static readonly int[] days31Array = GetIncrementalArray(1, 31);
		private static readonly int[] hoursArray = GetIncrementalArray(0, 24);
		private static readonly int[] minutesArray = GetIncrementalArray(0, 12, 5);
		#endregion

		private static bool IsOptionalString(SerializedProperty property) => property.type == nameof(OptionalString);

		private static SerializedProperty GetStringProperty(SerializedProperty property)
		{
			return IsOptionalString(property) ? property.FindPropertyRelative("Value") : property;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			// TODO: wrap input fields if the inspector is to narrow
			var noValue = IsOptionalString(property) && !property.FindPropertyRelative("HasValue").boolValue;
			var useTextField = EditorPrefs.GetBool(TextFieldPref, false);
			return (EditorGUIUtility.currentViewWidth >= SingleLineWidth || useTextField || noValue ? 1f : 2f) * EditorGUIUtility.singleLineHeight;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label.tooltip = PropertyDrawerHelper.SetTooltipWithFallback(fieldInfo, property);

			var useTextField = EditorPrefs.GetBool(TextFieldPref, false);

			var date = GetCurrentDateTime(property);

			var rectController = new EditorGUIRectController(position);

			EditorGUI.LabelField(rectController.ReserveLabelRect(), label);

			var disableEditing = false;
			if (IsOptionalString(property))
			{
				var hasValueProperty = property.FindPropertyRelative("HasValue");
				EditorGUI.BeginChangeCheck();
				hasValueProperty.boolValue = GUI.Toggle(rectController.ReserveWidthFromRight(16f), hasValueProperty.boolValue, "");
				disableEditing = !hasValueProperty.boolValue;
				if (EditorGUI.EndChangeCheck())
				{
					property.serializedObject.ApplyModifiedProperties();
				}
			}

			if (disableEditing)
			{
				var temp = GUI.enabled;
				GUI.enabled = false;
				GUI.TextField(rectController.rect, "optional value not set");
				GUI.enabled = temp;
			}
			else
			{
				Event e = Event.current;
				if (e.type == EventType.MouseDown && e.button == 1 && position.Contains(e.mousePosition))
				{

					GenericMenu context = new GenericMenu();

					context.AddItem(new GUIContent("Copy"), false, () => Copy(property));
					context.AddItem(new GUIContent("Paste"), false, () => Paste(property));
					context.AddSeparator("");
					context.AddItem(new GUIContent("Use Text Field"), useTextField,
						() => EditorPrefs.SetBool(TextFieldPref, !EditorPrefs.GetBool(TextFieldPref, false)));

					context.ShowAsContext();
				}

				if (useTextField)
				{
					DrawSingleFieldProperty(property, rectController, date);
				}
				else
				{
					DrawMultiFieldProperty(property, rectController, date);
				}
			}

		}

		private void DrawSingleFieldProperty(SerializedProperty property, EditorGUIRectController rectController, DateTime date)
		{
			EditorGUI.BeginChangeCheck();
			var text = EditorGUI.DelayedTextField(
				rectController.ReserveWidth(rectController.rect.width - CalendarButtonWidth - SpaceWidth),
				date.ToUniversalTime().ToString(DateUtility.ISO_FORMAT));
			if (EditorGUI.EndChangeCheck() && DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
			{
				ApplyNewDate(property, date.ToUniversalTime());
			}
			rectController.ReserveWidth(SpaceWidth);

			if (GUI.Button(rectController.ReserveWidth(CalendarButtonWidth),
				EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? CalendarIconDark : CalendarIconLight)))
			{
				ShowCalendar(date, property);
			}
		}

		private void DrawMultiFieldProperty(SerializedProperty property, EditorGUIRectController rectController, DateTime date)
		{
			const float yearFieldWidth = 50f;
			const float doubleDigitFieldWidth = 36f;
			const float shortLabelWidth = 14f;

			EditorGUIRectController dateRectController, timeRectController;
			if (EditorGUIUtility.currentViewWidth >= SingleLineWidth)
			{
				dateRectController = timeRectController = rectController;
			}
			else
			{
				dateRectController = new EditorGUIRectController(rectController.ReserveSingleLine());
				timeRectController = rectController;
				timeRectController.ReserveWidth(14f);
			}

			GUI.Label(dateRectController.ReserveWidth(shortLabelWidth), "Y");
			var year = DrawYearSelection(date, dateRectController.ReserveWidth(yearFieldWidth), property);
			dateRectController.ReserveWidth(SpaceWidth);

			GUI.Label(dateRectController.ReserveWidth(shortLabelWidth), "M");
			var month = DrawMonthSelection(date, dateRectController.ReserveWidth(doubleDigitFieldWidth), property);
			dateRectController.ReserveWidth(SpaceWidth);

			GUI.Label(dateRectController.ReserveWidth(shortLabelWidth), "D");
			var day = DrawDaySelection(date, month, dateRectController.ReserveWidth(doubleDigitFieldWidth), property);
			dateRectController.ReserveWidth(SpaceWidth);

			GUI.Label(timeRectController.ReserveWidth(shortLabelWidth), "h");
			var hour = DrawHourSelection(date, timeRectController.ReserveWidth(doubleDigitFieldWidth), property);
			timeRectController.ReserveWidth(SpaceWidth);

			GUI.Label(timeRectController.ReserveWidth(shortLabelWidth), "m");
			var minute = DrawMinuteSelection(date, timeRectController.ReserveWidth(doubleDigitFieldWidth), property);
			timeRectController.ReserveWidth(SpaceWidth);

			GUI.Label(timeRectController.ReserveWidth(shortLabelWidth), "s");
			var second = DrawSecondSelection(date, timeRectController.ReserveWidth(doubleDigitFieldWidth), property);
			timeRectController.ReserveWidth(SpaceWidth);

			if (GUI.Button(dateRectController.ReserveWidth(CalendarButtonWidth),
				EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? "d_GridLayoutGroup Icon" : "GridLayoutGroup Icon")))
			{
				ShowCalendar(date, property);
			}

			if (DateUtility.TryToCreateDateTime(year, month, day, hour, minute, second, out date))
			{
				ApplyNewDate(property, date);
			}
		}

		private static void ShowCalendar(DateTime date, SerializedProperty property)
		{
			CalendarWindow.ShowWindow(date, property.displayName).onSelectionChanged += newDate =>
			{
				newDate = newDate.AddHours(date.Hour);
				newDate = newDate.AddMinutes(date.Minute);
				newDate = newDate.AddSeconds(date.Second);
				ApplyNewDate(property, newDate);
			};
		}

		private static void ApplyNewDate(SerializedProperty property, DateTime date)
		{
			var stringProperty = GetStringProperty(property);
			var dateString = date.ToString(DateUtility.ISO_FORMAT, CultureInfo.InvariantCulture);
			if (dateString != stringProperty.stringValue)
			{
				stringProperty.stringValue = dateString;
				Undo.RecordObjects(property.serializedObject.targetObjects, "change date");
				property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
				property.serializedObject.Update();
			}
		}

		private static DateTime GetCurrentDateTime(SerializedProperty property)
		{
			if (!DateTime.TryParseExact(GetStringProperty(property).stringValue, DateUtility.ISO_FORMAT, CultureInfo.InvariantCulture,
				DateTimeStyles.None, out var date))
			{
				var now = DateTime.UtcNow;
				date = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second); // skipping milliseconds
				ApplyNewDate(property, date.ToUniversalTime());
			}

			date = date.ToUniversalTime();

			return date;
		}

		private int DrawYearSelection(DateTime date, Rect rect, SerializedProperty property)
		{
			void SetYear(int year)
			{
				if (GetCurrentDateTime(property).TryReplaceYear(year, out var newDate, true))
				{
					ApplyNewDate(property, newDate);
				}
			}

			var currentYear = date.Year;
			return EditorGUIExtension.IntFieldWithDropdown(rect, currentYear,
				GetIncrementalArray(currentYear - 5, 11), SetYear);
		}

		private int DrawMonthSelection(DateTime date, Rect rect, SerializedProperty property)
		{
			void SetMonth(int month)
			{
				if (GetCurrentDateTime(property).TryReplaceMonth(month, out var newDate, true))
				{
					ApplyNewDate(property, newDate);
				}
			}
			return EditorGUIExtension.IntFieldWithDropdown(rect, date.Month,
				monthsArray, SetMonth, "00", monthNames);
		}

		private int DrawDaySelection(DateTime date, int month, Rect rect, SerializedProperty property)
		{
			void SetDay(int day)
			{
				if (GetCurrentDateTime(property).TryReplaceDay(day, out var newDate))
				{
					ApplyNewDate(property, newDate);
				}
			}
			return EditorGUIExtension.IntFieldWithDropdown(rect, date.Day,
				GetDayArray(date.Year, month), SetDay, "00");
		}

		private int DrawHourSelection(DateTime date, Rect rect, SerializedProperty property)
		{
			void SetHour(int hour)
			{
				if (GetCurrentDateTime(property).TryReplaceHour(hour, out var newDate))
				{
					ApplyNewDate(property, newDate);
				}
			}
			return EditorGUIExtension.IntFieldWithDropdown(rect, date.Hour,
				hoursArray, SetHour, "00");
		}

		private int DrawMinuteSelection(DateTime date, Rect rect, SerializedProperty property)
		{
			void SetMinute(int minute)
			{
				if (GetCurrentDateTime(property).TryReplaceMinute(minute, out var newDate))
				{
					ApplyNewDate(property, newDate);
				}
			}
			return EditorGUIExtension.IntFieldWithDropdown(rect, date.Minute,
				minutesArray, SetMinute, "00");
		}

		private int DrawSecondSelection(DateTime date, Rect rect, SerializedProperty property)
		{
			void SetSecond(int second)
			{
				if (GetCurrentDateTime(property).TryReplaceSecond(second, out var newDate))
				{
					ApplyNewDate(property, newDate);
				}
			}
			return EditorGUIExtension.IntFieldWithDropdown(rect, date.Second,
				minutesArray, SetSecond, "00");
		}

		private static int[] GetDayArray(int year, int month)
		{
			try
			{
				var dayCount = DateTime.DaysInMonth(year, month);
				switch (dayCount)
				{
					case 28: return days28Array;
					case 29: return days29Array;
					case 30: return days30Array;
					case 31: return days31Array;
				}
				return GetIncrementalArray(1, dayCount);
			}
			catch (ArgumentOutOfRangeException)
			{
				return days28Array;
			}
		}

		private static int[] GetIncrementalArray(int from, int length, int step = 1)
		{
			var array = new int[length];
			for (int i = 0; i < length; i++)
			{
				array[i] = i * step + from;
			}

			return array;
		}

		private void Copy(SerializedProperty property)
		{
			GUIUtility.systemCopyBuffer = property.stringValue;
		}

		private void Paste(SerializedProperty property)
		{
			GetStringProperty(property).stringValue = GUIUtility.systemCopyBuffer;
			property.serializedObject.ApplyModifiedProperties();
		}
	}
}
