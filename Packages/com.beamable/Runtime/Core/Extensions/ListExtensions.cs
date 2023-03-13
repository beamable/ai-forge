using System;
using System.Collections.Generic;

namespace Beamable.Extensions
{
	public static class ExtensionMethods
	{

		////////////////////////////////////////////////////////////
		// IList Extension Methods
		////////////////////////////////////////////////////////////

		[Obsolete("Unsupported Beamable Feature")]
		public static T SelectRandom<T>(this IList<T> list)
		{
			if (list.Count == 0)
			{
				return default(T);
			}

			int index = UnityEngine.Random.Range(0, list.Count);
			return list[index];
		}

		public interface IWeightedListItem { uint Weight { get; } }

		[Obsolete("Unsupported Beamable Feature")]
		public static T SelectRandomWeighted<T>(this IList<T> list) where T : IWeightedListItem
		{
			if (list.Count == 0)
				return default(T);

			if (list.Count == 1)
				return list[0];

			uint total = 0;
			for (int i = 0; i < list.Count; ++i)
			{
				total += list[i].Weight;
			}

			if (total == 0)
				return default(T);

			long roll = UnityEngine.Random.Range(0, (int)total);
			for (int i = 0; i < list.Count; ++i)
			{
				if (roll < list[i].Weight)
					return list[i];

				roll -= list[i].Weight;
			}

			return default(T);
		}

		[Obsolete("Unsupported Beamable Feature")]
		public static void Shuffle<T>(this IList<T> list)
		{
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = UnityEngine.Random.Range(0, n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}

		[Obsolete("Unsupported Beamable Feature")]
		public static bool Overlaps<T>(this IList<T> list, IList<T> other)
		{
			for (int i = 0; i < list.Count; ++i)
			{
				if (other.Contains(list[i]))
				{
					return true;
				}
			}

			return false;
		}

		// jukebox random system; pass it a src array, and it will create/manage the
		// list, shuffle it, restock it, etc, for you.
		[Obsolete("Unsupported Beamable Feature")]
		public static T Jukebox<T>(this List<T> list, T[] src)
		{
			if (src == null || src.Length == 0)
			{
				return default(T);
			}
			if (list.Count == 0)
			{
				list.AddRange(src);
				list.Shuffle();
			}
			else if (list.Count == 1)
			{
				T last = list[0];
				list.Clear();
				list.AddRange(src);
				list.Shuffle();
				if (list[0].Equals(last))
				{
					list.RemoveAt(0);
					list.Add(last);
				}
				return last;
			}
			T ret = list[0];
			list.RemoveAt(0);
			return ret;
		}

		[Obsolete("Unsupported Beamable Feature")]
		public static void AddSorted<T>(this List<T> @this, T item) where T : IComparable<T>
		{
			if (@this.Count == 0)
			{
				@this.Add(item);
				return;
			}
			if (@this[@this.Count - 1].CompareTo(item) <= 0)
			{
				@this.Add(item);
				return;
			}
			if (@this[0].CompareTo(item) >= 0)
			{
				@this.Insert(0, item);
				return;
			}

			int index = @this.BinarySearch(item);
			if (index >= 0)
				return; // Already in the list.

			@this.Insert(~index, item);
		}

		[Obsolete("Unsupported Beamable Feature")]
		public static bool RemoveSorted<T>(this List<T> @this, T item) where T : IComparable<T>
		{
			if (@this.Count == 0)
				return false;

			int index = @this.BinarySearch(item);
			if (index < 0)
				return false;

			@this.RemoveAt(index);
			return true;
		}

		[Obsolete("Unsupported Beamable Feature")]
		public static T UnsafeLast<T>(this List<T> list)
		{
			return list[list.Count - 1];
		}

		[Obsolete("Unsupported Beamable Feature")]
		public static bool IsEmpty<T>(this List<T> list)
		{
			return list.Count == 0;
		}
	}
}
