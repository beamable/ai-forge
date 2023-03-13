#if UNITY_WEBGL
#define DISABLE_THREADING
#endif

using Beamable.Common.Runtime.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

#if !DISABLE_BEAMABLE_ASYNCMETHODBUILDER && !UNITY_2021_2_OR_NEWER
namespace System.Runtime.CompilerServices
{
	public sealed class AsyncMethodBuilderAttribute : Attribute
	{
		public AsyncMethodBuilderAttribute(Type taskLike)
		{ }
	}
}
#endif

namespace Beamable.Common
{
	/// <summary>
	/// This type defines the base for the %Beamable %Promise.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Common.Promise script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public abstract class PromiseBase
	{
		protected Action<Exception> errbacks;

		/// <summary>
		/// True if there are any registered error handlers.
		/// </summary>
		public bool HadAnyErrbacks { protected set; get; }

		protected Exception err;
		protected ExceptionDispatchInfo errInfo;
		protected StackTrace _errStackTrace;
		protected object _lock = new object();
		internal bool RaiseInnerException { get; set; }

#if DISABLE_THREADING
      protected bool done { get; set; }
#else
		private int _doneSignal = 0; // https://stackoverflow.com/questions/29411961/c-sharp-and-thread-safety-of-a-bool
		protected bool done
		{
			get => (System.Threading.Interlocked.CompareExchange(ref _doneSignal, 1, 1) == 1);
			set
			{
				if (value) System.Threading.Interlocked.CompareExchange(ref _doneSignal, 1, 0);
				else System.Threading.Interlocked.CompareExchange(ref _doneSignal, 0, 1);
			}
		}
#endif

		public static readonly Unit Unit = new Unit();

		public static Promise<Unit> SuccessfulUnit => Promise<Unit>.Successful(Unit);

		/// <summary>
		/// True when the promise has completed; false otherwise.
		/// </summary>
		public bool IsCompleted => done;

		/// <summary>
		/// True when the promise has completed and the promise has failed.
		/// </summary>
		public bool IsFailed => done && err != null;

		private static event PromiseEvent OnPotentialUncaughtError;

		public static bool HasUncaughtErrorHandler => OnPotentialUncaughtError != null;

		/// <summary>
		/// Set error handlers for uncaught promise errors. Beamable has a default handler set in its API initialization.
		/// </summary>
		/// <param name="handler">The new error handler.</param>
		/// <param name="replaceExistingHandlers">When TRUE, will replace all previously set handlers. When FALSE, will add the given handler.</param>
		public static void SetPotentialUncaughtErrorHandler(PromiseEvent handler, bool replaceExistingHandlers = true)
		{
			// This overwrites it everytime, blowing away any other listeners.
			if (replaceExistingHandlers)
			{
				OnPotentialUncaughtError = handler;
			}
			else // This allows someone to override the functionality.
			{
				OnPotentialUncaughtError += handler;
			}
		}

		protected void InvokeUncaughtPromise()
		{
			OnPotentialUncaughtError?.Invoke(this, errInfo?.SourceException ?? err);
		}

	}

	public delegate void PromiseEvent(PromiseBase promise, Exception err);


	public interface ITaskLike<TResult, TSelf> : ICriticalNotifyCompletion
	   where TSelf : ITaskLike<TResult, TSelf>
	{
		TResult GetResult();

		bool IsCompleted { get; }

		TSelf GetAwaiter();
	}

	public static class ITaskLikeExtensions
	{
		public static Promise<TResult> ToPromise<TResult, TSelf>(this ITaskLike<TResult, TSelf> taskLike)
		   where TSelf : ITaskLike<TResult, TSelf>
		{
			var promise = new Promise<TResult>();
			taskLike.UnsafeOnCompleted(() => promise.CompleteSuccess(taskLike.GetResult()));
			return promise;
		}
	}

	/// <summary>
	/// This type defines the %Beamable %Promise.
	///
	/// A promise is an object that may produce a single value some time in the future:
	/// either a resolved value, or a reason that it’s not resolved (e.g., a network error occurred).
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/learning-fundamentals">Learning Fundamentals</a> documentation
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	///
	[AsyncMethodBuilder(typeof(PromiseAsyncMethodBuilder<>))]
	public class Promise<T> : PromiseBase, ICriticalNotifyCompletion
	{
		private Action<T> _callbacks;
		private T _val;

