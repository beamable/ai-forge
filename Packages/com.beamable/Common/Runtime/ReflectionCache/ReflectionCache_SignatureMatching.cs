using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Beamable.Common.Reflection
{
	/// <summary>
	/// Defines a method signature of interest so that we can validate that game-makers
	/// are placing <see cref="IReflectionAttribute"/> on methods that match expected signatures.
	/// </summary>
	public readonly struct SignatureOfInterest
	{
		/// <summary>
		/// Whether or not the signature is expected to be <b>static</b>. Assumes it is an <b>instanced</b> call if <b>false</b>.
		/// </summary>
		public readonly bool IsStatic;

		/// <summary>
		/// The return type for the method. Currently we don't support detecting for <b>ref</b> or <b>readonly</b> returns. 
		/// </summary>
		public readonly Type ReturnType;

		/// <summary>
		/// List of <see cref="ParameterOfInterest"/> that define the parameters of the signature we are matching.
		/// </summary>
		public readonly ParameterOfInterest[] Parameters;

		/// <summary>
		/// <see cref="SignatureOfInterest"/> default constructor.  
		/// </summary>
		/// <param name="parameters">
		/// If no parameters were passed, we assume the user does not care about it and we don't validate parameters.
		/// If has length 0, it will be enforced that the method has 0 parameters.
		/// </param> 
		public SignatureOfInterest(bool isStatic, Type returnType, ParameterOfInterest[] parameters)
		{
			IsStatic = isStatic;
			ReturnType = returnType;
			Parameters = parameters;
		}
	}

	/// <summary>
	/// Defines a parameter signature of interest so we can guarantee parameters are being declared as we expect them to be.
	/// </summary>
	public readonly struct ParameterOfInterest
	{
		/// <summary>
		/// The type of the parameter we are expecting.
		/// </summary>
		public readonly Type ParameterType;

		/// <summary>
		/// Whether or not the parameter is an <b>in</b> parameter.
		/// </summary>
		public readonly bool IsIn;

		/// <summary>
		/// Whether or not the parameter is an <b>out</b> parameter.
		/// </summary>
		public readonly bool IsOut;

		/// <summary>
		/// Whether or not the parameter is a <b>ref</b> parameter.
		/// </summary>
		public readonly bool IsByRef;

		/// <summary>
		/// <see cref="ParameterOfInterest"/> default constructor. Validates in/ref/out correctness.
		/// </summary>
		public ParameterOfInterest(Type parameterType, bool isIn, bool isOut, bool isByRef)
		{
			ParameterType = parameterType;

			IsIn = isIn;
			IsOut = isOut;
			IsByRef = isIn || isOut || isByRef;
		}

		public string ToTypeString() => ParameterType.Name;
	}

	public static partial class ReflectionCacheExtensions
	{
		/// <summary>
		/// Generates a human-readable string of the given <paramref name="signatureOfInterest"/> (see <see cref="SignatureOfInterest"/> for more details).
		/// </summary>
		public static string ToHumanReadableSignature(this in SignatureOfInterest signatureOfInterest)
		{
			var paramsDeclaration = string.Join(", ", signatureOfInterest.Parameters.Select(param =>
			{
				var prefix = param.IsOut ? "out " :
					param.IsIn ? "in " :
					param.ParameterType.IsByRef ? "ref " :
					"";

				return $"{prefix}{param.ParameterType.Name}";
			}));
			var staticModifier = signatureOfInterest.IsStatic ? "static " : "";
			return $"{staticModifier}{signatureOfInterest.ReturnType.Name}({paramsDeclaration})";
		}

		/// <summary>
		/// Iterates through a list of <paramref name="acceptedSignatures"/> and match the given <paramref name="methodInfo"/> against them.
		/// Allow classes and subclasses when matching the type. 
		/// </summary>
		/// <param name="acceptedSignatures">A list of <see cref="SignatureOfInterest"/> that the method is allowed to have.</param>
		/// <param name="methodInfo">The <see cref="MethodInfo"/> to match against the <paramref name="acceptedSignatures"/>.</param>
		///
		/// <returns>
		/// A list, parallel to <paramref name="acceptedSignatures"/>, containing the index or -1 for each of the given <paramref name="acceptedSignatures"/>.
		/// </returns>
		public static List<int> FindMatchingMethodSignatures(this IEnumerable<SignatureOfInterest> acceptedSignatures, MethodInfo methodInfo)
		{
			var parameters = methodInfo.GetParameters();
			var retValType = methodInfo.ReturnType;
			var isStatic = methodInfo.IsStatic;

			var matchedSignaturesIndices = acceptedSignatures.Select((acceptableSignature, signatureIdx) =>
			{
				if (isStatic != acceptableSignature.IsStatic) return -1;
				if (retValType != acceptableSignature.ReturnType) return -1;

				// If no parameters were passed, we assume the user does not care about it.
				// If a parameter was passed, but has 0 length, it will be enforced that the method has 0 parameters.
				if (acceptableSignature.Parameters != null)
				{
					if (parameters.Length != acceptableSignature.Parameters.Length) return -1;

					for (var i = 0; i < parameters.Length; i++)
					{
						var parameter = parameters[i];
						var acceptableParameter = acceptableSignature.Parameters[i];

						// Use assignable from in case we accept interfaces.
						bool matchParameter = MatchParameter(acceptableParameter, parameter);
						if (!(matchParameter))
							return -1;
					}
				}

				return signatureIdx;
			}).ToList();

			return matchedSignaturesIndices;
		}

		/// <summary>
		/// Helper method that can be used to check if the given <paramref name="methodInfo"/> matches the signature at the given <paramref name="idx"/>.
		/// </summary>
		public static bool MatchSignatureAtIdx(this IEnumerable<SignatureOfInterest> acceptedSignatures, int idx, MethodInfo methodInfo)
		{
			var parameters = methodInfo.GetParameters();
			var retValType = methodInfo.ReturnType;
			var isStatic = methodInfo.IsStatic;

			var acceptableSignature = acceptedSignatures.ElementAt(idx);

			if (isStatic != acceptableSignature.IsStatic) return false;
			if (retValType != acceptableSignature.ReturnType) return false;

			// If no parameters were passed, we assume the user does not care about it.
			// If a parameter was passed, but has 0 length, it will be enforced that the method has 0 parameters.
			if (acceptableSignature.Parameters != null)
			{
				if (parameters.Length != acceptableSignature.Parameters.Length) return false;

				for (var i = 0; i < parameters.Length; i++)
				{
					var parameter = parameters[i];
					var acceptableParameter = acceptableSignature.Parameters[i];

					// Use assignable from in case we accept interfaces.
					bool matchParameter = MatchParameter(acceptableParameter, parameter);
					if (!(matchParameter))
						return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Utility to checks if the given method info is an async method with the given return type.
		/// </summary>
		public static bool IsAsyncMethodOfType(this MethodInfo methodInfo, Type returnType) =>
			methodInfo.GetCustomAttribute<AsyncStateMachineAttribute>() != null && methodInfo.ReturnType == returnType;

		/// <summary>
		/// Checks all parameters of <paramref name="methodInfo"/> and returns true if any of its parameters match any of <paramref name="parametersOfInterest"/>.
		/// Used mostly to detect forbidden types in method signatures. 
		/// </summary>
		/// <param name="parametersOfInterest">
		/// List of parameters declarations we care about. Currently, this only matches the type. It doesn't care about in/ref/out modifiers.
		/// </param>
		/// <param name="methodInfo">
		/// The method whose parameters we want to look at.
		/// </param>
		/// <returns>
		/// True, if any of the <paramref name="parametersOfInterest"/> match any of the parameters of <paramref name="methodInfo"/>.
		/// </returns>
		public static bool MatchAnyParametersOfMethod(this IReadOnlyList<ParameterOfInterest> parametersOfInterest, MethodInfo methodInfo, out List<ParameterInfo> invalidParamTypes)
		{
			invalidParamTypes = new List<ParameterInfo>();
			var parameters = methodInfo.GetParameters();
			var result = false;
			for (var i = 0; i < parameters.Length; i++)
			{
				if (parametersOfInterest.Any(interest => MatchParameterTypeOnly(interest, parameters[i])))
				{
					invalidParamTypes.Add(parameters[i]);
				}
				result = invalidParamTypes.Count > 0;
			}
			return result;
		}

		private static bool MatchParameter(ParameterOfInterest acceptableParameter, ParameterInfo parameter)
		{
			var matchType = acceptableParameter.ParameterType.IsInterface ? acceptableParameter.ParameterType.IsAssignableFrom(parameter.ParameterType) :
				acceptableParameter.ParameterType == parameter.ParameterType || parameter.ParameterType.IsSubclassOf(acceptableParameter.ParameterType);
			var matchIn = acceptableParameter.IsIn == parameter.IsIn;
			var matchOut = acceptableParameter.IsOut == parameter.IsOut;
			var matchRef = acceptableParameter.IsByRef == parameter.ParameterType.IsByRef;

			var matchParameter = matchType && matchIn && matchOut && matchRef;
			return matchParameter;
		}

		private static bool MatchParameterTypeOnly(ParameterOfInterest acceptableParameter, ParameterInfo parameter)
		{
			var matchType = acceptableParameter.ParameterType.IsInterface ? acceptableParameter.ParameterType.IsAssignableFrom(parameter.ParameterType) :
				acceptableParameter.ParameterType == parameter.ParameterType || parameter.ParameterType.IsSubclassOf(acceptableParameter.ParameterType);
			return matchType;
		}


	}
}
