using System;
using System.Collections.Generic;
using System.Reflection;

namespace Beamable.Tests.Runtime
{
	public static class MockConfigurationHelper
	{
		public static void Mock<TConfig>(TConfig config) where TConfig : BaseModuleConfigurationObject
		{
			var t = typeof(ModuleConfigurationObject).BaseType;
			var field = t.GetField("_typeToConfig", BindingFlags.Static | BindingFlags.NonPublic);

			var raw = field.GetValue(null);
			var dict = raw as Dictionary<Type, BaseModuleConfigurationObject>;
			dict[typeof(TConfig)] = config;
		}
	}
}
