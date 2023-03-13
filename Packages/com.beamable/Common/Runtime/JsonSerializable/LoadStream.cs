#if UNITY_2018_1_OR_NEWER || BEAMABLE_ENABLE_UNITY_SERIALIZATION_TYPES
#define BEAMABLE_ENABLE_UNITY_SERIALIZATION_TYPES
#endif

using Beamable.Common;
using Beamable.Common.Pooling;
using Beamable.Common.Spew;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
#if BEAMABLE_ENABLE_UNITY_SERIALIZATION_TYPES
using UnityEngine;
#endif
namespace Beamable.Serialization
{
	public partial class JsonSerializable
	{
		public interface ISerializableFactory
		{
			// Returns true if the factory /should/ be able to create the object, not if it will.  It's
			// possible for the factory to return true and then TryCreate to return null.  This indicates
			// to the caller that it's not possible to create the requested object (eg. in the case of
			// insufficient data in the dictionary).  Callers should both check CanCreate and null-check
			// the result from Create.
			bool CanCreate(Type type);
			ISerializable TryCreate(Type type, IDictionary<string, object> dict);
		}

		/// <summary>
		/// Class <c>LoadStream</c> is an IStreamSerializer for JsonSerializable
		/// Provides deserialization of Dictionary(string, object) into ISerializable objects
		/// </summary>
		public class LoadStream : ClassPool<LoadStream>, IStreamSerializer
		{
			public void Init(IDictionary<string, object> dict, JsonSerializable.ListMode m)
			{
				mode = m;
				curDict = dict;
				factories.Clear();
			}

			public override void OnRecycle()
			{
				curDict = null;
			}

			public bool isSaving { get { return false; } }
			public bool isLoading { get { return !isSaving; } }
			public bool HasKey(string key) { return curDict.ContainsKey(key); }
			public object GetValue(string key) { return curDict[key]; }
			public void SetValue(string key, object value) { BeamableLogger.LogError("Set value called on load"); }

			public ListMode Mode { get { return mode; } }

			public IDictionary<string, object> curDict;
			public JsonSerializable.ListMode mode;

			private List<ISerializableFactory> factories = new List<ISerializableFactory>();

			public void RegisterISerializableFactory(ISerializableFactory factory)
			{
				factories.Add(factory);
			}

			public void DeregisterISerializableFactory(ISerializableFactory factory)
			{
				factories.Remove(factory);
			}

			public bool Serialize(string key, ref IDictionary<string, object> target)
			{
				object tmp;
				if (curDict.TryGetValue(key, out tmp))
				{
					IDictionary<string, object> asDict = tmp as IDictionary<string, object>;
					if (asDict != null)  // support merge?
					{
						target = asDict;
						return true;
					}
				}

				return false;
			}

			public bool SerializeDictionary<TDict, T>(string parentKey, ref TDict target)
				where TDict : IDictionary<string, T>, new()
			{
				object tmp;
				if (curDict.TryGetValue(parentKey, out tmp))
				{
					IDictionary<string, object> asDict = tmp as IDictionary<string, object>;
					if (asDict == null)
					{
						if (mode != ListMode.kMerge)
							target = default(TDict);
						return false;
					}

					if (target == null)
					{
						target = new TDict();
					}

					// TODO: Check this!
					// if this.mode
					if (this.mode != ListMode.kMerge)
						target.Clear();

					var iter = asDict.GetEnumerator();
					while (iter.MoveNext())
					{
						var key = iter.Current.Key;
						var value = iter.Current.Value;
						var valueType = value.GetType();

						if (typeof(T) == valueType)
						{
							target[key] = (T)value;
						}
						else if (typeof(T).IsPrimitive && valueType.IsPrimitive)
						{
							target[key] = (T)Convert.ChangeType(value, typeof(T));
						}
						else
						{
							T elem;
							if (!target.TryGetValue(key, out elem))
							{
								object objElem = null;
								if (CreateElemType(typeof(T), value, ref objElem))
								{
									elem = (T)objElem;
									target[key] = elem;
								}
							}
							else
							{
								// It does exist, just deserialize
								IDictionary<string, object> d = curDict;
								curDict = asDict;
								InternalSerializeObject(iter.Current.Key, elem, typeof(T));
								curDict = d;
							}
						}
					}
					return true;
				}
				return false;
			}

