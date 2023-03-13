using Beamable.Common;
using Beamable.Common.Dependencies;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEngine;

namespace Beamable.Editor
{
	/// <summary>
	/// The dispatcher allows you to enqueue work to happen on the main unity thread without waiting for editor render frames.
	/// Use the <see cref="Schedule(System.Action)"/> method to schedule work.
	/// </summary>
	public class BeamableDispatcher : IBeamableDisposable
	{
		public const string DEFAULT_QUEUE_NAME = "beamable";
		private Dictionary<string, Queue<Action>> _workQueues;
		private Dictionary<string, EditorCoroutine> _runningSchedulers;
		private bool _forceStop;

		public BeamableDispatcher()
		{
			_workQueues = new Dictionary<string, Queue<Action>>();
			_runningSchedulers = new Dictionary<string, EditorCoroutine>();
			Start(DEFAULT_QUEUE_NAME);
		}

		IEnumerator Scheduler(string queueName, Queue<Action> workQueue)
		{
			while (_workQueues.ContainsKey(queueName) && !_forceStop)
			{
				// Debug.Log("Scheduler waiting for work");
				yield return new WaitForWork(workQueue);
				// Debug.Log("Scheduler woke up");

				if (_forceStop) break;

				Queue<Action> pendingWork;
				lock (workQueue)
				{
					pendingWork = new Queue<Action>(workQueue);
					workQueue.Clear();
				}

				// Debug.Log("Scheduler has " + pendingWork.Count + " things to do");
				foreach (var workItem in pendingWork)
				{
					try
					{
						// Debug.Log("Doing work");
						workItem?.Invoke();
					}
					catch (Exception ex)
					{
						Debug.LogError($"Failed scheduled work. queue=[{queueName}]");
						Debug.LogException(ex);
					}
				}
			}
			// Debug.Log("Scheduler exited");

			_runningSchedulers.Remove(queueName);
		}

		/// <summary>
		/// Begin a new work queue.
		/// There is always a default work queue, but if you'd like to start more for load reasons, use this.
		/// You can stop a work queue by using the <see cref="StopAcceptingWork"/> method
		/// </summary>
		/// <param name="queueName">a unique name for your work queue</param>
		/// <returns>true if the work queue was spawned, or false if the queue name is already running.</returns>
		public bool Start(string queueName)
		{
			if (_workQueues.ContainsKey(queueName))
			{
				return false;
			}

			var queue = new Queue<Action>();
			_workQueues.Add(queueName, queue);
			var coroutine = EditorCoroutineUtility.StartCoroutine(Scheduler(queueName, queue), this);
			_runningSchedulers[queueName] = coroutine;
			return true;
		}

		/// <summary>
		/// Stop a work queue.
		/// This will not cancel pending work on the queue, but will disallow new work to be scheduled. Existing work will execute, and then the work queue will stop.
		/// </summary>
		/// <param name="queueName">A queue name that was passed to <see cref="Start"/></param>
		/// <returns>true if the queue was stopped, or false if there was no queue by the given name</returns>
		public bool StopAcceptingWork(string queueName)
		{
			if (!_workQueues.ContainsKey(queueName))
			{
				return false;
			}

			_workQueues.Remove(queueName);
			return true;
		}

		/// <summary>
		/// Schedule a piece of work to happen on the main Unity thread.
		/// This method will automatically place the work on the default queue.
		/// </summary>
		/// <param name="work">The piece of work to execute later.</param>
		public void Schedule(Action work) => Schedule(DEFAULT_QUEUE_NAME, work);

		/// <summary>
		/// Schedule a piece of work to happen on the main Unity thread.
		/// Work can be split across multiple queues.
		/// </summary>
		/// <param name="queueName">The name of the queue to run the work on. The <see cref="Start"/> method must be called with the given queue name first. </param>
		/// <param name="work">The piece of work to execute later.</param>
		/// <exception cref="Exception">If the <see cref="Start"/> method has not been called with the given queueName, an exception will be thrown.</exception>
		public void Schedule(string queueName, Action work)
		{
			if (_forceStop) throw new Exception("Cannot schedule work, because the scheduler has been stopped.");
			if (_workQueues.TryGetValue(queueName, out var queue))
			{
				lock (queue)
				{
					queue.Enqueue(work);
				}
			}
			else
			{
				throw new Exception(
					$"Cannot schedule work on queue=[{queueName}] because no work queue has been started. Use the {nameof(Start)} method.");
			}
		}

		public Promise OnDispose()
		{
			_forceStop = true;
			_workQueues.Clear();
			foreach (var routine in _runningSchedulers)
			{
				EditorCoroutineUtility.StopCoroutine(routine.Value);
			}
			_runningSchedulers.Clear();
			return Promise.Success;
		}

		private class WaitForWork : CustomYieldInstruction
		{
			private readonly Queue<Action> _workQueue;

			public WaitForWork(Queue<Action> workQueue)
			{
				_workQueue = workQueue;
			}

			public override bool keepWaiting => _workQueue.Count == 0;
		}

	}
}