		/// <summary>
		/// Call to set the value and resolve the %Promise
		/// </summary>
		/// <param name="val"></param>
		public void CompleteSuccess(T val)
		{
			lock (_lock)
			{
				if (done)
				{
					return;
				}

				_val = val;
				done = true;
				try
				{
					_callbacks?.Invoke(val);
				}
				catch (Exception e)
				{
					BeamableLogger.LogException(e);
				}

				_callbacks = null;
				errbacks = null;
			}
		}

		/// <summary>
		/// Call to throw an exception and resolve the %Promise
		/// </summary>
		/// <param name="val"></param>
		public void CompleteError(Exception ex)
		{
			lock (_lock)
			{
				if (done)
				{
					return;
				}

				err = ex;
				if (err.StackTrace == null)
				{
					err.SetStackTrace(new StackTrace());
				}
				done = true;
				errInfo = ExceptionDispatchInfo.Capture(err);

				try
				{
					if (!HadAnyErrbacks)
					{
						InvokeUncaughtPromise();
					}
					else
					{
						errbacks?.Invoke(ex);
					}
				}
				catch (Exception e)
				{
					BeamableLogger.LogException(e);
				}

				_callbacks = null;
				errbacks = null;
			}
		}

		/// <summary>
		/// Call to register a success completion handler callback for the %Promise
		/// </summary>
		/// <param name="val"></param>
		public Promise<T> Then(Action<T> callback)
		{
			lock (_lock)
			{
				if (done)
				{
					if (err == null)
					{
						try
						{
							callback(_val);
						}
						catch (Exception e)
						{
							BeamableLogger.LogException(e);
						}
					}
					else
					{
						// maybe there is no error handler for this guy...
						if (!HadAnyErrbacks)
						{
							InvokeUncaughtPromise();
						}
					}
				}
				else
				{
					_callbacks += callback;
				}
			}

			return this;
		}

		/// <summary>
		/// Combine the outcome of this promise with the given promise.
		/// If this promise completes, the given promise will complete.
		/// If this promise fails, the given promise will fail.
		/// </summary>
		/// <param name="other">Some promise other than this promise.</param>
		/// <returns>The current promise instance</returns>
		public Promise<T> Merge(Promise<T> other)
		{
			Then(other.CompleteSuccess);
			Error(other.CompleteError);
			return this;
		}

		/// <summary>
		/// Call to register a failure completion handler callback for the %Promise
		/// </summary>
		/// <param name="val"></param>
		public Promise<T> Error(Action<Exception> errback)
		{
			lock (_lock)
			{
				HadAnyErrbacks = true;
				if (done)
				{
					if (err != null)
					{
						try
						{
							errback(err);
						}
						catch (Exception e)
						{
							BeamableLogger.LogException(e);
						}
					}
				}
				else
				{
					errbacks += errback;
				}
			}

			return this;
		}

		/// <summary>
		/// Takes a promise of type A, and returns a promise of
		/// type B with a conversion applied.
		/// </summary>
		/// <param name="callback"></param>
		/// <typeparam name="TU"></typeparam>
		/// <returns></returns>
		public Promise<TU> Map<TU>(Func<T, TU> callback)
		{
			var result = new Promise<TU>();
			// need to forward the error handles of this one, to the next one.x
			// result.Error(err => errbacks?.Invoke(err));
			Then(value =>
				{
					try
					{
						var nextResult = callback(value);
						result.CompleteSuccess(nextResult);
					}
					catch (Exception ex)
					{
						result.CompleteError(ex);
					}
				})
				.Error(ex => result.CompleteError(ex))
				;
			return result;
		}

		/// <summary>
		/// Takes a promise of type A, and returns a promise of
		/// type B with a conversion applied and the promise hierarchy is flattened.
		/// </summary>
		/// <param name="callback"></param>
		/// <param name="factory"></param>
		/// <typeparam name="PromiseU"></typeparam>
		/// <typeparam name="U"></typeparam>
		/// <returns></returns>
		public PromiseU FlatMap<PromiseU, U>(Func<T, PromiseU> callback, Func<PromiseU> factory)
		   where PromiseU : Promise<U>
		{
			var pu = factory();
			FlatMap(callback)
			   .Then(pu.CompleteSuccess)
			   .Error(pu.CompleteError);
			return pu;
		}