			public bool SerializeDictionary<T>(string parentKey, ref Dictionary<string, T> target)
			{
				object tmp;
				if (curDict.TryGetValue(parentKey, out tmp))
				{
					IDictionary<string, object> asDict = tmp as IDictionary<string, object>;
					if (asDict == null)
					{
						if (mode != ListMode.kMerge)
							target = null;
						return false;
					}

					if (target == null)
					{
						target = new Dictionary<string, T>();
					}

					// TODO: Check this!
					// if this.mode
					if (this.mode != ListMode.kMerge)
						target.Clear();

					var iter = asDict.GetEnumerator();
					while (iter.MoveNext())
					{
						var key = iter.Current.Key;
						var value = iter.Current.Value;
						var valueType = value.GetType();

						if (typeof(T) == valueType)
						{
							target[key] = (T)value;
						}
						else if (typeof(T).IsPrimitive && valueType.IsPrimitive)
						{
							target[key] = (T)Convert.ChangeType(value, typeof(T));
						}
						else
						{
							T elem;
							if (!target.TryGetValue(key, out elem))
							{
								object objElem = null;
								if (CreateElemType(typeof(T), value, ref objElem))
								{
									elem = (T)objElem;
									target[key] = elem;
								}
							}
							else
							{
								// It does exist, just deserialize
								IDictionary<string, object> d = curDict;
								curDict = asDict;
								InternalSerializeObject(iter.Current.Key, elem, typeof(T));
								curDict = d;
							}
						}
					}
					return true;
				}
				return false;
			}

			public bool Serialize(string key, ref bool target)
			{
				object tmp;
				if (curDict.TryGetValue(key, out tmp))
				{
					target = Convert.ToBoolean(tmp);
					return true;
				}
				return false;
			}

			public bool Serialize(string key, ref bool? target)
			{
				object tmp;
				if (curDict.TryGetValue(key, out tmp))
				{
					if (tmp == null)
						target = null;
					else
						target = Convert.ToBoolean(tmp);
					return true;
				}
				return false;
			}

			public bool Serialize(string key, ref int target)
			{
				object tmp;
				if (curDict.TryGetValue(key, out tmp))
				{
					target = Convert.ToInt32(tmp);
					return true;
				}
				return false;
			}

			public bool Serialize(string key, ref int? target)
			{
				object tmp;
				if (curDict.TryGetValue(key, out tmp))
				{
					if (tmp == null)
						target = null;
					else
						target = Convert.ToInt32(tmp);
					return true;
				}
				return false;
			}

			public bool Serialize(string key, ref long target)
			{
				object tmp;
				if (curDict.TryGetValue(key, out tmp))
				{
					target = Convert.ToInt64(tmp);
					return true;
				}
				return false;
			}

			public bool Serialize(string key, ref long? target)
			{
				object tmp;
				if (curDict.TryGetValue(key, out tmp))
				{
					if (tmp == null)
						target = null;
					else
						target = Convert.ToInt64(tmp);
					return true;
				}
				return false;
			}

			public bool Serialize(string key, ref ulong target)
			{
				object tmp;
				if (curDict.TryGetValue(key, out tmp))
				{
					target = Convert.ToUInt64(tmp);
					return true;
				}
				return false;
			}

			public bool Serialize(string key, ref ulong? target)
			{
				object tmp;
				if (curDict.TryGetValue(key, out tmp))
				{
					if (tmp == null)
						target = null;
					else
						target = Convert.ToUInt64(tmp);
					return true;
				}
				return false;
			}

			public bool Serialize(string key, ref float target)
			{
				object tmp;
				if (curDict.TryGetValue(key, out tmp))
				{
					target = Convert.ToSingle(tmp);
					return true;
				}
				return false;
			}

			public bool Serialize(string key, ref float? target)
			{
				object tmp;
				if (curDict.TryGetValue(key, out tmp))
				{
					if (tmp == null)
						target = null;
					else
						target = Convert.ToSingle(tmp);
					return true;
				}
				return false;
			}

			public bool Serialize(string key, ref double target)
			{
				object tmp;
				if (curDict.TryGetValue(key, out tmp))
				{
					target = Convert.ToDouble(tmp);
					return true;
				}
				return false;
			}
			public bool Serialize(string key, ref double? target)
			{
				object tmp;
				if (curDict.TryGetValue(key, out tmp))
				{
					if (tmp == null)
						target = null;
					else
						target = Convert.ToDouble(tmp);
					return true;
				}
				return false;
			}

			public bool Serialize(string key, ref string target)
			{
				object tmp;
				if (curDict.TryGetValue(key, out tmp))
				{
					if (tmp == null)
						target = null;
					else
						target = tmp.ToString();
					return true;
				}
				return false;
			}

