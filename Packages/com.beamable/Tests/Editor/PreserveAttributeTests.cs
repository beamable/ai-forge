using NUnit.Framework;
using System.Reflection;

namespace Beamable.Editor.Tests
{
	public class PreserveAttributeTests
	{
		[Test]
		public void ContextClassesArePreserved()
		{
			var t = typeof(SampleClass);

			var hasBeamablePreserve = null != t.GetCustomAttribute<BeamableReflection.PreserveAttribute>();
			var hasUnityPreserve = null != t.GetCustomAttribute<UnityEngine.Scripting.PreserveAttribute>();

			Assert.IsTrue(hasBeamablePreserve);
			Assert.IsFalse(hasUnityPreserve);
		}
	}

	[BeamContextSystem]
	public class SampleClass
	{

	}
}
