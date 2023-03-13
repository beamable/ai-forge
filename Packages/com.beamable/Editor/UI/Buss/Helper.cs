using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Beamable.Editor.UI.Buss
{
	public static class Helper
	{
		public static IEnumerable<Type> GetAllClassesInheritedFrom(Type baseClass) =>
			AppDomain.CurrentDomain.GetAssemblies()
					 .SelectMany(assembly => assembly.GetTypes())
					 .Where(x => x.IsClass && !x.IsAbstract && x.IsInheritedFrom(baseClass));

		public static List<string> GetAllClassesNamesInheritedFrom(Type baseClass) =>
			GetAllClassesInheritedFrom(baseClass).Select(x => x.Name).ToList();

		/// <summary>
		/// Working same as AssetDatabase.FindAssets but returns collection of loaded assets instead of GUIDs.
		/// </summary>
		/// <param name="filter">The filter string can contain search data. See below for details about this string.</param>
		/// <param name="searchInFolders">The folders where the search will start.</param>
		/// <typeparam name="T">Asset type</typeparam>
		/// <returns>Collection of loaded assets</returns>
		/// <footer><a href="file:///C:/Program%20Files/Unity/Hub/Editor/2018.4.36f1/Editor/Data/Documentation/en/ScriptReference/AssetDatabase.html">External documentation for `AssetDatabase`</a></footer>
		public static List<T> FindAssets<T>(string filter, string[] searchInFolders)
			where T : UnityEngine.Object
		{
			var GUIDs = AssetDatabase.FindAssets(filter, searchInFolders);
			var assets = new List<T>();
			foreach (var GUID in GUIDs)
			{
				var path = AssetDatabase.GUIDToAssetPath(GUID);
				assets.Add(AssetDatabase.LoadAssetAtPath<T>(path));
			}
			return assets;
		}
	}
}
