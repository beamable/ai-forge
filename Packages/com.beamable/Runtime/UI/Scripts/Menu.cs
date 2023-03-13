using Beamable.Theme.Palettes;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Beamable.UI.Scripts
{
	[Serializable]
	public abstract class MenuBase : MonoBehaviour
	{
		public StringBinding Title;
		public bool Float = false;
		[FormerlySerializedAs("DestoryOnLeave")] public bool DestroyOnLeave;
		public UnityEvent OnOpen;
		public UnityEvent OnClosed;

		private MenuManagementBehaviour _manager;

		public MenuManagementBehaviour Manager
		{
			get => _manager;
			set => _manager = value;
		}

		public virtual void OnOpened()
		{
			// maybe do something?
		}

		public virtual string GetTitleText()
		{
			return Title?.Localize() ?? "Menu";
		}

		public virtual void OnWentBack()
		{
			// maybe do something?
		}

		public void Hide()
		{
			Manager.Close(this);
		}
	}

}