		/// <summary>
		/// Takes a promise of type A, and returns a promise of
		/// type B with a conversion applied and the promise hierarchy is flattened.
		/// </summary>
		/// <param name="callback"></param>
		/// <typeparam name="TU"></typeparam>
		/// <returns></returns>
		public Promise<TU> FlatMap<TU>(Func<T, Promise<TU>> callback)
		{
			var result = new Promise<TU>();
			Then(value =>
			{
				try
				{
					callback(value)
					.Then(valueInner => result.CompleteSuccess(valueInner))
					.Error(ex => result.CompleteError(ex));
				}
				catch (Exception ex)
				{
					result.CompleteError(ex);
				}
			}).Error(ex => { result.CompleteError(ex); });
			return result;
		}

		/// <summary>
		/// Call to set the value and resolve the %Promise
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static Promise<T> Successful(T value)
		{
			return new Promise<T>
			{
				done = true,
				_val = value
			};
		}

		/// <summary>
		/// Call to throw an exception and resolve the %Promise
		/// </summary>
		/// <param name="err"></param>
		/// <returns></returns>
		public static Promise<T> Failed(Exception err)
		{
			if (err?.StackTrace == null)
			{
				err?.SetStackTrace(new StackTrace());
			}
			ExceptionDispatchInfo errInfo = ExceptionDispatchInfo.Capture(err);
			return new Promise<T>
			{
				done = true,
				err = err,
				errInfo = errInfo
			};
		}

		void ICriticalNotifyCompletion.UnsafeOnCompleted(Action continuation)
		{
			Error(_ => continuation());
			Then(_ => continuation());
		}

		void INotifyCompletion.OnCompleted(Action continuation)
		{
			((ICriticalNotifyCompletion)this).UnsafeOnCompleted(continuation);
		}

		/// <summary>
		/// Get the result of the <see cref="Promise"/>.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public T GetResult()
		{
			if (err != null)
			{
				if (RaiseInnerException)
				{
					errInfo.Throw();
				}
				else throw err;
			}
			return _val;
		}

		/// <summary>
		/// Get the awaiter of the <see cref="Promise"/>.
		/// Once an awaiter is established, this promise will never raise an uncaught exception.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public Promise<T> GetAwaiter()
		{
			Error((ex) =>
			{
				// remove the ability for an uncaught exception to raise. As an awaiter, the .GetResult() method will trigger, which will THROW an error if one exists.
			});
			return this;
		}

	}

	public static class ExceptionUtilities
	{
		private static readonly FieldInfo STACK_TRACE_STRING_FI = typeof(Exception).GetField("_stackTraceString", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly Type TRACE_FORMAT_TI = Type.GetType("System.Diagnostics.StackTrace").GetNestedType("TraceFormat", BindingFlags.NonPublic);
		private static readonly MethodInfo TRACE_TO_STRING_MI = typeof(StackTrace).GetMethod("ToString", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { TRACE_FORMAT_TI }, null);

		public static Exception SetStackTrace(this Exception target, StackTrace stack)
		{
			var getStackTraceString = TRACE_TO_STRING_MI.Invoke(stack, new object[] { Enum.GetValues(TRACE_FORMAT_TI).GetValue(0) });
			STACK_TRACE_STRING_FI.SetValue(target, getStackTraceString);
			return target;
		}
	}
	public class PromiseException : Exception
	{
		public PromiseException(Exception inner) : base($"Promise failed with exception {inner.Message}", inner)
		{

		}
	}

