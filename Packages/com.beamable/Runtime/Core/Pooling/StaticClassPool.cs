using System;
using System.Collections.Generic;

namespace Beamable.Pooling
{
	public interface IStaticClassPoolObject
	{
		void OnRecycle();
	}

	public static class StaticClassPool
	{
		private static Dictionary<Type, Stack<IStaticClassPoolObject>> Pool = new Dictionary<Type, Stack<IStaticClassPoolObject>>();

		public static T Spawn<T>() where T : IStaticClassPoolObject, new()
		{
			Stack<IStaticClassPoolObject> typePool;
			if (Pool.TryGetValue(typeof(T), out typePool))
			{
				if (typePool.Count > 0)
				{
					return (T)typePool.Pop();
				}
				else
				{
					return new T();
				}
			}
			else
			{
				Pool.Add(typeof(T), new Stack<IStaticClassPoolObject>());
				return new T();
			}
		}

		public static void Recycle(IStaticClassPoolObject instance)
		{
			instance.OnRecycle();

			Stack<IStaticClassPoolObject> typePool;
			if (Pool.TryGetValue(instance.GetType(), out typePool))
			{
				typePool.Push(instance);
			}
			else
			{
				typePool = new Stack<IStaticClassPoolObject>();
				Pool.Add(instance.GetType(), typePool);
				typePool.Push(instance);
			}
		}

		public static void Preallocate<T>(int count) where T : IStaticClassPoolObject, new()
		{
			Stack<IStaticClassPoolObject> typePool;
			if (!Pool.TryGetValue(typeof(T), out typePool))
			{
				typePool = new Stack<IStaticClassPoolObject>();
				Pool.Add(typeof(T), typePool);
			}

			while (typePool.Count < count)
			{
				typePool.Push(new T());
			}
		}

		public static void Clear()
		{
			Pool.Clear();
		}

		public static void ClearPool<T>() where T : IStaticClassPoolObject
		{
			Stack<IStaticClassPoolObject> typePool;
			if (Pool.TryGetValue(typeof(T), out typePool))
			{
				typePool.Clear();
			}
		}
	}
}
