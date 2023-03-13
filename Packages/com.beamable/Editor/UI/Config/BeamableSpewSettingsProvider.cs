using Beamable.Common.Spew;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Logger = UnityEngine.Logger;

namespace Beamable.Spew
{
	public class BeamableSpewSettingsProvider : SettingsProvider
	{
		private List<string> _allFlags;

		public BeamableSpewSettingsProvider(string path, SettingsScope scopes = SettingsScope.Project,
			IEnumerable<string> keywords = null) : base(path, scopes, keywords)
		{
		}

		[SettingsProvider]
		public static SettingsProvider CreateSpewSettingsProvider()
		{
			BeamableSpewSettingsProvider provider =
				new BeamableSpewSettingsProvider("Project/Beamable/Verbose Logging", SettingsScope.Project);
			provider.keywords = new HashSet<string>(new[] { "Spew" });
			return provider;
		}

		public override void OnGUI(string searchContext)
		{
			ScrapeAllFlags();

			List<string> flagState = GetFlagStates();
			for (int i = 0; i < _allFlags.Count; ++i)
			{
				string flag = _allFlags[i];
				bool state = flagState.Contains(flag);

				using (new EditorGUILayout.HorizontalScope(GUI.skin.box, GUILayout.ExpandWidth(true)))
				{
					bool newState = GUILayout.Toggle(state, flag);
					if (state != newState)
					{
						if (newState)
						{
							flagState.Add(flag);
						}
						else
						{
							flagState.Remove(flag);
						}

						SetFlagStates(flagState);
					}
				}
			}
		}

		private void SetFlagStates(List<string> flags)
		{
			var sb = new StringBuilder();
			for (int i = 0; i < flags.Count; ++i)
			{
				sb.Append(flags[i]);
				sb.Append(";");
			}

			PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, sb.ToString());
		}

		private void ScrapeAllFlags()
		{
			if (_allFlags != null) return;

			var allFlagsSet = new HashSet<string>();

			// TEMP: Scrape the Logger methods for Conditional attributes
			Type type = typeof(Logger);
			ScrapeConditionals(type, allFlagsSet);

			var taggedClasses = AttributeUtil.GetClassesInAllAssemblies<SpewLoggerAttribute>();
			for (int i = 0; i < taggedClasses.Count; ++i)
			{
				ScrapeConditionals(taggedClasses[i], allFlagsSet);
			}

			_allFlags = new List<string>(allFlagsSet);
			_allFlags.Sort();
		}

		private void ScrapeConditionals(Type type, HashSet<string> flags)
		{
			MethodInfo[] infos = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
			for (int i = 0; i < infos.Length; ++i)
			{
				object[] atr = infos[i].GetCustomAttributes(typeof(ConditionalAttribute), false);
				for (int j = 0; j < atr.Length; ++j)
				{
					var ca = atr[j] as ConditionalAttribute;
					if (ca != null)
					{
						flags.Add(ca.ConditionString);
					}
				}
			}
		}

		private List<string> GetFlagStates()
		{
			string s = PlayerSettings.GetScriptingDefineSymbolsForGroup(
				EditorUserBuildSettings.selectedBuildTargetGroup);
			string[] split = s.Split(';');
			return new List<string>(split);
		}
	}

	// This belongs in Core.Utility.AttributeUtil, however, unfinished asmdef work made this difficult.
	public static class AttributeUtil
	{
		public static List<Type> GetClassesInAllAssemblies<TAttr>(bool inherit = true) where TAttr : Attribute
		{
			return GetClassesInAssemblies<TAttr>(AppDomain.CurrentDomain.GetAssemblies(), inherit);
		}

		public static List<Type> GetClassesInAssemblies<Tattr>(IList<Assembly> assemblies, bool inherit = true)
			where Tattr : Attribute
		{
			var result = new List<Type>();

			for (int a = 0; a < assemblies.Count; ++a)
			{
				var assembly = assemblies[a];
				var types = assembly.GetTypes();
				for (int i = 0; i < types.Length; ++i)
				{
					var t = types[i];
					if (t.IsPublic) // CONSIDER: Accept BindingFlags?
					{
						var att = t.GetCustomAttributes(typeof(Tattr), inherit);
						if (att.Length > 0)
						{
							// CONSIDER: Callback form to avoid List?
							result.Add(t);
						}
					}
				}
			}

			return result;
		}
	}
}
