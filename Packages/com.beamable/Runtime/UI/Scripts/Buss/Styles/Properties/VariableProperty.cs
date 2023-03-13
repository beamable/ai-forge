using System;
using UnityEngine;

namespace Beamable.UI.Buss
{
	[Serializable]
	public class VariableProperty : DefaultBussProperty, IBussProperty
	{
		[SerializeField]
		private string _variableName = "";
		public string VariableName
		{
			get => _variableName;
			set => _variableName = value;
		}

		public VariableProperty() { }

		public VariableProperty(string variableName)
		{
			VariableName = variableName;
		}

		public IBussProperty CopyProperty()
		{
			return new VariableProperty(VariableName);
		}
	}
}
