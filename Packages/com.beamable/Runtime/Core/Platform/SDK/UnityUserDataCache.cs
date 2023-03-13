using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Dependencies;
using Beamable.Coroutines;
using System.Collections;
using System.Collections.Generic;

namespace Beamable.Api
{
	public class UnityUserDataCache<T> : UserDataCache<T>
	{
		private readonly IDependencyProvider _provider;

		public static UnityUserDataCache<T> CreateInstance(string name, long ttlMs, CacheResolver resolver, IDependencyProvider provider)
		{
			return new UnityUserDataCache<T>(name, ttlMs, resolver, provider);
		}

		private string coroutineContext;

		public string Name { get; }
		public long TtlMs { get; }
		public CacheResolver Resolver { get; }

		protected Dictionary<long, UserDataCacheEntry> cache = new Dictionary<long, UserDataCacheEntry>();
		protected List<long> gamerTagsPending = new List<long>();
		protected List<long> gamerTagsInFlight = new List<long>();
		protected Promise<Dictionary<long, T>> nextPromise = new Promise<Dictionary<long, T>>();
		protected Dictionary<long, T> result = new Dictionary<long, T>();


		// If TTL is 0, then never expire anything
		public UnityUserDataCache(string name, long ttlMs, CacheResolver resolver, IDependencyProvider provider)
		{
			_provider = provider;
			coroutineContext = $"userdatacache_{name}";
			Name = name;
			TtlMs = ttlMs;
			Resolver = resolver;
		}

		private IEnumerator ScheduleResolve()
		{
			yield return Yielders.EndOfFrame;
			while (gamerTagsInFlight.Count != 0)
			{
				yield return Yielders.EndOfFrame;
			}
			Resolve();

		}


		public override Promise<T> Get(long gamerTag)
		{
			if (gamerTagsPending.Count == 0)
			{
				PerformScheduleResolve();
			}
			gamerTagsPending.Add(gamerTag);
			return nextPromise.Map(rsp => rsp[gamerTag]);
		}

		public override Promise<Dictionary<long, T>> GetBatch(List<long> gamerTags)
		{
			if (gamerTagsPending.Count == 0)
			{
				PerformScheduleResolve();
			}
			gamerTagsPending.AddRange(gamerTags);
			return nextPromise;
		}

		public override void Set(long gamerTag, T data)
		{
			cache[gamerTag] = new UserDataCacheEntry(data);
		}

		public override void Remove(long gamerTag)
		{
			cache.Remove(gamerTag);
		}

		public override void Clear()
		{
			cache.Clear();
		}

		protected virtual void Resolve()
		{

			// Save in flight state and reset pending state
			var promise = nextPromise;
			nextPromise = new Promise<Dictionary<long, T>>();
			result.Clear();
			gamerTagsInFlight.Clear();

			// Resolve cache
			for (int i = 0; i < gamerTagsPending.Count; i++)
			{
				UserDataCacheEntry found;
				long nextGamerTag = gamerTagsPending[i];
				if (result.ContainsKey(nextGamerTag))
				{
					continue;
				}

				if (cache.TryGetValue(nextGamerTag, out found))
				{
					if (found.IsExpired(TtlMs))
					{
						cache.Remove(nextGamerTag);
						gamerTagsInFlight.Add(nextGamerTag);
					}
					else
					{
						result.Add(nextGamerTag, found.data);
					}
				}
				else
				{
					if (!gamerTagsInFlight.Contains(nextGamerTag))
					{
						gamerTagsInFlight.Add(nextGamerTag);
					}
				}
			}
			gamerTagsPending.Clear();

			// Short circuit if cache deflected everything
			if (gamerTagsInFlight.Count == 0)
			{
				promise.CompleteSuccess(result);
			}
			else
			{
				var resolvedData = Resolver.Invoke(gamerTagsInFlight);
				resolvedData.Then(data =>
				{
					gamerTagsInFlight.Clear();

					// Update cache and fill result
					foreach (var next in data)
					{
						Set(next.Key, next.Value);
						result.Add(next.Key, next.Value);
					}

					// Resolve waiters
					promise.CompleteSuccess(result);
				}).Error(err =>
				{
					gamerTagsInFlight.Clear();
					promise.CompleteError(err);
				});
			}
		}


		protected void PerformScheduleResolve()
		{

			_provider.GetService<CoroutineService>().StartNew(coroutineContext, ScheduleResolve());

		}


	}
}
