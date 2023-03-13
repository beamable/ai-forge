//using System.Collections.Generic;
//using System.IO;
//using UnityEditor;
//using UnityEditor.Build;
//using UnityEditor.Build.Reporting;
//using UnityEditor.VersionControl;
//using UnityEngine;
//
//namespace Beamable.Server.Editor
//{
//   public class SanitizeServerLogicBuildStep : IPreprocessBuildWithReport, IPostprocessBuildWithReport
//   {
//      public static List<MicroserviceDescriptor> Descriptors;
//
//      public int callbackOrder { get; }
//
//      public void OnPostprocessBuild(BuildReport report)
//      {
//         foreach (var descriptor in Descriptors)
//         {
//            Debug.Log($"Postprocess {descriptor.Name} from {descriptor.HidePath} {descriptor.SourcePath}");
//            DirectoryCopy(descriptor.HidePath, descriptor.SourcePath, true);
//            Directory.Delete(descriptor.HidePath, true);
//
//         }
//      }
//
//      public void OnPreprocessBuild(BuildReport report)
//      {
//         Descriptors = Microservices.Descriptors;
//
//         foreach (var descriptor in Descriptors)
//         {
//            Debug.Log($"Preprocess {descriptor.Name} from {descriptor.SourcePath} to {descriptor.HidePath}");
//            DirectoryCopy(descriptor.SourcePath, descriptor.HidePath, true);
//            Directory.Delete(descriptor.SourcePath, true);
//         }
//
//      }
//
//      private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
//      {
//         // Get the subdirectories for the specified directory.
//         DirectoryInfo dir = new DirectoryInfo(sourceDirName);
//
//         if (!dir.Exists)
//         {
//            throw new DirectoryNotFoundException(
//               "Source directory does not exist or could not be found: "
//               + sourceDirName);
//         }
//
//         DirectoryInfo[] dirs = dir.GetDirectories();
//
//         // If the destination directory doesn't exist, create it.
//         Directory.CreateDirectory(destDirName);
//
//         // Get the files in the directory and copy them to the new location.
//         FileInfo[] files = dir.GetFiles();
//         foreach (FileInfo file in files)
//         {
//            string tempPath = Path.Combine(destDirName, file.Name);
//            var isUsingVersionControl = Provider.enabled && Provider.isActive;
//            if (isUsingVersionControl)
//            {
//               var checkoutTargetFileTask = Provider.Checkout(tempPath, CheckoutMode.Both);
//               checkoutTargetFileTask.Wait();
//               if (checkoutTargetFileTask.success)
//               {
//                  Debug.LogWarning($"Unable to checkout file {tempPath}");
//               }
//               var checkoutSourceFileTask = Provider.Checkout(file.Name, CheckoutMode.Both);
//               checkoutSourceFileTask.Wait();
//               if (checkoutSourceFileTask.success)
//               {
//                  Debug.LogWarning($"Unable to checkout file {file.Name}");
//               }
//            }
//            file.CopyTo(tempPath, true);
//         }
//
//         // If copying subdirectories, copy them and their contents to new location.
//         if (copySubDirs)
//         {
//            foreach (DirectoryInfo subdir in dirs)
//            {
//               string tempPath = Path.Combine(destDirName, subdir.Name);
//               DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
//            }
//         }
//      }
//
//   }
//}
