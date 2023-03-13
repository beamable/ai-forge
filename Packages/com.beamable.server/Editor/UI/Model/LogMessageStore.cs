using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beamable.Editor.UI.Model
{
	[System.Serializable]
	public class LogMessageStore : ISerializationCallbackReceiver
	{
		public List<LogMessage> Messages = new List<LogMessage>();
		public List<LogMessage> FilteredMessages = new List<LogMessage>();

		public LogMessage Selected;
		public float ScrollValue;
		public bool HasScrolled;
		public bool IsTailingLog = true;
		public string Filter = string.Empty;

		public DateTime LatestReceivedLogTime { get; private set; }
		[SerializeField]
		private long _highestLogTimeTicks; // this field is meant to be used for serialization help

		public long DebugCount;
		public long InfoCount;
		public long WarningCount;
		public long ErrorCount;
		public long FatalCount;

		public bool ViewDebugEnabled = false;
		public bool ViewInfoEnabled = true;
		public bool ViewWarningEnabled = true;
		public bool ViewErrorEnabled = true;

		public event Action OnMessagesUpdated;
		public event Action OnViewFilterChanged;
		public event Action OnSelectedMessageChanged;
		public void AddMessage(LogMessage message)
		{
			if (DateTime.TryParse(message.Timestamp, out var messageTime))
			{
				if (messageTime.Ticks < LatestReceivedLogTime.Ticks)
				{
					return; //we've already processed logs from this time
				}
				LatestReceivedLogTime = messageTime;
			}

			IncrementCount(message);
			Messages.Add(message);
			if (DoesMessagePassFilter(message))
			{
				FilteredMessages.Add(message);
			}

			OnMessagesUpdated?.Invoke();
		}

		public void ToggleViewDebugEnabled() => SetViewDebugEnabled(!ViewDebugEnabled);
		public void SetViewDebugEnabled(bool enabled)
		{
			if (ViewDebugEnabled == enabled) return;
			ViewDebugEnabled = enabled;
			UpdateFiltered();
			OnViewFilterChanged?.Invoke();
		}

		public void ToggleViewInfoEnabled() => SetViewInfoEnabled(!ViewInfoEnabled);
		public void SetViewInfoEnabled(bool enabled)
		{
			if (ViewInfoEnabled == enabled) return;
			ViewInfoEnabled = enabled;
			UpdateFiltered();
			OnViewFilterChanged?.Invoke();
		}

		public void ToggleViewWarningEnabled() => SetViewWarningEnabled(!ViewWarningEnabled);
		public void SetViewWarningEnabled(bool enabled)
		{
			if (ViewWarningEnabled == enabled) return;
			ViewWarningEnabled = enabled;
			UpdateFiltered();
			OnViewFilterChanged?.Invoke();
		}

		public void ToggleViewErrorEnabled() => SetViewErrorEnabled(!ViewErrorEnabled);
		public void SetViewErrorEnabled(bool enabled)
		{
			if (ViewErrorEnabled == enabled) return;
			ViewErrorEnabled = enabled;
			UpdateFiltered();
			OnViewFilterChanged?.Invoke();
		}

		public void SetSearchLogFilter(string filter)
		{
			Filter = filter;
			UpdateFiltered();
			OnViewFilterChanged?.Invoke();
		}

		public void SetSelectedLog(LogMessage message)
		{
			Selected = message;
			OnSelectedMessageChanged?.Invoke();
		}

		public bool DoesMessagePassFilter(LogMessage message)
		{
			if (!string.IsNullOrEmpty(Filter))
			{
				if (!message.Message.ToLower().Contains(Filter.ToLower()))
					return false;
			}

			switch (message.Level)
			{
				case LogLevel.INFO:
					return ViewInfoEnabled;
				case LogLevel.DEBUG:
					return ViewDebugEnabled;
				case LogLevel.WARNING:
					return ViewWarningEnabled;
				case LogLevel.ERROR:
				case LogLevel.FATAL:
					return ViewErrorEnabled;
				default:
					return false;
			}
		}

		public void Clear()
		{
			LatestReceivedLogTime = DateTime.Now;
			InfoCount = 0;
			DebugCount = 0;
			WarningCount = 0;
			ErrorCount = 0;
			FatalCount = 0;
			Messages.Clear();
			FilteredMessages.Clear();
			Selected = null;
			ScrollValue = 0;
			OnSelectedMessageChanged?.Invoke();
			OnMessagesUpdated?.Invoke();
		}

		private void UpdateFiltered()
		{
			FilteredMessages.Clear();
			FilteredMessages.AddRange(Messages.Where(DoesMessagePassFilter).ToList());

			OnMessagesUpdated?.Invoke();
		}

		private void IncrementCount(LogMessage message)
		{
			switch (message.Level)
			{
				case LogLevel.INFO:
					InfoCount++;
					break;
				case LogLevel.DEBUG:
					DebugCount++;
					break;
				case LogLevel.WARNING:
					WarningCount++;
					break;
				case LogLevel.ERROR:
					ErrorCount++;
					break;
				case LogLevel.FATAL:
					FatalCount++;
					break;
			}
		}

		public void OnBeforeSerialize()
		{
			_highestLogTimeTicks = LatestReceivedLogTime.Ticks;
		}

		public void OnAfterDeserialize()
		{
			LatestReceivedLogTime = default(DateTime).AddTicks(_highestLogTimeTicks);
		}
	}

	[System.Serializable]
	public class LogMessage
	{
		public string Message;
		public string Timestamp;
		public string ParameterText;
		public Dictionary<string, object> Parameters;
		public LogLevel Level;
		public Color MessageColor;
		public bool IsBoldMessage;
		public string PostfixMessageIcon;

		public static string GetTimeDisplay(DateTime time)
		{
			return time.ToString("HH:mm:ss");
		}
	}

	public enum LogLevel
	{
		FATAL,
		ERROR,
		WARNING,
		INFO,
		DEBUG
	}
}
