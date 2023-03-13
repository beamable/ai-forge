using Beamable.AccountManagement;
using Beamable.Common;
using Beamable.Common.Api.Auth;
using Beamable.Platform.Tests;
using Beamable.Tests.Runtime.Api;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Beamable.Tests.Modules.AccountManagement.AccountManagementSignalsTests
{
	public class UpdateLoginEmailTests
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
		public IEnumerator WhenEmailInvalid_SignalsInvalid()
		{
			_signaler.EmailIsInvalid = new EmailEvent();
			_signaler.EmailIsInvalid.AddListener(x => _pendingPromise.CompleteSuccess(PromiseBase.Unit));

			_signaler.UpdateLoginEmail("abc");

			yield return _pendingPromise.AsYield(5);

			Assert.AreEqual(true, _pendingPromise.IsCompleted);
		}

		[UnityTest]
		public IEnumerator WhenEmailAvailable_SignalsAvailable()
		{
			_engine.MockAuthService.IsEmailAvailableDelegate = email => Promise<bool>.Successful(true);

			_signaler.EmailIsAvailable = new EmailEvent();
			_signaler.EmailIsAvailable.AddListener(x => _pendingPromise.CompleteSuccess(PromiseBase.Unit));
			_signaler.UpdateLoginEmail("test@test.com");
			yield return _pendingPromise.AsYield(1);
			Assert.AreEqual(true, _pendingPromise.IsCompleted);
		}

		[UnityTest]
		public IEnumerator WhenEmailRegistered_SignalsRegistered()
		{
			_engine.MockAuthService.IsEmailAvailableDelegate = email => Promise<bool>.Successful(false);

			_signaler.EmailIsRegistered = new EmailEvent();
			_signaler.EmailIsRegistered.AddListener(x => _pendingPromise.CompleteSuccess(PromiseBase.Unit));
			_signaler.UpdateLoginEmail("test@test.com");
			yield return _pendingPromise.AsYield(1);
			Assert.AreEqual(true, _pendingPromise.IsCompleted);
		}

	}
}
