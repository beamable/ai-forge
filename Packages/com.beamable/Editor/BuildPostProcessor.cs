using Beamable.AccountManagement;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace DemoGame.Scripts.Editor
{
	/// <summary>
	/// Post-process builds before compiling to native platform (such as Xcode for iOS).
	/// </summary>
	public static class BuildPostProcessor
	{
#if UNITY_IOS
      [PostProcessBuild]
      public static void OnPostProcessBuild(BuildTarget target, string path)
      {
         if (target == BuildTarget.iOS && AccountManagementConfiguration.Instance.EnableGoogleSignInOnApple)
         {
            UpdateAppleProjectSettings(path);
         }
      }

      private static void UpdateAppleProjectSettings(string path)
      {
         var pbxPath = PBXProject.GetPBXProjectPath(path);
         var project = new PBXProject();
         project.ReadFromFile(pbxPath);

#if UNITY_2019_1_OR_NEWER
         var target = project.GetUnityMainTargetGuid();
#else
         var target = project.TargetGuidByName(PBXProject.GetUnityTargetName());
#endif

         project.AddBuildProperty(target, "OTHER_LDFLAGS", "-ObjC");

         File.WriteAllText(pbxPath, project.WriteToString());
      }
#endif
	}
}
