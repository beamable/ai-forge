using System;
using UnityEngine;

namespace Variables
{
	[Serializable]
	public abstract class ScriptableVariable<T> : ScriptableObject
	{
		public abstract T Value { get; protected set; }
		public Action<T> OnUpdated { get; set; }
		
		public void Set(T newValue)
		{
			if (Equals(Value, newValue)) return;
			
			Value = newValue;
			OnUpdated?.Invoke(newValue);
		}
	}
}
