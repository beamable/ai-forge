using System;

namespace Variables
{
	[Serializable]
	public abstract class VariableReference<T,U> where U : ScriptableVariable<T>
	{
		public T GetValue() => useConstant ? constant : variable.Value;
		public bool useConstant;
		public U variable;
		public T constant;
		
	}
}
