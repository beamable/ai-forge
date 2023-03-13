using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Beamable.Common
{
	public static class ObjectExtensions
	{
		public static bool TryInvokeCallback(this object target, string callbackMethodName, BindingFlags bindingFlags =
												 BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
		{
			bool Attempt(Type type)
			{
				while (true)
				{
					var method = type.GetMethods(bindingFlags).FirstOrDefault(m => m.Name == callbackMethodName);
					if (method == null)
					{
						// we couldn't find a method on this type.
						var baseType = type.BaseType;
						var isPrivate = bindingFlags.ContainsAnyFlag(BindingFlags.NonPublic);
						var allowedToTraverseUp = isPrivate && baseType != typeof(System.Object);
						if (allowedToTraverseUp)
						{
							type = baseType;
							continue;
						}

						Debug.LogError("Callback method not found");
						return false;
					}

					if (method.GetParameters().Any())
					{
						Debug.LogError("Callback method cannot not have parameters.");
						return false;
					}

					method.Invoke(target, null);
					return true;
				}
			}

			return Attempt(target.GetType());
		}
	}
}
