using Beamable.Common;
using Beamable.Common.Runtime.Collections;
using Beamable.Coroutines;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Beamable
{
	public static class PromiseExtensions
	{
		private static IConcurrentDictionary<long, Task> _uncaughtTasks = new ConcurrentDictionary<long, Task>();

		public static async Task WaitForAllUncaughtHandlers()
		{
			var tasks = _uncaughtTasks.Select(k => k.Value).Where(t => t != null).ToArray();
			await Task.WhenAll(tasks);
		}

		/// <summary>
		/// Registers Beamable's default Uncaught Promise Handler. This removes all other handlers
		/// </summary>
		public static void RegisterBeamableDefaultUncaughtPromiseHandler(bool replaceExistingHandlers = true)
		{
			PromiseBase.SetPotentialUncaughtErrorHandler(PromiseBaseOnPotentialOnPotentialUncaughtError, replaceExistingHandlers);
		}

		private static void PromiseBaseOnPotentialOnPotentialUncaughtError(PromiseBase promise, Exception ex)
		{
			var id = promise.GetHashCode();
			// we need to wait one frame before logging anything.
			async Task DelayedCheck()
			{
				var startFrame = Time.frameCount;
				var maxIter = 3; // TODO: really, we need a none BeamContext specific coroutine service to run this on, but then in Editor we need to proxy... Complicated...
				while (maxIter-- > 0 && Time.frameCount == startFrame)
				{
					await Task.Yield();
				}

				// execute check.
				if (!promise.HadAnyErrbacks)
				{
					Beamable.Common.BeamableLogger.LogException(new UncaughtPromiseException(promise, ex));
				}

				_uncaughtTasks.Remove(id);
			}
			var task = DelayedCheck(); // we don't want to await this call.
			_uncaughtTasks.TryAdd(id, task);
		}

		/// <summary>
		/// Returns a promise that will complete successfully in <paramref name="seconds"/>.
		/// Don't use this version when writing tests, instead use <see cref="WaitForSeconds{T}(Beamable.Common.Promise{T},float,CoroutineService)"/>.
		/// </summary>
		public static Promise<T> WaitForSeconds<T>(this Promise<T> promise, float seconds)
		{
			var result = new Promise<T>();
			IEnumerator Wait()
			{
				yield return Yielders.Seconds(seconds);
				promise.Then(x => result.CompleteSuccess(x));
			};

			BeamContext.Default.CoroutineService.StartCoroutine(Wait());

			return result;
		}

		/// <summary>
		/// Returns a promise that will complete successfully in <paramref name="seconds"/> by kicking off a coroutine via the given Coroutine <paramref name="service"/>.
		/// </summary>
		public static Promise<T> WaitForSeconds<T>(this Promise<T> promise, float seconds, CoroutineService service)
		{
			var result = new Promise<T>();
			IEnumerator Wait()
			{
				yield return Yielders.Seconds(seconds);
				promise.Then(x => result.CompleteSuccess(x));
			};

			service.StartCoroutine(Wait());
			return result;
		}

		/// <summary>
		/// This has the same behaviour as <see cref="RecoverWith{T}(Beamable.Common.Promise{T},System.Func{System.Exception,int,Beamable.Common.Promise{T}},float[],CoroutineService,System.Nullable{int})"/>.
		/// However, it's configured to automatically use the <see cref="BeamContext.Default"/>'s <see cref="CoroutineService"/>.
		/// </summary>
		public static Promise<T> RecoverWith<T>(this Promise<T> promise, Func<Exception, int, Promise<T>> callback, float[] falloffSeconds, int? maxRetries = null)
		{
			return RecoverWith(promise, callback, falloffSeconds, BeamContext.Default.CoroutineService, maxRetries);
		}

		/// <summary>
		/// Returns a promise configured to be attempted multiple times --- waiting for the amount of seconds defined by <paramref name="falloffSeconds"/> for each attempt. If <paramref name="maxRetries"/>
		/// is greater than the number of items in <paramref name="falloffSeconds"/>, it will reuse the final item of the array for every attempt over the array's size.
		/// </summary>
		/// <param name="promise">The promise to recover from in case of failure.</param>
		/// <param name="callback">A callback that returns a promise based on which attempt you are making and the error that happened in the previous attempt or original promise.</param>
		/// <param name="falloffSeconds">An array defining, for each attempt, the amount of seconds to wait before attempting again.</param>
		/// <param name="service">The <see cref="CoroutineService"/> that we'll use to wait before attempting again.</param>
		/// <param name="maxRetries">The maximum number of retry attempts we can make. If this number is larger than <paramref name="falloffSeconds"/>'s length, the final falloff value is used for
		/// every attempt over the length.</param>
		/// <typeparam name="T">The result value type of the promise.</typeparam>
		public static Promise<T> RecoverWith<T>(this Promise<T> promise, Func<Exception, int, Promise<T>> callback, float[] falloffSeconds, CoroutineService service, int? maxRetries = null)
		{
			var result = new Promise<T>();

			var attempt = -1;
			maxRetries = maxRetries ?? falloffSeconds.Length;

			promise.Then(value => result.CompleteSuccess(value))
				   .Error(HandleError);

			void HandleError(Exception err)
			{
				attempt += 1;
				if (attempt >= maxRetries)
				{
					result.CompleteError(err);
					return;
				}

				// Will reuse the last fall-off value in cases where maxRetries is larger than falloffSeconds.Length.
				var idx = Mathf.Clamp(attempt, 0, falloffSeconds.Length - 1);
				var delay = falloffSeconds[idx];
				var delayPromise = Promise.Success.WaitForSeconds(delay, service);
				_ = delayPromise.FlatMap(_ => callback(err, attempt)
											  .Then(v => result.CompleteSuccess(v))
											  .Error(HandleError)
										);
			}

			return result;
		}

		public static CustomYieldInstruction ToYielder<T>(this Promise<T> self)
		{
			return new PromiseYieldInstruction<T>(self);
		}

		public static void SetupDefaultHandler()
		{
			if (Application.isPlaying)
			{
				var promiseHandlerConfig = CoreConfiguration.Instance.DefaultUncaughtPromiseHandlerConfiguration;
				switch (promiseHandlerConfig)
				{
					case CoreConfiguration.EventHandlerConfig.Guarantee:
					{
						if (!PromiseBase.HasUncaughtErrorHandler)
							PromiseExtensions.RegisterBeamableDefaultUncaughtPromiseHandler();

						break;
					}
					case CoreConfiguration.EventHandlerConfig.Replace:
					case CoreConfiguration.EventHandlerConfig.Add:
					{
						PromiseExtensions.RegisterBeamableDefaultUncaughtPromiseHandler(promiseHandlerConfig == CoreConfiguration.EventHandlerConfig.Replace);
						break;
					}
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
	}

	public class PromiseYieldInstruction<T> : CustomYieldInstruction
	{
		private readonly Promise<T> _promise;

		public PromiseYieldInstruction(Promise<T> promise)
		{
			_promise = promise;
		}

		public override bool keepWaiting => !_promise.IsCompleted;
	}
}
