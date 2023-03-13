/*
 * Simplified fork of MiniJSON optimized to minimize heap allocations
 *
 * Zach Kamsler
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Beamable.Serialization.SmallerJSON
{
	/// <summary>
	/// An exception that is thrown an object cannot be serialized for some reason.
	/// </summary>
	public class CannotSerializeException : Exception
	{
		public CannotSerializeException(string message) : base(message)
		{

		}
	}

	public static class Json
	{
		/// <summary>
		/// Parses the string json into a value
		/// </summary>
		/// <param name="json">a utf8 byte array containing json</param>
		/// <returns>An List&lt;object&gt;, a Dictionary&lt;string, object&gt;, a double, an integer,a string, null, true, or false</returns>
		public static object Deserialize(byte[] json)
		{
			// save the string for debug information
			if (json == null)
			{
				return null;
			}

			var obj = Parser.Parse(json);
			return obj;
		}

		public static object Deserialize(string json)
		{
			// save the string for debug information
			if (json == null)
			{
				return null;
			}

			using (var parser = new StringBasedParser(json))
			{
				var obj = parser.ParseValue();
				return obj;
			}
		}

		public static bool IsValidJson(string strInput)
		{
			if (string.IsNullOrWhiteSpace(strInput))
			{
				return false;
			}

			strInput = strInput.Trim();
			if ((strInput.StartsWith("{") && strInput.EndsWith("}")) ||
				(strInput.StartsWith("[") && strInput.EndsWith("]")))
			{
				try
				{
					var obj = Json.Deserialize(strInput);
					return true;
				}
				catch (Exception)
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		public static string FormatJson(string json, char indent = ' ', int times = 2)
		{
			var indentation = 0;
			var quoteCount = 0;
			var indentString = string.Concat(Enumerable.Repeat(indent, times));
			var result = "";
			foreach (var ch in json)
			{
				switch (ch)
				{
					case '"':
						quoteCount++;
						result += ch;
						break;
					case ',':
						if (quoteCount % 2 == 0)
						{
							result += ch + Environment.NewLine +
									  string.Concat(Enumerable.Repeat(indentString, indentation));
						}

						break;
					case '{':
					case '[':
						var openFormatted = string.Concat(Enumerable.Repeat(indentString, ++indentation))
											+ indent;
						result += string.Format("{0}{1}{2}", ch, Environment.NewLine, openFormatted);
						break;
					case '}':
					case ']':
						var closeFormatted = string.Concat(Enumerable.Repeat(indentString, --indentation));
						result += string.Format("{0}{1}{2}", Environment.NewLine, closeFormatted, ch);
						break;
					default:
						result += ch;
						break;
				}
			}

			return result;
		}

		// a slice into the byte array representing a utf8 string
		struct StringSlice : IEquatable<StringSlice>
		{
			public readonly byte[] array;
			public readonly int start;
			public readonly int length;
			readonly bool ascii;

			public StringSlice(byte[] array, int start, int length, bool ascii)
			{
				this.array = array;
				this.start = start;
				this.length = length;
				this.ascii = ascii;
			}

			public bool Equals(StringSlice other)
			{
				if (length != other.length) return false;
				for (int i = 0; i < length; ++i)
				{
					if (array[start + i] != other.array[other.start + i])
					{
						return false;
					}
				}

				return true;
			}

			public override string ToString()
			{
				var encoding = ascii ? Encoding.ASCII : Encoding.UTF8;
				return encoding.GetString(array, start, length);
			}
		}

		struct StringCacheEntry
		{
			public StringSlice key;
			public string result;
			public int counter;
		}

		sealed class Parser : IDisposable
		{
			public static bool IsWhiteSpace(byte c)
			{
				switch (c)
				{
					case (byte)' ':
					case (byte)'\t':
					case (byte)'\n':
					case (byte)'\r':
						return true;
					default:
						return false;
				}
			}

			enum TOKEN
			{
				NONE,
				CURLY_OPEN,
				CURLY_CLOSE,
				SQUARED_OPEN,
				SQUARED_CLOSE,
				COLON,
				COMMA,
				STRING,
				NUMBER,
				TRUE,
				FALSE,
				NULL
			};

			byte[] _bytes;
			int _pos;

			StringCacheEntry[] _stringCache = new StringCacheEntry[20];
			int _stringCacheCounter;

			Parser(byte[] jsonBytes)
			{
				_bytes = jsonBytes;
			}

			public static object Parse(byte[] jsonBytes)
			{
				using (var instance = new Parser(jsonBytes))
				{
					return instance.ParseValue();
				}
			}

			public void Dispose()
			{
				_bytes = null;
				_pos = 0;
			}

			IDictionary<string, object> ParseObject()
			{
				var table = new ArrayDict();

				// ditch opening brace
				_pos++;

				// {
				while (true)
				{
					switch (NextToken())
					{
						case TOKEN.NONE:
							return null;
						case TOKEN.COMMA:
							continue;
						case TOKEN.CURLY_CLOSE:
							return table;
						default:
							// name
							string name = ParseString(true);
							if (name == null)
							{
								return null;
							}

							// :
							if (NextToken() != TOKEN.COLON)
							{
								return null;
							}

							// ditch the colon
							_pos++;

							// value
							table.AddUnchecked(name, ParseValue());
							break;
					}
				}
			}

			// re-use string keys using lru cache
			string StringFromCache(StringSlice slice)
			{
				int length = _stringCache.Length;
				int lowestCounter = int.MaxValue;
				int lowestCounterIndex = 0;
				for (int i = 0; i < length; ++i)
				{
					if (slice.Equals(_stringCache[i].key))
					{
						_stringCache[i].counter = ++_stringCacheCounter;
						return _stringCache[i].result;
					}

					var counter = _stringCache[i].counter;
					if (counter < lowestCounter)
					{
						lowestCounter = counter;
						lowestCounterIndex = i;
					}
				}

				string result = slice.ToString();
				_stringCache[lowestCounterIndex].key = slice;
				_stringCache[lowestCounterIndex].result = result;
				_stringCache[lowestCounterIndex].counter = ++_stringCacheCounter;
				return result;
			}

			List<object> ParseArray()
			{
				var array = new List<object>();

				// ditch opening bracket
				_pos++;

				// [
				var parsing = true;
				while (parsing)
				{
					TOKEN nextToken = NextToken();

					switch (nextToken)
					{
						case TOKEN.NONE:
							return null;
						case TOKEN.COMMA:
							continue;
						case TOKEN.SQUARED_CLOSE:
							parsing = false;
							break;
						default:
							object value = ParseByToken(nextToken);

							array.Add(value);
							break;
					}
				}

				return array;
			}

			object ParseValue()
			{
				TOKEN nextToken = NextToken();
				return ParseByToken(nextToken);
			}

			object ParseByToken(TOKEN token)
			{
				switch (token)
				{
					case TOKEN.STRING:
						return ParseString(false);
					case TOKEN.NUMBER:
						return ParseNumber();
					case TOKEN.CURLY_OPEN:
						return ParseObject();
					case TOKEN.SQUARED_OPEN:
						return ParseArray();
					case TOKEN.TRUE:
						return true;
					case TOKEN.FALSE:
						return false;
					case TOKEN.NULL:
						return null;
					default:
						return null;
				}
			}

			string ParseString(bool useCache)
			{
				// ditch opening quote
				_pos++;

				int stringStartPos = _pos;
				int currentInPos = _pos;

				bool ascii = true;
				bool parsing = true;
				while (parsing)
				{
					if (_pos >= _bytes.Length)
					{
						parsing = false;
						break;
					}

					byte c = _bytes[_pos++];
					if ((c & 0x80) != 0)
					{
						ascii = false;
					}

					switch (c)
					{
						case (byte)'"':
							parsing = false;
							break;
						case (byte)'\\':
							if (_pos >= _bytes.Length)
							{
								parsing = false;
								break;
							}

							byte escaped = _bytes[_pos++];
							switch (escaped)
							{
								case (byte)'"':
								case (byte)'\\':
								case (byte)'/':
									_bytes[currentInPos++] = escaped;
									break;
								case (byte)'b':
									_bytes[currentInPos++] = (byte)'\b';
									break;
								case (byte)'f':
									_bytes[currentInPos++] = (byte)'\f';
									break;
								case (byte)'n':
									_bytes[currentInPos++] = (byte)'\n';
									break;
								case (byte)'r':
									_bytes[currentInPos++] = (byte)'\r';
									break;
								case (byte)'t':
									_bytes[currentInPos++] = (byte)'\t';
									break;
								case (byte)'u':
									ascii = false;
									if (_pos + 3 >= _bytes.Length)
									{
										parsing = false;
										break;
									}

									uint accum = 0;
									for (int i = 0; i < 4; i++)
									{
										var hexByte = _bytes[_pos++];
										uint hexDigit = (uint)ParseHexByte(hexByte);
										if (i > 0)
										{
											accum *= 16;
										}

										accum += hexDigit;
									}

									currentInPos = WriteUtf8(accum, currentInPos);
									break;
							}

							break;
						default:
							_bytes[currentInPos++] = c;
							break;
					}
				}

				if (currentInPos == stringStartPos)
				{
					return string.Empty;
				}

				var slice = new StringSlice(_bytes, stringStartPos, currentInPos - stringStartPos, ascii);
				if (useCache)
				{
					return StringFromCache(slice);
				}
				else
				{
					return slice.ToString();
				}
			}

			// writes a utf8 code point returns the index after the end
			int WriteUtf8(uint codePoint, int startIndex)
			{
				int outPos = startIndex;
				if (codePoint < 0x80)
				{
					_bytes[outPos++] = (byte)codePoint;
				}
				else if (codePoint <= 0x7FF)
				{
					_bytes[outPos++] = (byte)((codePoint >> 6) + 0xC0);
					_bytes[outPos++] = (byte)((codePoint & 0x3F) + 0x80);
				}
				else if (codePoint <= 0xFFFF)
				{
					_bytes[outPos++] = (byte)((codePoint >> 12) + 0xE0);
					_bytes[outPos++] = (byte)(((codePoint >> 6) & 0x3F) + 0x80);
					_bytes[outPos++] = (byte)((codePoint & 0x3F) + 0x80);
				}
				else if (codePoint <= 0x10FFFF)
				{
					_bytes[outPos++] = (byte)((codePoint >> 18) + 0xF0);
					_bytes[outPos++] = (byte)(((codePoint >> 12) & 0x3F) + 0x80);
					_bytes[outPos++] = (byte)(((codePoint >> 6) & 0x3F) + 0x80);
					_bytes[outPos++] = (byte)((codePoint & 0x3F) + 0x80);
				}

				return outPos;
			}

			static int ParseHexByte(byte hexByte)
			{
				if (hexByte >= (byte)'0' && hexByte <= (byte)'9')
				{
					return hexByte - (byte)'0';
				}

				if (hexByte >= (byte)'a' && hexByte <= (byte)'f')
				{
					return 10 + hexByte - (byte)'a';
				}

				if (hexByte >= (byte)'A' && hexByte <= (byte)'F')
				{
					return 10 + hexByte - (byte)'A';
				}

				return 0;
			}

			object ParseNumber()
			{
				bool negative = false;
				bool floatingPoint = false;
				if (PeekByte() == (byte)'-')
				{
					negative = true;
					++_pos;
				}

				int numberStart = _pos;
				while (_pos < _bytes.Length)
				{
					byte current = PeekByte();
					if (current >= (byte)'0' && current <= (byte)'9')
					{
						++_pos;
						continue;
					}

					if (current == (byte)'.' ||
						current == (byte)'e' ||
						current == (byte)'E' ||
						current == (byte)'+' ||
						current == (byte)'-')
					{
						floatingPoint = true;
						++_pos;
						continue;
					}

					break;
				}

				string numberString = Encoding.ASCII.GetString(_bytes, numberStart, _pos - numberStart);

				if (floatingPoint)
				{
					return ParseFloatingPoint(numberString, negative);
				}
				else
				{
					long result;
					if (long.TryParse(numberString, System.Globalization.NumberStyles.Any,
									  System.Globalization.CultureInfo.InvariantCulture, out result))
					{
						if (negative)
							result = -result;

						return result;
					}
					else
						return ParseFloatingPoint(numberString, negative);
				}
			}

			double ParseFloatingPoint(string numberString, bool negative)
			{
				double parsedDouble;
				double.TryParse(numberString, System.Globalization.NumberStyles.Any,
								System.Globalization.CultureInfo.InvariantCulture, out parsedDouble);
				if (negative)
				{
					parsedDouble = -parsedDouble;
				}

				return parsedDouble;
			}

			void EatWhitespace()
			{
				while (_pos < _bytes.Length && IsWhiteSpace(_bytes[_pos]))
				{
					++_pos;
				}
			}

			byte PeekByte()
			{
				return _bytes[_pos];
			}

			byte NextByte()
			{
				return _bytes[_pos++];
			}

			bool ReadFalse()
			{
				if (_bytes.Length - _pos < 5)
				{
					return false;
				}

				if ((_bytes[_pos + 0] == (byte)'f') &&
					(_bytes[_pos + 1] == (byte)'a') &&
					(_bytes[_pos + 2] == (byte)'l') &&
					(_bytes[_pos + 3] == (byte)'s') &&
					(_bytes[_pos + 4] == (byte)'e'))
				{
					_pos += 5;
					return true;
				}

				return false;
			}

			bool ReadTrue()
			{
				if (_bytes.Length - _pos < 4)
				{
					return false;
				}

				if ((_bytes[_pos + 0] == (byte)'t') &&
					(_bytes[_pos + 1] == (byte)'r') &&
					(_bytes[_pos + 2] == (byte)'u') &&
					(_bytes[_pos + 3] == (byte)'e'))
				{
					_pos += 4;
					return true;
				}

				return false;
			}

			bool ReadNull()
			{
				if (_bytes.Length - _pos < 4)
				{
					return false;
				}

				if ((_bytes[_pos + 0] == (byte)'n') &&
					(_bytes[_pos + 1] == (byte)'u') &&
					(_bytes[_pos + 2] == (byte)'l') &&
					(_bytes[_pos + 3] == (byte)'l'))
				{
					_pos += 4;
					return true;
				}

				return false;
			}

			TOKEN NextToken()
			{
				EatWhitespace();

				if (_pos >= _bytes.Length)
				{
					return TOKEN.NONE;
				}

				switch (PeekByte())
				{
					case (byte)'{':
						return TOKEN.CURLY_OPEN;
					case (byte)'}':
						_pos++;
						return TOKEN.CURLY_CLOSE;
					case (byte)'[':
						return TOKEN.SQUARED_OPEN;
					case (byte)']':
						_pos++;
						return TOKEN.SQUARED_CLOSE;
					case (byte)',':
						_pos++;
						return TOKEN.COMMA;
					case (byte)'"':
						return TOKEN.STRING;
					case (byte)':':
						return TOKEN.COLON;
					case (byte)'0':
					case (byte)'1':
					case (byte)'2':
					case (byte)'3':
					case (byte)'4':
					case (byte)'5':
					case (byte)'6':
					case (byte)'7':
					case (byte)'8':
					case (byte)'9':
					case (byte)'-':
						return TOKEN.NUMBER;
				}

				if (ReadFalse())
					return TOKEN.FALSE;
				if (ReadTrue())
					return TOKEN.TRUE;
				if (ReadNull())
					return TOKEN.NULL;

				return TOKEN.NONE;
			}
		}

		/// <summary>
		/// Converts a IDictionary / IList object or a simple type (string, int, etc.) into a JSON string
		/// </summary>
		/// <param name="json">A Dictionary&lt;string, object&gt; / List&lt;object&gt;</param>
		/// <returns>A JSON encoded string, or null if object 'json' is not serializable</returns>
		public static string Serialize(object obj, StringBuilder builder)
		{
			return Serializer.Serialize(obj, builder);
		}

		sealed class Serializer
		{
			public static string Serialize(object obj, StringBuilder builder)
			{
				if (builder == null)
					builder = new StringBuilder(512);
				else
					builder.Length = 0;
				SerializeValue(obj, builder);
				return builder.ToString();
			}

			private static void SerializeValue(object value, StringBuilder builder)
			{
				IList asList;
				IDictionary asDict;
				string asStr;

				if (value == null)
				{
					builder.Append("null");
				}
				else if ((asStr = value as string) != null)
				{
					SerializeString(asStr, builder);
				}
				else if (value is bool)
				{
					builder.Append((bool)value ? "true" : "false");
				}
				else if ((asList = value as IList) != null)
				{
					if (value is ISerializationCallbackReceiver receiver)
					{
						receiver.OnBeforeSerialize();
					}

					SerializeArray(asList, builder);
				}
				else if ((asDict = value as IDictionary) != null)
				{
					if (value is ISerializationCallbackReceiver receiver)
					{
						receiver.OnBeforeSerialize();
					}

					SerializeObject(asDict, builder);
				}
				else if (value is char)
				{
					SerializeString(new string((char)value, 1), builder);
				}
				else
				{
					if (value is ISerializationCallbackReceiver receiver)
					{
						receiver.OnBeforeSerialize();
					}

					SerializeOther(value, builder);
				}
			}

			private static void SerializeObject(IDictionary obj, StringBuilder builder)
			{
				bool first = true;

				builder.Append('{');

				foreach (object e in obj.Keys)
				{
					if (!first)
					{
						builder.Append(',');
					}

					SerializeString(e.ToString(), builder);
					builder.Append(':');

					SerializeValue(obj[e], builder);

					first = false;
				}

				builder.Append('}');
			}

			private static void SerializeArray(IList anArray, StringBuilder builder)
			{
				builder.Append('[');

				bool first = true;

				for (int i = 0; i < anArray.Count; i++)
				{
					object obj = anArray[i];
					if (!first)
					{
						builder.Append(',');
					}

					SerializeValue(obj, builder);

					first = false;
				}

				builder.Append(']');
			}

			private static void SerializeRaw(IRawJsonProvider provider, StringBuilder builder)
			{
				var str = provider.ToJson();
				char[] charArray = str.ToCharArray();
				for (int i = 0; i < charArray.Length; i++)
				{
					char c = charArray[i];
					switch (c)
					{
						default:
							int codepoint = Convert.ToInt32(c);
							if ((codepoint >= 32) && (codepoint <= 126))
							{
								builder.Append(c);
							}
							else
							{
								builder.Append("\\u");
								builder.Append(codepoint.ToString("x4"));
							}

							break;
					}
				}
			}

			private static void SerializeString(string str, StringBuilder builder)
			{
				builder.Append('\"');

				char[] charArray = str.ToCharArray();
				for (int i = 0; i < charArray.Length; i++)
				{
					char c = charArray[i];
					switch (c)
					{
						case '"':
							builder.Append("\\\"");
							break;
						case '\\':
							builder.Append("\\\\");
							break;
						case '\b':
							builder.Append("\\b");
							break;
						case '\f':
							builder.Append("\\f");
							break;
						case '\n':
							builder.Append("\\n");
							break;
						case '\r':
							builder.Append("\\r");
							break;
						case '\t':
							builder.Append("\\t");
							break;
						default:
							int codepoint = Convert.ToInt32(c);
							if ((codepoint >= 32) && (codepoint <= 126))
							{
								builder.Append(c);
							}
							else
							{
								builder.Append("\\u");
								builder.Append(codepoint.ToString("x4"));
							}

							break;
					}
				}

				builder.Append('\"');
			}

			private static void SerializeOther(object value, StringBuilder builder)
			{
				// NOTE: decimals lose precision during serialization.
				// They always have, I'm just letting you know.
				// Previously floats and doubles lost precision too.
				if (value is float)
				{
					builder.Append(((float)value).ToString("R", System.Globalization.CultureInfo.InvariantCulture));
				}
				else if (value is int
						 || value is uint
						 || value is long
						 || value is sbyte
						 || value is byte
						 || value is short
						 || value is ushort
						 || value is ulong)
				{
					builder.Append(value);
				}
				else if (value is double
						 || value is decimal)
				{
#if BEAMABLE_JSON_ALLOW_NAN
					builder.Append(Convert.ToDouble(value)
					                      .ToString("R", System.Globalization.CultureInfo.InvariantCulture));
#else
					var dbl = Convert.ToDouble(value);
					if (double.IsNaN(dbl) || double.IsInfinity(dbl))
					{
						throw new CannotSerializeException("Beamable cannot serialize values that are NaN.");
					}
					builder.Append(dbl.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
#endif
				}
				else if (value is IRawJsonProvider provider)
				{
					SerializeRaw(provider, builder);
				}
				else
				{
					SerializeString(value.ToString(), builder);
				}
			}
		}
	}
}
