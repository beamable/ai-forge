using Beamable.Coroutines;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Beamable.Tournaments
{
	public class CountdownTextBehaviour : MonoBehaviour
	{
		public TextMeshProUGUI Text;
		public string Prefix, Postfix;
		private float _debugSpeed = 1;
		private float _endTime;

		private Coroutine _schedule;
		// Start is called before the first frame update
		void OnEnable()
		{
			if (_schedule != null)
			{
				StopCoroutine(_schedule);
			}
			_schedule = StartCoroutine(UpdateSchedule());
		}

		IEnumerator UpdateSchedule()
		{
			while (gameObject && gameObject.activeInHierarchy)
			{
				Refresh();
				var refreshSeconds = .3f / _debugSpeed;
				yield return Yielders.Seconds(refreshSeconds);
			}
		}

		public void SetSecondsLeft(long secondsLeft)
		{
			_endTime = Time.realtimeSinceStartup + secondsLeft;
			Refresh();
		}

		public int SecondsLeft => Mathf.Max(0, Mathf.FloorToInt(_endTime - (Time.realtimeSinceStartup * _debugSpeed)));
		public int MinutesLeft => SecondsLeft / 60;
		public int HoursLeft => MinutesLeft / 60;

		public string GetTimeString()
		{
			var hourUnits = HoursLeft;
			var minuteUnits = MinutesLeft - (60 * HoursLeft);
			var secondUnits = SecondsLeft - (60 * MinutesLeft);

			// TODO: What about days?
			var hourString = hourUnits > 0 ? $"{hourUnits}h" : "";
			var minuteString = minuteUnits > 0 ? $"{minuteUnits}m" : "";
			var secondsString = hourUnits == 0 ? $"{secondUnits}s" : "";

			return $"{Prefix}{hourString} {minuteString} {secondsString}{Postfix}";
		}

		public void Refresh()
		{
			Text.text = GetTimeString();
		}
	}
}
