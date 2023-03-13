// unset

using Beamable.Common.Reflection;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Beamable.Common.Dependencies
{
	/// <summary>
	/// Use this enum to specify where your dependencies should be applied.
	/// </summary>
	[Flags]
	public enum RegistrationOrigin
	{
		RUNTIME = 1,
		EDITOR = 2
	}

	/// <summary>
	/// Use this attribute to register custom services when Beamable starts up.
	/// You should use this on a method that takes one parameter of type <see cref="IDependencyBuilder"/>.
	/// Add whatever services you want to on the builder instance. Any service you register will exist for each BeamContext.
	/// </summary>
	[AttributeUsage(validOn: AttributeTargets.Method)]
	public class RegisterBeamableDependenciesAttribute : BeamableReflection.PreserveAttribute, IReflectionAttribute
	{
		/// <summary>
		/// Valid signatures on top of which you can place <see cref="RegisterBeamableDependenciesAttribute"/>s.
		/// </summary>
		public static readonly SignatureOfInterest[] ValidSignatures = new[]
		{
			new SignatureOfInterest(true, typeof(void), new[] {new ParameterOfInterest(typeof(IDependencyBuilder), false, false, false)})
		};

		public static readonly string ValidSignaturesText = string.Join(", ", ValidSignatures.Select(sig => sig.ToHumanReadableSignature()));
		public RegistrationOrigin Origin { get; }

		/// <summary>
		/// Defines the order in which the functions with <see cref="RegisterBeamableDependenciesAttribute"/> will run.
		/// </summary>
		public int Order { get; set; }
		public string DeclarationPath { get; }

		public RegisterBeamableDependenciesAttribute(int order = 0, RegistrationOrigin origin = RegistrationOrigin.RUNTIME, [CallerFilePath] string declarationPath = "")
		{
			Origin = origin;
			Order = order;
			DeclarationPath = declarationPath;
		}

		public AttributeValidationResult IsAllowedOnMember(MemberInfo member)
		{
			var method = (MethodInfo)member;

			// Check against matching signatures.
			var matchingSignatureIndices = ValidSignatures.FindMatchingMethodSignatures(method);
			if (matchingSignatureIndices.TrueForAll(idx => idx == -1))
			{
				return new AttributeValidationResult(this,
													 method,
													 ReflectionCache.ValidationResultType.Error,
													 $"{method.ToHumanReadableSignature()} must have one of the following signatures: {ValidSignaturesText}");
			}

			return new AttributeValidationResult(this, method, ReflectionCache.ValidationResultType.Valid, "");
		}
	}
}
