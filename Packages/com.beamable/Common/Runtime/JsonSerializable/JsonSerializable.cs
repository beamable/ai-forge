#if UNITY_2018_1_OR_NEWER || BEAMABLE_ENABLE_UNITY_SERIALIZATION_TYPES
#define BEAMABLE_ENABLE_UNITY_SERIALIZATION_TYPES
#endif

//
// @2021 Beamable
// Creator : Jason Booth
//

// Serialization of classes to and from Json, with merge or replace modes for IList<T> types.
//
// Currently supports:
// primitives
// ISerializable
// List<T> where T is ISerializable/primitive and new()
// Array[] where elements are ISerializable/primitive and new()
// Rect, gradients, quats, vectors, animation curves, enums
//    - Note: these are currently not supported in List<> or Array[]
//
//    Long ago, this class performed automatic serialization via reflection. Life was simpler
// back then. However, reflection is slow; GetValue and SetValue are 59 times slower than
// native field access. So now we manually serialize everything with a serialize function,
// which is much, much faster.
//
// To use, inherit ISerializable and impliment Serialize(IStreamSerializer s).
//
// You can check to see if your saving or loading on the IStreamSerializer and even prefetch data
// Serialize most things with s.Serialize(key, ref value);
//
// Types which do not impliment a standard new opperator must use SerializeInline, and must
// ensure that the passed in value reference is never null. Array[] follows this pattern as well,
// because we need to know the type to be able to use reflection to create new entries in the array.
//
// List<T> supports merge operations, give the T has a field called id used to identify which
// objects are new or changed in the list.

using Beamable.Common;
using Beamable.Common.Pooling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

#if BEAMABLE_ENABLE_UNITY_SERIALIZATION_TYPES
using UnityEngine;
#endif

namespace Beamable.Serialization
{
	// public static class Extensions
	// {
	// 	public static void SerializeDictionary<TDict, TElem>(this JsonSerializable.IStreamSerializer serializer, string key, ref TDict dict)
	// 		where TDict : IDictionary<string, TElem>
	// 	{
	// 		serializer.SerializeDictionary<string>(key, ref dict);
	// 	}
	// }

	public partial class JsonSerializable
	{
		// interface for all serializers
		public interface IStreamSerializer
		{
			bool isSaving { get; }
			bool isLoading { get; }
			object GetValue(string key);
			void SetValue(string key, object value);
			bool HasKey(string key);
			JsonSerializable.ListMode Mode { get; }

			bool Serialize(string key, ref IDictionary<string, object> target);
			bool Serialize(string key, ref bool target);
			bool Serialize(string key, ref bool? target);
			bool Serialize(string key, ref int target);
			bool Serialize(string key, ref int? target);
			bool Serialize(string key, ref long target);
			bool Serialize(string key, ref long? target);
			bool Serialize(string key, ref ulong target);
			bool Serialize(string key, ref ulong? target);
			bool Serialize(string key, ref float target);
			bool Serialize(string key, ref float? target);
			bool Serialize(string key, ref double target);
			bool Serialize(string key, ref double? target);
			bool Serialize(string key, ref string target);
			bool Serialize(string key, ref Guid target);
			bool Serialize(string key, ref StringBuilder target);
#if BEAMABLE_ENABLE_UNITY_SERIALIZATION_TYPES
			bool Serialize(string key, ref DateTime target);
			bool Serialize(string key, ref Rect target);
			bool Serialize(string key, ref Vector2 target);
			bool Serialize(string key, ref Vector3 target);
			bool Serialize(string key, ref Vector4 target);
			bool Serialize(string key, ref Color target);
			bool Serialize(string key, ref Quaternion target);
			bool Serialize(string key, ref Gradient target);
#endif
			bool Serialize<T>(string key, ref T value) where T : class, ISerializable, new();
			bool SerializeInline<T>(string key, ref T value) where T : ISerializable;
			bool SerializeList<TList>(string key, ref TList value) where TList : IList, new();
			bool SerializeKnownList<TElem>(string key, ref List<TElem> value) where TElem : ISerializable, new();
			bool SerializeArray<T>(string key, ref T[] value);

