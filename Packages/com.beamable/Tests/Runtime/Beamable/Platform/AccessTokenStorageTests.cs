using Beamable.Api;
using Beamable.Common.Api.Auth;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beamable.Platform.Tests
{
	public class AccessTokenStorageTests
	{
		private const string Prefix = "TEST_CODE";

		private AccessTokenStorage _storage;
		private string _cid;
		private string _pid;
		private List<string> _refreshTokens;

		private string DeviceTokensKey => $"{Prefix}device-tokens{_cid}{_pid}";


		[SetUp]
		public void Init()
		{
			_cid = "123";
			_pid = "pid";

			_storage = new AccessTokenStorage(Prefix);
			PlayerPrefs.DeleteKey(DeviceTokensKey);

			_refreshTokens = new List<string>
		 {
			"test1", "test2", "test3", "test2" // <- toss in a dupe, for funs-sake.
         };
		}


		[TearDown]
		public void Cleanup()
		{
			PlayerPrefs.DeleteKey(DeviceTokensKey);
		}

		[Test]
		public void CanGetMany()
		{
			PlayerPrefs.SetString(DeviceTokensKey, "test1|refresh,test2,test3");
			var value = _storage.RetrieveDeviceRefreshTokens(_cid, _pid);
			Assert.AreEqual("test1", value[0].access_token);
			Assert.AreEqual("refresh", value[0].refresh_token);
			Assert.AreEqual("test2", value[1].access_token);
			Assert.AreEqual("test3", value[2].access_token);
		}

		[Test]
		public void CanGetOne()
		{
			PlayerPrefs.SetString(DeviceTokensKey, "test1|refresh");
			var value = _storage.RetrieveDeviceRefreshTokens(_cid, _pid);
			Assert.AreEqual("test1", value[0].access_token);
			Assert.AreEqual("refresh", value[0].refresh_token);
		}

		[Test]
		public void CanGetNone()
		{
			PlayerPrefs.SetString(DeviceTokensKey, "");
			var value = _storage.RetrieveDeviceRefreshTokens(_cid, _pid);
			Assert.AreEqual(0, value.Length);
		}

		[Test]
		public void CanDeleteAll()
		{
			PlayerPrefs.SetString(DeviceTokensKey, "test1,test2,test3");
			_storage.ClearDeviceRefreshTokens(_cid, _pid);
			var value = PlayerPrefs.GetString(DeviceTokensKey, "didntExist");
			Assert.AreEqual("didntExist", value);
		}

		[Test]
		public void CanAddMany()
		{
			var tokens = _refreshTokens.Select(refresh => new AccessToken(_storage, _cid, _pid, "token", refresh, 123)).ToList();
			tokens.ForEach(t => _storage.StoreDeviceRefreshToken(_cid, _pid, t));

			var value = PlayerPrefs.GetString(DeviceTokensKey);
			Assert.AreEqual("token|test1,token|test2,token|test3", value);
		}

		[Test]
		public void CanOverwriteBasedOnRefresh()
		{
			PlayerPrefs.SetString(DeviceTokensKey, "token1|test1,token2|test2");
			var at = new AccessToken(_storage, _cid, _pid, "different", "test1", 123);
			_storage.StoreDeviceRefreshToken(_cid, _pid, at);

			var value = PlayerPrefs.GetString(DeviceTokensKey);
			Assert.AreEqual("different|test1,token2|test2", value);
		}

		[Test]
		public void CanRemove_AtEndOfList()
		{
			PlayerPrefs.SetString(DeviceTokensKey, "token1|test1,token2|test2");
			_storage.RemoveDeviceRefreshToken(_cid, _pid, new TokenResponse
			{
				access_token = "token2",
				refresh_token = "test2"
			});
			var value = PlayerPrefs.GetString(DeviceTokensKey);
			Assert.AreEqual("token1|test1", value);
		}

		[Test]
		public void CanRemove_AtStartOfList()
		{
			PlayerPrefs.SetString(DeviceTokensKey, "token1|test1,token2|test2");
			_storage.RemoveDeviceRefreshToken(_cid, _pid, new TokenResponse
			{
				access_token = "token1",
				refresh_token = "test1"
			});
			var value = PlayerPrefs.GetString(DeviceTokensKey);
			Assert.AreEqual("token2|test2", value);
		}

		[Test]
		public void CanRemove_AtMiddleOfList()
		{
			PlayerPrefs.SetString(DeviceTokensKey, "token1|test1,token2|test2,token3|test3");
			_storage.RemoveDeviceRefreshToken(_cid, _pid, new TokenResponse
			{
				access_token = "token2",
				refresh_token = "test2"
			});
			var value = PlayerPrefs.GetString(DeviceTokensKey);
			Assert.AreEqual("token1|test1,token3|test3", value);
		}

		[Test]
		public void CanRemove_OneThatDoesntExist()
		{
			PlayerPrefs.SetString(DeviceTokensKey, "token1|test1,token2|test2,token3|test3");
			_storage.RemoveDeviceRefreshToken(_cid, _pid, new TokenResponse
			{
				access_token = "token4",
				refresh_token = "test4"
			});
			var value = PlayerPrefs.GetString(DeviceTokensKey);
			Assert.AreEqual("token1|test1,token2|test2,token3|test3", value);
		}
	}
}
