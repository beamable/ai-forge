using UnityEngine;

namespace Variables
{
	[CreateAssetMenu(fileName = "IntVariable", menuName = "Variables/IntVariable", order = 0)]
	public class IntVariable : ScriptableVariable<int>
	{
		public override int Value { get => rawValue; protected set => Set(value); }
		[SerializeField] private int rawValue;
	}
}
