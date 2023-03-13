using Beamable.Api.Sessions;
using Beamable.Sessions;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Beamable.Editor.Modules.Sessions
{
	public class SessionParameterProviderCreator
	{
		public const string EditorPrefKey = "Beamabe.Create.CustomSettingProvider";

		[MenuItem("Assets/Create/Beamable - Scripts/C# Session Parameter Provider")]
		public static void CreateProviderScript()
		{

			var startPath = "";
			var obj = Selection.activeObject;
			if (obj == null)
			{
				startPath = "Assets";
			}
			else
			{
				startPath = AssetDatabase.GetAssetPath(obj.GetInstanceID());
			}

			if (startPath.Length == 0)
			{
				startPath = "Assets";
			}

			var path = EditorUtility.SaveFilePanelInProject("Provider Name", "CustomSessionParameterProvider", "cs", "Create your custom session provider script", startPath);
			if (string.IsNullOrEmpty(path)) return;

			// TODO: replace this with codeDom
			var name = Path.GetFileNameWithoutExtension(path);
			using (StreamWriter outfile =
			   new StreamWriter(path))
			{
				outfile.WriteLine("using UnityEngine;");
				outfile.WriteLine("using System.Collections.Generic;");
				outfile.WriteLine("using Beamable.Api.Sessions;");
				outfile.WriteLine("using Beamable.Common.Api.Auth;");
				outfile.WriteLine("");
				outfile.WriteLine($"public class {name} : {nameof(SessionParameterProvider)} {{");
				outfile.WriteLine("\t");
				outfile.WriteLine("\t// Provide custom session parameters");
				outfile.WriteLine($"\tpublic override void {nameof(SessionParameterProvider.AddCustomParameters)}(Dictionary<string, string> parameters, User user) {{");
				outfile.WriteLine("\t\t");
				outfile.WriteLine("\t}");
				outfile.WriteLine("}");
			}


			EditorPrefs.SetString(EditorPrefKey, path);
			AssetDatabase.Refresh();

		}

		[DidReloadScripts]
		public static void After()
		{
			var path = EditorPrefs.GetString(EditorPrefKey);
			if (string.IsNullOrEmpty(path)) return;

			var typeName = Path.GetFileNameWithoutExtension(path);

			Object assetInstance = null;
			try
			{
				assetInstance = ScriptableObject.CreateInstance(typeName);

			}
			finally
			{
				if (assetInstance == null)
				{
					Debug.LogError("Unable to create custom settings asset. " + path);
					EditorPrefs.SetString(EditorPrefKey, null);
				}
			}

			if (assetInstance == null)
			{
				return;
			}

			var assetPath = Path.ChangeExtension(path, ".asset");
			AssetDatabase.CreateAsset(assetInstance, assetPath);

			if (SessionConfiguration.Instance.CustomParameterProvider == null || EditorUtility.DisplayDialog("Set Beamable Session Parameter Provider",
				   $"You already have a custom setting provider selected in the Beamable Session settings. You are using {SessionConfiguration.Instance.CustomParameterProvider.name}. Would you like to override the settings with {typeName}?",
				   "Override", "No"))
			{
				SessionConfiguration.Instance.CustomParameterProvider = assetInstance as SessionParameterProvider;
			}
			EditorPrefs.SetString(EditorPrefKey, null);
			Selection.SetActiveObjectWithContext(assetInstance, assetInstance);
			AssetDatabase.Refresh();
		}
	}
}
