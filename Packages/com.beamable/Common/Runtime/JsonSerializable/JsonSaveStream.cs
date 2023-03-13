#if UNITY_2018_1_OR_NEWER || BEAMABLE_ENABLE_UNITY_SERIALIZATION_TYPES
#define BEAMABLE_ENABLE_UNITY_SERIALIZATION_TYPES
#endif

using Beamable.Common.Pooling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
#if BEAMABLE_ENABLE_UNITY_SERIALIZATION_TYPES
using UnityEngine;
#endif

namespace Beamable.Serialization
{
	public partial class JsonSerializable
	{

		/// <summary>
		/// Class <c>JsonSaveStream</c> is an IStreamSerializer for JsonSerializable
		/// Provides serialization of ISerializable objects to json strings
		/// </summary>
		public class JsonSaveStream : ClassPool<JsonSaveStream>, IStreamSerializer
		{
			private StringBuilderPool.PooledStringBuilder _pooledBuilder;
			private StringBuilder _builder;
			private JsonType _jsonType;

			public enum JsonType
			{
				None, Array, Object
			}

			public void Init(JsonType jsonType = JsonType.None)
			{
				_pooledBuilder = StringBuilderPool.LargeStaticPool.Spawn();
				_builder = _pooledBuilder.Builder;
				_jsonType = jsonType;
				switch (_jsonType)
				{
					case JsonType.Array:
						StartArray();
						break;
					case JsonType.Object:
						StartObject();
						break;
				}
			}

			public void Conclude()
			{
				switch (_jsonType)
				{
					case JsonType.Array:
						EndArray();
						break;
					case JsonType.Object:
						EndObject();
						break;
					case JsonType.None:
						TrimComma();
						break;
				}
			}

			// We make this public so that things can use the string builder directly instead of needing to do another allocation
			public StringBuilder InternalStringBuilder
			{
				get { return _builder; }
			}

			public override string ToString()
			{
				return _builder.ToString();
			}

			public override void OnRecycle()
			{
				_builder = null;
				if (_pooledBuilder != null)
					_pooledBuilder.Dispose();
				_pooledBuilder = null;
			}


			// No-ops, we don't retain knowledge of what we have serialized.
			public object GetValue(string key) { return null; }
			public bool HasKey(string key) { return false; }

			public void SetValue(string key, object value)
			{
				SerializeAny(key, value);
			}

			public bool isSaving { get { return true; } }
			public bool isLoading { get { return !isSaving; } }

