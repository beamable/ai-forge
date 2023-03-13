using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
#if UNITY_2018
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEditor.UIElements;
#endif

namespace Beamable.Server.Editor.UI
{
	public static class WindowStateUtility
	{
		/// <summary>
		/// Checks if disabling process is still in use.
		/// </summary>
		private static bool InUse { get; set; }

		private static EditorApplication.CallbackFunction _changeWindowEnableStateCallback;

		/// <summary>
		/// Disables all active editor windows (even if you close and reopen) 
		/// </summary>
		/// <param name="ignoredWindowNames">Editor windows names which should be ignored in disabling process</param>
		public static void DisableAllWindows(IEnumerable<string> ignoredWindowNames = null)
		{
			if (InUse)
				return;

			InUse = true;
			_changeWindowEnableStateCallback = ChangeWindowEnableStates(ignoredWindowNames);
			EditorApplication.update -= _changeWindowEnableStateCallback;
			EditorApplication.update += _changeWindowEnableStateCallback;
		}

		/// <summary>
		/// Enables all disabled editor windows
		/// </summary>
		public static void EnableAllWindows()
		{
			if (!InUse)
				return;
			InUse = false;

			EditorApplication.update -= _changeWindowEnableStateCallback;
			Resources.FindObjectsOfTypeAll<EditorWindow>().ToList().ForEach(window =>
			{
				window.GetRootVisualContainer()?.SetEnabled(true);
			});
		}

		private static EditorApplication.CallbackFunction ChangeWindowEnableStates(IEnumerable<string> ignoredWindowNames)
		{
			return () => Resources.FindObjectsOfTypeAll<EditorWindow>().ToList().ForEach(window =>
			{
				if (!InUse)
					return;

				if (!IsWindowInIgnoreList(window.name, ignoredWindowNames))
				{
					window.GetRootVisualContainer()?.SetEnabled(false);
				}
			});
		}
		private static bool IsWindowInIgnoreList(string name, IEnumerable<string> ignoredNames) => ignoredNames != null && ignoredNames.Any(ignoreName => name == ignoreName);
	}
}
