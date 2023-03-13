using Beamable.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Coroutines
{
	public class ActionDebouncer : DebounceService<Action>
	{
		public ActionDebouncer(CoroutineService coroutineService) : base(coroutineService) { }
		protected override Promise Invoke(Action action)
		{
			action?.Invoke();
			return Promise.Success;
		}
	}

	public class FunctionPromiseDebouncer : DebounceService<Func<Promise>>
	{
		public FunctionPromiseDebouncer(CoroutineService coroutineService) : base(coroutineService) { }
		protected override Promise Invoke(Func<Promise> action)
		{
			return action?.Invoke();
		}
	}


	public abstract class DebounceService<T> where T : Delegate
	{
		private readonly CoroutineService _coroutineService;

		private Dictionary<T, TimeoutYieldInstruction> _actionToYielder =
			new Dictionary<T, TimeoutYieldInstruction>();

		public DebounceService(CoroutineService coroutineService)
		{
			_coroutineService = coroutineService;
		}

		protected abstract Promise Invoke(T action);

		/// <summary>
		/// Enqueue some action to run "later", but only once.
		/// If this method were to be called many times in a short order, the given <see cref="action"/>
		/// would only run once.
		///
		/// The action's execution can be cancelled using the <see cref="ClearTimeout"/> function.
		/// </summary>
		/// <param name="action">Some action to execute one time</param>
		/// <param name="bouncer">A <see cref="CustomYieldInstruction"/> that controls how long to wait
		/// before executing the action. By default, it produces a wait time of .1 seconds.</param>
		public Promise SetTimeout(T action, CustomYieldInstruction bouncer = null)
		{
			if (bouncer == null)
			{
				bouncer = new WaitForSecondsRealtime(.3f); // TODO: is this the right default?
			}

			if (!_actionToYielder.TryGetValue(action, out var instruction))
			{
				_actionToYielder[action] = instruction = new TimeoutYieldInstruction(bouncer);
				IEnumerator Method()
				{
					yield return instruction;
					_actionToYielder.Remove(action);

					if (instruction.IsCancelled)
					{
						instruction.Promise.CompleteSuccess();
					}
					Invoke(action).Merge(instruction.Promise);
				}
				_coroutineService.StartNew("debouncer", Method());
			}
			else
			{
				instruction.Instruction = bouncer;
			}

			return instruction.Promise;
		}

		/// <summary>
		/// If an action has been scheduled with the <see cref="SetTimeout"/> method,
		/// this method can be used to cancel that action before it executes.
		/// </summary>
		/// <param name="action">The same action instance given to <see cref="SetTimeout"/></param>
		/// <returns>true if the action was cancelled, or false if it was not cancelled. It may have failed to cancel
		/// if the action didn't exist, or if the action was already cancelled. </returns>
		public bool ClearTimeout(T action) // TODO: maybe remove this since it isn't used?
		{
			if (_actionToYielder.TryGetValue(action, out var yielder) && !yielder.IsCancelled)
			{
				yielder.Cancel();
				return true;
			}

			return false;
		}
	}


	public class Debouncer
	{
		private ActionDebouncer _actions;
		private FunctionPromiseDebouncer _funcs;

		public Debouncer(CoroutineService coroutineService)
		{
			_actions = new ActionDebouncer(coroutineService);
			_funcs = new FunctionPromiseDebouncer(coroutineService);
		}

		public Promise SetTimeout(Func<Promise> generator, CustomYieldInstruction bouncer = null) =>
			_funcs.SetTimeout(generator, bouncer);

		public Promise SetTimeout(Action generator, CustomYieldInstruction bouncer = null) =>
			_actions.SetTimeout(generator, bouncer);
	}

	class TimeoutYieldInstruction : CustomYieldInstruction
	{
		public CustomYieldInstruction Instruction { get; set; }
		public override bool keepWaiting => !IsCancelled && Instruction.keepWaiting;

		public bool IsCancelled { get; private set; }
		public Promise Promise { get; }

		public TimeoutYieldInstruction(CustomYieldInstruction instruction)
		{
			Instruction = instruction;
			Promise = new Promise();
		}

		public void Cancel()
		{
			IsCancelled = true;
		}
	}
}
