using System;

namespace Beamable.Server
{
	[System.AttributeUsage(System.AttributeTargets.Parameter)]
	public class ParameterAttribute : Attribute
	{
		public string ParameterNameOverride { get; set; }
		public ParameterAttribute(string parameterName = null)
		{
			ParameterNameOverride = parameterName;
		}
	}
}
