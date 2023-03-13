using Beamable.AccountManagement;
using Beamable.Common;
using Beamable.Common.Api.Auth;
using Beamable.Platform.Tests;
using Beamable.Tests.Runtime.Api;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace Beamable.Tests.Modules.AccountManagement.AccountManagementSignalsTests
{
	public class LoginTests
	{
		private GameObject _gob;
		private AccountManagementSignals _signaler;
		private MockBeamableApi _engine;
		private User _engineUser;
		private Promise<Unit> _pendingPromise;
		private LoadingArg _loadingArg;

		[SetUp]
		public void Init()
		{
			_engineUser = new User();
			_engine = new MockBeamableApi();
			_engine.User = _engineUser;
			_engine.GetDeviceUsersDelegate = () => Promise<ISet<UserBundle>>.Successful(new HashSet<UserBundle>());

			API.Instance = Promise<IBeamableAPI>.Successful(_engine);

			_gob = new GameObject();
			_signaler = _gob.AddComponent<AccountManagementSignals>();
			_signaler.PrepareForTesting(_gob, arg => _loadingArg = arg);

			_pendingPromise = new Promise<Unit>();
		}

		[TearDown]
		public void Cleanup()
		{
			Object.Destroy(_gob);
		}

		[UnityTest]
		public IEnumerator SignalsALoadingEvent()
		{

			_signaler.Loading.AddListener(arg => _pendingPromise.CompleteSuccess(PromiseBase.Unit));

			_engine.MockAuthService.IsEmailAvailableDelegate = email => Promise<bool>.Successful(true);
			_engine.MockAuthService.RegisterDbCredentialsDelegate =
			   (email, password) => Promise<User>.Successful(null);

			_signaler.UserLoggedIn = new UserEvent();
			_signaler.Login("test@test.com", "123456");

			yield return _pendingPromise.AsYield();
			Assert.AreEqual(true, _pendingPromise.IsCompleted);
			Assert.AreEqual(true, _loadingArg.Promise.IsCompleted);
		}

		[UnityTest]
		public IEnumerator WhenNewAccount_SignalsLoginAndUpdatesUser()
		{
			var nextUser = new User();

			_engine.MockAuthService.IsEmailAvailableDelegate = email => Promise<bool>.Successful(true);
			_engine.MockAuthService.RegisterDbCredentialsDelegate =
			   (email, password) => Promise<User>.Successful(nextUser);

			_signaler.UserLoggedIn = new UserEvent();
			_signaler.UserLoggedIn.AddListener(x => _pendingPromise.CompleteSuccess(PromiseBase.Unit));
			_signaler.Login("test@test.com", "123456");

			yield return _pendingPromise.AsYield();

			Assert.AreEqual(true, _pendingPromise.IsCompleted);
			Assert.AreEqual(nextUser, _engine.User);
			Assert.AreEqual(true, _loadingArg.Promise.IsCompleted);
		}

		[UnityTest]
		public IEnumerator WhenExistingAccount_SignalsSwitch()
		{
			var token = new TokenResponse();
			var nextUser = new User { id = _engineUser.id + 1 }; // the id needs to be different for the switch to get broadcast.
			_engine.MockAuthService.IsEmailAvailableDelegate = email => Promise<bool>.Successful(false);
			_engine.MockAuthService.LoginDelegate = (email, password, merge) => Promise<TokenResponse>.Successful(token);
			_engine.MockAuthService.GetUserDelegate = t => Promise<User>.Successful(nextUser);


			_signaler.UserSwitchAvailable = new UserEvent();
			_signaler.UserSwitchAvailable.AddListener(x =>
			{
				Assert.AreEqual(nextUser, x);
				_pendingPromise.CompleteSuccess(PromiseBase.Unit);
			});
			_signaler.Login("test@test.com", "12345");

			yield return _pendingPromise.AsYield();

			Assert.AreEqual(true, _pendingPromise.IsCompleted);
			Assert.AreEqual(true, _loadingArg.Promise.IsCompleted);
		}
	}
}