			public ListMode Mode { get { return ListMode.kRead; } }

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected void StartObject()
			{
				_builder.Append('{');
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected void TrimComma()
			{
				if (_builder[_builder.Length - 1] == ',')
					_builder.Length -= 1;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected void EndObject()
			{
				TrimComma();
				_builder.Append('}');
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected void AppendSeperator()
			{
				_builder.Append(',');
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected void StartArray()
			{
				_builder.Append('[');
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected void EndArray()
			{
				TrimComma();
				_builder.Append(']');
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected void AppendKey(string key)
			{
				if (key != null)
				{
					_builder.Append("\"");
					_builder.Append(key);
					_builder.Append("\":");
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected void AppendNull()
			{
				_builder.Append("null");
				AppendSeperator();
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected void AppendBooleanValue(bool value)
			{
				_builder.Append(value ? "true" : "false");
				AppendSeperator();
			}

			// StringBuilder allocated a considerable amount of garbage or int types, but when all else fails... it can append anything...
			protected void AppendValueUsingStringBuilder<T>(T val)
			{
				_builder.Append(val);
				AppendSeperator();
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected void AppendNumericValue(ulong number) { _builder.Append(number); AppendSeperator(); }

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected void AppendNumericValue(long number) { _builder.Append(number); AppendSeperator(); }

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected void AppendNumericValue(uint number) { _builder.Append(number); AppendSeperator(); }

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected void AppendNumericValue(int number) { _builder.Append(number); AppendSeperator(); }

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected void AppendNumericValue(ushort number) { _builder.Append(number); AppendSeperator(); }

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected void AppendNumericValue(short number) { _builder.Append(number); AppendSeperator(); }

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected void AppendNumericValue(byte number) { _builder.Append(number); AppendSeperator(); }

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected void AppendNumericValue(float number) { _builder.Append(number.ToString("R", System.Globalization.CultureInfo.InvariantCulture)); AppendSeperator(); }

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected void AppendNumericValue(double number) { _builder.Append(number.ToString("R", System.Globalization.CultureInfo.InvariantCulture)); AppendSeperator(); }

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected void AppendStringValue(string value)
			{
				if (value == null)
				{
					AppendNull();
				}
				else
				{
					_builder.Append('"');
					for (int i = 0; i < value.Length; ++i)
					{
						char c = value[i];
						AppendEscapedCharacter(c);
					}
					_builder.Append('"');
					AppendSeperator();
				}
			}

			protected void AppendStringValue(StringBuilder value)
			{
				if (value == null)
				{
					AppendNull();
				}
				else
				{
					_builder.Append('"');
					for (int i = 0; i < value.Length; ++i)
					{
						char c = value[i];
						AppendEscapedCharacter(c);
					}
					_builder.Append('"');
					AppendSeperator();
				}
			}

			protected void AppendEscapedCharacter(char c)
			{
				switch (c)
				{
					case '"':
						_builder.Append("\\\"");
						break;
					case '\\':
						_builder.Append("\\\\");
						break;
					case '\b':
						_builder.Append("\\b");
						break;
					case '\f':
						_builder.Append("\\f");
						break;
					case '\n':
						_builder.Append("\\n");
						break;
					case '\r':
						_builder.Append("\\r");
						break;
					case '\t':
						_builder.Append("\\t");
						break;
					default:
						// AR - Not sure if this is needed, but mini json
						// Seems to do something like this for non-ansi
						// int codepoint = Convert.ToInt32(c);
						// if ((codepoint >= 32) && (codepoint <= 126))
						// {
						_builder.Append(c);
						// }
						// else
						// {
						// 	_builder.Append("\\u");
						// 	_builder.Append(codepoint.ToString("x4"));
						// }
						break;
				}
			}


			public bool Serialize(string key, ref IDictionary<string, object> target)
			{
				Dictionary<string, object> asTarget = target as Dictionary<string, object>;
				if (asTarget != null)
					return SerializeDictionary(key, ref asTarget);
				else
					throw new NotSupportedException("Serilization of non-Dictionary IDictionaries is not supported yet");
			}

			public bool Serialize(string key, ref bool target)
			{
				AppendKey(key);
				AppendBooleanValue(target);
				return true;
			}

			public bool Serialize(string key, ref bool? target)
			{
				AppendKey(key);
				if (target.HasValue)
					AppendBooleanValue(target.Value);
				else
					AppendNull();
				return true;
			}

			public bool Serialize(string key, ref int target)
			{
				AppendKey(key);
				AppendNumericValue(target);
				return true;
			}

			public bool Serialize(string key, ref int? target)
			{
				AppendKey(key);
				if (target.HasValue)
					AppendNumericValue(target.Value);
				else
					AppendNull();
				return true;
			}

			public bool Serialize(string key, ref long target)
			{
				AppendKey(key);
				AppendNumericValue(target);
				return true;
			}

			public bool Serialize(string key, ref long? target)
			{
				AppendKey(key);
				if (target.HasValue)
					AppendNumericValue(target.Value);
				else
					AppendNull();
				return true;
			}

			public bool Serialize(string key, ref ulong target)
			{
				AppendKey(key);
				AppendNumericValue(target);
				return true;
			}

			public bool Serialize(string key, ref ulong? target)
			{
				AppendKey(key);
				if (target.HasValue)
					AppendNumericValue(target.Value);
				else
					AppendNull();
				return true;
			}

			public bool Serialize(string key, ref float target)
			{
				AppendKey(key);
				AppendNumericValue(target);
				return true;
			}

			public bool Serialize(string key, ref float? target)
			{
				AppendKey(key);
				if (target.HasValue)
					AppendNumericValue(target.Value);
				else
					AppendNull();
				return true;
			}

			public bool Serialize(string key, ref double target)
			{
				AppendKey(key);
				AppendNumericValue(target);
				return true;
			}

			public bool Serialize(string key, ref double? target)
			{
				AppendKey(key);
				if (target.HasValue)
					AppendNumericValue(target.Value);
				else
					AppendNull();
				return true;
			}

			public bool Serialize(string key, ref string target)
			{
				AppendKey(key);
				AppendStringValue(target);
				return true;
			}

			public bool Serialize(string key, ref Guid target)
			{
				AppendKey(key);
				AppendStringValue(target.ToString());
				return true;
			}

			public bool Serialize(string key, ref StringBuilder target)
			{
				AppendKey(key);
				AppendStringValue(target);
				return true;
			}

			public bool Serialize(string key, ref DateTime target)
			{
				//The "O" or "o" standard format specifier represents a custom date and
				//time format string using a pattern that preserves time zone information.
				string tmp = target.ToString("O");
				AppendKey(key);
				AppendStringValue(tmp);
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
				return SerializeArray(key, ref data);
			}

			public bool Serialize(string key, ref Vector2 target)
			{
				float[] data = new float[2];
				data[0] = target.x;
				data[1] = target.y;
				return SerializeArray(key, ref data);
			}
			public bool Serialize(string key, ref Vector3 target)
			{
				float[] data = new float[3];
				data[0] = target.x;
				data[1] = target.y;
				data[2] = target.z;
				return SerializeArray(key, ref data);
			}
			public bool Serialize(string key, ref Vector4 target)
			{
				float[] data = new float[4];
				data[0] = target.x;
				data[1] = target.y;
				data[2] = target.z;
				data[3] = target.w;
				return SerializeArray(key, ref data);
			}

			public bool Serialize(string key, ref Color target)
			{
				float[] data = new float[4];
				data[0] = target.r;
				data[1] = target.g;
				data[2] = target.b;
				data[3] = target.a;
				return SerializeArray(key, ref data);
			}
			public bool Serialize(string key, ref Quaternion target)
			{
				float[] data = new float[4];
				data[0] = target.x;
				data[1] = target.y;
				data[2] = target.z;
				data[3] = target.w;
				return SerializeArray(key, ref data);
			}
			public bool Serialize(string key, ref Gradient target)
			{
				throw new NotSupportedException("Gradient Not Supported in Direct JSON Serialization");
			}
#endif

			public bool SerializeDictionary<TDict, T>(string key, ref TDict target) where TDict : IDictionary<string, T>, new()
			{
				AppendKey(key);
				if (target == null)
				{
					AppendNull();
				}
				else
				{
					StartObject();
					{
						var iter = target.GetEnumerator();

						var elemType = typeof(T) == typeof(object) ? null : typeof(T);


						while (iter.MoveNext())
						{
							var val = iter.Current;
							SerializeAny(val.Key, val.Value, elemType);
						}
					}
					EndObject();
					AppendSeperator();
				}

				return true;
			}

			public bool SerializeDictionary<T>(string key, ref Dictionary<string, T> target)
			{
				AppendKey(key);
				if (target == null)
				{
					AppendNull();
				}
				else
				{
					StartObject();
					{
						var iter = target.GetEnumerator();

						var elemType = typeof(T) == typeof(object) ? null : typeof(T);


						while (iter.MoveNext())
						{
							var val = iter.Current;
							SerializeAny(val.Key, val.Value, elemType);
						}
					}
					EndObject();
					AppendSeperator();
				}

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
				throw new NotSupportedException("LinkedList<T> llist Not Supported in Direct JSON Serialization");
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private bool SerializeListInternal2<TElem>(string key, List<TElem> value)
				where TElem : ISerializable
			{
				AppendKey(key);
				if (value == null)
				{
					AppendNull();
					return true;
				}

				StartArray();
				{
					foreach (var t in value)
					{
						// TODO: CH: this isn't actually as fast as it could be.
						// The problem is that we need to rely on the dynamic dispatch to the sub object, which costs us like, .3ms from my Benchmarking.
						// I tried to create another variant where you passed in a MethodGroup/delegate, and it saved .3ms, but made for grosser code.
						InternalSerialize(null, t);
					}
				}
				EndArray();
				AppendSeperator();
				return true;
			}

			private bool SerializeListInternal<TList>(string key, TList value)
			   where TList : IList
			{
				AppendKey(key);
				if (value == null)
				{
					AppendNull();
					return true;
				}

				StartArray();
				{
					Type elemType = value.GetType().GetElementType();
					if (elemType == null)
						elemType = value.GetType().GetGenericArguments()[0];
					if (elemType == typeof(object))
						elemType = null;

					for (int i = 0; i < value.Count; i++)
					{
						SerializeAny(null, value[i], elemType);
					}
				}
				EndArray();
				AppendSeperator();
				return true;
			}

			public bool SerializeArray<T>(string key, ref T[] value)
			{
				return SerializeListInternal(key, value);
			}



			private void SerializeAny<T>(string key, T elem, Type elemType = null)
			{
				// We allow the caller to set the type, like for a list, otherwise we just take the generic arg
				if (elemType == null)
				{
					// explicitly handle null here, deferring boxing until it probably is already boxed
					if (elem == null)
					{
						AppendKey(key);
						AppendNull();
						return;
					}

					elemType = elem.GetType();
				}

				if (typeof(ISerializable).IsAssignableFrom(elemType))
				{
					InternalSerialize(key, (ISerializable)elem);
				}
				else if (typeof(IDictionary<string, object>).IsAssignableFrom(elemType))
				{
					var asTmp = elem as IDictionary<string, object>;
					Serialize(key, ref asTmp);
				}
				else if (typeof(IList).IsAssignableFrom(elemType))
				{
					SerializeListInternal(key, elem as IList);
				}
				// Most things can just pass to the generic, but add catchs for the specifics
				else if (typeof(string).IsAssignableFrom(elemType))
				{
					var asTmp = elem as string;
					Serialize(key, ref asTmp);
				}
				else if (typeof(StringBuilder).IsAssignableFrom(elemType))
				{
					var asTmp = elem as StringBuilder;
					Serialize(key, ref asTmp);
				}
				else
				{
					if (!elemType.IsValueType && EqualityComparer<T>.Default.Equals(elem, default(T)))
					{
						AppendKey(key);
						AppendNull();
					}
					else
					{
						AppendKey(key);
						// Numeric types need to be routed to the NumericValue functions, so that Stringbuilder doesn't allocate
						// Since ultimately they get cast anyway, just cast to the largest related data type;
						switch (Type.GetTypeCode(elemType))
						{
							case TypeCode.Byte:
							case TypeCode.UInt16:
							case TypeCode.UInt32:
							case TypeCode.UInt64:
								AppendNumericValue(Convert.ToUInt64(elem));
								break;
							case TypeCode.Int16:
							case TypeCode.Int32:
							case TypeCode.Int64:
								AppendNumericValue(Convert.ToInt64(elem));
								break;
							case TypeCode.Single:
							case TypeCode.Double:
								AppendNumericValue(Convert.ToDouble(elem));
								break;
							case TypeCode.Boolean:
								AppendBooleanValue(Convert.ToBoolean(elem));
								break;
							case TypeCode.DateTime:
								var date = Convert.ToDateTime(elem);
								AppendStringValue(date.ToString("O"));
								break;
							default:
								AppendValueUsingStringBuilder(elem);
								break;
						}
					}
				}
			}

			private bool InternalSerialize<T>(string key, T value)
			   where T : ISerializable
			{
				AppendKey(key);
				if (value == null)
				{
					AppendNull();
				}
				else
				{
					StartObject();
					{
						value.Serialize(this);
					}
					EndObject();
					AppendSeperator();
				}

				return true;
			}


		}
	}
}
