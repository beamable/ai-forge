using System;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Modules.Generics
{
	[AddComponentMenu("UI/Beamable/Generic Button", 1)]
	public class GenericButton : Button
	{
		public void Setup(Action onClickAction)
		{
			if (onClickAction == null)
			{
				return;
			}

			onClick.RemoveAllListeners();
			onClick.AddListener(onClickAction.Invoke);
		}
	}
}
