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

// Controls whether to use alternate dictionary which trades off
// lower memory usage for a linear search for keys. May be not
// be compatible with users that expect an actual Dictionary.
#define USE_ARRAY_DICT

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Beamable.Serialization.SmallerJSON
{

	struct StringBasedParser : IDisposable
	{

		public static bool IsWhiteSpace(char c)
		{
			switch (c)
			{
				case ' ':
				case '\t':
				case '\n':
				case '\r':
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

		struct StringSlice : IEquatable<StringSlice>
		{
			public readonly string buffer;
			public readonly int start;
			public readonly int length;

			public StringSlice(string buffer, int start, int length)
			{
				this.buffer = buffer;
				this.start = start;
				this.length = length;
			}

			public bool Equals(StringSlice other)
			{
				if (length != other.length) return false;
				for (int i = 0; i < length; ++i)
				{
					if (buffer[start + i] != other.buffer[other.start + i])
					{
						return false;
					}
				}
				return true;
			}

			public override string ToString()
			{
				return buffer.Substring(start, length);
			}
		}

		struct StringCacheEntry
		{
			public StringSlice key;
			public string result;
			public int counter;
		}

		readonly string _chars;
		readonly int _length;
		int _pos;

		private static ThreadLocal<StringCacheEntry[]> safePooledStringCache = new ThreadLocal<StringCacheEntry[]>();

		private static StringCacheEntry[] PooledCacheString
		{
			get => safePooledStringCache.Value;
			set => safePooledStringCache.Value = value;
		}
		readonly StringCacheEntry[] _stringCache;
		int _stringCacheCounter;

		private StringBuilder _builder;

		internal StringBasedParser(string jsonChars)
		{
			_chars = jsonChars;
			_pos = 0;
			_length = _chars.Length;
			_stringCache = GetStringCache();
			_stringCacheCounter = 0;
			_builder = new StringBuilder();
		}

		public void Dispose()
		{
			ReturnStringCache(_stringCache);
		}

		private static StringCacheEntry[] GetStringCache()
		{
			if (PooledCacheString != null)
			{
				var cache = PooledCacheString;
				PooledCacheString = null;
				return cache;
			}

			return new StringCacheEntry[20];
		}

		private static void ReturnStringCache(StringCacheEntry[] cache)
		{
			if (PooledCacheString == null && cache != null)
			{
				for (int i = 0; i < cache.Length; ++i)
				{
					cache[i] = default(StringCacheEntry);
				}
				PooledCacheString = cache;
			}
		}

		IDictionary<string, object> ParseObject()
		{
#if USE_ARRAY_DICT
			var table = new ArrayDict();
#else
         var table = new Dictionary<string, object>();
#endif

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
						var value = ParseValue();
						if (value is ISerializationCallbackReceiver receiver)
							receiver.OnAfterDeserialize();
#if USE_ARRAY_DICT
						table.AddUnchecked(name, value);
#else
               table.Add(name, value);
#endif
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

		public object ParseValue()
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

			bool parsing = true;
			bool hasEscapes = false;
			while (parsing)
			{

				if (_pos >= _length)
				{
					break;
				}

				char c = _chars[_pos++];
				switch (c)
				{
					case '"':
						parsing = false;
						break;
					case '\\':
						if (_pos >= _length)
						{
							parsing = false;
							break;
						}

						if (!hasEscapes)
						{
							_builder.Length = 0;
							_builder.Append(_chars, stringStartPos, _pos - stringStartPos - 1);
							hasEscapes = true;
						}

						char escaped = _chars[_pos++];
						switch (escaped)
						{
							case '"':
							case '\\':
							case '/':
								_builder.Append(escaped);
								break;
							case 'b':
								_builder.Append('\b');
								break;
							case 'f':
								_builder.Append('\f');
								break;
							case 'n':
								_builder.Append('\n');
								break;
							case 'r':
								_builder.Append('\r');
								break;
							case 't':
								_builder.Append('\t');
								break;
							case 'u':
								if (_pos + 3 >= _length)
								{
									parsing = false;
									break;
								}

								uint accum = 0;
								for (int i = 0; i < 4; i++)
								{
									var hexByte = _chars[_pos++];
									uint hexDigit = (uint)ParseHexByte(hexByte);
									if (i > 0)
									{
										accum *= 16;
									}
									accum += hexDigit;
								}
								_builder.Append((char)accum); // TODO: surrogate pairs
								break;
						}
						break;
					default:
						if (hasEscapes)
						{
							_builder.Append(c);
						}
						break;
				}
			}

			int length = _pos - stringStartPos - 1;
			if (length <= 0)
			{
				return string.Empty;
			}

			if (hasEscapes)
			{
				return _builder.ToString();
			}

			var slice = new StringSlice(_chars, stringStartPos, length);
			return useCache ? StringFromCache(slice) : slice.ToString();
		}

		static int ParseHexByte(char hexByte)
		{
			if (hexByte >= '0' && hexByte <= '9')
			{
				return hexByte - (int)'0';
			}
			if (hexByte >= 'a' && hexByte <= 'f')
			{
				return 10 + hexByte - (int)'a';
			}
			if (hexByte >= 'A' && hexByte <= 'F')
			{
				return 10 + hexByte - (int)'A';
			}
			return 0;
		}

		object ParseNumber()
		{
			bool negative = false;
			bool floatingPoint = false;
			if (PeekChar() == '-')
			{
				negative = true;
				++_pos;
			}

			int numberStart = _pos;
			while (_pos < _length)
			{
				char current = PeekChar();
				if (current >= '0' && current <= '9')
				{
					++_pos;
					continue;
				}

				if (current == '.' ||
				   current == 'e' ||
				   current == 'E' ||
				   current == '+' ||
				   current == '-')
				{
					floatingPoint = true;
					++_pos;
					continue;
				}
				break;
			}

			string numberString = _chars.Substring(numberStart, _pos - numberStart);

			if (floatingPoint)
			{
				return ParseFloatingPoint(numberString, negative);
			}
			else
			{
				long result;
				if (long.TryParse(numberString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out result))
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
			double.TryParse(numberString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out parsedDouble);
			if (negative)
			{
				parsedDouble = -parsedDouble;
			}

			return parsedDouble;
		}

		void EatWhitespace()
		{
			while (_pos < _length && IsWhiteSpace(_chars[_pos]))
			{
				++_pos;
			}
		}

		char PeekChar()
		{
			return _chars[_pos];
		}

		char NextByte()
		{
			return _chars[_pos++];
		}

		bool ReadFalse()
		{
			if (_length - _pos < 5)
			{
				return false;
			}

			if ((_chars[_pos + 0] == 'f') &&
			   (_chars[_pos + 1] == 'a') &&
			   (_chars[_pos + 2] == 'l') &&
			   (_chars[_pos + 3] == 's') &&
			   (_chars[_pos + 4] == 'e'))
			{
				_pos += 5;
				return true;
			}
			return false;
		}

		bool ReadTrue()
		{
			if (_length - _pos < 4)
			{
				return false;
			}

			if ((_chars[_pos + 0] == 't') &&
			   (_chars[_pos + 1] == 'r') &&
			   (_chars[_pos + 2] == 'u') &&
			   (_chars[_pos + 3] == 'e'))
			{
				_pos += 4;
				return true;
			}
			return false;
		}

		bool ReadNull()
		{
			if (_length - _pos < 4)
			{
				return false;
			}

			if ((_chars[_pos + 0] == 'n') &&
			   (_chars[_pos + 1] == 'u') &&
			   (_chars[_pos + 2] == 'l') &&
			   (_chars[_pos + 3] == 'l'))
			{
				_pos += 4;
				return true;
			}
			return false;
		}

		TOKEN NextToken()
		{
			EatWhitespace();

			if (_pos >= _length)
			{
				return TOKEN.NONE;
			}

			switch (PeekChar())
			{
				case '{':
					return TOKEN.CURLY_OPEN;
				case '}':
					_pos++;
					return TOKEN.CURLY_CLOSE;
				case '[':
					return TOKEN.SQUARED_OPEN;
				case ']':
					_pos++;
					return TOKEN.SQUARED_CLOSE;
				case ',':
					_pos++;
					return TOKEN.COMMA;
				case '"':
					return TOKEN.STRING;
				case ':':
					return TOKEN.COLON;
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
				case '-':
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
}
