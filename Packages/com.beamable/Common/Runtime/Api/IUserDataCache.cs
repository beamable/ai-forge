using Beamable.Common.Dependencies;
using System;
using System.Collections.Generic;

namespace Beamable.Common.Api
{
	/// <summary>
	/// A <see cref="UserDataCache{T}"/> is a utility class that stores some generic type per player gamertag.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class UserDataCache<T>
	{
		/// <summary>
		/// A general purpose factory function that returns a new instance of the <see cref="UserDataCache{T}"/>.
		/// </summary>
		public delegate UserDataCache<T> FactoryFunction(string name, long ttlMs, CacheResolver resolver, IDependencyProvider provider);

		/// <summary>
		/// A <see cref="UserDataCache{T}"/> has a mapping from player gamertags to some generic type per player.
		/// This function takes a set of gamertags, and fetches the latest data for each.
		/// </summary>
		public delegate Promise<Dictionary<long, T>> CacheResolver(List<long> gamerTags);

		/// <summary>
		/// Get the given player's data.
		/// If the data does not exist in the cache yet, the <see cref="CacheResolver"/> function will be triggered to resolve the data.
		/// </summary>
		/// <param name="gamerTag">The gamertag for the player to get data for.</param>
		/// <returns>A <see cref="Promise{T}"/> containing the player data.</returns>
		public abstract Promise<T> Get(long gamerTag);

		/// <summary>
		/// Get multiple players' data.
		/// If the players do not have the data in the cache yet, the <see cref="CacheResolver"/> function will be triggered to resolve all the data.
		/// </summary>
		/// <param name="gamerTags">A set of gamertags</param>
		/// <returns>A <see cref="Promise{T}"/> containing a dictionary from gamertag to player data</returns>
		public abstract Promise<Dictionary<long, T>> GetBatch(List<long> gamerTags);

		/// <summary>
		/// Manually set the player data
		/// </summary>
		/// <param name="gamerTag">The gamertag of the player to set data for</param>
		/// <param name="data">The player's new data, which will overwrite the old data.</param>
		public abstract void Set(long gamerTag, T data);

		/// <summary>
		/// Remove a player from the cache. The next time this player's data is requested, the <see cref="CacheResolver"/> will be used
		/// to get the latest data for the player
		/// </summary>
		/// <param name="gamerTag">The gamertag of the player to remove from the cache</param>
		public abstract void Remove(long gamerTag);

		/// <summary>
		/// Remove all players from the cache. The next time any player data is requested, the <see cref="CacheResolver"/> will be used
		/// to get the latest data for the players.
		/// </summary>
		public abstract void Clear();

		protected class UserDataCacheEntry
		{
			public T data;
			private long cacheTime;

			public UserDataCacheEntry(T data)
			{
				this.data = data;
				this.cacheTime = Environment.TickCount;
			}

			/// <summary>
			/// Checks if the entry in the user data cache has outlived a given time-to-live.
			/// </summary>
			/// <param name="ttlMs">A time-to-live in milliseconds</param>
			/// <returns>true if the <see cref="cacheTime"/> is older than the given <see cref="ttlMs"/>, false otherwise</returns>
			public bool IsExpired(long ttlMs)
			{
				if (ttlMs == 0)
				{
					return false;
				}
				return ((Environment.TickCount - cacheTime) > ttlMs);
			}
		}
	}
}
