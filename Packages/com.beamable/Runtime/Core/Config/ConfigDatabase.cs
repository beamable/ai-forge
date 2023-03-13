using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Beamable.Config
{
	public static class ConfigDatabase
	{
		private static Dictionary<string, string> database = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		private static readonly Dictionary<string, string> sessionOverrides = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		private const string ConfigFileKey = "config_file_key";

		public static void Init()
		{
			var alreadyInitialized = database != null && database.Count > 0;

			if (!alreadyInitialized)
			{
				SetConfigValuesFromFile(GetConfigFileName());
			}
		}

		public static string GetConfigFileName()
		{
			//load the file name from player prefs if it exists.
			string configFileName = PlayerPrefs.GetString(ConfigFileKey);
			if ((configFileName == null) || (configFileName == ""))
			{
				configFileName = "config-defaults";
				PlayerPrefs.SetString(ConfigFileKey, configFileName);
			}

			return configFileName;
		}

		public static TextAsset GetConfigAsset()
		{
			return Resources.Load<TextAsset>(GetConfigFileName());
		}

		public static void SetPreferredConfigFile(string configFileName)
		{
			PlayerPrefs.SetString(ConfigFileKey, configFileName);
		}

		public static bool HasConfigFile(string filename)
		{
			// this is hardly efficient, but if it is done infrequently enough, it should be fine
#if UNITY_EDITOR
			return File.Exists(GetFullPath(filename)) || Resources.Load<TextAsset>(filename) != null;
#else
			return Resources.Load<TextAsset>(filename) != null;
#endif
		}

		public static void SetConfigValuesFromFile(string fileName)
		{
			var text = GetFileContent(fileName);

			if (!(Serialization.SmallerJSON.Json.Deserialize(text) is IDictionary<string, object> dictionaryJson))
			{
				Debug.LogError("Config is invalid json");
				return;
			}

			using (var iter = dictionaryJson.GetEnumerator())
			{
				while (iter.MoveNext())
				{
					database[iter.Current.Key] = iter.Current.Value.ToString();
				}
			}

		}

		public static ICollection<string> GetAllValueNames()
		{
			return database.Keys;
		}

		public static string GetString(string name)
		{
			name = name.Trim();
			if (ConfigDatabase.database.ContainsKey(name))
			{
				if (sessionOverrides.ContainsKey(name))
				{
					return sessionOverrides[name];
				}
				return PlayerPrefs.HasKey(name) ? PlayerPrefs.GetString(name).Trim() : ConfigDatabase.database[name];
			}
			else
			{
				Debug.LogError("Could not find '" + name + "' in Config");
				throw new KeyNotFoundException();
			}
		}

		public static bool TryGetString(string name, out string value)
		{
			name = name.Trim();
			if (ConfigDatabase.database.ContainsKey(name))
			{
				if (sessionOverrides.ContainsKey(name))
				{
					value = sessionOverrides[name];
				}
				value = PlayerPrefs.HasKey(name) ? PlayerPrefs.GetString(name).Trim() : ConfigDatabase.database[name];
				return true;
			}
			value = null;
			return false;
		}

		public static bool HasKey(string key)
		{
			return database.ContainsKey(key);
		}

		public static void SetString(string name, string value, bool persist = true, bool createField = false)
		{
			if (createField || ConfigDatabase.database.ContainsKey(name))
			{
				if (!persist)
				{
					sessionOverrides[name] = value;
					return;
				}
				sessionOverrides.Remove(name);
				database[name] = value;
				PlayerPrefs.SetString(name.Trim(), value.Trim());
			}
			else
			{
				Debug.LogError("Could not find '" + name + "' in Config");
				throw new KeyNotFoundException();
			}
		}

		public static int GetInt(string name)
		{
			string strVal = GetString(name);
			try
			{
				return Int32.Parse(strVal);
			}
			catch (FormatException)
			{
				return 0;
			}
		}

		public static void SetInt(string name, int value, bool persist = true)
		{
			SetString(name, value.ToString(), persist);
		}

		public static float GetFloat(string name)
		{
			string strVal = GetString(name);
			return float.Parse(strVal);
		}

		public static void SetFloat(string name, float value, bool persist = true)
		{
			SetString(name, value.ToString(), persist);
		}

		public static bool GetBool(string name)
		{
			return Convert.ToBoolean(GetString(name));
		}

		public static bool GetBool(string name, bool defaultValue)
		{
			return HasKey(name) ? GetBool(name) : defaultValue;
		}

		public static void SetBool(string name, bool value, bool persist = true)
		{
			SetString(name, value.ToString(), persist);
		}

		public static void Reset(string name)
		{
			if (ConfigDatabase.database.ContainsKey(name))
			{
				sessionOverrides.Remove(name);
				ConfigDatabase.database.Remove(name);
				PlayerPrefs.DeleteKey(name);
			}
			else
			{
				Debug.LogError("Could not find '" + name + "' in Config");
				throw new KeyNotFoundException();
			}
		}

		public static string GetFullPath(string fileName) =>
			Path.Combine("Assets", "Beamable", "Resources", $"{fileName}.txt");


		/// <summary>
		/// Remove the config-defaults file. Calling this outside the Unity Editor has no effect.
		/// </summary>
		public static void DeleteConfigDatabase()
		{
#if !UNITY_EDITOR
			Debug.LogWarning($"Cannot call {nameof(DeleteConfigDatabase)} unless in UnityEditor. The config database will not be deleted.");
			return;
#else
			const string assetsFolder = "Assets/";
			var path = GetFullPath(GetConfigFileName());
			if (path.Length > assetsFolder.Length)
			{
				UnityEditor.FileUtil.DeleteFileOrDirectory(path);
				UnityEditor.FileUtil.DeleteFileOrDirectory(path + ".meta");
			}
#endif
		}

		private static string GetFileContent(string fileName)
		{
#if UNITY_EDITOR
			var fullPath = GetFullPath(fileName);

			if(File.Exists(fullPath))
			{
				var result = File.ReadAllText(fullPath);
				return result;
			}
#endif
			var asset = Resources.Load(fileName) as TextAsset;
			if (asset == null)
			{
				throw new FileNotFoundException("Cannot find config file in Resource directory", fileName);
			}

			return asset.text;
		}
	}
}