			bool SerializeDictionary<T>(string key, ref Dictionary<string, T> target);

			bool SerializeDictionary<TDict, TElem>(string key, ref TDict target) where TDict : IDictionary<string, TElem>, new();

			bool SerializeILL<T>(string key, ref LinkedList<T> list) where T : ClassPool<T>, new();
		}



		/*
         * The AOT compiler on IOS does not generate code for SerializeDictionary<T> where T is a primitive type,
         * So, we have a public static function which returns a value which forces it to generate the code for
         * the common types we need. This code is never actually called. Ugh..
         */

		public static int FixAOTCompileIssues()
		{
			Dictionary<string, int> ints = new Dictionary<string, int>();
			Dictionary<string, float> floats = new Dictionary<string, float>();
			Dictionary<string, bool> bools = new Dictionary<string, bool>();
			Dictionary<string, long> longs = new Dictionary<string, long>();
			Dictionary<string, double> doubles = new Dictionary<string, double>();
			int[] intArray = null;
			float[] floatArray = null;
			bool[] boolArray = null;
			long[] longArray = null;
			double[] doubleArray = null;

			LoadStream ls = LoadStream.Spawn();
			ls.Init(null, ListMode.kMerge);
			ls.SerializeDictionary<int>("blah", ref ints);
			ls.SerializeDictionary<float>("blah", ref floats);
			ls.SerializeDictionary<bool>("blah", ref bools);
			ls.SerializeDictionary<long>("blah", ref longs);
			ls.SerializeDictionary<double>("blah", ref doubles);
			ls.SerializeArray("blah", ref intArray);
			ls.SerializeArray("blah", ref floatArray);
			ls.SerializeArray("blah", ref boolArray);
			ls.SerializeArray("blah", ref longArray);
			ls.SerializeArray("blah", ref doubleArray);
			ls.Recycle();

			DeleteStream ds = DeleteStream.Spawn();
			ds.Init(null);
			ds.SerializeDictionary<int>("blah", ref ints);
			ds.SerializeDictionary<float>("blah", ref floats);
			ds.SerializeDictionary<bool>("blah", ref bools);
			ls.SerializeDictionary<long>("blah", ref longs);
			ls.SerializeDictionary<double>("blah", ref doubles);
			ds.SerializeArray("blah", ref intArray);
			ds.SerializeArray("blah", ref floatArray);
			ds.SerializeArray("blah", ref boolArray);
			ls.SerializeArray("blah", ref longArray);
			ls.SerializeArray("blah", ref doubleArray);
			ds.Recycle();

			SaveStream ss = SaveStream.Spawn();
			ss.Init(null);
			ss.SerializeDictionary<int>("blah", ref ints);
			ss.SerializeDictionary<float>("blah", ref floats);
			ss.SerializeDictionary<bool>("blah", ref bools);
			ls.SerializeDictionary<long>("blah", ref longs);
			ls.SerializeDictionary<double>("blah", ref doubles);
			ss.SerializeArray("blah", ref intArray);
			ss.SerializeArray("blah", ref floatArray);
			ss.SerializeArray("blah", ref boolArray);
			ls.SerializeArray("blah", ref longArray);
			ls.SerializeArray("blah", ref doubleArray);
			ss.Recycle();

			return ints.Count + bools.Count;
		}


		// how should a list<T> be handled?
		public enum ListMode
		{
			kMerge = 0,  // any matching items are replaced, new items inserted
			kReplace,    // the entire list is replaced with a new list
			kRead,       // Any matching items are read out (for saving).
			kDelete,     // Any matching items are deleted
			kSoftReplace // any matching items are updated, no longer extant elements in a list are deleted
		}


		public interface IDeletable
		{
			void AfterDelete();
		}

		public interface ISerializable
		{
			void Serialize(IStreamSerializer s);
		}

		public interface ISerializeIdentifiable : ISerializable
		{
			long SerializationID { get; }
		}

		public static void Deserialize(ISerializable obj, IDictionary<string, object> data, ListMode mode = ListMode.kReplace)
		{
			LoadStream ls = LoadStream.Spawn();
			ls.Init(data, mode);
			obj.Serialize(ls);
			ls.Recycle();
		}

