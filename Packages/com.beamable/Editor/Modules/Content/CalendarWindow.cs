using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.Content
{
	public class CalendarWindow : EditorWindow, ISerializationCallbackReceiver
	{

		private GUIStyle _dayStyle;

		[SerializeField]
		private string _startOfCurrentMonthString, _selectionString;
		private DateTime _startOfCurrentMonth;
		private DateTime _selection;
		public event Action<DateTime> onSelectionChanged;

		public static CalendarWindow ShowWindow(DateTime date, string title = "Date")
		{
			var calendar = GetWindow<CalendarWindow>();
			if (calendar != null)
			{
				calendar.Close();
			}
			calendar = CreateInstance<CalendarWindow>();
			calendar.titleContent = new GUIContent(title);
			calendar.InitSelection(date);
			calendar.minSize = calendar.maxSize = new Vector2(300f, 210f);
			calendar.ShowUtility();
			return calendar;
		}

		public void InitSelection(DateTime date)
		{
			_selection = date.Date;
			_startOfCurrentMonth = new DateTime(date.Year, date.Month, 1);
		}

		private void OnEnable()
		{
			_dayStyle = new GUIStyle(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).box);
		}

		private void OnGUI()
		{
			EditorGUILayout.BeginHorizontal();
			_startOfCurrentMonth = _startOfCurrentMonth.AddYears(
				DrawValueWithArrows(_startOfCurrentMonth.ToString("yyyy")));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			_startOfCurrentMonth = _startOfCurrentMonth.AddMonths(
				DrawValueWithArrows(_startOfCurrentMonth.ToString("MMMMMMMMMMMMM", CultureInfo.InvariantCulture)));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			DrawCalendarDays();
		}

		private void DrawCalendarDays()
		{
			var dayOfWeek = 0;
			var date = _startOfCurrentMonth;
			var dayWidth = GUILayout.Width((EditorGUIUtility.currentViewWidth - 24f) / 7f);
			var dayHeight = GUILayout.Height(20f);

			EditorGUILayout.BeginHorizontal();
			var dayNames = DateTimeFormatInfo.InvariantInfo.ShortestDayNames;
			for (int i = 0; i < 7; i++)
			{
				var dayName = dayNames[(i + 1) % 7]; // starting from monday
				var weekDayRect = EditorGUILayout.GetControlRect(false, dayWidth, dayHeight);
				GUI.Label(weekDayRect, dayName);
			}
			EditorGUILayout.EndHorizontal();

			while (date.Month == _startOfCurrentMonth.Month)
			{
				if (dayOfWeek == 0)
				{
					EditorGUILayout.BeginHorizontal();
				}

				var rect = EditorGUILayout.GetControlRect(false, dayWidth, dayHeight);
				if (dayOfWeek == GetDayOfWeek(date))
				{
					var temp = GUI.color;
					if (rect.Contains(Event.current.mousePosition))
					{
						GUI.color = Color.yellow;
					}
					else if (date == _selection)
					{
						GUI.color = Color.cyan;
					}

					if (GUI.Button(rect, date.Day.ToString("00"), _dayStyle))
					{
						_selection = date;
						onSelectionChanged?.Invoke(_selection);
					}

					GUI.color = temp;

					date = date.AddDays(1);
				}

				dayOfWeek++;
				if (dayOfWeek >= 7)
				{
					dayOfWeek = 0;
					EditorGUILayout.EndHorizontal();
				}
				else if (date.Month != _startOfCurrentMonth.Month)
				{
					EditorGUILayout.EndHorizontal();
				}
			}
		}

		private int DrawValueWithArrows(string value)
		{
			const float buttonWidth = 44f;
			var offset = 0;
			var rect = EditorGUILayout.GetControlRect(GUILayout.Height(EditorGUIUtility.singleLineHeight));
			var rc = new EditorGUIRectController(rect);
			if (GUI.Button(rc.ReserveWidth(buttonWidth), "<"))
			{
				offset = -1;
			}

			var valueRect = rc.ReserveWidth(rc.rect.width - buttonWidth);
			var centeredStyle = GUI.skin.GetStyle("Label");
			centeredStyle.alignment = TextAnchor.MiddleCenter;
			GUI.Label(valueRect, value, centeredStyle);

			if (GUI.Button(rc.rect, ">"))
			{
				offset = 1;
			}

			return offset;
		}

		private void Update()
		{
			Repaint();
		}

		private int GetDayOfWeek(DateTime day)
		{
			switch (day.DayOfWeek)
			{
				case DayOfWeek.Monday: return 0;
				case DayOfWeek.Tuesday: return 1;
				case DayOfWeek.Wednesday: return 2;
				case DayOfWeek.Thursday: return 3;
				case DayOfWeek.Friday: return 4;
				case DayOfWeek.Saturday: return 5;
				case DayOfWeek.Sunday: return 6;
				default: return -1;
			}
		}

		public void OnBeforeSerialize()
		{
			_startOfCurrentMonthString = _startOfCurrentMonth.ToString();
			_selectionString = _selection.ToString();
		}

		public void OnAfterDeserialize()
		{
			if (DateTime.TryParse(_startOfCurrentMonthString, out var date))
			{
				_startOfCurrentMonth = date;
				_startOfCurrentMonthString = null;
			}

			if (DateTime.TryParse(_selectionString, out date))
			{
				_selection = date;
				_selectionString = null;
			}
		}
	}
}