			public bool Serialize(string key, ref Guid target)
			{
				object tmp;
				if (curDict.TryGetValue(key, out tmp))
				{
					return Guid.TryParse(tmp?.ToString(), out target);
				}
				return false;
			}


			public bool Serialize(string key, ref StringBuilder target)
			{
				object tmp;
				if (curDict.TryGetValue(key, out tmp))
				{
					if (tmp == null)
						target = null;
					else
						target.Append(tmp.ToString());
					return true;
				}
				return false;
			}

			public bool Serialize(string key, ref DateTime target)
			{
				object tmp;
				if (curDict.TryGetValue(key, out tmp))
				{
					if (tmp == null)
						return false;
					try
					{
						target = DateTime.ParseExact(tmp.ToString(), "O", CultureInfo.InvariantCulture);
					}
					catch (Exception e)
					{
						BeamableLogger.LogWarning("DateTime could not deserialize: " + tmp + "  " + e.Message);
						return false;
					}

					return true;
				}
				return false;
			}

#if BEAMABLE_ENABLE_UNITY_SERIALIZATION_TYPES
			public bool Serialize(string key, ref Rect target)
			{
				object tmp;
				if (curDict.TryGetValue(key, out tmp))
				{
					IList vals = tmp as IList;
					if (vals == null || vals.Count != 4)
						return false;
					target = new Rect(Convert.ToSingle(vals[0]), Convert.ToSingle(vals[1]), Convert.ToSingle(vals[2]), Convert.ToSingle(vals[3]));
					return true;
				}
				return false;
			}

			public bool Serialize(string key, ref Vector2 v)
			{
				object tmp;
				if (curDict.TryGetValue(key, out tmp))
				{
					IList vals = tmp as IList;
					if (vals != null && vals.Count == 2)
					{
						v.x = Convert.ToSingle(vals[0]);
						v.y = Convert.ToSingle(vals[1]);
						return true;
					}
				}
				return false;
			}

			public bool Serialize(string key, ref Vector3 v)
			{
				object tmp;
				if (curDict.TryGetValue(key, out tmp))
				{
					IList vals = tmp as IList;
					if (vals != null && vals.Count == 3)
					{
						v.x = Convert.ToSingle(vals[0]);
						v.y = Convert.ToSingle(vals[1]);
						v.z = Convert.ToSingle(vals[2]);
						return true;
					}
				}
				return false;
			}

			public bool Serialize(string key, ref Vector4 v)
			{
				object tmp;
				if (curDict.TryGetValue(key, out tmp))
				{
					IList vals = tmp as IList;
					if (vals != null && vals.Count == 4)
					{
						v.x = Convert.ToSingle(vals[0]);
						v.y = Convert.ToSingle(vals[1]);
						v.z = Convert.ToSingle(vals[2]);
						v.w = Convert.ToSingle(vals[3]);
						return true;
					}
				}
				return false;
			}

			public bool Serialize(string key, ref Color v)
			{
				object tmp;
				if (curDict.TryGetValue(key, out tmp))
				{
					IList vals = tmp as IList;
					if (vals != null || vals.Count == 4)
					{
						v.r = Convert.ToSingle(vals[0]);
						v.g = Convert.ToSingle(vals[1]);
						v.b = Convert.ToSingle(vals[2]);
						v.a = Convert.ToSingle(vals[3]);
						return true;
					}
				}
				return false;
			}

			public bool Serialize(string key, ref Quaternion target)
			{
				object tmp;
				if (curDict.TryGetValue(key, out tmp))
				{
					IList vals = tmp as IList;
					if (vals == null || vals.Count != 4)
						return false;
					target = new Quaternion(Convert.ToSingle(vals[0]),
					   Convert.ToSingle(vals[1]),
					   Convert.ToSingle(vals[2]),
					   Convert.ToSingle(vals[3]));
					return true;
				}
				return false;
			}

