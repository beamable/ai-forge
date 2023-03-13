using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Beamable.Editor
{
	public static class PlayerSettingsHelper
	{
		public static HashSet<string> GetDefines()
		{
			var definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
			var allDefines = definesString.Split(';').ToList();
			return new HashSet<string>(allDefines);
		}

		public static void SetDefines(HashSet<string> allDefines)
		{
			PlayerSettings.SetScriptingDefineSymbolsForGroup(
			   EditorUserBuildSettings.selectedBuildTargetGroup,
			   string.Join(";", allDefines.ToArray()));
		}

		public static void EnableFlag(string flag)
		{
			var allDefines = GetDefines();

			if (allDefines.Contains(flag)) return;

			allDefines.Add(flag);
			SetDefines(allDefines);
		}

		public static void DisableFlag(string flag)
		{
			var allDefines = GetDefines();
			if (!allDefines.Contains(flag)) return;

			allDefines.Remove(flag);
			SetDefines(allDefines);
		}

	}
}
