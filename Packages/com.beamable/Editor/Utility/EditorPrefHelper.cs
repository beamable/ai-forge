using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Beamable.Editor
{
	public static class EditorPrefHelper
	{
		public const string DEFAULT_DELIMITER = ";";

		public static bool TryGetValue<TValue>(this EditorPrefItemSet<KeyValuePair<string, TValue>> self, string key, out TValue value)
		{
			value = default;

			var existing = self.FirstOrDefault(kvp => string.Equals(kvp.Key, key));
			if (string.IsNullOrEmpty(existing.Key)) return false;

			value = existing.Value;
			return true;
		}

		public static EditorPrefItemSet<KeyValuePair<string, TValue>> Set<TValue>(this EditorPrefItemSet<KeyValuePair<string, TValue>> self, string key, TValue value)
		{
			self.RemoveWhere(kvp => string.Equals(kvp.Key, key));
			self.Add(new KeyValuePair<string, TValue>(key, value));
			return self;
		}

		public static EditorPrefItemSet<string> GetStrings(string key, string delimiter = DEFAULT_DELIMITER)
		   => GetItems<string>(key, delimiter, x => x);

		public static EditorPrefItemSet<KeyValuePair<string, string>> GetMap(string key, string elementDelimiter = "|",
		   string delimiter = DEFAULT_DELIMITER)
		{
			KeyValuePair<string, string> Deserializer(string raw)
			{
				var parts = raw.Split(new[] { elementDelimiter }, StringSplitOptions.RemoveEmptyEntries);
				return new KeyValuePair<string, string>(parts[0], parts[1]);
			}
			string Serializer(KeyValuePair<string, string> kvp) => $"{kvp.Key}{elementDelimiter}{kvp.Value}";

			return GetItems(key, delimiter, Deserializer, Serializer);
		}


		public static EditorPrefItemSet<T> GetItems<T>(string key, string delimiter = DEFAULT_DELIMITER, Func<string, T> deserializer = null, Func<T, string> serializer = null)
		{
			var raw = EditorPrefs.GetString(key, "");
			var parts = raw.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);

			if (deserializer == null)
			{
				deserializer = x => (T)Convert.ChangeType(x, typeof(T));
			}
			var items = parts.Select(deserializer).Where(x => x != null);
			return new EditorPrefItemSet<T>(key, delimiter, items, serializer);
		}

		public static void SetItems<T>(string key, HashSet<T> items, string delimiter = DEFAULT_DELIMITER, Func<T, string> serializer = null)
		{
			if (serializer == null)
			{
				serializer = x => x?.ToString();
			}
			var serializedItems = items.Select(serializer).Where(x => !string.IsNullOrEmpty(x));
			var combined = string.Join(delimiter, serializedItems);

			EditorPrefs.SetString(key, combined);
		}

		public class EditorPrefItemSet<T> : HashSet<T>
		{
			public string Key { get; }
			public string Delimiter { get; }
			public Func<T, string> Serializer { get; }

			public EditorPrefItemSet(string key, string delimiter, IEnumerable<T> items, Func<T, string> serializer = null) : base(items)
			{
				Key = key;
				Delimiter = delimiter;
				Serializer = serializer;
			}

			public new EditorPrefItemSet<T> Add(T item)
			{
				base.Add(item);
				return this;
			}


			public EditorPrefItemSet<T> Save(Func<T, string> serializer = null)
			{
				EditorPrefHelper.SetItems(Key, this, Delimiter, serializer ?? Serializer);
				return this;
			}
		}
	}
}
