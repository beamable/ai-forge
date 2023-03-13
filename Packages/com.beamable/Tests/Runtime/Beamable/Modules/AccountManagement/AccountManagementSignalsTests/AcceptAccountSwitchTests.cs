using Beamable.AccountManagement;
using Beamable.Common;
using Beamable.Common.Api.Auth;
using Beamable.Tests.Runtime.Api;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Beamable.Tests.Modules.AccountManagement.AccountManagementSignalsTests
{
	public class AcceptAccountSwitchTests
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
		public IEnumerator WhenNoUser_Fails()
		{
			var caught = false;
			try
			{
				_signaler.AcceptAccountSwitch();
			}
			catch (Exception)
			{
				caught = true;
			}

			yield return null;
			Assert.AreEqual(true, caught);
		}

		[UnityTest]
		public IEnumerator WhenAvailableUser_AppliesTokenAndSignalsLogin()
		{
			var email = "tuna@paste.com";
			var token = new TokenResponse();
			var user = new User { id = _engineUser.id + 1 }; // the id needs to be different for the test to signal login
			_engine.MockAuthService.IsEmailAvailableDelegate = x => Promise<bool>.Successful(false);
			_engine.MockAuthService.LoginDelegate = (e, p, m) => Promise<TokenResponse>.Successful(token);
			_engine.MockAuthService.GetUserDelegate = t => Promise<User>.Successful(user);
			_engine.GetDeviceUsersDelegate = () => Promise<ISet<UserBundle>>.Successful(new HashSet<UserBundle>());
			_signaler.UserSwitchAvailable = new UserEvent(); // allow.
			_signaler.Login(email, "12345");

			var calledToken = false;

			_engine.ApplyTokenDelegate = t =>
			{
				Assert.AreEqual(token, t);
				calledToken = true;
				_engine.TriggerOnUserChanged(null);
				return Promise<Unit>.Successful(PromiseBase.Unit);
			};

			_signaler.UserLoggedIn = new UserEvent();
			_signaler.UserLoggedIn.AddListener(x => _pendingPromise.CompleteSuccess(PromiseBase.Unit));
			_signaler.AcceptAccountSwitch();

			yield return null;

			Assert.AreEqual(true, _pendingPromise.IsCompleted);
			Assert.AreEqual(true, calledToken);
		}
	}
}
