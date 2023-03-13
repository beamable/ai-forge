#if UNITY_WEBGL
#define DISABLE_THREADING
#endif

using System;
using System.Collections;
using System.Collections.Generic;

namespace Beamable.Common.Runtime.Collections
{

	public interface IConcurrentDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
	{
		bool ContainsKey(TKey key);
		bool TryAdd(TKey key, TValue value);
		bool TryGetValue(TKey key, out TValue value);
		int Count { get; }
		IEnumerable<TValue> Values { get; }
		void Clear();
		bool Remove(TKey key);

		TValue this[TKey key] { get; set; }

	}


	public class ConcurrentDictionary<TKey, TValue> : IConcurrentDictionary<TKey, TValue>
	{
#if !DISABLE_THREADING
		private System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue> _internal =
		   new System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>();

		public bool ContainsKey(TKey key) => _internal.ContainsKey(key);

		public bool TryAdd(TKey key, TValue value) => _internal.TryAdd(key, value);

		public int Count => _internal.Count;
		public IEnumerable<TValue> Values => _internal.Values;
		public void Clear() => _internal.Clear();
		public bool TryGetValue(TKey key, out TValue value) => _internal.TryGetValue(key, out value);
		public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory) => _internal.GetOrAdd(key, valueFactory);

		public TValue this[TKey key]
		{
			get => _internal[key];
			set => _internal[key] = value;
		}


		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _internal.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _internal.GetEnumerator();

		public bool Remove(TKey key) => _internal.TryRemove(key, out _);
#else
      private Dictionary<TKey, TValue> _internal = new Dictionary<TKey, TValue>();
      public bool ContainsKey(TKey key) => _internal.ContainsKey(key);

      public bool TryAdd(TKey key, TValue value)
      {
         if (ContainsKey(key))
         {
            return false;
         }

         _internal.Add(key, value);
         return true;

      }

      public void Clear() => _internal.Clear();

      public bool TryGetValue(TKey key, out TValue value) => _internal.TryGetValue(key, out value);
      public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory) {
        TValue value;
        if (!_internal.TryGetValue(key, out value))
        {
          value = valueFactory(key);
          _internal.Add(key, value);
        }
        return value;
      }

      public int Count => _internal.Count;
      public IEnumerable<TValue> Values => _internal.Values;

      public TValue this[TKey key]
      {
         get => _internal[key];
         set => _internal[key] = value;
      }


      public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _internal.GetEnumerator();

      IEnumerator IEnumerable.GetEnumerator() => _internal.GetEnumerator();

      public bool Remove(TKey key)
      {
	      return _internal.Remove(key);
      }
#endif

	}

}
