using Beamable.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static Beamable.Common.Constants.Features.Config;

namespace Beamable
{
	public interface IConfigurationConstants
	{

		string GetSourcePath(Type type);
	}

	public class BeamableConfigurationConstants : IConfigurationConstants
	{
		public string GetSourcePath(Type type)
		{
			var name = type.Name;
			var sourcePath = $"{BASE_PATH}/{name}.asset";
			return sourcePath;
		}
	}

	public abstract class BaseModuleConfigurationObject : ScriptableObject
	{
		protected const string CONFIG_RESOURCES_DIR = "Assets/Beamable/Resources";

		/// <summary>
		/// Called by the Configuration.Instance spawn function, the FIRST time the configuration is copied from the Beamable package into the /Assets
		/// </summary>
		public virtual void OnFreshCopy()
		{

		}

#if UNITY_EDITOR
      public static Promise PrepareInstances(params Type[] configTypes)
      {
         var writtenAssetPathToType = new Dictionary<string, Type>();
         var promise = new Promise();

         try
         {
            UnityEditor.AssetDatabase.StartAssetEditing();
            foreach (var type in configTypes)
            {
               var name = type.Name;
               var data = Resources.Load(name, type);

               if (data != null) continue;

               var assetPath = $"{CONFIG_RESOURCES_DIR}/{name}.asset";
               if (File.Exists(assetPath)) continue;

               MethodInfo FindStaticParentMethod(Type searchType)
               {
                  if (searchType == null || searchType == typeof(object)) return null;
                  var constantsGeneratorMethod = searchType.GetMethod("GetStaticConfigConstants", BindingFlags.Static | BindingFlags.Public);
                  return constantsGeneratorMethod ?? FindStaticParentMethod(searchType.BaseType);
               }

               var method = FindStaticParentMethod(type);
               var constants = method?.Invoke(null, new object[] { }) as IConfigurationConstants;

               // var constants = new TConstants();
               var sourcePath = constants.GetSourcePath(type);
               Directory.CreateDirectory(CONFIG_RESOURCES_DIR);
               var sourceData = File.ReadAllText(sourcePath);
               File.WriteAllText(assetPath, sourceData);
               writtenAssetPathToType.Add(assetPath, type);
               UnityEditor.AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
            }
         }
         finally
         {
            UnityEditor.AssetDatabase.StopAssetEditing();
            UnityEditor.AssetDatabase.SaveAssets();
         }

         var failedTypes = new HashSet<Type>();
         foreach (var kvp in writtenAssetPathToType)
         {
            var assetPath = kvp.Key;
            var assetType = kvp.Value;

            var data = Resources.Load(assetType.Name, assetType) ?? UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, assetType);
            var configData = data as BaseModuleConfigurationObject;
            if (configData == null)
            {
               failedTypes.Add(assetType);
               continue;
            }
            configData.OnFreshCopy();
            UnityEditor.EditorUtility.SetDirty(data);
         }

         if (failedTypes.Count > 0)
         {
            EditorApplication.delayCall += () =>
            {
               PrepareInstances(configTypes)
                  .Then(promise.CompleteSuccess)
                  .Error(promise.CompleteError);
            };
            return promise;
         }
         if (writtenAssetPathToType.Count > 0)
         {
            UnityEditor.SettingsService.NotifySettingsProviderChanged();
         }

         promise.CompleteSuccess(PromiseBase.Unit);
         return promise;
      }
#endif
	}

	public abstract class AbsModuleConfigurationObject<TConstants> : BaseModuleConfigurationObject
	   where TConstants : IConfigurationConstants, new()
	{
		private static Dictionary<Type, BaseModuleConfigurationObject> _typeToConfig = new Dictionary<Type, BaseModuleConfigurationObject>();

		public static bool Exists<TConfig>() where TConfig : BaseModuleConfigurationObject
		{
			var type = typeof(TConfig);
			if (_typeToConfig.TryGetValue(type, out var existingData) && existingData && existingData != null)
			{
				return true;
			}

			var name = type.Name;
			var data = Resources.Load<TConfig>(name);
			return data != null;
		}

		public static IConfigurationConstants GetStaticConfigConstants() => new TConstants();

		public static TConfig Get<TConfig>() where TConfig : BaseModuleConfigurationObject
		{
			var type = typeof(TConfig);
			if (_typeToConfig.TryGetValue(type, out var existingData) && existingData && existingData != null)
			{
				return existingData as TConfig;
			}

			var constants = new TConstants();
			var name = type.Name;

			var data = Resources.Load<TConfig>(name);
#if UNITY_EDITOR
         if (data == null)
         {

            var sourcePath = constants.GetSourcePath(type);

            if (!File.Exists(sourcePath))
            {
               throw new Exception($"No module configuration exists at {sourcePath}. Please create it.");
            }

            Directory.CreateDirectory(CONFIG_RESOURCES_DIR);
            var assetPath = $"{CONFIG_RESOURCES_DIR}/{name}.asset";
            var sourceData = File.ReadAllText(sourcePath);
            File.WriteAllText(assetPath, sourceData);
            UnityEditor.AssetDatabase.ImportAsset(assetPath, UnityEditor.ImportAssetOptions.DontDownloadFromCacheServer);
            data =  Resources.Load<TConfig>(name) ?? UnityEditor.AssetDatabase.LoadAssetAtPath<TConfig>(assetPath);
            if (data == null)
            {
               throw new ModuleConfigurationNotReadyException(typeof(TConfig));
            }
            data.OnFreshCopy();

            UnityEditor.EditorUtility.SetDirty(data);
            UnityEditor.SettingsService.NotifySettingsProviderChanged();
         }
#endif
			_typeToConfig[type] = data;
			return data;
		}

	}

	public class ModuleConfigurationNotReadyException : Exception
	{
		public ModuleConfigurationNotReadyException(Type type) : base($"Configuration of type=[{type.Name}] is not available yet.")
		{

		}

	}


	public class ModuleConfigurationObject : AbsModuleConfigurationObject<BeamableConfigurationConstants>
	{

	}

}
