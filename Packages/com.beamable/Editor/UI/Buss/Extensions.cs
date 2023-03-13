using System;

namespace Beamable.Editor.UI.Buss
{
	public static class Extensions
	{
		public static bool IsInheritedFrom(this Type type, Type baseClass)
		{
			var baseType = type.BaseType;
			if (baseType == null)
				return false;

			if (baseType.IsGenericType
				&& baseType.GetGenericTypeDefinition() == baseClass)
				return true;

			return baseType.IsInheritedFrom(baseClass);
		}
	}
}
