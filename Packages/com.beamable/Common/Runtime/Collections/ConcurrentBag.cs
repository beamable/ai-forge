#if UNITY_WEBGL
#define DISABLE_THREADING
#endif

using System.Collections;
using System.Collections.Generic;

namespace Beamable.Common.Runtime.Collections
{


	public interface IConcurrentBag<T> :
	   IEnumerable<T>,
	   IEnumerable,
	   IReadOnlyCollection<T>
	{
		void Add(T item);
	}

	public class ConcurrentBag<T> : IConcurrentBag<T>
	{
#if !DISABLE_THREADING

		private System.Collections.Concurrent.ConcurrentBag<T> _internal =
		   new System.Collections.Concurrent.ConcurrentBag<T>();

		public int Count => _internal.Count;

		public IEnumerator<T> GetEnumerator() => _internal.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _internal.GetEnumerator();

		public void Add(T item) => _internal.Add(item);
#else
      private HashSet<T> _internal = new HashSet<T>();
      public void Add(T item) => _internal.Add(item);

      public IEnumerator<T> GetEnumerator() => _internal.GetEnumerator();

      IEnumerator IEnumerable.GetEnumerator() => _internal.GetEnumerator();

      public int Count => _internal.Count;
#endif

	}
}
