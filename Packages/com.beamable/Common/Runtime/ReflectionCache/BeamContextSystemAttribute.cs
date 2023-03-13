using System;

[AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Struct)]
public class BeamContextSystemAttribute : BeamableReflection.PreserveAttribute
{

}

