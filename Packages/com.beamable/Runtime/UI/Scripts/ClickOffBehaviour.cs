using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Beamable.UI.Scripts
{
	public class ClickOffBehaviour : UIBehaviour
	{
		public UnityEvent OnUnselected;

		public bool ActivateOnEnable = true;
		private bool _wasSelected, _needsToBroadcast;
		public GameObject Target;

		protected override void OnEnable()
		{
			if (ActivateOnEnable)
			{
				EventSystem.current.SetSelectedGameObject(Target);
			}
			base.OnEnable();
		}

		private void Update()
		{
			var selected = (EventSystem.current.currentSelectedGameObject == Target);
			if (!selected && _wasSelected)
			{
				_needsToBroadcast = true;
			}

			if (_needsToBroadcast && !Input.GetMouseButton(0))
			{
				_needsToBroadcast = false;
				OnUnselected?.Invoke();
			}
			_wasSelected = selected;
		}


	}
}