			public bool Serialize(string key, ref Gradient target)
			{
				object tmp;
				if (curDict.TryGetValue(key, out tmp))
				{
					List<List<float>> data = tmp as List<List<float>>;
					if (data == null || data.Count != 2)
						return false;

					List<float> alphaKeys = data[0];
					List<float> colorKeys = data[1];
					if (alphaKeys.Count % 2 != 0)
						return false;
					if (colorKeys.Count % 4 != 0)
						return false;

					target.alphaKeys = new GradientAlphaKey[alphaKeys.Count / 2];
					target.colorKeys = new GradientColorKey[alphaKeys.Count / 4];

					int index = 0;
					for (int i = 0; i < alphaKeys.Count; i = i + 2)
					{
						GradientAlphaKey ak = new GradientAlphaKey(alphaKeys[i + 1], alphaKeys[i]);
						target.alphaKeys[index] = ak;
						index++;

					}
					index = 0;
					for (int i = 0; i < colorKeys.Count; i = i + 4)
					{
						GradientColorKey ck = new GradientColorKey(new Color(colorKeys[i + 1], colorKeys[i + 2], colorKeys[i + 3]), colorKeys[i]);
						target.colorKeys[index] = ck;
						index++;
					}
					return true;
				}
				return false;
			}
#endif

			public bool Serialize<T>(string key, ref T value)
			   where T : class, ISerializable, new()
			{
				object tmp;
				if (curDict.TryGetValue(key, out tmp))
				{
					if (tmp == null)
					{
						// clear if value is explicitly set to null
						value = default(T);
						return true;
					}

					if (value == null)
					{
						object elem = null;
						if (CreateElemType(typeof(T), tmp, ref elem))
						{
							value = (T)elem;
						}
						return elem != null;
					}

					if (value != null)
					{
						return InternalSerialize<T>(key, ref value);
					}
				}
				return false;
			}

			public bool SerializeInline<T>(string key, ref T value)
			   where T : ISerializable
			{
				if (value == null)
				{
					BeamableLogger.LogError("Cannot deserialize into null value when type doesn't conform to new()");
					return false;
				}
				return InternalSerialize<T>(key, ref value);
			}

			private T ExtractKey<T>(string key) where T : class
			{
				object entry;
				if (!curDict.TryGetValue(key, out entry))
				{
					return default(T);
				}

				var list = entry as T;
				if (list == null && entry != null)
				{
					BeamableLogger.LogError(string.Format("Could not match data ({0}) to field: {1} != {2}", key, typeof(T), entry.GetType()));
				}
				return list;
			}

			public bool SerializeILL<T>(string key, ref LinkedList<T> ill) where T : ClassPool<T>, new()
			{
				var list = ExtractKey<IList>(key);
				if (list == null)
				{
					return false;
				}

				InternalSerializeILList<T>(list, ill);
				return true;
			}

			private void InternalSerializeILList<T>(IList list, LinkedList<T> target)
			   where T : ClassPool<T>, new()
			{
				// create an elem and see if it has an id field
				Type elemType = typeof(T);

				if (target == null)
				{
					target = new LinkedList<T>();
				}

				// merge/replace existing based on id string
				// This is mainly used by our client/server sync, but can be used to merge/replace any data
				if ((mode == ListMode.kMerge) && (typeof(ISerializeIdentifiable).IsAssignableFrom(elemType)))
				{
					// go through the list of dictionaries (one per class) and see if ID's match
					for (int i = 0; i < list.Count; ++i)
					{
						var d = list[i] as IDictionary<string, object>;
						bool found = false;
						if (d != null && d.ContainsKey("id"))
						{
							long id = Convert.ToInt64(d["id"]);
							// see if we have an entry in our current list
							var node = target.First;
							while (node != null)
							{
								var val = node.Value;

								// Safety Checked Above
								var identifiable = (ISerializeIdentifiable)val;
								long lhs = identifiable.SerializationID;

								if (id == lhs)
								{
									// we do, so just deserialize it over the old one to preserve any data not specified
									SerializeWith(ref identifiable, d);
									found = true;
									break;
								}
								node = node.Next;
							}
						}

						// our id is not in the list, so add a new entry
						if (!found)
						{
							T elem = ClassPool<T>.Spawn();
							ISerializable ml = elem as ISerializable;
							SerializeWith(ref ml, d);
							target.AddLast(elem.poolNode);
						}
					}
				}
				else // replace or merge with no id
				{
					// clear list
					var node = target.First;
					while (node != null)
					{
						//we have a node that we will remove and recycle.  Remove it from the list before recycling.
						//removing node will set its own prev/next to null
						target.Remove(node);

						var ecp = node.Value as ClassPool<T>;
						if (ecp != null)
						{
							ecp.Recycle();
						}

						//now set our next item to be the new front of the list to continue clearing.
						node = target.First;
					}

					for (int j = 0; j < list.Count; ++j)
					{
						var d = list[j] as IDictionary<string, object>;
						T elem = ClassPool<T>.Spawn();
						ISerializable ml = elem as ISerializable;
						SerializeWith(ref ml, d);

						target.AddLast(elem.poolNode);
					}
				}
			}

