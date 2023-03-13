using Beamable.Common;
using Beamable.Platform.Tests;
using NUnit.Framework;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace Beamable.Editor.Tests.PromiseTests
{
	public class ErrorTests
	{
		[UnityTest]
		public IEnumerator AsyncAwaitPromise_ErrorHasGreatStack()
		{
			var eventRan = false;
			PromiseBase.SetPotentialUncaughtErrorHandler((promise, err) =>
			{
				eventRan = true;
			});

			async Promise Inner()
			{
				string x = null;
				var y = x.Length; // NOTE: This _should_ throw a null ref
				await Inner();
			}

			async Promise<bool> Outter()
			{
				try
				{
					await Inner();
				}
				catch (NullReferenceException)
				{
					Debug.Log("Caught the right type of exception");
					return true;
				}

				return false;
			}

			var p = Outter();
			yield return p.ToYielder();

			Assert.IsTrue(eventRan);
			Assert.IsTrue(p.GetResult());
		}


#if !UNITY_WEBGL
		[UnityTest]
		public IEnumerator AsyncAwait_AFailedPromiseShould()
		{
			var knownEx = new Exception();

			var caught = false;
			var mockLogger = new MockLogProvider();
			BeamableLogProvider.Provider = mockLogger;

			mockLogger.onException += exception =>
			{
				Assert.Fail("error log should not be called");
			};
			PromiseExtensions.RegisterBeamableDefaultUncaughtPromiseHandler();


			Promise<Unit> SubMethod()
			{
				var promise = new Promise<Unit>();
				promise.CompleteError(knownEx);
				return promise;
			}

			async Promise Test()
			{
				try
				{
					var promise = SubMethod();
					await promise;
				}
				catch (Exception ex)
				{
					Assert.AreEqual(knownEx, ex);
					caught = true;
				}
			}

			yield return Test().AsYield();
			yield return PromiseExtensions.WaitForAllUncaughtHandlers().ToPromise().AsYield();

			Assert.IsTrue(caught, "The try/catch didn't catch");
		}
#endif

		[Test]
		public void UncaughtPromise_RaisesEvent()
		{
			var p = new Promise<int>();
			var knownEx = new Exception();

			var eventRan = false;
			PromiseBase.SetPotentialUncaughtErrorHandler((promise, err) =>
			{
				Assert.AreEqual(err, knownEx);
				eventRan = true;
			});

			p.CompleteError(knownEx);

			Assert.IsTrue(eventRan);
		}

		[Test]
		public void UncaughtPromise_FromFailedRaisesOnThen()
		{
			var knownEx = new Exception();
			var p = Promise<int>.Failed(knownEx);

			var eventRan = false;
			PromiseBase.SetPotentialUncaughtErrorHandler((promise, err) =>
			{
				Assert.AreEqual(err, knownEx);

				// err.Throw();

				eventRan = true;
			});

			p.Then(n =>
			{
				Assert.Fail("This should never run");
			});

			Assert.IsTrue(eventRan);
		}


		[Test, TestCase(1), TestCase(2), TestCase(4)]
		public void UncaughtPromise_MultipleHandlers_RaisesEvent(int handlerCount)
		{
			var p = new Promise<int>();
			var knownEx = new Exception();

			var eventRuns = new bool[handlerCount];
			for (var i = 0; i < handlerCount; i++)
			{
				var indexIntoEventRan = i;
				eventRuns[i] = false;
				PromiseBase.SetPotentialUncaughtErrorHandler((promise, err) =>
				{
					Assert.AreEqual(err, knownEx);
					eventRuns[indexIntoEventRan] = true;
				},
				i == 0); // Clears all previously set handlers when setting the first handler so we don't need a cleanup function for this test.
			}

			p.CompleteError(knownEx);

			Assert.IsTrue(eventRuns.All(ran => ran));
		}

		[UnityTest]
		public IEnumerator UncaughtPromise_MultipleHandlers_ReplaceWithDefaultHandler_RaisesEvent()
		{
			var handlerCount = 4;
			var p = new Promise<int>();
			var knownEx = new Exception();

			// Sets a bunch of events that won't be called
			for (var i = 0; i < handlerCount; i++)
			{
				PromiseBase.SetPotentialUncaughtErrorHandler((promise, err) =>
				{
					Assert.Fail("Log should not run");
				}, i == 0); // Clears all previously set handlers when setting the first handler so we don't need a cleanup function for this test.
			}

			// Sets our default handler first, replacing all callbacks previously set. Fails if any of the callbacks previously set are called.
			var mockLogger = new MockLogProvider();
			BeamableLogProvider.Provider = mockLogger;
			PromiseExtensions.RegisterBeamableDefaultUncaughtPromiseHandler();
			var ranDefault = false;
			mockLogger.onException += exception =>
			{
				ranDefault = true;
			};
			p.CompleteError(knownEx);

			var task = PromiseExtensions.WaitForAllUncaughtHandlers();
			while (!task.IsCompleted) { yield return null; }
			if (task.IsFaulted) { throw task.Exception; }

			Assert.IsTrue(ranDefault);
		}

		[UnityTest]
		public IEnumerator UncaughtPromise_MultipleHandlers_AddDefaultHandler_RaisesEvent()
		{
			var handlerCount = 4;
			var p = new Promise<int>();
			var knownEx = new Exception();

			var eventRuns = new bool[handlerCount];
			for (var i = 0; i < handlerCount; i++)
			{
				var indexIntoEventRan = i;
				eventRuns[i] = false;
				PromiseBase.SetPotentialUncaughtErrorHandler((promise, err) =>
				{
					Assert.AreEqual(err, knownEx);
					eventRuns[indexIntoEventRan] = true;
				},
				   i == 0); // Clears all previously set handlers when setting the first handler so we don't need a cleanup function for this test.
			}

			// Adds our default handler to the list of existing callbacks
			var mockLogger = new MockLogProvider();
			BeamableLogProvider.Provider = mockLogger;
			var ranDefault = false;
			PromiseExtensions.RegisterBeamableDefaultUncaughtPromiseHandler(false);
			mockLogger.onException += exception =>
			{
				ranDefault = true;
			};

			p.CompleteError(knownEx);

			var task = PromiseExtensions.WaitForAllUncaughtHandlers();
			while (!task.IsCompleted) { yield return null; }
			if (task.IsFaulted) { throw task.Exception; }

			Assert.IsTrue(ranDefault && eventRuns.All(ran => ran));
		}

		[Test]
		public void CaughtPromise_Before_DoesntRaiseEvent()
		{
			var p = new Promise<int>();
			var knownEx = new Exception();

			var eventRan = false;
			PromiseBase.SetPotentialUncaughtErrorHandler((promise, err) =>
			{
				Assert.Fail("uncaught error");
			});

			p.Error(ex =>
			{
				eventRan = true;
				Assert.AreEqual(knownEx, ex);
			}).CompleteError(knownEx);


			Assert.IsTrue(eventRan);
		}


		[UnityTest]
		public IEnumerator CaughtPromise_After_RaisesEvent_NoLog()
		{
			var p = new Promise<int>();
			var knownEx = new Exception();
			var mockLogger = new MockLogProvider();
			BeamableLogProvider.Provider = mockLogger;

			var eventRan = false;
			PromiseExtensions.RegisterBeamableDefaultUncaughtPromiseHandler();
			mockLogger.onException += exception =>
			{
				Assert.Fail("Log should not run");
			};

			p.CompleteError(knownEx);
			p.Error(ex =>
			{
				eventRan = true;
				Assert.AreEqual(knownEx, ex);
			});

			var task = PromiseExtensions.WaitForAllUncaughtHandlers();
			while (!task.IsCompleted) { yield return null; }
			if (task.IsFaulted) { throw task.Exception; }

			Assert.IsTrue(eventRan);
		}

		[UnityTest]
		public IEnumerator UncaughtPromise_TriggersBeamLog()
		{
			var p = new Promise<int>();
			var knownEx = new Exception();
			var mockLogger = new MockLogProvider();
			BeamableLogProvider.Provider = mockLogger;
			var logRan = false;

			mockLogger.onException += exception =>
			{
				Assert.AreEqual(exception.InnerException, knownEx);
				logRan = true;
			};

			PromiseExtensions.RegisterBeamableDefaultUncaughtPromiseHandler();

			p.CompleteError(knownEx);

			var task = PromiseExtensions.WaitForAllUncaughtHandlers();
			while (!task.IsCompleted) { yield return null; }
			if (task.IsFaulted) { throw task.Exception; }

			Assert.IsTrue(logRan);
		}

		[Test]
		public void ErrorOnFailedPromise()
		{

			var mockLogger = new MockLogProvider();
			BeamableLogProvider.Provider = mockLogger;

			mockLogger.onException += exception =>
			{
				Assert.Fail("error log should not be called");
			};

			var knownEx = new Exception();
			var errorCallbackRan = false;
			Exception errorEx = null;
			var p = Promise<int>.Failed(knownEx).Error(ex =>
			{
				errorCallbackRan = true;
				errorEx = ex;
			});

			Assert.IsTrue(errorCallbackRan);
			Assert.AreEqual(knownEx, errorEx);
		}

		[UnityTest]
		public IEnumerator FlatMapAfterAFailedPromise_WithHandler_ShouldNotLog()
		{

			var mockLogger = new MockLogProvider();
			BeamableLogProvider.Provider = mockLogger;

			mockLogger.onException += exception =>
			{
				Assert.Fail("error log should not be called");
			};
			PromiseExtensions.RegisterBeamableDefaultUncaughtPromiseHandler();


			var knownEx = new Exception();
			var errorCallbackRan = false;
			Exception errorEx = null;
			var p = Promise<int>.Failed(knownEx).FlatMap(Promise<int>.Successful).Error(ex =>
			{
				errorCallbackRan = true;
				errorEx = ex;
			});

			var task = PromiseExtensions.WaitForAllUncaughtHandlers();
			while (!task.IsCompleted) { yield return null; }
			if (task.IsFaulted) { throw task.Exception; }


			Assert.IsTrue(errorCallbackRan);
			Assert.AreEqual(knownEx, errorEx);
		}

		[UnityTest]
		public IEnumerator FlatMapAfterAFailedPromise_WithNoHandler_ShouldLog()
		{

			var mockLogger = new MockLogProvider();
			BeamableLogProvider.Provider = mockLogger;
			var knownEx = new Exception();
			var logRan = false;

			mockLogger.onException += exception =>
			{
				Assert.AreEqual(knownEx, exception.InnerException);
				logRan = true;
			};
			PromiseExtensions.RegisterBeamableDefaultUncaughtPromiseHandler();

			var p = Promise<int>.Failed(knownEx).FlatMap(Promise<int>.Successful);

			var task = PromiseExtensions.WaitForAllUncaughtHandlers();
			while (!task.IsCompleted) { yield return null; }
			if (task.IsFaulted) { throw task.Exception; }


			Assert.IsTrue(logRan);
		}

		[UnityTest]
		public IEnumerator FlatMapOverAFailedPromise_WithHandler_ShouldNotLog()
		{

			var mockLogger = new MockLogProvider();
			BeamableLogProvider.Provider = mockLogger;

			mockLogger.onException += exception =>
			{
				Assert.Fail("error log should not be called");
			};
			PromiseExtensions.RegisterBeamableDefaultUncaughtPromiseHandler();

			var knownEx = new Exception();
			var errorCallbackRan = false;
			Exception errorEx = null;
			var p = Promise<int>.Successful(0)
			   .FlatMap(_ => Promise<int>.Failed(knownEx))
			   .Error(ex =>
			   {
				   errorCallbackRan = true;
				   errorEx = ex;
			   });
			var task = PromiseExtensions.WaitForAllUncaughtHandlers();
			while (!task.IsCompleted) { yield return null; }
			if (task.IsFaulted) { throw task.Exception; }

			Assert.IsTrue(errorCallbackRan);
			Assert.AreEqual(knownEx, errorEx);
		}

		[UnityTest]
		public IEnumerator FlatMapOverAFailedPromise_WithNoHandler_ShouldLog()
		{
			var knownEx = new Exception();

			var mockLogger = new MockLogProvider();
			BeamableLogProvider.Provider = mockLogger;

			var logRan = false;
			mockLogger.onException += exception =>
			{
				Assert.AreEqual(knownEx, exception.InnerException);
				logRan = true;
			};
			PromiseExtensions.RegisterBeamableDefaultUncaughtPromiseHandler();

			Promise<int>.Successful(0)
			   .FlatMap(_ => Promise<int>.Failed(knownEx));

			var task = PromiseExtensions.WaitForAllUncaughtHandlers();
			while (!task.IsCompleted) { yield return null; }
			if (task.IsFaulted) { throw task.Exception; }

			Assert.IsTrue(logRan);
		}


		[UnityTest]
		public IEnumerator MapOverException_WithHandler_ShouldNotLog()
		{

			var mockLogger = new MockLogProvider();
			BeamableLogProvider.Provider = mockLogger;

			mockLogger.onException += exception =>
			{
				Assert.Fail("error log should not be called");
			};
			PromiseExtensions.RegisterBeamableDefaultUncaughtPromiseHandler();


			var knownEx = new Exception();
			var errorCallbackRan = false;
			var p = Promise<int>.Successful(0).Map<int>(_ => throw knownEx).Error(ex =>
			{
				errorCallbackRan = true;
				Assert.AreEqual(knownEx, ex);
			});

			var task = PromiseExtensions.WaitForAllUncaughtHandlers();
			while (!task.IsCompleted) { yield return null; }
			if (task.IsFaulted) { throw task.Exception; }

			Assert.IsTrue(errorCallbackRan);
		}

		[UnityTest]
		public IEnumerator MapOverException_WithNoHandler_ShouldLog()
		{

			var mockLogger = new MockLogProvider();
			BeamableLogProvider.Provider = mockLogger;
			var knownEx = new Exception();
			var logRan = false;

			mockLogger.onException += exception =>
			{
				logRan = true;
				Assert.AreEqual(knownEx, exception.InnerException);
			};
			PromiseExtensions.RegisterBeamableDefaultUncaughtPromiseHandler();

			var p = Promise<int>.Successful(0).Map<int>(_ => throw knownEx);

			var task = PromiseExtensions.WaitForAllUncaughtHandlers();
			while (!task.IsCompleted) { yield return null; }
			if (task.IsFaulted) { throw task.Exception; }

			Assert.IsTrue(logRan);
		}

		[UnityTest]
		public IEnumerator MapAfterAFailedPromise_WithHandler_ShouldNotLog()
		{

			var mockLogger = new MockLogProvider();
			BeamableLogProvider.Provider = mockLogger;

			PromiseExtensions.RegisterBeamableDefaultUncaughtPromiseHandler();

			mockLogger.onException += exception =>
			{
				Assert.Fail("error log should not be called");
			};

			var knownEx = new Exception();
			var errorCallbackRan = false;
			Exception errorEx = null;
			var p = Promise<int>.Failed(knownEx).Map(Promise<int>.Successful).Error(ex =>
			{
				errorCallbackRan = true;
				errorEx = ex;
			});

			var task = PromiseExtensions.WaitForAllUncaughtHandlers();
			while (!task.IsCompleted) { yield return null; }
			if (task.IsFaulted) { throw task.Exception; }


			Assert.IsTrue(errorCallbackRan);
			Assert.AreEqual(knownEx, errorEx);
		}

		[UnityTest]
		public IEnumerator MapAfterAFailedPromise_WithNoHandler_ShouldLog()
		{

			var mockLogger = new MockLogProvider();
			BeamableLogProvider.Provider = mockLogger;

			PromiseExtensions.RegisterBeamableDefaultUncaughtPromiseHandler();
			var logRan = false;
			var knownEx = new Exception();

			mockLogger.onException += exception =>
			{
				logRan = true;
				Assert.AreEqual(knownEx, exception.InnerException);
			};

			var p = Promise<int>.Failed(knownEx).Map(Promise<int>.Successful);
			var task = PromiseExtensions.WaitForAllUncaughtHandlers();
			while (!task.IsCompleted) { yield return null; }
			if (task.IsFaulted) { throw task.Exception; }

			Assert.IsTrue(logRan);
		}

		[UnityTest]
		public IEnumerator RecoverWithAfterAFailedPromise_ShouldNotLog()
		{
			var mockLogger = new MockLogProvider();
			BeamableLogProvider.Provider = mockLogger;

			PromiseExtensions.RegisterBeamableDefaultUncaughtPromiseHandler();
			var knownEx = new Exception();
			var recoverRan = false;
			mockLogger.onException += exception =>
			{
				Assert.Fail("error log should not be called");
			};

			Promise<int>.Failed(knownEx).RecoverWith(ex =>
			{
				recoverRan = true;
				Assert.AreEqual(knownEx, ex);
				return Promise<int>.Successful(1);
			});

			var task = PromiseExtensions.WaitForAllUncaughtHandlers();
			while (!task.IsCompleted) { yield return null; }
			if (task.IsFaulted) { throw task.Exception; }

			Assert.IsTrue(recoverRan);
		}

	}
}
