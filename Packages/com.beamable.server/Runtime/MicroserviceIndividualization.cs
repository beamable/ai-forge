using Beamable.Config;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Server
{
	public static class MicroserviceIndividualization
	{
		private const string PlayerPrefPrefix = "BeamableMicroservicePrefixes";

		public static string Prefix
		{
			get
			{
				if (ConfigDatabase.TryGetString("containerPrefix", out var customPrefix) &&
					!string.IsNullOrWhiteSpace(customPrefix))
					return customPrefix;
				return SystemInfo.deviceUniqueIdentifier;
			}
		}

		public static void UseServicePrefix(string serviceName)
		{
			var values = GetValues();
			if (!values.TryGetValue(serviceName, out var existing))
			{
				values[serviceName] = Prefix;
				SetValues(values);
			}
		}

		public static void ClearServicePrefix(string serviceName)
		{
			var values = GetValues();
			values.Remove(serviceName);
			SetValues(values);
		}

		public static string GetServicePrefix(string serviceName)
		{
#if !UNITY_EDITOR
			return ""; // if we aren't in the editor, never ever use a service prefix.
#else
         var prefix = "";
         GetValues().TryGetValue(serviceName, out prefix);
         return prefix;
#endif
		}

		static void SetValues(Dictionary<string, string> values)
		{
			var cid = ConfigDatabase.GetString("cid");
			var pid = ConfigDatabase.GetString("pid");
			var key = $"{PlayerPrefPrefix}.{cid}.{pid}";

			var parts = new List<string>();
			foreach (var kvp in values)
			{
				parts.Add($"{kvp.Key}={kvp.Value}");
			}

			var str = string.Join(";", parts);
			PlayerPrefs.SetString(key, str);
		}

		static Dictionary<string, string> GetValues()
		{
			var cid = ConfigDatabase.GetString("cid");
			var pid = ConfigDatabase.GetString("pid");
			var key = $"{PlayerPrefPrefix}.{cid}.{pid}";
			var raw = PlayerPrefs.GetString(key);
			var services = raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			var output = new Dictionary<string, string>();
			foreach (var service in services)
			{
				var parts = service.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
				output.Add(parts[0], parts[1]);
			}
			return output;
		}

	}
}
