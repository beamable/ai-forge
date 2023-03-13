using Beamable.Avatars;
using Beamable.Editor.Config.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static Beamable.Common.Constants;

namespace Beamable.Editor.Config
{
	public class ConfigModuleDescriptor
	{
		public string Name;
		public bool Exists;
		public Func<BaseModuleConfigurationObject> Getter;
		public Type ConfigType;
	}
	public class ConfigManager
	{

		public static BaseModuleConfigurationObject[] ConfigObjects;
		public static string[] ConfigModules;
		public static List<ConfigModuleDescriptor> ConfigModuleDescriptors;

		static ConfigManager()
		{
			Initialize();
		}

		public event Action<ConfigQuery, bool> OnQueryUpdated;

		public List<ConfigOption> ConfigOptions { get; private set; }

		public ConfigQuery Query { get; set; }

		public ConfigManager()
		{
			ConfigOptions = GenerateOptions();
			Query = null;
		}

		public static IEnumerable<ConfigModuleDescriptor> MissingConfigurations => ConfigModuleDescriptors.Where(d => !d.Exists);

		public static bool MissingAnyConfiguration
		{
			get
			{
				if (ConfigModuleDescriptors == null)
				{
					ConfigModuleDescriptors = GetConfigDescriptors();
				}

				return MissingConfigurations.Any();
			}
		}

		public static List<ConfigModuleDescriptor> GetConfigDescriptors()
		{
			var descriptors = new List<ConfigModuleDescriptor>();
			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				try
				{
					var types = asm.GetTypes();


					foreach (var type in types)
					{
						var isConfigurationType = typeof(BaseModuleConfigurationObject).IsAssignableFrom(type);
						if (!isConfigurationType) continue;

						var staticInstanceProperty = type.GetProperty(nameof(AvatarConfiguration.Instance),
						   BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);


						var hasInstanceProperty = staticInstanceProperty != null && staticInstanceProperty.CanRead;
						if (!hasInstanceProperty) continue;

						MethodInfo staticExistenceGenericMethod = null;
						var searchType = type.BaseType;
						while (staticExistenceGenericMethod == null)
						{
							staticExistenceGenericMethod = searchType.GetMethod(nameof(AvatarConfiguration.Exists),
							   BindingFlags.Static | BindingFlags.Public);
							searchType = searchType.BaseType;
						}

						var staticExistenceMethod = staticExistenceGenericMethod.MakeGenericMethod(type);
						var exists = (bool)staticExistenceMethod.Invoke(null, new object[] { });


						var descriptor = new ConfigModuleDescriptor
						{
							ConfigType = type,
							Name = type.Name,
							Exists = exists,
							Getter = () => staticInstanceProperty.GetValue(null) as BaseModuleConfigurationObject
						};
						descriptors.Add(descriptor);
					}
				}
				catch (ReflectionTypeLoadException ex)
				{
					Debug.LogError($"Unable to load asm {asm.FullName} and scan for configurations. {ex.Message}\n{ex.StackTrace}");
				}
			}

			return descriptors;

		}

		public static void Initialize(bool forceCreation = false)
		{
			ConfigModuleDescriptors = GetConfigDescriptors();
			BaseModuleConfigurationObject.PrepareInstances(ConfigModuleDescriptors.Select(c => c.ConfigType).ToArray())
			   .Then(
				  _ =>
				  {
					  ConfigObjects = ConfigModuleDescriptors.Where(d => forceCreation || d.Exists).Select(d => d.Getter())
					  .ToArray();
					  ConfigModules = ConfigObjects.Select(c => c.GetType().Name.Replace("Configuration", "")).ToArray();
				  });
		}

		public void Destroy()
		{

		}

		public static List<ConfigOption> GenerateOptions()
		{
			return ConfigObjects.SelectMany(GenerateOptions).ToList();
		}

		public static List<ConfigOption> GenerateOptions(BaseModuleConfigurationObject obj)
		{
			var output = new List<ConfigOption>();
			var serialized = new SerializedObject(obj);
			var iter = serialized.GetIterator();
			iter.Next(true);

			var asmName = typeof(PropertyDrawer).AssemblyQualifiedName;
			var t2 = Type.GetType(asmName.Replace("UnityEditor.PropertyDrawer", "UnityEditor.ScriptAttributeUtility"));
			var getFieldMethod = t2.GetMethod("GetFieldInfoFromProperty", BindingFlags.Static | BindingFlags.NonPublic);

			while (iter.Next(false))
			{
				var likelyUnityInternal = iter.name.StartsWith("m_"); // TODO: Temporary solution to not showing Unity internal properties
				if (likelyUnityInternal) continue;

				var parameters = new[] { iter, null };
				var fieldObj = (FieldInfo)getFieldMethod.Invoke(null, parameters);
				var help = iter.tooltip;
				if (fieldObj != null)
				{
					var tooltipAttr = fieldObj.GetCustomAttribute<TooltipAttribute>();
					if (tooltipAttr != null)
					{
						help = tooltipAttr.tooltip;
					}

					var hideInInspector = fieldObj.GetCustomAttribute<HideInInspector>() != null;
					if (hideInInspector)
					{
						continue;
					}
				}
				output.Add(new ConfigOption(serialized, obj, iter.Copy(), help));
			}
			return output;
		}

		public void SetFilter(ConfigQuery query)
		{
			var changed = !query.ToString().Equals(Query?.ToString());

			Query = ConfigQuery.Parse(query.ToString());

			if (changed)
			{
				OnQueryUpdated?.Invoke(Query, false);
			}

		}

		public void ShowDocs()
		{
			Application.OpenURL(URLs.Documentations.URL_DOC_WINDOW_CONFIG_MANAGER);
		}

		public void ToggleModuleFilter(string module, bool shouldFilterOn)
		{
			var tagExistsInFilter = Query?.ModuleConstraint?.Contains(module) ?? false;
			var next = new ConfigQuery(Query);

			if (tagExistsInFilter && !shouldFilterOn)
			{
				if (next.ModuleConstraint.Count == 1)
				{
					next.ModuleConstraint = null;
				}
				else
				{
					next.ModuleConstraint.Remove(module);
				}
				SetFilter(next);
				OnQueryUpdated?.Invoke(Query, true);

			}
			else if (!tagExistsInFilter && shouldFilterOn)
			{
				var tagConstraints = next.ModuleConstraint ?? new HashSet<string>();
				tagConstraints.Add(module);

				next.ModuleConstraint = tagConstraints;
				SetFilter(next);
			}
		}
	}
}
