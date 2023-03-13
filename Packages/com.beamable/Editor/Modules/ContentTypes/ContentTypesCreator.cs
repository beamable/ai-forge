using System.IO;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.Modules.ContentTypes
{
	public class ContentTypesCreator
	{
		private const string TemplatesPath = "Packages/com.beamable/Editor/Modules/ContentTypes/ScriptTemplates/";

		[MenuItem("Content", menuItem = "Assets/Create/Beamable - Scripts/Content", priority = 200)]
		public static void CreateContent()
		{
			CreateFile("Content");
		}

		[MenuItem("CurrencyContent", menuItem = "Assets/Create/Beamable - Scripts/Currency Content", priority = 200)]
		public static void CreateCurrencyContent()
		{
			CreateFile("CurrencyContent");
		}

		[MenuItem("ItemContent", menuItem = "Assets/Create/Beamable - Scripts/Item Content", priority = 200)]
		public static void CreateItemContent()
		{
			CreateFile("ItemContent");
		}

		private static void CreateFile(string className)
		{
			string buildPath = BuildPath(className);
			if (!string.IsNullOrEmpty(buildPath))
			{
#if UNITY_2018
                typeof(UnityEditor.ProjectWindowUtil).GetMethod("CreateScriptAsset",
                        System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                    ?.Invoke(null, new object[] {buildPath, $"Custom{className}.cs"});
#elif UNITY_2019_1_OR_NEWER
                ProjectWindowUtil.CreateScriptAssetFromTemplateFile(buildPath, $"Custom{className}.cs");
#endif
			}
		}

		private static string BuildPath(string name)
		{
			string fullPath = Path.GetFullPath($"{TemplatesPath}/{name}.cs.txt");
			bool templateExists = File.Exists(fullPath);

			if (templateExists)
			{
				return $"{TemplatesPath}/{name}.cs.txt";
			}

			Debug.Assert(templateExists, $"Template file for class {name} doesn't exist");
			return string.Empty;
		}
	}
}
