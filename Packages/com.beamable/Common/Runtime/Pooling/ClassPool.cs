using Beamable.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Common.Pooling
{
	/*
	Simple pool to hold classes. Use:

	class MyClass : ClassPool<MyClass>
	{
	   public LinkedListNode<MyClass> node;
	}

	MyClass.Spawn();
	MyClass.Recycle(myClassInstance);
	*/

	public class ClassPool<T> : IDisposable where T : ClassPool<T>, new()
	{

		public static LinkedList<T> free = new LinkedList<T>();

		public LinkedListNode<T> poolNode;

		public ClassPool()
		{
			poolNode = new LinkedListNode<T>(this as T);
		}

		public static void ClearList(LinkedList<T> list)
		{
			while (list.First != null)
			{
				T val = list.First.Value;
				list.RemoveFirst();
				Recycle(val as ClassPool<T>);
			}
		}

		public static T Spawn()
		{
			var node = free.First;
			if (node == null)
			{
				var t = new T();
				t.poolNode = new LinkedListNode<T>(t);
#if UNITY_EDITOR
            if (!ClassPoolProfiler.totalAllocated.ContainsKey(typeof(T)))
            {
               ClassPoolProfiler.totalAllocated.Add(typeof(T), 1);
            }
            else
            {
               int count = ClassPoolProfiler.totalAllocated[typeof(T)];
               ClassPoolProfiler.totalAllocated[typeof(T)] = count+1;
            }
#endif
				return t;
			}
			var ret = node.Value;
			free.RemoveFirst();
			return ret;
		}

		public static void Recycle(ClassPool<T> obj)
		{
			obj.OnRecycle();
			free.AddFirst(obj.poolNode);
		}

		public static void Preallocate(int count)
		{
			// Skip preallocation if the pool is not empty
			// This tolerates force-restart
			if (free.First != null) return;

			for (int i = 0; i < count; ++i)
			{
				var t = new T();
				t.poolNode = new LinkedListNode<T>(t);
				free.AddFirst(t.poolNode);
			}
#if UNITY_EDITOR
         ClassPoolProfiler.totalAllocated[typeof(T)] = count;
#endif
		}

		public void Recycle()
		{
			Recycle(this);
		}

		public void Dispose()
		{
			Recycle();
		}

		// overload to do cleanup
		public virtual void OnRecycle()
		{
		}

	}

#if UNITY_EDITOR
   public class ClassPoolProfiler
   {
      public static Dictionary<System.Type, int> totalAllocated = new Dictionary<System.Type, int>();

      [UnityEditor.MenuItem(Constants.MenuItems.Windows.Paths.MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES  +
         "/Show ClassPool Stats in Console",
         priority = Constants.MenuItems.Windows.Orders.MENU_ITEM_PATH_WINDOW_PRIORITY_3)]
      public static void PrintStats()
      {
         string s = "";
         foreach (var key in totalAllocated.Keys)
         {
            int count = totalAllocated[key];
            s += count.ToString() + "    " + key.ToString() + "\n";
         }
         Debug.Log(s);
      }

   }
#endif
}
