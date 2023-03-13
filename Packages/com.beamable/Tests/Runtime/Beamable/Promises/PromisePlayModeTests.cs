using Beamable;
using Beamable.Common;
using Beamable.Coroutines;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Beamable.Tests.Runtime.PromiseTests
{
	public class PromisePlayModeTests
	{
		private CoroutineService _coroutineService;


		[SetUp]
		public void SetUp()
		{
			_coroutineService = new GameObject("__CoroutineService__").AddComponent<CoroutineService>();
		}

		[TearDown]
		public void Teardown()
		{
			Object.DestroyImmediate(_coroutineService.gameObject);
		}

		[UnityTest]
		public IEnumerator RecoverWithFalloff_ShouldThrowAfterMaxAttempts()
		{
			var attemptCounter = 0;
			var expectedExceptionReceived = false;
			var retryAttemptFalloffTimers = new[] { .1f, .25f, .5f, .75f };

			// Ignoring this so the test has the opportunity to succeed ----> When we call CompleteError in our fake work promise, it will fail immediately,
			// before the RecoverWith has the chance to attach any callbacks.
			PromiseBase.SetPotentialUncaughtErrorHandler((promise, err) => { });

			var mainPromise = new Promise();
			var recoveringPromise = mainPromise.RecoverWith((exception, attempt) =>
			{
				attemptCounter += 1;

				var fakeWork = Promise.Success.WaitForSeconds(.00001f, _coroutineService).FlatMap(_ =>
				{
					var newPromise = new Promise();
					newPromise.CompleteError(new Exception($"ExceptionDuringRecovery {attempt}"));
					return newPromise;
				});
				return fakeWork;
			}, retryAttemptFalloffTimers, _coroutineService).Error(err => expectedExceptionReceived = err.Message == $"ExceptionDuringRecovery {retryAttemptFalloffTimers.Length - 1}");

			mainPromise.CompleteError(new Exception("UnexpectedException"));
			yield return recoveringPromise.ToYielder();

			Assert.IsTrue(expectedExceptionReceived);
			Assert.AreEqual(retryAttemptFalloffTimers.Length, attemptCounter);
		}

		[UnityTest]
		public IEnumerator RecoverWithFalloff_ShouldSucceedAfterAttempts()
		{
			// Start at -1 as attempts are counted as indices (this is meant to make it simpler to write code that uses attempts to index into some array of messages and the like)
			var attemptCounter = -1;
			var attemptToSucceedAt = 2;
			var successWasAchieved = false;
			var retryAttemptFalloffTimers = new[] { .1f, .25f, .5f, .75f };

			// Ignoring this so the test has the opportunity to succeed ----> When we call CompleteError in our fake work promise, it will fail immediately,
			// before the RecoverWith has the chance to attach any callbacks.
			PromiseBase.SetPotentialUncaughtErrorHandler((promise, err) => { });

			var mainPromise = new Promise();
			var recoveringPromise = mainPromise.RecoverWith((exception, attemptIdx) =>
			{
				attemptCounter += 1;

				var fakeWork = Promise.Success.WaitForSeconds(.00001f, _coroutineService).FlatMap(_ =>
				{
					var newPromise = new Promise();
					if (attemptIdx < attemptToSucceedAt)
						newPromise.CompleteError(new Exception($"ExceptionDuringRecovery {attemptIdx}"));
					else
						newPromise.CompleteSuccess();
					return newPromise;
				});

				return fakeWork;
			}, retryAttemptFalloffTimers, _coroutineService).Then(_ => successWasAchieved = true);

			mainPromise.CompleteError(new Exception("UnexpectedException"));
			yield return recoveringPromise.ToYielder();

			Assert.IsTrue(successWasAchieved);
			Assert.AreEqual(attemptToSucceedAt, attemptCounter);
		}

		[UnityTest]
		public IEnumerator RecoverWithFalloff_ShouldReuseFallOffValueThenThrow()
		{
			var attemptCounter = 0;
			var expectedExceptionReceived = false;
			var retryAttemptFalloffTimers = new[] { .1f, .25f, .5f, .75f };

			// Ignoring this so the test has the opportunity to succeed ----> When we call CompleteError in our fake work promise, it will fail immediately,
			// before the RecoverWith has the chance to attach any callbacks.
			PromiseBase.SetPotentialUncaughtErrorHandler((promise, err) => { });

			var stopwatch = new Stopwatch();
			var mainPromise = new Promise();

			int maxRetries = 5;
			var recoveringPromise = mainPromise.RecoverWith((exception, attempt) =>
			{
				attemptCounter += 1;

				var fakeWork = Promise.Success.WaitForSeconds(.00001f, _coroutineService).FlatMap(_ =>
				{
					var newPromise = new Promise();
					newPromise.CompleteError(new Exception($"ExceptionDuringRecovery {attempt}"));
					return newPromise;
				});
				return fakeWork;
			}, retryAttemptFalloffTimers, _coroutineService, maxRetries).Error(err =>
			{
				// We expect the last exception we receive to be the maximum number of retries (minus one since attempts are 0-indexed).
				expectedExceptionReceived = err.Message == $"ExceptionDuringRecovery {maxRetries - 1}";
			});

			stopwatch.Start();
			mainPromise.CompleteError(new Exception("UnexpectedException"));
			yield return recoveringPromise.ToYielder();
			stopwatch.Stop();

			Assert.IsTrue(expectedExceptionReceived);
			Assert.AreEqual(retryAttemptFalloffTimers.Length + 1, attemptCounter);
		}

	}
}