	/// <summary>
	/// This type defines the %Beamable %SequencePromise.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Common.Promise script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class SequencePromise<T> : Promise<IList<T>>
	{
		private Action<SequenceEntryException> _entryErrorCallbacks;
		private Action<SequenceEntrySuccess<T>> _entrySuccessCallbacks;

		private ConcurrentBag<SequenceEntryException> _errors = new ConcurrentBag<SequenceEntryException>();
		private ConcurrentBag<SequenceEntrySuccess<T>> _successes = new ConcurrentBag<SequenceEntrySuccess<T>>();

		private ConcurrentDictionary<int, object> _indexToResult = new ConcurrentDictionary<int, object>();

		/// <summary>
		/// The current count of successful promises
		/// </summary>
		public int SuccessCount => _successes.Count;

		/// <summary>
		/// The current count of failed promises
		/// </summary>
		public int ErrorCount => _errors.Count;

		/// <summary>
		/// The current count of completed promises
		/// </summary>
		public int Total => _errors.Count + _successes.Count;

		/// <summary>
		/// The number of promises that this sequence reprensents
		/// </summary>
		public int Count { get; }

		/// <summary>
		/// The ratio of completed promises to total promises. This will be 1 when all promises have completed.
		/// </summary>
		public float Ratio => HasProcessedAllEntries ? 1 : Total / (float)Count;

		/// <summary>
		/// True when all promises have completed; false otherwise
		/// </summary>
		public bool HasProcessedAllEntries => Total == Count;

		/// <summary>
		/// An enumeration of the successful results. There will be a <see cref="T"/> for each successful promise.
		/// </summary>
		public IEnumerable<T> SuccessfulResults => _successes.Select(s => s.Result);

		public SequencePromise(int count)
		{
			Count = count;
			if (Count == 0)
			{
				CompleteSuccess();
			}
		}

		/// <summary>
		/// Attach a callback that will trigger anytime a promise fails
		/// </summary>
		/// <param name="handler">A callback that will be given a <see cref="SequenceEntryException"/> everytime a promise fails</param>
		/// <returns>This instance</returns>
		public SequencePromise<T> OnElementError(Action<SequenceEntryException> handler)
		{
			foreach (var existingError in _errors)
			{
				handler?.Invoke(existingError);
			}

			_entryErrorCallbacks += handler;
			return this;
		}

		/// <summary>
		/// Attach a callback that will trigger anytime a promise succeeds
		/// </summary>
		/// <param name="handler">A callback that will be given a <see cref="SequenceEntrySuccess{T}"/> everytime a promise succeeds</param>
		/// <returns>This instance</returns>
		public SequencePromise<T> OnElementSuccess(Action<SequenceEntrySuccess<T>> handler)
		{
			foreach (var success in _successes)
			{
				handler?.Invoke(success);
			}

			_entrySuccessCallbacks += handler;
			return this;
		}

		/// <summary>
		/// Mark the entire sequence promise as complete
		/// </summary>
		public void CompleteSuccess()
		{
			base.CompleteSuccess(SuccessfulResults.ToList());
		}

		/// <summary>
		/// When a promise has failed, report the failure.
		/// One failed promise will cause the entire sequence promise to be considered a failed promise.
		/// </summary>
		/// <param name="exception">The <see cref="SequenceEntryException"/> that occured</param>
		public void ReportEntryError(SequenceEntryException exception)
		{
			if (_indexToResult.ContainsKey(exception.Index) || exception.Index >= Count) return;

			_errors.Add(exception);
			_indexToResult.TryAdd(exception.Index, exception);
			_entryErrorCallbacks?.Invoke(exception);

			CompleteError(exception.InnerException);
		}

		/// <summary>
		/// When a promise has succeeded, report the success.
		/// All promises must report success for the entire sequence promise to succeed.
		/// </summary>
		/// <param name="success">The <see cref="SequenceEntrySuccess{T}"/> that occured</param>
		public void ReportEntrySuccess(SequenceEntrySuccess<T> success)
		{
			if (_indexToResult.ContainsKey(success.Index) || success.Index >= Count) return;

			_successes.Add(success);
			_indexToResult.TryAdd(success.Index, success);
			_entrySuccessCallbacks?.Invoke(success);

			if (HasProcessedAllEntries)
			{
				CompleteSuccess();
			}
		}

		/// <summary>
		/// <inheritdoc cref="ReportEntrySuccess(Beamable.Common.SequenceEntrySuccess{T})"/>
		/// </summary>
		/// <param name="index">The promise index that succeeded.</param>
		/// <param name="result">The success value of the promise</param>
		public void ReportEntrySuccess(int index, T result) =>
		   ReportEntrySuccess(new SequenceEntrySuccess<T>(index, result));

		/// <summary>
		/// <inheritdoc cref="ReportEntryError(Beamable.Common.SequenceEntryException)"/>
		/// </summary>
		/// <param name="index">The promise index that failed</param>
		/// <param name="err">The exception that failed the promise</param>
		public void ReportEntryError(int index, Exception err) =>
		   ReportEntryError(new SequenceEntryException(index, err));
	}

