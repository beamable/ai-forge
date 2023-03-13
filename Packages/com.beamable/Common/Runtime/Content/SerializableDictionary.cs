using Beamable.Content;
using Beamable.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Common.Content
{
	[Serializable] public class MapOfLong : SerializableDictionaryStringToLong { }
	[Serializable] public class OptionalMapOfLong : Optional<MapOfLong> { }
	[Serializable] public class MapOfInt : SerializableDictionaryStringToInt { }
	[Serializable] public class OptionalMapOfInt : Optional<MapOfInt> { }
	[Serializable] public class MapOfString : SerializableDictionaryStringToString { }
	[Serializable] public class MapOfStringArray : SerializableDictionaryStringToSomething<string[]> { }
	[Serializable] public class MapOfMapOfString : SerializableDictionaryStringToSomething<MapOfString> { }
	[Serializable] public class OptionalMapOfString : Optional<MapOfString> { }
	[Serializable] public class MapOfObject : SerializableDictionaryStringToObject { }
	[Serializable] public class OptionalMapOfObject : Optional<MapOfObject> { }
	[Serializable] public class MapOfBool : SerializableDictionaryStringToBool { }
	[Serializable] public class OptionalMapOfBool : Optional<MapOfBool> { }
	[Serializable] public class MapOfShort : SerializableDictionaryStringToShort { }
	[Serializable] public class OptionalMapOfShort : Optional<MapOfShort> { }
	[Serializable] public class MapOfDouble : SerializableDictionaryStringToDouble { }
	[Serializable] public class OptionalMapOfDouble : Optional<MapOfDouble> { }
	[Serializable] public class MapOfFloat : SerializableDictionaryStringToFloat { }
	[Serializable] public class OptionalMapOfFloat : Optional<MapOfFloat> { }
	[Serializable] public class MapOfByte : SerializableDictionaryStringToByte { }
	[Serializable] public class OptionalMapOfByte : Optional<MapOfByte> { }
	[Serializable] public class MapOfGuid : SerializableDictionaryStringToGuid { }
	[Serializable] public class OptionalMapOfGuid : Optional<MapOfGuid> { }
	[Serializable] public class SerializableDictionaryStringToInt : SerializableDictionaryStringToSomething<int> { }
	[Serializable] public class SerializableDictionaryStringToBool : SerializableDictionaryStringToSomething<bool> { }
	[Serializable] public class SerializableDictionaryStringToShort : SerializableDictionaryStringToSomething<short> { }
	[Serializable] public class SerializableDictionaryStringToDouble : SerializableDictionaryStringToSomething<double> { }
	[Serializable] public class SerializableDictionaryStringToFloat : SerializableDictionaryStringToSomething<float> { }
	[Serializable] public class SerializableDictionaryStringToByte : SerializableDictionaryStringToSomething<byte> { }
	[Serializable] public class SerializableDictionaryStringToGuid : SerializableDictionaryStringToSomething<Guid> { }

	[Serializable]
	public class SerializableDictionaryStringToLong : SerializableDictionaryStringToSomething<long>
	{
		public SerializableDictionaryStringToLong() { }

		public SerializableDictionaryStringToLong(IDictionary<string, long> existing) : base(existing) { }
	}

	[Serializable]
	public class OptionalSerializableDictionaryStringToString : Optional<SerializableDictionaryStringToString> { }

	[Serializable]
	public class OptionalSerializableDictionaryStringToSomething<T> : Optional<SerializableDictionaryStringToSomething<T>> { }

	[Serializable]
	public class SerializableDictionaryStringToStringArray : SerializableDictionaryStringToSomething<string[]> { }


	[Serializable]
	public class SerializableDictionaryStringToStringMap : SerializableDictionaryStringToSomething<SerializableDictionaryStringToString> { }

	[Serializable]
	public class SerializableDictionaryStringToString : SerializableDictionaryStringToSomething<string>
	{
		public SerializableDictionaryStringToString() { }

		public SerializableDictionaryStringToString(IDictionary<string, string> existing)
		{
			if (existing == null) return;
			foreach (var kvp in existing)
			{
				Add(kvp.Key, kvp.Value);
			}
		}
	}

	[Serializable]
	public class SerializableDictionaryStringToObject : SerializableDictionaryStringToSomething<object>
	{

	}

	public static class SerializableDictionaryStringToStringExtensions
	{
		public static SerializableDictionaryStringToString ToSerializable(this IDictionary<string, string> data) =>
			new SerializableDictionaryStringToString(data);
	}

	[Serializable]
	public class SerializableDictionaryStringToSomething<T> : SerializableDictionary<string, T>, IDictionaryWithValue
	{
		public Type ValueType => typeof(T);

		public SerializableDictionaryStringToSomething()
		{

		}

		public SerializableDictionaryStringToSomething(IDictionary<string, T> existing)
		{
			foreach (var kvp in existing)
			{
				Add(kvp.Key, kvp.Value);
			}
		}
	}

	public interface IDictionaryWithValue : IDictionary, IIgnoreSerializationCallbacks
	{
		Type ValueType { get; }
	}


	[Serializable]
	public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
	{
		[SerializeField] private List<TKey> keys = new List<TKey>();

		[SerializeField] private List<TValue> values = new List<TValue>();

		// save the dictionary to lists
		public void OnBeforeSerialize()
		{
			keys.Clear();
			values.Clear();
			foreach (KeyValuePair<TKey, TValue> pair in this)
			{
				keys.Add(pair.Key);
				values.Add(pair.Value);
			}
		}

		// load dictionary from lists
		public void OnAfterDeserialize()
		{
			this.Clear();

			if (keys.Count != values.Count)
				throw new System.Exception(string.Format(
				   "there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));

			for (int i = 0; i < keys.Count; i++)
				this.Add(keys[i], values[i]);
		}
	}
}
