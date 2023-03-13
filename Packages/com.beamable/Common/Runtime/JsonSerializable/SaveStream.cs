#if UNITY_2018_1_OR_NEWER || BEAMABLE_ENABLE_UNITY_SERIALIZATION_TYPES
#define BEAMABLE_ENABLE_UNITY_SERIALIZATION_TYPES
#endif

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
	public partial class JsonSerializable
	{
		/// <summary>
		/// Class <c>SaveStream</c> is an IStreamSerializer for JsonSerializable
		/// Provides serialization of ISerializable objects to Dictionary(string, object)
		/// </summary>
		public class SaveStream : ClassPool<SaveStream>, IStreamSerializer
		{
			public void Init(Dictionary<string, object> dict)
			{
				curDict = dict;
			}
			public override void OnRecycle()
			{
				curDict = null;
			}

			public object GetValue(string key) { return curDict[key]; }
			public void SetValue(string key, object value) { curDict[key] = value; }
			public bool HasKey(string key) { return curDict.ContainsKey(key); }

			public bool isSaving { get { return true; } }
			public bool isLoading { get { return !isSaving; } }

			public ListMode Mode { get { return ListMode.kRead; } }

			Dictionary<string, object> curDict;

			public bool Serialize(string key, ref IDictionary<string, object> target)
			{
				curDict[key] = target; return true;
			}
			public bool Serialize(string key, ref bool target)
			{
				curDict[key] = target; return true;
			}
			public bool Serialize(string key, ref bool? target)
			{
				curDict[key] = target; return true;
			}
			public bool Serialize(string key, ref int target)
			{
				curDict[key] = target; return true;
			}
			public bool Serialize(string key, ref int? target)
			{
				curDict[key] = target; return true;
			}
			public bool Serialize(string key, ref long target)
			{
				curDict[key] = target; return true;
			}
			public bool Serialize(string key, ref long? target)
			{
				curDict[key] = target; return true;
			}
			public bool Serialize(string key, ref ulong target)
			{
				curDict[key] = target; return true;
			}
			public bool Serialize(string key, ref ulong? target)
			{
				curDict[key] = target; return true;
			}
			public bool Serialize(string key, ref float target)
			{
				curDict[key] = target; return true;
			}
			public bool Serialize(string key, ref float? target)
			{
				curDict[key] = target; return true;
			}
			public bool Serialize(string key, ref double target)
			{
				curDict[key] = target; return true;
			}
			public bool Serialize(string key, ref double? target)
			{
				curDict[key] = target; return true;
			}
			public bool Serialize(string key, ref string target)
			{
				curDict[key] = target; return true;
			}

			public bool Serialize(string key, ref Guid target)
			{
				curDict[key] = target; return true;
			}

			public bool Serialize(string key, ref StringBuilder target)
			{
				curDict[key] = target.ToString(); return true;
			}

			public bool Serialize(string key, ref DateTime target)
			{
				//The "O" or "o" standard format specifier represents a custom date and
				//time format string using a pattern that preserves time zone information.
				string data = target.ToString("O");
				curDict[key] = data;
				return true;
			}
#if BEAMABLE_ENABLE_UNITY_SERIALIZATION_TYPES
			public bool Serialize(string key, ref Rect target)
			{
				float[] data = new float[4];
				data[0] = target.xMin;
				data[1] = target.yMin;
				data[2] = target.width;
				data[3] = target.height;
				curDict[key] = data;
				return true;
			}

			public bool Serialize(string key, ref Vector2 target)
			{
				float[] data = new float[2];
				data[0] = target.x;
				data[1] = target.y;
				curDict[key] = data;
				return true;
			}
			public bool Serialize(string key, ref Vector3 target)
			{
				float[] data = new float[3];
				data[0] = target.x;
				data[1] = target.y;
				data[2] = target.z;
				curDict[key] = data;
				return true;
			}
			public bool Serialize(string key, ref Vector4 target)
			{
				float[] data = new float[4];
				data[0] = target.x;
				data[1] = target.y;
				data[2] = target.z;
				data[3] = target.w;
				curDict[key] = data;
				return true;
			}

			public bool Serialize(string key, ref Color target)
			{
				float[] data = new float[4];
				data[0] = target.r;
				data[1] = target.g;
				data[2] = target.b;
				data[3] = target.a;
				curDict[key] = data;
				return true;
			}
			public bool Serialize(string key, ref Quaternion target)
			{
				float[] data = new float[4];
				data[0] = target.x;
				data[1] = target.y;
				data[2] = target.z;
				data[3] = target.w;
				curDict[key] = data;
				return true;
			}
			public bool Serialize(string key, ref Gradient target)
			{
				List<List<float>> data = new List<List<float>>(2);
				List<float> alphaKeys = new List<float>(target.alphaKeys.Length * 2);
				List<float> colorKeys = new List<float>(target.colorKeys.Length * 4);
				int index = 0;
				for (int i = 0; i < target.alphaKeys.Length * 2; i = i + 2)
				{
					alphaKeys.Add(target.alphaKeys[index].time);
					alphaKeys.Add(target.alphaKeys[index].alpha);
					index++;
				}
				index = 0;
				for (int i = 0; i < target.colorKeys.Length * 4; i = i + 4)
				{
					colorKeys.Add(target.colorKeys[index].time);
					colorKeys.Add(target.colorKeys[index].color.r);
					colorKeys.Add(target.colorKeys[index].color.g);
					colorKeys.Add(target.colorKeys[index].color.b);
					index++;
				}
				data.Add(alphaKeys);
				data.Add(colorKeys);
				curDict[key] = data;
				return true;
			}
#endif


			public bool SerializeDictionary<TDict, TElem>(string key, ref TDict target)
				where TDict : IDictionary<string, TElem>, new()
			{
				if (target == null)
				{
					curDict[key] = null;
					return true;
				}
				Dictionary<string, object> old = curDict;
				Dictionary<string, object> newDict = new Dictionary<string, object>();
				curDict = newDict;

				var iter = target.GetEnumerator();
				while (iter.MoveNext())
				{
					TElem value = iter.Current.Value;
					SerializeAny(iter.Current.Key, value);
				}

				curDict = old;
				curDict[key] = newDict;
				return true;
			}

			public bool SerializeDictionary<T>(string key, ref Dictionary<string, T> target)
			{
				if (target == null)
				{
					curDict[key] = null;
					return true;
				}
				Dictionary<string, object> old = curDict;
				Dictionary<string, object> newDict = new Dictionary<string, object>();
				curDict = newDict;

				Dictionary<string, T>.Enumerator iter = target.GetEnumerator();
				while (iter.MoveNext())
				{
					T value = iter.Current.Value;
					SerializeAny(iter.Current.Key, value);
				}

				curDict = old;
				curDict[key] = newDict;
				return true;
			}

			public bool SerializeInline<T>(string key, ref T value)
			   where T : ISerializable
			{
				return InternalSerialize<T>(key, value);
			}

			public bool Serialize<T>(string key, ref T value)
			   where T : class, ISerializable, new()
			{
				return InternalSerialize<T>(key, value);
			}

			public bool SerializeList<TList>(string key, ref TList value)
			   where TList : IList, new()
			{
				return SerializeListInternal(key, value);
			}

			public bool SerializeKnownList<TElem>(string key, ref List<TElem> value) where TElem : ISerializable, new()
			{
				return SerializeListInternal2(key, value);
			}


			public bool SerializeILL<T>(string key, ref LinkedList<T> list) where T : ClassPool<T>, new()
			{
				return SerializeILLInternal<T>(key, ref list);
			}

			bool SerializeILLInternal<T>(string key, ref LinkedList<T> llist) where T : ClassPool<T>, new()
			{
				if (llist == null)
				{
					return false;
				}
				if (llist.First == null)
				{
					return false;
				}
				var elemType = typeof(T);

				if (typeof(ISerializable).IsAssignableFrom(elemType))
				{
					Dictionary<string, object>[] list = new Dictionary<string, object>[llist.Count];
					var node = llist.First;
					int index = 0;
					while (node != null)
					{
						var serializable = node.Value as ISerializable;
						list[index++] = (SerializeNested(serializable));
						node = node.Next;
					}
					curDict[key] = list;
					return true;
				}
				else
				{
					BeamableLogger.LogError("Cannot serialize LinkedList<T> of non JsonSerializable types");
					return false;
				}
			}

			private bool SerializeListInternal2<TElem>(string key, IList<TElem> value)
				where TElem : ISerializable
			{
				if (value == null)
				{
					return false;
				}
				var list = new List<Dictionary<string, object>>(value.Count);
				foreach (var serializable in value)
				{
					list.Add(SerializeNested(serializable));
				}
				curDict[key] = list;
				return true;
			}

			private bool SerializeListInternal<TList>(string key, TList value)
			   where TList : IList
			{

				if (value == null)
				{
					return false;
				}

				var elemType = typeof(TList).GetElementType();

				if (elemType == null)
					elemType = value.GetType().GetGenericArguments()[0];

				if (typeof(ISerializable).IsAssignableFrom(elemType))
				{
					var list = new List<Dictionary<string, object>>(value.Count);

					for (int i = 0; i < value.Count; ++i)
					{
						var serializable = value[i] as ISerializable;
						list.Add(SerializeNested(serializable));
					}
					curDict[key] = list;
					return true;
				}
				else
				{
					curDict[key] = value;
					return true;
				}
			}

			public bool SerializeArray<T>(string key, ref T[] value)
			{
				return SerializeListInternal(key, value);
			}

			private void SerializeAny<T>(string key, T elem)
			{
				if (typeof(ISerializable).IsAssignableFrom(typeof(T)))
				{
					InternalSerialize(key, (ISerializable)elem);
				}
				else if (typeof(IList).IsAssignableFrom(typeof(T)))
				{
					SerializeListInternal(key, elem as IList);
				}
			}

			private bool InternalSerialize<T>(string key, T value)
			   where T : ISerializable
			{
				if (value != null)
				{
					Dictionary<string, object> old = curDict;
					var newDict = new Dictionary<string, object>();
					curDict = newDict;
					value.Serialize(this);
					curDict = old;
					curDict[key] = newDict;
					return true;
				}
				return false;
			}

			private Dictionary<string, object> SerializeNested(ISerializable serializable)
			{
				var newDict = new Dictionary<string, object>();
				var old = curDict;
				curDict = newDict;
				serializable.Serialize(this);
				curDict = old;
				return newDict;
			}
		}
	}
}
