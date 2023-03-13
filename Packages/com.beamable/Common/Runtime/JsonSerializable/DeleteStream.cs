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

		//////////////////////////////////////////
		///
		/// Deleting
		///
		//////////////////////////////////////////

		public interface IDeleteListener
		{
			void OnDeleted(object deletedObject);
		}


		public class DeleteStream : ClassPool<DeleteStream>, IStreamSerializer
		{
			private IDictionary<string, object> curDict;
			public void Init(IDictionary<string, object> dict)
			{
				curDict = dict;
			}

			public override void OnRecycle()
			{
				curDict = null;
				deleteListener = null;
			}

			public bool isSaving { get { return false; } }
			public bool isLoading { get { return false; } }
			public object GetValue(string key) { throw new Exception("Getting Value in Delete Stream"); }
			public void SetValue(string key, object value) { throw new Exception("Getting Value in Delete Stream"); }
			public bool HasKey(string key) { return false; }

			public ListMode Mode { get { return ListMode.kDelete; } }

			// Note - it wouldn't be hard to turn this into a list of listeners if we ever wanted to support multiple.
			private IDeleteListener deleteListener = null;
			public void RegisterDeleteListener(IDeleteListener listener)
			{
				deleteListener = listener;
			}

			public bool Serialize(string key, ref IDictionary<string, object> target) { return false; }
			public bool Serialize(string key, ref bool target) { return false; }
			public bool Serialize(string key, ref bool? target) { return false; }
			public bool Serialize(string key, ref int target) { return false; }
			public bool Serialize(string key, ref int? target) { return false; }
			public bool Serialize(string key, ref long target) { return false; }
			public bool Serialize(string key, ref long? target) { return false; }
			public bool Serialize(string key, ref ulong target) { return false; }
			public bool Serialize(string key, ref ulong? target) { return false; }
			public bool Serialize(string key, ref float target) { return false; }
			public bool Serialize(string key, ref float? target) { return false; }
			public bool Serialize(string key, ref double target) { return false; }
			public bool Serialize(string key, ref double? target) { return false; }
			public bool Serialize(string key, ref string target) { return false; }
			public bool Serialize(string key, ref Guid target) { return false; }
			public bool Serialize(string key, ref StringBuilder target) { return false; }
			public bool Serialize(string key, ref DateTime target) { return false; }

#if BEAMABLE_ENABLE_UNITY_SERIALIZATION_TYPES
			public bool Serialize(string key, ref Rect target) { return false; }
			public bool Serialize(string key, ref Vector2 target) { return false; }
			public bool Serialize(string key, ref Vector3 target) { return false; }
			public bool Serialize(string key, ref Vector4 target) { return false; }
			public bool Serialize(string key, ref Color target) { return false; }
			public bool Serialize(string key, ref Quaternion target) { return false; }
			public bool Serialize(string key, ref Gradient target) { return false; }
#endif
			public bool SerializeArray<T>(string key, ref T[] value) { return false; }

			public void DeleteChildMost(JsonSerializable.ISerializable obj)
			{
				if (IsLeafDictionary(curDict))
				{
					NotifyOfDelete(obj);
				}
				else
				{
					obj.Serialize(this);
				}
			}

			public bool SerializeILL<T>(string key, ref LinkedList<T> list) where T : ClassPool<T>, new()
			{
				if (list == null)
				{
					return false;
				}
				return InternalSerializeILL<T>(key, ref list);
			}

			private bool InternalSerializeILL<T>(string key, ref LinkedList<T> ll) where T : ClassPool<T>, new()
			{
				IList data = curDict[key] as IList;
				if (data == null)
				{
					return false;
				}

				Type elemType = typeof(T);

				// merge/replace existing based on id string
				// Check if object is Identifiable
				if (!typeof(ISerializeIdentifiable).IsAssignableFrom(elemType))
				{
					return false;
				}

				// go through the list of dictionaries (one per class) and see if ID's match
				for (int i = 0; i < data.Count; ++i)
				{
					IDictionary<string, object> d = data[i] as IDictionary<string, object>;
					if (d != null && d.ContainsKey("id"))
					{
						long id = Convert.ToInt64(d["id"]);
#if UNITY_EDITOR
                        bool found = false;
#endif
						// see if we have an entry in our current list
						var n = ll.First;
						while (n != null)
						{
							var node = n.Value;

							// Safety Checked Above
							long lhs = ((ISerializeIdentifiable)node).SerializationID;

							if (id == lhs)
							{
#if UNITY_EDITOR
                                found = true;
#endif
								ISerializable ml = node as ISerializable;
								ClassPool<T> pl = node as ClassPool<T>;
								if (IsLeafDictionary(d))
								{
									NotifyOfDelete(ml);
									ll.Remove(pl.poolNode);
									pl.Recycle();
								}
								else
								{
									SerializeWith(ml, d);
								}
								break;
							}
							n = n.Next;
						}
#if UNITY_EDITOR
                        if (!found)
                        {
                            BeamableLogger.LogWarning("Id not found in delete: " + key + "[" + id + "]");
                        }
#endif

					}
				}
				return true;
			}

			public bool SerializeList<L>(string key, ref L value)
			   where L : IList, new()
			{
				if (value == null)
				{
					return false;
				}
				return InternalSerialize(key, ref value);
			}

			public bool SerializeKnownList<TElem>(string key, ref List<TElem> value) where TElem : ISerializable, new()
			{
				throw new NotImplementedException(nameof(SerializeKnownList));
			}


			public bool Serialize<T>(string key, ref T value)
			   where T : class, ISerializable, new()
			{
				if (value == null)
				{
					return false;
				}
				return InternalSerialize<T>(key, ref value);
			}

			public bool SerializeInline<T>(string key, ref T value)
			   where T : ISerializable
			{
				if (value == null)
				{
					return false;
				}
				return InternalSerialize<T>(key, ref value);
			}

			public bool SerializeDictionary<TDict, T>(string parentKey, ref TDict target)
				where TDict : IDictionary<string, T>, new()
			{
				if (!curDict.ContainsKey(parentKey))
					return false;

				IDictionary<string, object> asDict = curDict[parentKey] as IDictionary<string, object>;
				if (asDict == null || target == null)
					return false;

				bool isList = typeof(IList).IsAssignableFrom(typeof(T));
				bool isManual = typeof(ISerializable).IsAssignableFrom(typeof(T));
				bool shouldNotify = typeof(IDeletable).IsAssignableFrom(typeof(T));

				var iter = asDict.GetEnumerator();
				while (iter.MoveNext())
				{
					var key = iter.Current.Key;
					var value = iter.Current.Value;

					T current;
					if (!target.TryGetValue(key, out current))
						continue;

					if (isList || (isManual && !IsLeafDictionary((IDictionary<string, object>)value)))
					{
						IDictionary<string, object> d = curDict;
						curDict = asDict;
						InternalSerialize<T>(iter.Current.Key, ref current);
						curDict = d;
					}
					else
					{
						if (shouldNotify)
							NotifyOfDelete(current);

						target.Remove(key);
					}
				}
				return true;
			}

			public bool SerializeDictionary<T>(string parentKey, ref Dictionary<string, T> target)
			{
				if (!curDict.ContainsKey(parentKey))
					return false;

				IDictionary<string, object> asDict = curDict[parentKey] as IDictionary<string, object>;
				if (asDict == null || target == null)
					return false;

				bool isList = typeof(IList).IsAssignableFrom(typeof(T));
				bool isManual = typeof(ISerializable).IsAssignableFrom(typeof(T));
				bool shouldNotify = typeof(IDeletable).IsAssignableFrom(typeof(T));

				var iter = asDict.GetEnumerator();
				while (iter.MoveNext())
				{
					var key = iter.Current.Key;
					var value = iter.Current.Value;

					T current;
					if (!target.TryGetValue(key, out current))
						continue;

					if (isList || (isManual && !IsLeafDictionary((IDictionary<string, object>)value)))
					{
						IDictionary<string, object> d = curDict;
						curDict = asDict;
						InternalSerialize<T>(iter.Current.Key, ref current);
						curDict = d;
					}
					else
					{
						if (shouldNotify)
							NotifyOfDelete(current);

						target.Remove(key);
					}
				}
				return true;
			}

			public bool InternalSerialize<T>(string key, ref T value)
			{
				if (!curDict.ContainsKey(key))
					return false;

				IList asList = value as IList;
				ISerializable manual = value as ISerializable;

				if (manual != null)
				{
					IDictionary<string, object> d = curDict[key] as IDictionary<string, object>;
					if (d != null)
					{
						if (IsLeafDictionary(d))
						{
							NotifyOfDelete(value);
							value = default(T);
						}
						else
						{
							SerializeWith(manual, d);
							return true;
						}
					}
					return false;
				}
				else if (asList != null)   // IList<T>
				{
					return SerializeList(key, asList);
				}
				else
				{
#if UNITY_EDITOR
                    BeamableLogger.LogError("Could not match data to field " + value + " != " + curDict[key].GetType());
#endif
					return false;
				}
			}

			private bool SerializeList(string key, IList asList)
			{
				IList data = curDict[key] as IList;
				if (data == null)
				{
					return false;
				}

				// get types for IList<T> subclass and <T> element
				Type elemType = asList.GetType().GetGenericArguments()[0];

				// merge/replace existing based on id string
				// Check if object is Identifiable
				if (!typeof(ISerializeIdentifiable).IsAssignableFrom(elemType))
				{
					return false;
				}

				// go through the list of dictionaries (one per class) and see if ID's match
				for (int i = 0; i < data.Count; ++i)
				{
					IDictionary<string, object> d = data[i] as IDictionary<string, object>;
					if (d != null && d.ContainsKey("id"))
					{
						long id = Convert.ToInt64(d["id"]);
#if UNITY_EDITOR
                        bool found = false;
#endif
						// see if we have an entry in our current list
						for (int j = 0; j < asList.Count; ++j)
						{
							// Safety Checked Above
							long lhs = ((ISerializeIdentifiable)asList[j]).SerializationID;

							if (id == lhs)
							{
#if UNITY_EDITOR
                                found = true;
#endif
								ISerializable ml = asList[j] as ISerializable;
								if (IsLeafDictionary(d))
								{
									NotifyOfDelete(ml);
									asList.RemoveAt(j);
								}
								else
								{
									SerializeWith(ml, d);
								}
								break;
							}
						}
#if UNITY_EDITOR
                        if (!found)
                        {
	                        BeamableLogger.LogWarning("Id not found in delete: " + key + "[" + id + "]");
                        }
#endif
					}
				}
				return true;
			}

			private bool IsLeafDictionary(IDictionary<string, object> dict)
			{
				return dict.Count == 0 || (dict.Count == 1 && dict.ContainsKey("id"));
			}

			private void NotifyOfDelete(object deletedObject)
			{
				var deletable = deletedObject as IDeletable;
				if (deletable != null)
				{
					deletable.AfterDelete();
				}

				if (deleteListener != null)
				{
					deleteListener.OnDeleted(deletedObject);
				}
			}

			private void SerializeWith(ISerializable serializable, IDictionary<string, object> dict)
			{
				IDictionary<string, object> old = curDict;
				curDict = dict;
				serializable.Serialize(this);
				curDict = old;
			}
		}

	}
}