	// Do not add doxygen comments to "public static class Promise" because
	// it confuses this with the doxygen output with "public class Promise" - srivello
	[AsyncMethodBuilder(typeof(PromiseAsyncMethodBuilder))]
	public class Promise : Promise<Unit>
	{
		public static Promise Success { get; } = new Promise { done = true };

		public void CompleteSuccess() => CompleteSuccess(PromiseBase.Unit);

		/// <summary>
		/// Create a <see cref="SequencePromise{T}"/> from List of <see cref="Promise{T}"/>
		/// </summary>
		/// <param name="promises"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static SequencePromise<T> ObservableSequence<T>(IList<Promise<T>> promises)
		{
			var result = new SequencePromise<T>(promises.Count);

			if (promises == null || promises.Count == 0)
			{
				result.CompleteSuccess();
				return result;
			}

			for (var i = 0; i < promises.Count; i++)
			{
				var index = i;
				promises[i].Then(reply =>
				{
					result.ReportEntrySuccess(new SequenceEntrySuccess<T>(index, reply));

					if (result.Total == promises.Count)
					{
						result.CompleteSuccess();
					}
				}).Error(err =>
				{
					result.ReportEntryError(new SequenceEntryException(index, err));
					result.CompleteError(err);
				});
			}

			return result;
		}

		/// <summary>
		/// Create a <see cref="Promise"/> of List from a List of <see cref="Promise"/>s.
		/// </summary>
		/// <param name="promises"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static Promise<List<T>> Sequence<T>(IList<Promise<T>> promises)
		{
			var result = new Promise<List<T>>();
			var replies = new ConcurrentDictionary<int, T>();

			if (promises == null || promises.Count == 0)
			{
				result.CompleteSuccess(replies.Values.ToList());
				return result;
			}

			for (var i = 0; i < promises.Count; i++)
			{
				var index = i;

				promises[i].Then(reply =>
				{
					replies.TryAdd(index, reply);

					if (replies.Count == promises.Count)
					{
						result.CompleteSuccess(replies.Values.ToList());
					}
				}).Error(err => result.CompleteError(err));
			}

			return result;
		}

		/// <summary>
		/// Create Sequence <see cref="Promise"/> from an array of <see cref="Promise"/>s.
		/// </summary>
		/// <param name="promises"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static Promise<List<T>> Sequence<T>(params Promise<T>[] promises)
		{
			return Sequence((IList<Promise<T>>)promises);
		}

		/// <summary>
		/// Given a list of promise generator functions, process the whole list, but serially.
		/// Only one promise will be active at any given moment.
		/// </summary>
		/// <param name="generators"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns>A single promise of Unit to represent the completion of the processing. Any other side effects need to be handled separately</returns>
		public static Promise<Unit> ExecuteSerially<T>(List<Func<Promise<T>>> generators, Func<bool> stopWhen = null)
		{
#if DISABLE_THREADING // unity doesn't supporting System.Threading
         // use a tail recursion approach. It'll stink for massive lists, but at least it works for small ones
         if (generators.Count == 0)
         {
            return PromiseBase.SuccessfulUnit;
         }
         var first = generators.First();
         var rest = generators.Skip(1).ToList();
         return first().FlatMap(_ => ExecuteSerially<T>(rest));
#else
			async System.Threading.Tasks.Task Execute()
			{
				for (var i = 0; i < generators.Count; i++)
				{
					if (stopWhen != null && stopWhen())
					{
						break;
					}

					var generator = generators[i];
					var promise = generator();
					await promise;
				}
			}

			return Execute().ToPromise();
#endif
		}

		private interface IAtomicInt
		{
			int Value { get; }
			void Increment();
			void Decrement();
		}

#if DISABLE_THREADING
      private class AtomicInt : IAtomicInt
      {
         public int Value { get; private set; }
         public void Increment()
         {
            Value++;
         }

         public void Decrement()
         {
            Value--;
         }
      }
#else
		private class AtomicInt : IAtomicInt
		{
			private int v;

