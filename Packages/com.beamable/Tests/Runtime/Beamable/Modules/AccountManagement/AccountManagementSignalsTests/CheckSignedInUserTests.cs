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
	public class CheckSignedInUserTests
	{
		private const float TIMEOUT = 3f;
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
			// allow calls...
			_signaler.UserAnonymous = new UserEvent();
			_signaler.UserAvailable = new UserEvent();
			_signaler.Loading.AddListener(arg => _pendingPromise.CompleteSuccess(PromiseBase.Unit));

			yield return null;
			_signaler.CheckSignedInUser();

			yield return _pendingPromise.AsYield(TIMEOUT);

			Assert.AreEqual(true, _pendingPromise.IsCompleted);
			Assert.AreEqual(true, _loadingArg.Promise.IsCompleted);
		}

		[UnityTest]
		public IEnumerator WhenUserHasDBCredentials_SignalsALoggedInUser()
		{
			var listenerCalled = false;

			_engineUser.email = "someRegisteredEmail@yahoo.moc";

			_signaler.UserAvailable = new UserEvent();
			_signaler.UserAvailable.AddListener(arg =>
			{
				Assert.AreEqual(_engineUser, arg);
				listenerCalled = true;
				_pendingPromise.CompleteSuccess(PromiseBase.Unit);
			});

			yield return null;
			_signaler.CheckSignedInUser();

			yield return _pendingPromise.AsYield(TIMEOUT);

			Assert.AreEqual(true, listenerCalled);
			Assert.AreEqual(true, _loadingArg.Promise.IsCompleted);
		}

		[UnityTest]
		public IEnumerator WhenUserHas3rdPartyCredentials_SignalsALoggedInUsr()
		{
			var listenerCalled = false;

			_engineUser.email = "";
			_engineUser.thirdPartyAppAssociations = new List<string> { "facebook" };
			_engineUser.deviceIds = new List<string>();
			_signaler.UserAvailable = new UserEvent();
			_signaler.UserAvailable.AddListener(arg =>
			{
				Assert.AreEqual(_engineUser, arg);
				listenerCalled = true;
				_pendingPromise.CompleteSuccess(PromiseBase.Unit);
			});

			yield return null;
			_signaler.CheckSignedInUser();

			yield return _pendingPromise.AsYield(TIMEOUT);
			Assert.AreEqual(true, listenerCalled);
			Assert.AreEqual(true, _loadingArg.Promise.IsCompleted);
		}

		[UnityTest]
		public IEnumerator WhenUserHasNoCredentials_SignalsAnonymousUser()
		{
			var listenerCalled = false;
			_engineUser.email = "";

			_signaler.UserAnonymous = new UserEvent();
			_signaler.UserAnonymous.AddListener(arg =>
			{
				Assert.AreEqual(_engineUser, arg);
				listenerCalled = true;
				_pendingPromise.CompleteSuccess(PromiseBase.Unit);
			});

			yield return null;
			_signaler.CheckSignedInUser();

			yield return _pendingPromise.AsYield(TIMEOUT);
			Assert.AreEqual(true, listenerCalled);
			Assert.AreEqual(true, _loadingArg.Promise.IsCompleted);
		}
	}
}
