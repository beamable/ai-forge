using UnityEngine;

namespace Variables
{
	[CreateAssetMenu(fileName = "FloatVariable", menuName = "Variables/FloatVariable", order = 0)]
	public class FloatVariable : ScriptableVariable<float>
	{
		public override float Value { get => rawValue; protected set => rawValue = value; }
		[SerializeField] private float rawValue;
	}
}