			public int Value => System.Threading.Interlocked.CompareExchange(ref v, 0, 0);

			public void Increment()
			{
				System.Threading.Interlocked.Increment(ref v);
			}

			public void Decrement()
			{
				System.Threading.Interlocked.Decrement(ref v);
			}
		}
#endif

		/// <summary>
		/// Given a list of promise generator functions, process the list, but in a rolling fashion.
		/// </summary>
		/// <param name="maxProcessSize"></param>
		/// <param name="generators"></param>
		/// <param name="stopWhen"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[Obsolete("This method isn't safe to use one devices with low callstack limits like Javascript, or Android. Use " + nameof(ExecuteInBatchSequence) + " instead.")]
		public static SequencePromise<T> ExecuteRolling<T>(int maxProcessSize, List<Func<Promise<T>>> generators, Func<bool> stopWhen = null)
		{
			return ExecuteInBatchSequence(maxProcessSize, generators, stopWhen);
		}

		/// <summary>
		/// Given a list of promise generator functions, process the list, but in batches of some size.
		/// The batches themselves will run one at a time. Every promise in the current batch must finish before the next batch can start.
		/// </summary>
		/// <param name="maxBatchSize"></param>
		/// <param name="generators"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns>A single promise of Unit to represent the completion of the processing. Any other side effects need to be handled separately</returns>
		public static Promise<Unit> ExecuteInBatch<T>(int maxBatchSize, List<Func<Promise<T>>> generators)
		{
			var batches = new List<List<Func<Promise<T>>>>();

			// create batches...
			for (var i = 0; i < generators.Count; i += maxBatchSize)
			{
				var start = i;
				var minBatchSize = generators.Count - start;
				var count = minBatchSize < maxBatchSize ? minBatchSize : maxBatchSize; // min()
				var batch = generators.GetRange(start, count);
				batches.Add(batch);
			}

			Promise<List<T>> ProcessBatch(List<Func<Promise<T>>> batch)
			{
				// start all generators in batch...
				return Promise.Sequence(batch.Select(generator => generator()).ToList());
			}

			// run each batch, serially...
			var batchRunners = batches.Select(batch => new Func<Promise<List<T>>>(() => ProcessBatch(batch))).ToList();

			return ExecuteSerially(batchRunners);
		}

		public static SequencePromise<T> ExecuteInBatchSequence<T>(int maxBatchSize, List<Func<Promise<T>>> generators, Func<bool> stopWhen = null)
		{
			var batches = new List<List<Func<Promise<T>>>>();

			var seq = new SequencePromise<T>(generators.Count);
			var current = new AtomicInt();
			// create batches...
			for (var i = 0; i < generators.Count; i += maxBatchSize)
			{
				var start = i;
				var minBatchSize = generators.Count - start;
				var count = minBatchSize < maxBatchSize ? minBatchSize : maxBatchSize; // min()
				var batch = generators.GetRange(start, count);
				batches.Add(batch);
			}

			Promise<List<T>> ProcessBatch(List<Func<Promise<T>>> batch)
			{
				// start all generators in batch...
				return Promise.Sequence(batch.Select(generator =>
				{
					var promise = generator();
					var index = current.Value;
					current.Increment();
					promise.Then(res => seq.ReportEntrySuccess(index, res))
						   .Error(err => seq.ReportEntryError(index, err));
					return promise;
				}).ToList());
			}

			// run each batch, serially...
			var batchRunners = batches.Select(batch => new Func<Promise<List<T>>>(() => ProcessBatch(batch))).ToList();

			ExecuteSerially(batchRunners, stopWhen);
			return seq;
		}
	}

	/// <summary>
	/// This type defines the static %Beamable %SequenceEntryException.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Common.Promise script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class SequenceEntryException : Exception
	{
		public int Index { get; }

		public SequenceEntryException(int index, Exception inner) : base($"index[{index}]. {inner.Message}", inner)
		{
			Index = index;
		}
	}

	/// <summary>
	/// This type defines the static %Beamable %SequenceEntrySuccess.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Common.Promise script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class SequenceEntrySuccess<T>
	{
		public int Index { get; private set; }
		public T Result { get; private set; }

		public SequenceEntrySuccess(int index, T result)
		{
			Index = index;
			Result = result;
		}
	}

