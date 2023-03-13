using System.Diagnostics;
using UnityEditor;
using static Beamable.Common.Constants.MenuItems.Windows.Paths;

#if BEAMABLE_DEVELOPER
namespace Beamable.Server.Editor.DockerCommands
{
   public class BuildBeamServiceCommand : DockerCommand
   {
#if BEAMABLE_DEVELOPER
       [MenuItem(MENU_ITEM_PATH_WINDOW_BEAMABLE + "/Build Beam Service")]
       public static void Run()
       {
           var command = new BuildBeamServiceCommand();
           command.WriteCommandToUnity = true;
           command.WriteLogToUnity = true;
           command.Start();
       }
#endif

       public BuildBeamServiceCommand()
       {
           UnityLogLabel = "BUILD BEAM";
       }

      protected override void ModifyStartInfo(ProcessStartInfo processStartInfo)
      {
         base.ModifyStartInfo(processStartInfo);
         processStartInfo.EnvironmentVariables["DOCKER_BUILDKIT"] = MicroserviceConfiguration.Instance.DisableDockerBuildkit ? "0" : "1";
         processStartInfo.EnvironmentVariables["BEAMABLE_MICROSERVICE_ARCH"] = MicroserviceConfiguration.Instance.GetCPUArchitecture(CPUArchitectureContext.LOCAL);
      }

      public override string GetCommandString()
      {

#if UNITY_EDITOR_WIN
         return "..\\microservice\\build.bat";
#else
         return "../microservice/build.sh";
#endif

      }
   }
}


#endif
