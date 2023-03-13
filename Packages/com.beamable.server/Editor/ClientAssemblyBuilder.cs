using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Beamable.Server.Editor
{
	public class ClientAssemblyBuilder
	{

		public static void BuildDependencyAssembly()
		{
			//var asm = new AssemblyBuilder();
		}

		public static void BuildAssembly(MicroserviceDescriptor descriptor, bool wait, string[] scripts)
		{
			//var scripts = new[] { "Temp/MyAssembly/MyScript1.cs", "Temp/MyAssembly/MyScript2.cs" };
			var outputAssembly = $"Temp/Microservices/{descriptor.Name}.dll";
			var assemblyProjectPath = $"Assets/Beamable/Microservices/{descriptor.Name}Client.dll";

			Directory.CreateDirectory("Temp/Microservices");

			// Create scripts
			//        foreach (var scriptPath in scripts)
			//        {
			//            var scriptName = Path.GetFileNameWithoutExtension(scriptPath);
			//            File.WriteAllText(scriptPath, string.Format("using UnityEngine; public class {0} : MonoBehaviour {{ void Start() {{ Debug.Log(\"{0}\"); }} }}", scriptName));
			//        }

			var assemblyBuilder = new AssemblyBuilder(outputAssembly, scripts);


			// Exclude a reference to the copy of the assembly in the Assets folder, if any.
			assemblyBuilder.excludeReferences = new string[] { assemblyProjectPath };

			// Called on main thread
			assemblyBuilder.buildStarted += delegate (string assemblyPath)
			{
				Debug.LogFormat("Assembly build started for {0}", assemblyPath);
			};


			// Called on main thread
			assemblyBuilder.buildFinished += delegate (string assemblyPath, CompilerMessage[] compilerMessages)
			{
				var errorCount = compilerMessages.Count(m => m.type == CompilerMessageType.Error);
				var warningCount = compilerMessages.Count(m => m.type == CompilerMessageType.Warning);

				Debug.LogFormat("Assembly build finished for {0}", assemblyPath);
				Debug.LogFormat("Warnings: {0} - Errors: {0}", errorCount, warningCount);

				foreach (var msg in compilerMessages)
				{
					Debug.LogError($"<color=red>{msg.message} - {msg.line}:{msg.column} </color>");
				}
				if (errorCount == 0)
				{
					File.Copy(outputAssembly, assemblyProjectPath, true);
					AssetDatabase.ImportAsset(assemblyProjectPath);
				}
			};

			// Start build of assembly
			if (!assemblyBuilder.Build())
			{
				Debug.LogErrorFormat("Failed to start build of assembly {0}!", assemblyBuilder.assemblyPath);
				return;
			}

			if (wait)
			{
				while (assemblyBuilder.status != AssemblyBuilderStatus.Finished)
					System.Threading.Thread.Sleep(10);
			}
		}
	}
}