	/// <summary>
	/// This type defines the static %Beamable %PromiseExtensions.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Common.Promise script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public static class PromiseExtensions
	{
		/// <summary>
		/// Create a new promise that potentially recovers from a failure that occurs in the given promise.
		/// If the given promise succeeds, this method's returned promise will succeed with the same value.
		/// If the given promise fails, the exception will be passed to the callback, and this method's returned promise will succeed
		/// or fail based on the return value of the callback. If the callback returns a <see cref="T"/>, then the returned promise
		/// will succeed with that value. If the callback throws the same exception, or raises a new one, then the returned promise
		/// will fail with the given exception.
		/// </summary>
		/// <param name="promise">A promise</param>
		/// <param name="callback">A recovery function</param>
		/// <typeparam name="T">The type of the promise</typeparam>
		/// <returns>A new promise that may recover from a potential failure in the given promise</returns>
		public static Promise<T> Recover<T>(this Promise<T> promise, Func<Exception, T> callback)
		{
			var result = new Promise<T>();
			promise.Then(value => result.CompleteSuccess(value))
			   .Error(err => result.CompleteSuccess(callback(err)));
			return result;
		}

		/// <summary>
		/// Similar to <see cref="Recover{T}"/>.
		/// However, The callback returns a <see cref="Promise{T}"/> instead of a <see cref="T"/> directly.
		/// </summary>
		/// <param name="promise">A promise</param>
		/// <param name="callback">A recovery function</param>
		/// <typeparam name="T">The type of the promise</typeparam>
		/// <returns>A new promise that may recover from a potential failure in the given promise</returns>
		public static Promise<T> RecoverWith<T>(this Promise<T> promise, Func<Exception, Promise<T>> callback)
		{
			var result = new Promise<T>();
			promise.Then(value => result.CompleteSuccess(value)).Error(err =>
			{
				try
				{
					var nextPromise = callback(err);
					nextPromise.Then(value => result.CompleteSuccess(value)).Error(errInner =>
				 {
					 result.CompleteError(errInner);
				 });
				}
				catch (Exception ex)
				{
					result.CompleteError(ex);
				}
			});
			return result;
		}

#if !UNITY_WEBGL || UNITY_EDITOR // webgl does not support the system.threading library
		/// <summary>
		/// Convert <see cref="Task"/> to <see cref="Promise{Unit}"/>.
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public static Promise<Unit> ToPromise(this System.Threading.Tasks.Task task)
		{
			var promise = new Promise<Unit>();

			async System.Threading.Tasks.Task Helper()
			{
				try
				{
					await task;
					promise.CompleteSuccess(PromiseBase.Unit);

				}
				catch (Exception ex)
				{
					promise.CompleteError(ex);
				}
			}

			var _ = Helper();

			return promise;
		}

		/// <summary>
		/// Convert <see cref="Task{T}"/> to <see cref="Promise{T}"/>.
		/// </summary>
		/// <param name="task"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static Promise<T> ToPromise<T>(this System.Threading.Tasks.Task<T> task)
		{
			var promise = new Promise<T>();

			async System.Threading.Tasks.Task Helper()
			{
				try
				{
					var result = await task;
					promise.CompleteSuccess(result);
				}
				catch (Exception ex)
				{
					promise.CompleteError(ex);
				}
			}

			var _ = Helper();

			return promise;
		}
#endif

		/// <summary>
		/// Convert the given promise to a <see cref="Promise{Unit}"/>
		/// </summary>
		/// <param name="self">some promise of type <see cref="T"/></param>
		/// <typeparam name="T">some type</typeparam>
		/// <returns>A promise of type Unit</returns>
		public static Promise<Unit> ToUnit<T>(this Promise<T> self)
		{
			return self.Map(_ => PromiseBase.Unit);
		}

		/// <summary>
		/// Create a new promise that strips away the generic type information of the given promise.
		/// </summary>
		/// <param name="self">Some promise</param>
		/// <typeparam name="T">some type</typeparam>
		/// <returns>A typeless promise</returns>
		public static Promise ToPromise<T>(this Promise<T> self)
		{
			var p = new Promise();
			self.Then(_ => p.CompleteSuccess(PromiseBase.Unit));
			self.Error(p.CompleteError);
			return p;
		}
	}

