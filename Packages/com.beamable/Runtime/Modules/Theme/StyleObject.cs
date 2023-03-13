using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Beamable.Theme
{

	[System.Serializable]
	public abstract class StyleObjectBase
	{
		[HideInInspector]
		public bool Enabled;

		public abstract Type StyledType { get; }
		public abstract bool Accepts(UIBehaviour target);
		public abstract void Apply(UIBehaviour target);
	}

	[System.Serializable]
	public abstract class StyleObject<T> : StyleObjectBase where T : UIBehaviour
	{
		public override Type StyledType => typeof(T);

		public override void Apply(UIBehaviour target)
		{
			if (!Enabled)
			{
				return;
			}

			var instance = target as T;
			if (instance != null)
			{
				Apply(instance);
			}
		}

		public override bool Accepts(UIBehaviour target)
		{
			return (target as T) != null;
		}

		protected abstract void Apply(T target);
	}

}
