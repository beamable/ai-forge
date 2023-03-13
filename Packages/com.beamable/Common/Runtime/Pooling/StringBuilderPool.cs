using System;
using System.Collections.Generic;
using System.Text;

namespace Beamable.Common.Pooling
{
	public class StringBuilderPool
	{
#if UNITY_WEBGL
      private static StringBuilderPool _instance;
      private static StringBuilderPool _largePoolInstance;
      public static StringBuilderPool StaticPool
      {
         get
         {
            if (_instance == null)
            {
               _instance = new StringBuilderPool(4, 64);
            }

            return _instance;
         }
      }
	  public static StringBuilderPool LargeStaticPool
      {
         get
         {
            if (_largePoolInstance == null)
            {
               _largePoolInstance = new StringBuilderPool(3, 512);
            }

            return _largePoolInstance;
         }
      }
#else
		private static System.Threading.ThreadLocal<StringBuilderPool> safeStaticPoolLocal = new System.Threading.ThreadLocal<StringBuilderPool>();

		private static System.Threading.ThreadLocal<StringBuilderPool> kLargeStaticPool =
			new System.Threading.ThreadLocal<StringBuilderPool>();

		public static StringBuilderPool StaticPool
		{
			get
			{
				if (safeStaticPoolLocal.Value == null)
				{
					safeStaticPoolLocal.Value = new StringBuilderPool(4, 64);
				}

				return safeStaticPoolLocal.Value;
			}
		}

		public static StringBuilderPool LargeStaticPool
		{
			get
			{
				if (kLargeStaticPool.Value == null)
				{
					kLargeStaticPool.Value = new StringBuilderPool(3, 512);
				}

				return kLargeStaticPool.Value;
			}
		}
#endif

		public enum EmptyPoolBehavior
		{
			AllocateToHeap = 0, // Beyond capacity, builders are allocated into the heap to be garbage collected, this limits the amount of total memory this pool may use at a time.
			AllocateToPool = 1 // Beyond capacity, builders are added to the pool, this limits GC, but means more reserved memory for the pool
		}

		private Stack<PooledStringBuilder> mBuilderStack;
		private int mStartingCapacity;
		private EmptyPoolBehavior mEmptyBehavior;

		private StringBuilderPool(int builderCapacity, int startingCapacity, EmptyPoolBehavior emptyBehavior = EmptyPoolBehavior.AllocateToHeap)
		{
			mStartingCapacity = startingCapacity;
			mEmptyBehavior = emptyBehavior;

			mBuilderStack = new Stack<PooledStringBuilder>(builderCapacity);
			for (int i = 0; i < builderCapacity; i++)
			{
				mBuilderStack.Push(new PooledStringBuilder(this, mStartingCapacity));
			}
		}

		public PooledStringBuilder Spawn()
		{
			PooledStringBuilder builder = null;
			if (mBuilderStack.Count == 0)
			{
				switch (mEmptyBehavior)
				{
					case EmptyPoolBehavior.AllocateToPool:
						builder = new PooledStringBuilder(this, mStartingCapacity);
						break;
					case EmptyPoolBehavior.AllocateToHeap:
						// Without a pool reference, on dispose these will deactivate and remove their own pool
						builder = new PooledStringBuilder(null, mStartingCapacity);
						break;
				}
			}
			else
			{
				builder = mBuilderStack.Pop();
			}
			builder.Active = true;
			builder.Builder.Length = 0; // Clear the StringBuilder
			return builder;
		}

		private void Recycle(PooledStringBuilder poolChild)
		{
			mBuilderStack.Push(poolChild);
		}



		// This is a wrapper around a string builder, which lets us use using
		// which is the prefered method of use. Though you are of course free to
		// call Spawn and do whatever. Only PooledStringBuilder can Recycle.
		public class PooledStringBuilder : IDisposable
		{
			private StringBuilderPool mPool;
			private StringBuilder mBuilder;
			internal bool Active;

			internal PooledStringBuilder(StringBuilderPool inPool, int capacity)
			{
				mPool = inPool;
				mBuilder = new StringBuilder(capacity);
				Active = false;
			}

			public StringBuilder Builder
			{
				get
				{
					BeamableLogger.Assert(Active);
					return mBuilder; // If inactive, state of builder is undefined.
				}
			}

			public void Dispose()
			{
				BeamableLogger.Assert(Active);
				if (!Active)
					return;

				Active = false;
				if (mPool != null)
					mPool.Recycle(this);
				else
					mBuilder = null; // We arent pooled, but you should sure as shit not be using me after this.
			}
		}
	}
}
