using UnityEngine;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER

#if UNITY_2018
using UnityEngine.Experimental.Input;
#else
using UnityEngine.InputSystem;
#endif

#endif

namespace Beamable.InputManagerIntegration
{
	[System.Serializable]
	public class InputActionArg : IInputActionArg
	{
#if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
		public KeyCode KeyCode;

		public bool IsTriggered()
		{
			return Input.GetKeyDown(KeyCode);
		}

#elif ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
		public InputActionAsset actionAsset;
		public InputAction action;

		protected InputAction GetAction()
		{
#if UNITY_2018
			return actionAsset?.FindAction(action.name);
#else
			if (!actionAsset.enabled)
			{
				actionAsset.Enable();
			}
			return actionAsset?.FindAction(action.id);
#endif
		}

		public bool IsTriggered()
		{
			var action = GetAction();
			return action?.triggered ?? false;
		}
#endif
	}

	public interface IInputActionArg
	{
		bool IsTriggered();
	}
}