		/// <summary>
		/// Deserializes a JSON string into an ISerializable object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="json"></param>
		/// <returns></returns>
		public static T FromJson<T>(string json) where T : ISerializable, new()
		{
			var data = SmallerJSON.Json.Deserialize(json) as IDictionary<string, object>;
			if (data == null)
			{
				BeamableLogger.LogError($"Could not deserialize json into type {typeof(T).FullName}: {json}");
				return default;
			}
			else
			{
				LoadStream ls = LoadStream.Spawn();
				ls.Init(data, ListMode.kReplace);
				T obj = new T();
				obj.Serialize(ls);
				ls.Recycle();

				return obj;
			}
		}

		public static void Delete(ISerializable obj, IDictionary<string, object> data)
		{
			DeleteStream stream = DeleteStream.Spawn();
			stream.Init(data);
			obj.Serialize(stream);
			stream.Recycle();
		}

		public static Dictionary<string, object> Serialize(ISerializable obj)
		{
			Dictionary<string, object> dict = new Dictionary<string, object>();
			SaveStream ss = SaveStream.Spawn();
			ss.Init(dict);
			obj.Serialize(ss);
			ss.Recycle();
			return dict;
		}

		public static string ToJson(ISerializable obj)
		{
			using (var jsonSaveStream = JsonSaveStream.Spawn())
			{
				jsonSaveStream.Init(JsonSaveStream.JsonType.Object);
				obj.Serialize(jsonSaveStream);
				jsonSaveStream.Conclude();
				return jsonSaveStream.ToString();
			}
		}

		public static string ToSmallerJson(ISerializable obj)
		{
			using (var pooledBuilder = StringBuilderPool.StaticPool.Spawn())
			{
				Dictionary<string, object> data = Serialize(obj);
				return SmallerJSON.Json.Serialize(data, pooledBuilder.Builder);
			}
		}
	}

	public static class ISerializableExtension
	{
		public static T Copy<T>(this T from) where T : JsonSerializable.ISerializable, new()
		{
			T c = new T();
			JsonSerializable.Deserialize(c, JsonSerializable.Serialize(from));
			return c;
		}

		public delegate string EnumToStringer<T>(T enumValue);
		public delegate T EnumFromStringer<T>(string strValue);

		public static bool SerializeEnum<T>(this JsonSerializable.IStreamSerializer s, string key, ref T value, EnumToStringer<T> toStringer, EnumFromStringer<T> fromStringer)
		{
			if (s.isLoading)
			{
				if (s.HasKey(key))
				{
					if (!(s.GetValue(key) is string current))
					{
						return false;
					}

					value = fromStringer(current);
					return true;
				}
			}
			else if (s.isSaving)
			{
				var strVal = toStringer(value);
				return s.Serialize(key, ref strVal);
			}
			return false;
		}

		// if this was in the main interface, AOT compiler would try to JIT compile the types..
		public static bool SerializeEnum<T>(this JsonSerializable.IStreamSerializer s, string key, ref T value)
		{
			if (s.isLoading)
			{
				if (s.HasKey(key))
				{
					var current = s.GetValue(key);
					if (value == null)
					{
						return false;
					}

					var asString = current as String;
					if (asString != null)
					{
						try
						{
							value = (T)(Enum.Parse(typeof(T), asString, true));
							return true;
						}
						catch (Exception e)
						{
							BeamableLogger.LogError("Could not parse enum : " + key + " " + asString + "\n" + e.Message);
						}
					}
					else
					{
						value = (T)Enum.ToObject(typeof(T), current);
					}
				}
			}
			else if (s.isSaving)
			{
				s.SetValue(key, value.ToString());
				return true;
			}
			return false;
		}

		public static bool SerializeBitfield<T>(this JsonSerializable.IStreamSerializer s, string key, ref T value)
		{
			int i = (int)Convert.ToInt32(value);
			bool ret = s.Serialize(key, ref i);
			if (ret && s.isLoading)
				value = (T)Enum.ToObject(typeof(T), i);
			return ret;
		}
	}
}
