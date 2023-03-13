using UnityEditor;
using UnityEngine;

namespace Beamable.Editor
{
	[InitializeOnLoad]
	public static class NoErrorsValidator
	{
		public static bool LastCompilationSucceded => _compilationSucceeded;

		static bool IsCompiling
		{
			get => EditorPrefs.GetInt(KEY, 0) > 0;
			set => EditorPrefs.SetInt(KEY, value ? 1 : 0);
		}

		static bool _compilationSucceeded = true;

		private const string KEY = "isCompiling";

		static NoErrorsValidator()
		{
			EditorApplication.update -= Update;
			EditorApplication.update += Update;
		}

		static void Update()
		{
			if (EditorApplication.isCompiling == IsCompiling)
				return;

			IsCompiling = EditorApplication.isCompiling;

			if (IsCompiling)
			{
				Application.logMessageReceived += HandleLog;
			}
			else
			{
				Application.logMessageReceived -= HandleLog;
			}
		}

		static void HandleLog(string message, string stackTrace, LogType logType)
		{
			if (logType == LogType.Error)
				_compilationSucceeded = false;
		}
	}
}