			private void SerializeWith<T>(ref T serializable, IDictionary<string, object> dict)
			   where T : ISerializable
			{
				IDictionary<string, object> old = curDict;
				curDict = dict;
				serializable.Serialize(this);
				curDict = old;
			}

			private void InternalSerializeListSoftReplace(IList list, IList target, Type elemType)
			{
				// As list is deserialized, partition
				int outIndex = 0;
				for (int i = 0; i < list.Count; ++i)
				{
					var d = list[i] as IDictionary<string, object>;
					if (d == null) continue;

					bool found = false;
					object idObject;
					if (d.TryGetValue("id", out idObject))
					{
						long id = Convert.ToInt64(idObject);
						// see if we have an entry in our current list
						for (int j = 0; j < target.Count; ++j)
						{
							// Safety Checked Above
							var targetElem = (ISerializeIdentifiable)target[j];
							long lhs = targetElem.SerializationID;

							if (id == lhs)
							{
								SerializeWith(ref targetElem, d);
								if (outIndex < j)
								{
									target[j] = target[outIndex];
									target[outIndex] = targetElem;
								}
								if (outIndex <= j)
								{
									outIndex++;
								}
								found = true;
								break;
							}
						}
					}

					// our id is not in the list, so add a new entry
					if (!found)
					{
						object elem = null;
						if (CreateElemType(elemType, list[i], ref elem))
						{
							if (outIndex < target.Count)
							{
								target.Add(target[outIndex]);
								target[outIndex] = elem;
							}
							else
							{
								target.Add(elem);
							}
							outIndex++;
						}
					}
				}

				// remove end of array
				for (int i = target.Count - 1; i >= outIndex; --i)
				{
					var deleteable = target[i] as IDeletable;
					target.RemoveAt(i);
					if (deleteable != null)
					{
						deleteable.AfterDelete();
					}
				}
			}

			private void InternalSerializeList<TList>(IList list, TList target)
			   where TList : IList
			{
				// create an elem and see if it has an id field
				Type elemType = typeof(TList).GetGenericArguments()[0];

				// merge/replace existing based on id string
				// This is mainly used by our client/server sync, but can be used to merge/replace any data
				if ((mode == ListMode.kReplace) || (!typeof(ISerializeIdentifiable).IsAssignableFrom(elemType)) || target.Count == 0)
				{
					// replace or merge with no id
					target.Clear();
					for (int j = 0; j < list.Count; ++j)
					{
						object elem = null;
						if (CreateElemType(elemType, list[j], ref elem))
						{
							target.Add(elem);
						}
					}
				}
				else if (mode == ListMode.kSoftReplace)
				{
					InternalSerializeListSoftReplace(list, target, elemType);
				}
				else
				{
					// go through the list of dictionaries (one per class) and see if ID's match
					for (int i = 0; i < list.Count; ++i)
					{
						var d = list[i] as IDictionary<string, object>;
						bool found = false;
						if (d != null && d.ContainsKey("id"))
						{
							long id = Convert.ToInt64(d["id"]);
							// see if we have an entry in our current list
							for (int j = 0; j < target.Count; ++j)
							{
								// Safety Checked Above
								var targetElem = (ISerializeIdentifiable)target[j];
								long lhs = targetElem.SerializationID;

								if (id == lhs)
								{
									// we do, so just deserialize it over the old one to preserve any data not specified
									SerializeWith(ref targetElem, d);
									found = true;
									break;
								}
							}
						}

						// our id is not in the list, so add a new entry
						if (!found)
						{
							object elem = null;
							if (CreateElemType(elemType, list[i], ref elem))
							{
								target.Add(elem);
							}
						}
					}
				}
			}


			public bool SerializeKnownList<TElem>(string key, ref List<TElem> value) where TElem : ISerializable, new()
			{
				throw new NotImplementedException(nameof(SerializeKnownList));
			}
			public bool SerializeList<TList>(string key, ref TList value) where TList : IList, new()
			{
				var list = ExtractKey<IList>(key);
				if (list == null)
				{
					return false;
				}

				var target = value != null ? value : new TList();
				InternalSerializeList<TList>(list, target);
				value = target;
				return true;
			}

