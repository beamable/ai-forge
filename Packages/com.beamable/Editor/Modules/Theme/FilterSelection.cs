using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.Modules.Theme
{
	public class FilterSelection
	{
		public const int FILTERMODE_ALL = 0;
		public const int FILTERMODE_NAME = 1;
		public const int FILTERMODE_TYPE = 2;

		public static void SetSearchFilter(string filter, int filterMode)
		{

			SearchableEditorWindow[] windows = (SearchableEditorWindow[])Resources.FindObjectsOfTypeAll(typeof(SearchableEditorWindow));
			var sceneView = EditorWindow.GetWindow<SceneView>();

			object hierarchy = null;

			foreach (SearchableEditorWindow window in windows)
			{
				if (window.GetType().ToString().Equals("UnityEditor.SceneHierarchyWindow"))
				{
					hierarchy = window;
					break;
				}
			}

			if (hierarchy == null)
				return;

			MethodInfo setSearchType = typeof(SearchableEditorWindow).GetMethod("SetSearchFilter", BindingFlags.NonPublic | BindingFlags.Instance);
			object[] parameters = new object[] { filter, filterMode, false, false };

			setSearchType.Invoke(hierarchy, parameters);
			setSearchType.Invoke(sceneView, parameters);

		}
	}
}
