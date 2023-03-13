using Beamable.Common;
using Beamable.Common.Api.Auth;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beamable.Stats
{
	public static class UserExtensions
	{
		public static Promise<StatCollection> GetStats(this User user, params StatObject[] stats)
		{
			// all stats are assumed to be the same access.
			var access = StatAccess.Public;
			var foundAccess = false;
			for (var i = 0; i < stats.Length; i++)
			{
				if (!foundAccess && stats[i] != null)
				{
					access = stats[i].Access;
					foundAccess = true;
				}

				if (stats[i] != null && stats[i].Access != access)
				{
					throw new Exception("The get stat function only works when all stats are the same access type.");
				}
			}

			return GetStats(user, access, stats.Select(x => x?.StatKey).ToArray()).Map(values =>
			{
				var output = new StatCollection();
				for (var i = 0; i < stats.Length; i++)
				{
					if (stats[i] == null) continue;
					values.TryGetValue(stats[i].StatKey, out var value);
					output[stats[i]] = value ?? stats[i].DefaultValue;
				}

				return output;
			});
		}

		public static Promise<string> GetStat(this User user, StatObject statObject)
		{
			if (statObject == null) return null;

			return API.Instance.FlatMap(de =>
			{
				return de.StatsService.GetStats("client", statObject.Access.GetString(), "player", user.id).Map(all =>
			 {
				 all.TryGetValue(statObject.StatKey, out var result);
				 return result;
			 });
			});
		}

		public static Promise<Dictionary<string, string>> GetStats(this User user, StatAccess access, params string[] stats)
		{
			return API.Instance.FlatMap(de =>
			{
				return de.StatsService.GetStats("client", access.GetString(), "player", user.id).Map(all =>
			 {
				 var output = new Dictionary<string, string>();
				 for (var i = 0; i < stats.Length; i++)
				 {
					 if (string.IsNullOrEmpty(stats[i])) continue;
					 if (all.TryGetValue(stats[i], out var result))
					 {
						 output[stats[i]] = result;
					 }
				 }

				 return output;
			 });
			});
		}

		public class StatCollection : Dictionary<StatObject, string>
		{
			public string Get(StatObject stat)
			{
				if (stat == null) return null;
				TryGetValue(stat, out var value);
				return value ?? stat.DefaultValue;
			}
		}
	}
}
