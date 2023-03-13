using System;
using UnityEngine;

namespace Variables
{
	[Serializable]
	public abstract class ScriptableReference<T,U> : ScriptableObject where U : ScriptableVariable<T>
	{
		public T GetValue() => useConstant ? constant : variable.Value;
		[SerializeField] public bool useConstant;
		protected abstract U variable { get; }
		[SerializeField] public T constant;
	}
}