	/// <summary>
	/// This type defines the static %Beamable %UncaughtPromiseException.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Common.Promise script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class UncaughtPromiseException : Exception
	{
		public PromiseBase Promise { get; }

		public UncaughtPromiseException(PromiseBase promise, Exception ex) : base(
		   $"Uncaught promise innerMsg=[{ex.Message}] innerType=[{ex?.GetType()?.Name}] ", ex)
		{
			Promise = promise;
		}
	}

	/// <summary>
	/// This type defines the struct %Beamable %Unit.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Common.Promise script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public readonly struct Unit
	{
	}

	/// <summary>
	/// https://github.com/dotnet/roslyn/blob/main/docs/features/task-types.md
	/// https://referencesource.microsoft.com/#mscorlib/system/runtime/compilerservices/AsyncMethodBuilder.cs
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class PromiseAsyncMethodBuilder<T>
	{
		private IAsyncStateMachine _stateMachine;
		private Promise<T> _promise = new Promise<T>(); // TODO: allocation.

		public static PromiseAsyncMethodBuilder<T> Create()
		{
			return new PromiseAsyncMethodBuilder<T>();
		}

		public void SetResult(T res)
		{
			_promise.CompleteSuccess(res);
		}

		public void SetException(Exception ex)
		{
			_promise.RaiseInnerException = true;
			// TODO: there is a bug here, where an "uncaught" exception can still happen even if someone try/catches it.
			_promise.Error(err => { });
			_promise.CompleteError(ex);
		}

		public void SetStateMachine(IAsyncStateMachine machine)
		{
			_stateMachine = machine;
		}

		public void AwaitOnCompleted<TAwaiter, TStateMachine>(
		   ref TAwaiter awaiter, ref TStateMachine stateMachine)
		   where TAwaiter : INotifyCompletion
		   where TStateMachine : IAsyncStateMachine
		{
			if (_stateMachine == null)
			{
				_stateMachine = stateMachine;
				_stateMachine.SetStateMachine(stateMachine);
			}

			awaiter.OnCompleted(() =>
			{
				_stateMachine.MoveNext();
			});
		}

		public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
		   ref TAwaiter awaiter, ref TStateMachine stateMachine)
		   where TAwaiter : ICriticalNotifyCompletion
		   where TStateMachine : IAsyncStateMachine
		{
			AwaitOnCompleted(ref awaiter, ref stateMachine);
		}

		public void Start<TStateMachine>(ref TStateMachine stateMachine)
		   where TStateMachine : IAsyncStateMachine
		{
			stateMachine.MoveNext();
		}

		public Promise<T> Task => _promise;
	}

	public sealed class PromiseAsyncMethodBuilder
	{
		private IAsyncStateMachine _stateMachine;
		private Promise _promise = new Promise(); // TODO: allocation.

		public static PromiseAsyncMethodBuilder Create()
		{
			return new PromiseAsyncMethodBuilder();
		}

		public void SetResult()
		{
			_promise.CompleteSuccess(PromiseBase.Unit);
		}

		public void SetException(Exception ex)
		{
			_promise.RaiseInnerException = true;
			_promise.CompleteError(ex);
		}

		public void SetStateMachine(IAsyncStateMachine machine)
		{
			_stateMachine = machine;
		}

		public void AwaitOnCompleted<TAwaiter, TStateMachine>(
		   ref TAwaiter awaiter, ref TStateMachine stateMachine)
		   where TAwaiter : INotifyCompletion
		   where TStateMachine : IAsyncStateMachine
		{
			if (_stateMachine == null)
			{
				_stateMachine = stateMachine;
				_stateMachine.SetStateMachine(stateMachine);
			}

			awaiter.OnCompleted(() =>
			{
				_stateMachine.MoveNext();
			});
		}

		public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
		   ref TAwaiter awaiter, ref TStateMachine stateMachine)
		   where TAwaiter : ICriticalNotifyCompletion
		   where TStateMachine : IAsyncStateMachine
		{
			AwaitOnCompleted(ref awaiter, ref stateMachine);
		}

		public void Start<TStateMachine>(ref TStateMachine stateMachine)
		   where TStateMachine : IAsyncStateMachine
		{
			stateMachine.MoveNext();
		}

		public Promise Task => _promise;
	}
}
