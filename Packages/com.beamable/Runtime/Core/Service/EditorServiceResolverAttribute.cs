using System;
using System.Diagnostics;

namespace Beamable.Service
{
	[AttributeUsage(AttributeTargets.Class), Conditional("UNITY_EDITOR")]
	public class EditorServiceResolverAttribute : Attribute
	{
		public Type resolverType { get; private set; }
		public string userData { get; private set; }

		public EditorServiceResolverAttribute(Type _resolverType, string _userData = null)
		{
			resolverType = _resolverType;
			userData = _userData;
		}
	}
}
