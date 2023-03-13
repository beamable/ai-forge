using System;
using System.Collections.Generic;
using System.Text;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using static Beamable.Common.Constants;

namespace Beamable.Editor.UI.Components
{
	public class HourPickerVisualElement : BeamableVisualElement
	{
		public new class UxmlFactory : UxmlFactory<HourPickerVisualElement, UxmlTraits>
		{
		}

		private LabeledNumberPicker _hourPicker;
		private LabeledNumberPicker _minutePicker;
		private LabeledNumberPicker _secondPicker;
		private bool _activeHour = true;
		private bool _activeMinute = true;
		private bool _activeSecond = true;
		private Action _onHourChanged;
		private VisualElement _root;

		public string Hour => _hourPicker?.Value;
		public string Minute => _minutePicker?.Value;
		public string Second => _secondPicker?.Value;

		public HourPickerVisualElement() : base(
			$"{Directories.COMMON_COMPONENTS_PATH}/{nameof(HourPickerVisualElement)}/{nameof(HourPickerVisualElement)}")
		{
		}

		public override void Refresh()
		{
			base.Refresh();

			_root = Root.Q<VisualElement>("mainVisualElement");

			_hourPicker = Root.Q<LabeledNumberPicker>("hourPicker");

			if (!_activeHour)
			{
				_root.Remove(_hourPicker);
				_hourPicker = null;
			}

			_hourPicker?.Setup(_onHourChanged, GenerateHours(), _activeHour);
			_hourPicker?.Refresh();

			_minutePicker = Root.Q<LabeledNumberPicker>("minutePicker");

			if (!_activeMinute)
			{
				_root.Remove(_minutePicker);
				_minutePicker = null;
			}

			_minutePicker?.Setup(_onHourChanged, GenerateMinutesAndSeconds(), _activeMinute);
			_minutePicker?.Refresh();

			_secondPicker = Root.Q<LabeledNumberPicker>("secondPicker");

			if (!_activeSecond)
			{
				_root.Remove(_secondPicker);
				_secondPicker = null;
			}

			_secondPicker?.Setup(_onHourChanged, GenerateMinutesAndSeconds(), _activeSecond);
			_secondPicker?.Refresh();
		}

		public void Setup(Action onHourChanged, bool activeHour = true, bool activeMinute = true, bool activeSecond = true)
		{
			_onHourChanged = onHourChanged;
			_activeHour = activeHour;
			_activeMinute = activeMinute;
			_activeSecond = activeSecond;
		}

		public string GetFullHour()
		{
			StringBuilder builder = new StringBuilder();

			string hour = _hourPicker != null ? _hourPicker.Value : "00";
			string minute = _minutePicker != null ? _minutePicker.Value : "00";
			string second = _secondPicker != null ? _secondPicker.Value : "00";

			builder.Append($"{int.Parse(hour):00}:{int.Parse(minute):00}:{int.Parse(second):00}Z");
			return builder.ToString();
		}

		private List<string> GenerateHours()
		{
			List<string> options = new List<string>();

			for (int i = 0; i < 24; i++)
			{
				string hour = (i).ToString("00");
				options.Add(hour);
			}

			return options;
		}

		private List<string> GenerateMinutesAndSeconds()
		{
			List<string> options = new List<string>();

			for (int i = 0; i < 4; i++)
			{
				string hour = (i * 15).ToString("00");
				options.Add(hour);
			}

			return options;
		}

		public void Set(DateTime date)
		{
			_hourPicker?.Set(date.Hour.ToString());
			_minutePicker?.Set(date.Minute.ToString());
			_secondPicker?.Set(date.Second.ToString());
		}

		public void SetGroupEnabled(bool b)
		{
			_hourPicker?.SetEnabled(b);
			_minutePicker?.SetEnabled(b);
			_secondPicker?.SetEnabled(b);
		}
	}
}
