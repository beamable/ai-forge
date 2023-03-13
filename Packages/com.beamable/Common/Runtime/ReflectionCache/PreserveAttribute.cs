using System;

// the namespace is unique, "BeamableReflection" so that it doesn't collide with regular usage of the UnityEngine.Preserve in types that are in Beamable.
namespace BeamableReflection
{
	/// <summary>
	/// from Unity docs https://docs.unity3d.com/ScriptReference/Scripting.PreserveAttribute.html
	/// <para>
	/// For 3rd party libraries that do not want to take on a dependency on UnityEngine.dll, it is also possible to define their own PreserveAttribute. The code stripper will respect that too, and it will consider any attribute with the exact name "PreserveAttribute" as a reason not to strip the thing it is applied on, regardless of the namespace or assembly of the attribute.
	/// </para>
	///
	/// The reason we need to use a custom PreserveAttribute is so that we can don't need to reference UnityEngine
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Delegate | AttributeTargets.Enum | AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Struct,
					Inherited = false)]
	public class PreserveAttribute : System.Attribute
	{

	}
}
