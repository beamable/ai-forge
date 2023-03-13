using System;
using System.Collections.Generic;

namespace Beamable.Common.Api.Stats
{
	public interface IStatsApi
	{
		/// <summary>
		/// Get the <see cref="UserDataCache{T}"/> for the stat prefix
		/// </summary>
		/// <param name="prefix">
		/// A stat prefix is a dot separated string containing the ordered set of stat clauses.
		/// <para>{domain}.{access}.{type}</para>
		/// Domain can be "client" or "game"
		/// Access can be "public" or "private".
		/// Type should always be "player".
		/// </param>
		/// <returns>The <see cref="UserDataCache{T}"/> containing stats given the prefix.</returns>
		UserDataCache<Dictionary<string, string>> GetCache(string prefix);

		/// <summary>
		/// Get the <see cref="UserDataCache{T}"/> for the stat keys
		/// </summary>
		/// <param name="domain">Should be either "client" or "game"</param>
		/// <param name="access">Should be "public" or "private"</param>
		/// <param name="type">should always be "player" </param>
		/// <returns>The <see cref="UserDataCache{T}"/> containing stats given the prefix.</returns>
		UserDataCache<Dictionary<string, string>> GetCache(string domain, string access, string type);

		/// <summary>
		/// Removes any stored data for all local stats.
		/// </summary>
		void ClearCaches();

		/// <summary>
		/// Set the current player's client player stats.
		/// </summary>
		/// <param name="access">
		/// "public" or "private".
		/// Should always be "public", unless you are executing this method as a privileged user or from a Microserivce.</param>
		/// <param name="stats">
		/// A dictionary of stat keys and values to set. This will overwrite ONLY the stats that are present in the given dictionary.
		/// </param>
		/// <returns>A <see cref="Promise{T}"/> representing the network call.</returns>
		Promise<EmptyResponse> SetStats(string access, Dictionary<string, string> stats);

		/// <summary>
		/// Get all of the stats for a given player id
		/// </summary>
		/// <param name="domain">
		/// "client" or "game".
		/// Should always be "client" unless you are executing this method as a privileged user or from a Microservice.
		/// </param>
		/// <param name="access">
		/// "public" or "private". Should always be "public" unless you  are executing this method as a privileged user or from a Microserivce.
		/// </param>
		/// <param name="type">
		/// Should always be "player".
		/// </param>
		/// <param name="id">
		/// The player id to get stats for
		/// </param>
		/// <returns>
		/// A dictionary containing all of the stats for the given domain, access, and player id.
		/// </returns>
		Promise<Dictionary<string, string>> GetStats(string domain, string access, string type, long id);
	}

	[Serializable]
	public class BatchReadStatsResponse
	{
		public List<BatchReadEntry> results;

		public Dictionary<long, Dictionary<string, string>> ToDictionary()
		{
			Dictionary<long, Dictionary<string, string>> result = new Dictionary<long, Dictionary<string, string>>();
			foreach (var entry in results)
			{
				result[entry.id] = entry.ToStatsDictionary();
			}
			return result;
		}
	}

	[Serializable]
	public class BatchReadStatsRequest
	{
		public string objectIds;
		public string stats;
		public string format;
	}

	[Serializable]
	public class BatchReadEntry
	{
		public long id;
		public List<StatEntry> stats;

		public Dictionary<string, string> ToStatsDictionary()
		{
			Dictionary<string, string> result = new Dictionary<string, string>();
			foreach (var stat in stats)
			{
				var value = $"{stat.v}";
#if DB_MICROSERVICE
            if (stat.v is Newtonsoft.Json.Linq.JContainer jContainer)
            {
               value = jContainer.ToString(Newtonsoft.Json.Formatting.None);
            }
#endif

				result[stat.k] = value;
			}
			return result;
		}
	}

	[Serializable]
	public class StatEntry
	{
		public string k;
#if DB_MICROSERVICE
      public object v;
#else
		public string v;
#endif
	}

	[Serializable]
	public class StatUpdates
	{
		public List<StatEntry> set;

		public StatUpdates(Dictionary<string, string> stats)
		{
			set = new List<StatEntry>();
			foreach (var stat in stats)
			{
				var entry = new StatEntry { k = stat.Key, v = stat.Value };
				set.Add(entry);
			}
		}
	}
}
