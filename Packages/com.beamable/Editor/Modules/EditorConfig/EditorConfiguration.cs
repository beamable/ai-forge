using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.Modules.EditorConfig
{
	public class EditorConfiguration : ModuleConfigurationObject
	{
		public static EditorConfiguration Instance => Get<EditorConfiguration>();

		public const string BEAMABLE_DISABLE_CONTENT_INSPECTOR = "BEAMABLE_NO_CONTENT_INSPECTOR";
		public const string BEAMABLE_DISABLE_OPTIONAL_PROPERTYDRAWERS = "BEAMABLE_NO_OPTIONAL_DRAWERS";
		public const string BEAMABLE_DISABLE_CONTENT_VALIDATION_PROPERTYDRAWERS = "BEAMABLE_NO_VALIDATION_DRAWERS";
		public const string BEAMABLE_DISABLE_CONTENT_REF_PROPERTYDRAWERS = "BEAMABLE_NO_REF_DRAWERS";
		public const string BEAMABLE_DISABLE_DICT_PROPERTYDRAWERS = "BEAMABLE_NO_DICT_DRAWERS";
		public const string BEAMABLE_DISABLE_LIST_PROPERTYDRAWERS = "BEAMABLE_NO_LIST_DRAWERS";
		public const string BEAMABLE_DISABLE_DATE_STRING_PROPERTYDRAWERS = "BEAMABLE_NO_DATE_STRING_DRAWERS";

		public const string BEAMABLE_DEVELOPER = "BEAMABLE_DEVELOPER";

		[Tooltip("These are advanced Beamable editor features. Please only use these if you are confident in what you are doing.")]
		public AdvancedSettings Advanced = new AdvancedSettings();

		public override void OnFreshCopy()
		{
			var existing = GetDefineSymbols();
			Advanced.DisableOptionalPropertyDrawers = existing.Contains(BEAMABLE_DISABLE_OPTIONAL_PROPERTYDRAWERS);
			Advanced.RunAsBeamableDeveloper = existing.Contains(BEAMABLE_DEVELOPER);
			Advanced.DisableValidationPropertyDrawers = existing.Contains(BEAMABLE_DISABLE_CONTENT_VALIDATION_PROPERTYDRAWERS);
			Advanced.DisableContentInspector = existing.Contains(BEAMABLE_DISABLE_CONTENT_INSPECTOR);
			Advanced.DisableBeamableDictionaryPropertyDrawers = existing.Contains(BEAMABLE_DISABLE_DICT_PROPERTYDRAWERS);
			Advanced.DisableBeamableListPropertyDrawers = existing.Contains(BEAMABLE_DISABLE_LIST_PROPERTYDRAWERS);
			Advanced.DisableContentRefPropertyDrawers = existing.Contains(BEAMABLE_DISABLE_CONTENT_REF_PROPERTYDRAWERS);
			Advanced.DisableBeamableDateStringPropertyDrawers = existing.Contains(BEAMABLE_DISABLE_DATE_STRING_PROPERTYDRAWERS);
		}



#if UNITY_EDITOR

      private double _debounceUpdateAt = 0;

      private void OnValidate()
      {

         _debounceUpdateAt = EditorApplication.timeSinceStartup + 1.5;
         void Check()
         {
            if (EditorApplication.timeSinceStartup > _debounceUpdateAt)
            {
               _debounceUpdateAt = EditorApplication.timeSinceStartup + 1.5;
               ApplyProjectSettings();
            }
            else
            {
               EditorApplication.delayCall += Check;
            }
         }
         EditorApplication.delayCall += Check;

      }

      void OnEnable()
      {
         AssemblyReloadEvents.beforeAssemblyReload += OnReload;
         AssemblyReloadEvents.afterAssemblyReload += OnReload;
      }

      void OnDisable()
      {
         AssemblyReloadEvents.beforeAssemblyReload -= OnReload;
         AssemblyReloadEvents.afterAssemblyReload -= OnReload;
      }
      public void OnReload()
      {
         OnFreshCopy();
      }
#endif

		public void ApplyProjectSettings()
		{
			var symbols = GetDefineSymbols();
			var nextSymbols = new HashSet<string>(symbols);
			var settings = new Dictionary<string, bool>
		 {
			{BEAMABLE_DISABLE_OPTIONAL_PROPERTYDRAWERS, Advanced.DisableOptionalPropertyDrawers},
			{BEAMABLE_DISABLE_CONTENT_VALIDATION_PROPERTYDRAWERS, Advanced.DisableValidationPropertyDrawers},
			{BEAMABLE_DISABLE_CONTENT_REF_PROPERTYDRAWERS, Advanced.DisableContentRefPropertyDrawers},
			{BEAMABLE_DISABLE_DICT_PROPERTYDRAWERS, Advanced.DisableBeamableDictionaryPropertyDrawers},
			{BEAMABLE_DISABLE_CONTENT_INSPECTOR, Advanced.DisableContentInspector},
			{BEAMABLE_DISABLE_LIST_PROPERTYDRAWERS, Advanced.DisableBeamableListPropertyDrawers},
			{BEAMABLE_DEVELOPER, Advanced.RunAsBeamableDeveloper},
			{BEAMABLE_DISABLE_DATE_STRING_PROPERTYDRAWERS, Advanced.DisableBeamableDateStringPropertyDrawers},
		 };

			foreach (var kvp in settings)
			{
				var existingValue = nextSymbols.Contains(kvp.Key);
				if (existingValue && !kvp.Value)
				{
					nextSymbols.Remove(kvp.Key);
				}
				else if (!existingValue && kvp.Value)
				{
					nextSymbols.Add(kvp.Key);
				}
			}

			if (!nextSymbols.SetEquals(symbols))
			{
				SetDefineSymbols(nextSymbols);
			}
		}


		public static HashSet<string> GetDefineSymbols()
		{
			var symbolString = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
			var symbols = symbolString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			return new HashSet<string>(symbols);
		}

		public static void SetDefineSymbols(HashSet<string> symbols)
		{
			PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, string.Join(";", symbols));
		}

		[Serializable]
		public class AdvancedSettings
		{

			[Tooltip("[Danger] Beamable overrides the Inspector for any ContentObject class. If you wish to disable Beamable content inspector features, use this property.")]
			public bool DisableContentInspector;

			[Tooltip("[Danger] Beamable provides an Optional type class, and a custom property drawer to edit instances of that Operational type. If you wish to disable the custom visualization, use this property.")]
			public bool DisableOptionalPropertyDrawers;

			[Tooltip("[Danger] Beamable provides content validation inline in the inspector via a custom property drawer. If you wish to disable the custom validation visualization, use this property.")]
			public bool DisableValidationPropertyDrawers;

			[Tooltip("[Danger] Beamable provides a dropdown UX for picking ContentRef objects through a custom property drawer. If you wish to disable this, use this property.")]
			public bool DisableContentRefPropertyDrawers;

			[Tooltip("[Danger] Beamable provides a dictionary style object with a custom property drawer. if you wish to disable the property drawer, use this property.")]
			public bool DisableBeamableDictionaryPropertyDrawers;

			[Tooltip("[Danger] Beamable provides a list style object with a custom property drawer. if you wish to disable the property drawer, use this property.")]
			public bool DisableBeamableListPropertyDrawers;

			[Tooltip("[Danger] Internal Beamable Developers use this flag to enable preview features or system shortcuts. If you use this, expect subtle undocumented behaviour changes.")]
			public bool RunAsBeamableDeveloper;

			[Tooltip("[Danger] Beamable provides a date-string custom property drawer. if you wish to disable the property drawer, use this property.")]
			public bool DisableBeamableDateStringPropertyDrawers;

		}
	}
}
