using Beamable.Common;
using Beamable.Editor.Config;
using Beamable.Editor.Modules.Account;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Beamable.Common.Constants.MenuItems.Windows.Paths;

namespace Beamable.Editor.Environment
{
	[Serializable]
	public class ExceptionData
	{
		public bool Exists;
		public string Message;
		public string Type;
		public string StackTrace;

		public static ExceptionData From(Exception ex)
		{
			return new ExceptionData
			{
				Exists = true,
				Message = ex.Message,
				Type = ex.GetType().Name,
				StackTrace = ex.StackTrace
			};
		}
	}

	[System.Serializable]
	public class BeamableDiagnosticData
	{
		private const string ManifestPath = "Packages/manifest.json";
		private const string ConfigDefaultsPath = "Assets/Beamable/Resources/config-defaults.txt";

		public EnvironmentData EnvironmentData;
		public ExceptionData GlobalError;
		public ExceptionData InitException;
		public ExceptionData CheckServerPackageException;
		public ExceptionData ManifestJsonException;
		public ExceptionData ConfigDefaultsException;
		public ExceptionData ConfigurationException;
		public string Cid;
		public string Pid;
		public string CidOrAlias;
		public EditorUser EditorUser;
		public string Os;
		public string UnityVersion;
		public bool IsProSkin;
		public string MicroservicesPackageVersion;
		public bool HasMicroservicesPackage;
		public string ManifestJson;
		public string ConfigDefaultsJson;
		public List<string> Configurations; // cannot save the ScriptableObject directly, so we'll use the yml instead.
		public string CreatedAtUTC;
		public List<string> ScriptingDefineSymbols;

		[MenuItem(MENU_ITEM_PATH_WINDOW_BEAMABLE_HELP_DIAGNOSTIC_DATA)]
		public static void WriteDebugData()
		{
			Create().Then(data =>
			{
				var json = JsonUtility.ToJson(data, true);
				var filePath =
				$"{Directory.GetParent(Application.dataPath)}{Path.DirectorySeparatorChar}beamable-diag-{data.CreatedAtUTC}.json";
				Debug.Log($"Writing diagnostic data to file. [{filePath}]");

				File.WriteAllText(filePath, json);
				EditorUtility.RevealInFinder(filePath);
			});
		}

		private static void ApplySystemInfo(BeamableDiagnosticData data)
		{
			data.EnvironmentData = BeamableEnvironment.Data;
			data.Os = SystemInfo.operatingSystem;
			data.UnityVersion = Application.unityVersion;
			data.IsProSkin = EditorGUIUtility.isProSkin;
			data.CreatedAtUTC = DateTime.UtcNow.ToFileTimeUtc().ToString();
		}

		private static void ApplyPlayerSettingsInfo(BeamableDiagnosticData data)
		{
			data.ScriptingDefineSymbols = PlayerSettingsHelper.GetDefines().ToList();
		}

		private static Promise<Unit> ApplyServerInfo(BeamableDiagnosticData data)
		{
			return BeamablePackages.GetPackageInfo(BeamablePackages.ServerPackageName).Map(server =>
			{
				if (server == null)
				{
					data.HasMicroservicesPackage = false;
					return PromiseBase.Unit;
				}

				data.HasMicroservicesPackage = true;
				data.MicroservicesPackageVersion = server.version;

				return PromiseBase.Unit;
			}).Recover(ex =>
			{
				data.CheckServerPackageException = ExceptionData.From(ex);
				return PromiseBase.Unit;
			});
		}

		private static void ApplyConfigurations(BeamableDiagnosticData data)
		{
			try
			{
				data.Configurations = new List<string>();
				foreach (var config in ConfigManager.ConfigObjects.ToList())
				{
					var path = AssetDatabase.GetAssetPath(config);
					var yml = File.ReadAllText(path);
					data.Configurations.Add(yml);
				}
			}
			catch (Exception ex)
			{
				data.ConfigurationException = ExceptionData.From(ex);
			}
		}

		private static void ApplyManifestJson(BeamableDiagnosticData data)
		{
			try
			{
				data.ManifestJson = File.ReadAllText(ManifestPath);
			}
			catch (Exception ex)
			{
				data.ManifestJsonException = ExceptionData.From(ex);
			}
		}

		private static void ApplyConfigDefaults(BeamableDiagnosticData data)
		{
			try
			{
				data.ConfigDefaultsJson = File.ReadAllText(ConfigDefaultsPath);
			}
			catch (Exception ex)
			{
				data.ConfigDefaultsException = ExceptionData.From(ex);
			}
		}

		private static Promise<Unit> ApplyInstanceData(BeamableDiagnosticData data)
		{
			var api = BeamEditorContext.Default;

			return api.InitializePromise.Map(_ =>
			{
				data.Cid = api.CurrentCustomer.Cid;
				data.Pid = api.CurrentRealm.Pid;
				data.CidOrAlias = api.CurrentCustomer.Alias;
				data.EditorUser = api.CurrentUser;

				return PromiseBase.Unit;
			}).Recover(ex =>
			{
				data.InitException = ExceptionData.From(ex);
				return PromiseBase.Unit;
			});
		}

		public static Promise<BeamableDiagnosticData> Create()
		{
			var data = new BeamableDiagnosticData();
			ApplySystemInfo(data);
			ApplyPlayerSettingsInfo(data);

			try
			{
				ApplyConfigurations(data);
				ApplyManifestJson(data);
				ApplyConfigDefaults(data);
				var serverPromise = ApplyServerInfo(data);
				var instancePromise = ApplyInstanceData(data);

				return Promise.Sequence(serverPromise, instancePromise).Map(_ => data);
			}
			catch (Exception ex)
			{
				data.GlobalError = ExceptionData.From(ex);
				return Promise<BeamableDiagnosticData>.Successful(data);
			}

		}
	}

}
