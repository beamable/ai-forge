using Beamable.InputManagerIntegration;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Beamable.UI.Scripts
{
	public class MenuManagementBehaviour : MonoBehaviour
	{
		private const int SUSPICIOUS_DEPTH = 5;
		public Canvas Root;
		public TextMeshProUGUI Title;
		public GameObject BackButton;

		private class Moment
		{
			public readonly MenuBase Prefab;

			private readonly MenuManagementBehaviour _parent;
			private readonly Action<MenuBase> _creationCallback;

			public Moment(MenuManagementBehaviour parent, MenuBase prefab, Action<MenuBase> creationCallback)
			{
				Prefab = prefab;
				_parent = parent;
				_creationCallback = creationCallback;
			}


			private MenuBase GetInstance()
			{
				if (Prefab == null)
				{
					Debug.LogError("There was no instance of the menu available to use.");
				}

				return Prefab;
			}

			public bool Is(MenuBase menu)
			{
				return Prefab != null && Prefab.Equals(menu);
			}

			public void Cover()
			{
				if (Prefab == null)
				{
					return;
				}

				Prefab.gameObject.SetActive(false);

			}

			public void Close()
			{
				if (Prefab == null)
				{
					return;
				}

				Prefab.gameObject.SetActive(false);
				Prefab.OnClosed?.Invoke();
			}

			public MenuBase Activate()
			{
				var instance = GetInstance();
				instance.Manager = _parent;
				_creationCallback?.Invoke(instance);
				instance.gameObject.SetActive(true);
				instance.OnOpened();
				instance.OnOpen?.Invoke();
				return instance;
			}
		}

		public MenuBase StartMenu;
		public List<MenuBase> Menus;
		public MenuBase CurrentMenu => Current?.Prefab;

		public bool IsOpen => _history.Count > 0;
		public bool IsFirst { get; private set; }

		private readonly Stack<Moment> _history = new Stack<Moment>();
		private object _list;

		private Moment Current => _history.Count > 0 ? _history.Peek() : null;

		private void OnEnable()
		{
			// equip all menus with the manager component...
			foreach (var menu in Menus)
			{
				if (menu != null)
				{
					menu.Manager = this;
				}
			}

			if (StartMenu != null)
			{
				var prefab = FindPrefab(StartMenu.GetType());
				var moment = new Moment(this, prefab, null);
				_history.Push(moment);
				IsFirst = true;
				Activate(moment);
			}
		}

		public T Show<T>(Action<T> creationCallback = null) where T : MenuBase
		{
			var prefab = FindPrefab<T>();
			return Setup(prefab, m => creationCallback?.Invoke(m as T)) as T;
		}

		public void ShowInstance(MenuBase menu)
		{
			var prefab = FindPrefab(menu.GetType());
			Setup(prefab, m => { });
		}

		private MenuBase Setup(MenuBase prefab, Action<MenuBase> setupCallback)
		{
			var moment = new Moment(this, prefab, setupCallback);

			var current = Current;
			if (current != null)
			{
				if (current.Prefab.DestroyOnLeave)
				{
					current = _history.Pop();
					current.Close();
				}
				else if (!moment.Prefab.Float)
				{
					current.Cover();
				}
			}

			if (!IsFirst && _history.Count == 0)
			{
				IsFirst = true;
			}
			else if (IsFirst && _history.Count > 0)
			{
				IsFirst = false;
			}

			_history.Push(moment);

			if (_history.Count > SUSPICIOUS_DEPTH)
			{
				Debug.LogWarning($"MenuManager has a large stack. size=[{_history.Count}] suspicious size=[{SUSPICIOUS_DEPTH}]");
			}

			var instance = Activate(moment);
			return instance;
		}


		public void Close(MenuBase menu)
		{
			var history = new List<Moment>();
			while (_history.Count > 0)
			{
				var instance = _history.Pop();
				if (instance.Is(menu))
				{
					// we are removing this from our history...
					instance.Close();
					continue;
				}
				history.Add(instance);
			}

			_history.Clear();
			for (var i = history.Count - 1; i > -1; i--)
			{
				_history.Push(history[i]);
			}

			if (!menu.Float)
			{
				EnableTopMenu();
			}

			if (_history.Count == 0)
			{
				IsFirst = true;
			}
		}

		public void GoBack()
		{
			if (_history.Count < 2)
			{
				Debug.LogWarning("Can't go back, because there isn't anything to go back to");
				return; // nothing to do.
			}

			var menu = _history.Pop();
			menu.Close();
			menu.Prefab.OnWentBack();
			EnableTopMenu();
		}

		public void CloseAll()
		{
			while (_history.Count > 0)
			{
				var menu = _history.Pop();
				menu.Close();
			}
			Root.gameObject.SetActive(false);
			IsFirst = true;
		}

		private void EnableTopMenu()
		{
			if (_history.Count == 0) return;

			var menu = _history.Peek();
			Activate(menu);
		}

		private MenuBase FindPrefab<T>() where T : MenuBase
		{
			return Menus.Find(m => m.GetType() == typeof(T));
		}

		private MenuBase FindPrefab(Type type)
		{
			return Menus.Find(m => m.GetType() == type);
		}

		public void GoBackToPage<T>() where T : MenuBase
		{
			var t = typeof(T);
			while (_history.Count > 0 && _history.Peek().Prefab.GetType() != typeof(T))
			{
				var menu = _history.Pop();
				menu.Close();
			}

			EnableTopMenu();
		}

		private void Update()
		{
			if (BeamableInput.IsEscapeKeyDown())
			{
				GoBack();
			}
		}

		private MenuBase Activate(Moment moment)
		{
			var instance = moment.Activate();
			Root.gameObject.SetActive(true);
			Title.text = instance.GetTitleText();
			BackButton.SetActive(_history.Count > 1);
			return instance;
		}
	}
}
