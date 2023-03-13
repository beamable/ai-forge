using Beamable.AccountManagement;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.UI.Scripts
{
	public class LoadingDisableBehaviour : MonoBehaviour
	{
		public List<Button> ButtonsToDisable;

		private Dictionary<Button, bool> _defaultInteractiveState = new Dictionary<Button, bool>();
		private Queue<LoadingArg> _loadingQueue = new Queue<LoadingArg>();

		public bool OnlyAcceptCritical = true;

		public void DisableOnLoad(LoadingArg arg)
		{
			if (!ShouldAccept(arg))
			{
				return;
			}

			_loadingQueue.Enqueue(arg);
			if (_loadingQueue.Count == 1) // if there is only one thing in the queue, we need to register a callback...
			{
				DisableAllButtons();
				arg.Promise.Then(_ => HandleLoadingComplete());
			}
		}

		void HandleLoadingComplete()
		{
			_loadingQueue.Dequeue(); // clean the current promise off the queue.

			if (_loadingQueue.Count > 0) // if there is more stuff to wait on, do that.
			{
				var next = _loadingQueue.Peek();
				next.Promise.Then(_ => HandleLoadingComplete());
			}
			else // there is nothing else to load. we are done.
			{
				ReactiveAllButtons();
			}
		}


		void DisableAllButtons()
		{
			foreach (var button in ButtonsToDisable)
			{
				SetDefaultButtonInteractive(button, button.interactable);
				button.interactable = false;
			}
		}

		void ReactiveAllButtons()
		{
			foreach (var button in ButtonsToDisable)
			{
				button.interactable = GetDefaultButtonInteractive(button);
			}
		}

		void SetDefaultButtonInteractive(Button button, bool interactive)
		{
			if (_defaultInteractiveState.ContainsKey(button))
			{
				_defaultInteractiveState[button] = interactive;
			}
			else
			{
				_defaultInteractiveState.Add(button, interactive);
			}
		}

		bool GetDefaultButtonInteractive(Button button)
		{
			return !_defaultInteractiveState.ContainsKey(button) || _defaultInteractiveState[button];
		}

		bool ShouldAccept(LoadingArg arg)
		{
			return arg.Critical || !OnlyAcceptCritical;
		}
	}
}
