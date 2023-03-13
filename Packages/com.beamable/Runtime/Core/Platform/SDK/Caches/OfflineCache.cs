using Beamable.Common;
using Beamable.Common.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Beamable.Api.Caches
{
	public class OfflineCache
	{
		public bool UseOfflineCache { get; }

		public OfflineCache(bool useOfflineCache = true)
		{
			UseOfflineCache = useOfflineCache;
			// Flush cache that wasn't created with this version of the game.
			FlushInvalidCache();
		}

		private const string _offlineCacheRoot = "beamable";
		private const string _offlineCacheDir = "cache";
		private const string _offlineCacheExtension = ".json";

		private Dictionary<string, object> _offlineCacheData = new Dictionary<string, object>();

		private readonly static string _cid = Config.ConfigDatabase.HasKey("cid") ? Config.ConfigDatabase.GetString("cid") : "";
		private readonly static string _pid = Config.ConfigDatabase.HasKey("pid") ? Config.ConfigDatabase.GetString("pid") : "";
		private readonly static string _offlineCacheRootDir = Path.Combine(Application.persistentDataPath, _offlineCacheRoot, _offlineCacheDir, _cid, _pid, Application.version);
		private readonly MD5 _md5 = MD5.Create();

		private string GetKey(string key, IAccessToken token, bool includeAuthHeader)
		{
			if (!includeAuthHeader)
			{
				return key;
			}
			else
			{
				return key + token?.RefreshToken;
			}
		}

		public Promise<T> Get<T>(string key, IAccessToken token, bool includeAuthHeader)
		{
			return Read<T>(GetHash(GetKey(key, token, includeAuthHeader)), $"url=[{key}], token=[{token.Token}]");
			// .RecoverFromNoConnectivity(ex => throw new NoConnectivityException($"url=[{key}], token=[{token.Token}]\n{ex.Message}"));
		}

		public void Set<T>(string key, T data, IAccessToken token, bool includeAuthHeader)
		{
			Update(GetHash(GetKey(key, token, includeAuthHeader)), data);
		}

		public void Merge<TKey, TValue>(string key, IAccessToken token, Dictionary<long, Dictionary<TKey, TValue>> data)
		{
			Merge(key + token?.RefreshToken, data);
		}

		public bool Exists(string key, IAccessToken token, bool includeAuthHeader)
		{
			string actualKey = GetHash(GetKey(key, token, includeAuthHeader));
			bool existsInCache = _offlineCacheData.ContainsKey(actualKey);
			bool existsOnDisk = File.Exists(GetFullPathForKey(actualKey));

			return existsInCache || existsOnDisk;
		}

		public Promise<Dictionary<long, Dictionary<TKey, TValue>>> RecoverDictionary<TKey, TValue>(Exception ex, string key,
			IAccessToken token,
			List<long> gamerTags)
		{
			return HandleDictionaryCase<TKey, TValue>(ex, key + token?.RefreshToken, gamerTags);
		}


		public void FlushInvalidCache()
		{
			DeleteCache();
		}

		private void DeleteCache()
		{
			if (Directory.Exists(Directory.GetParent(_offlineCacheRootDir).ToString()))
			{
				string[] dirs = Directory.GetDirectories(Directory.GetParent(_offlineCacheRootDir).ToString());
				foreach (string dir in dirs)
				{
					if (Path.GetFileName(dir) != Application.version)
					{
						Directory.Delete(dir, true);
					}
				}
			}
		}

		private Promise<Dictionary<long, Dictionary<TKey, TValue>>> HandleDictionaryCase<TKey, TValue>(Exception ex, string key,
			List<long> gamerTags)
		{
			if (ex is NoConnectivityException)
			{
				return Read<OfflineUserCache>(key).Map(cacheData =>
				{
					var data = CacheToDict<TKey, TValue>(cacheData);
					var output = new Dictionary<long, Dictionary<TKey, TValue>>();
					foreach (var gamerTag in gamerTags)
					{
						if (data.ContainsKey(gamerTag))
						{
							output.Add(gamerTag, data[gamerTag]);
						}
						else
						{
							Debug.LogError("No cached data for " + gamerTag);
							throw ex;
						}
					}
					return output;
				});
			}
			else
			{
				throw ex;
			}
		}

		private void Merge<TKey, TValue>(string key, Dictionary<long, Dictionary<TKey, TValue>> nextData)
		{
			Read<OfflineUserCache>(key)
				.RecoverWith(err => Promise<OfflineUserCache>.Successful(new OfflineUserCache()))
				.Then(currentData =>
			{
				var result = new Dictionary<long, Dictionary<TKey, TValue>>();
				var currentDictionary = CacheToDict<TKey, TValue>(currentData); //Convert offlinecache data to dictionary

				//Take the Union of the data
				var dictionaries = new Dictionary<long, Dictionary<TKey, TValue>>[] { currentDictionary, nextData };

				foreach (var dict in dictionaries)
				{
					foreach (var kvp in dict)
					{
						result[kvp.Key] = kvp.Value;
					}
				}

				//Write updated offlinecache to disk
				Update(key, DictToCache<TKey, TValue>(result));
			});
		}
		public static Dictionary<K, V> Merge<K, V>(IEnumerable<Dictionary<K, V>> dictionaries)
		{
			Dictionary<K, V> result = new Dictionary<K, V>();
			foreach (Dictionary<K, V> dict in dictionaries)
			{
				result = result.Union(dict)
					.GroupBy(g => g.Key)
					.ToDictionary(pair => pair.Key, pair => pair.First().Value);
			}
			return result;
		}

		private void Update<T>(string key, T data)
		{
			if (_offlineCacheData.ContainsKey(key))
			{
				if (!_offlineCacheData[key].Equals(data))
				{
					_offlineCacheData[key] = data;
					WriteCacheToDisk(key, data);
				}
			}
			else
			{
				_offlineCacheData.Add(key, data);
				WriteCacheToDisk(key, data);
			}
		}

		private Promise<T> Read<T>(string key, string desc = "")
		{
			Promise<T> _localCacheResponse = new Promise<T>();

			if (_offlineCacheData.TryGetValue(key, out var _cacheFromMemory))
			{
				_localCacheResponse.CompleteSuccess((T)_cacheFromMemory);
			}
			else
			{
				if (!File.Exists(GetFullPathForKey(key)))
				{
					return Promise<T>.Failed(new NoConnectivityException($"{desc} {key} is not cached and requires internet connectivity."));
				}
				_offlineCacheData.Add(key, ReadCacheFromDisk<T>(key));
				_localCacheResponse.CompleteSuccess((T)_offlineCacheData[key]);
			}

			return _localCacheResponse;
		}

		private string GetFullPathForKey(string key)
		{
			return Path.Combine(_offlineCacheRootDir, key) + _offlineCacheExtension;
		}

		private void WriteCacheToDisk(string key, object data)
		{
			Directory.CreateDirectory(_offlineCacheRootDir);
			var json = JsonUtility.ToJson(data);
			File.WriteAllText(GetFullPathForKey(key), json);
		}
		private object ReadCacheFromDisk<T>(string key)
		{
			return JsonUtility.FromJson<T>(File.ReadAllText(GetFullPathForKey(key)));
		}

		protected string GetHash(string input)
		{
			byte[] data = _md5.ComputeHash(Encoding.UTF8.GetBytes(input));
			var sBuilder = new StringBuilder();
			for (int i = 0; i < data.Length; i++)
			{
				sBuilder.Append(data[i].ToString("x2"));
			}
			return sBuilder.ToString();
		}

		/// <summary>
		/// Unpacks the offlineCache object to a generic Dictionary.
		/// </summary>
		/// <param name="offlineUsers">Offline cache object to be unpacked</param>
		/// <returns></returns>
		private Dictionary<long, Dictionary<TKey, TValue>> CacheToDict<TKey, TValue>(OfflineUserCache offlineUsers)
		{
			Dictionary<long, Dictionary<TKey, TValue>> output = new Dictionary<long, Dictionary<TKey, TValue>>();

			foreach (OfflineUser user in offlineUsers.cache)
			{
				Dictionary<TKey, TValue> userStats = new Dictionary<TKey, TValue>();
				for (int i = 0; i < user.offlineDataList.keys.Count; i++)
				{
					userStats.Add(
						(TKey)Convert.ChangeType(user.offlineDataList.keys[i], typeof(TKey)),
						(TValue)Convert.ChangeType(user.offlineDataList.values[i], typeof(TValue)));
				}
				output.Add(user.dbid, userStats);
			}

			return output;
		}

		/// <summary>
		/// Packs dictionary for storage in cache as a OfflineUserCache object
		/// </summary>
		/// <param name="inputDict">dctionary to be packed</param>
		/// <returns>packed dictionary, as cache object</returns>
		private OfflineUserCache DictToCache<TKey, TValue>(Dictionary<long, Dictionary<TKey, TValue>> inputDict)
		{
			OfflineUserCache newOfflineUsers = new OfflineUserCache();
			int tempIndex = 0;
			foreach (KeyValuePair<long, Dictionary<TKey, TValue>> user in inputDict)
			{
				newOfflineUsers.cache.Add(new OfflineUser());
				newOfflineUsers.cache[tempIndex].dbid = user.Key; //store dbid
				foreach (KeyValuePair<TKey, TValue> stat in user.Value)
				{
					newOfflineUsers.cache[tempIndex].offlineDataList.keys.Add(stat.Key.ToString());
					newOfflineUsers.cache[tempIndex].offlineDataList.values.Add(stat.Value.ToString());
				}
				tempIndex++;
			}

			return newOfflineUsers;
		}

	}
	[Serializable]
	//OfflineUserCache
	public class OfflineUserCache
	{
		public List<OfflineUser> cache;

		public OfflineUserCache()
		{
			cache = new List<OfflineUser>();
		}
	}

	[Serializable]
	//OfflineUserList
	public class OfflineUser
	{
		public long dbid;
		public OfflineUserData offlineDataList;

		public OfflineUser()
		{
			dbid = 0;
			offlineDataList = new OfflineUserData();
		}
	}

	[Serializable]
	//OfflineUserData
	public class OfflineUserData
	{
		public List<string> keys;
		public List<string> values;

		public OfflineUserData()
		{
			keys = new List<string>();
			values = new List<string>();
		}
	}
}