			public bool SerializeArray<T>(string key, ref T[] target)
			{
				var list = ExtractKey<IList>(key);
				if (list == null)
				{
					return false;
				}

				//create new array if length has changed
				if (target == null || list.Count != target.Length)
				{
					target = new T[list.Count];
				}

				// copy data, no smart merge/replace in this mode - full replace only..
				for (int i = 0; i < list.Count; ++i)
				{
					object elem = null;
					CreateElemType(typeof(T), list[i], ref elem);
					target[i] = (T)elem;
				}
				return true;
			}

			private void InternalSerializeObject(string key, object value, Type type)
			{
				if (typeof(ISerializable).IsAssignableFrom(type))
				{
					ISerializable serializable = (ISerializable)value;
					InternalSerialize(key, ref serializable);
				}
				else if (typeof(IList).IsAssignableFrom(type))
				{
					var listIn = ExtractKey<IList>(key);
					var listOut = value as IList;
					if (listIn != null)
					{
						InternalSerializeList(listIn, listOut);
					}
				}
				else
				{
					throw new ArgumentException(string.Format("Invalid JsonSerializable dictionary value", type));
				}
			}

			private bool InternalSerialize<T>(string key, ref T value)
			   where T : ISerializable
			{
				object entry;
				curDict.TryGetValue(key, out entry);
				if (entry == null)
				{
					return false;
				}

				IDictionary<string, object> d = entry as IDictionary<string, object>;
				if (d != null)
				{
					SerializeWith(ref value, d);
					return true;
				}
				else
				{
					BeamableLogger.LogError(string.Format("Could not match data ({0}) to field: {1} != {2}", key, typeof(T), entry.GetType()));
					return false;
				}
			}

			private bool CreateElemType(Type elemType, object data, ref object target)
			{
				IDictionary<string, object> d = data as IDictionary<string, object>;
				var dataType = data.GetType();

				if (d != null && typeof(ISerializable).IsAssignableFrom(elemType))
				{
					ISerializable serializable = null;

					for (int i = 0; i < factories.Count; i++)
					{
						if (factories[i].CanCreate(elemType))
						{
							serializable = factories[i].TryCreate(elemType, d);
							target = serializable;
							if (target != null)
							{
								SerializeWith(ref serializable, d);
								return true;
							}
							else
							{
								NetMsgLogger.LogFormat("Nested update for {0}:{1} ignored, no root object found.", elemType.Name, d["id"]);
								return false;
							}
						}
					}

					if (JsonSerializable.IsPartial(d))
					{
						NetMsgLogger.LogFormat("Nested update for {0}:{1} ignored, no root object found.", elemType.Name, d["id"]);
						return false;
					}

					object elem = CreateInstance(elemType);
					serializable = (ISerializable)elem;
					SerializeWith(ref serializable, d);
					target = elem;
					return true;
				}
				else if (elemType == dataType)
				{
					target = data;
					return true;
				}
				else if (elemType.IsPrimitive && dataType.IsPrimitive)
				{
					target = Convert.ChangeType(data, elemType);
					return true;
				}
				else if (elemType == typeof(object))   // allow serialization of json data to type object
				{
					target = data;
					return true;
				}
				else if (elemType.IsEnum)
				{
					if (dataType.IsPrimitive)
					{
						target = Enum.ToObject(elemType, data);
					}
					else
					{
						target = Enum.Parse(elemType, data.ToString());
					}
					return true;
				}
				else
				{
					BeamableLogger.LogErrorFormat("List derserialization error, type {0} != type {1}", elemType, data.GetType());
					return false;
				}
			}

			// we have to handle value types manually because they have no constructors. Strangely, Activator.CreateInstance works fine
			// on OSX, but fails on the iPhone; likely due to a completely different path due to the AOT compiler.
			private object CreateInstance(Type type)
			{
				if (type == typeof(string))
				{
					return "";
				}
				else if (type == typeof(int))
				{
					return 0;
				}
				else if (type == typeof(float))
				{
					return 0.0f;
				}
				else if (type == typeof(double))
				{
					return 0.0;
				}
				else if (type == typeof(uint))
				{
					return (uint)0;
				}

				try
				{
					return Activator.CreateInstance(type);
				}
				catch (MissingMethodException)
				{
					BeamableLogger.LogErrorFormat("Derserialization error, type {0} has no empty constructor.", type);
					return null;
				}
			}
		}

		public static bool IsPartial(IDictionary<string, object> dict)
		{
			object partial = null;
			if (dict.TryGetValue("partial", out partial))
			{
				return Convert.ToBoolean(partial);
			}
			return false;
		}
	}
}
