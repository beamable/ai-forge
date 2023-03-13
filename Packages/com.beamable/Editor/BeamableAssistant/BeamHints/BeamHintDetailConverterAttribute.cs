using Beamable.Common.Assistant;
using Beamable.Common.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Beamable.Editor.Assistant
{
	/// <summary>
	/// Placed on top of any <see cref="BeamHintDetailConverterProvider"/> static methods that match one of the <see cref="AcceptedSignatures"/>.
	///
	/// These functions should know how to inject data from a <see cref="BeamHint"/> and especially its <see cref="BeamHint.ContextObject"/> into
	/// <see cref="BeamHintVisualsInjectionBag"/>. This is used by <see cref="BeamHintHeaderVisualElement"/> to render hint details.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class BeamHintDetailConverterAttribute : Attribute, IReflectionAttribute
	{
		private static readonly List<SignatureOfInterest> AcceptedSignatures = new List<SignatureOfInterest>()
		{
			new SignatureOfInterest(
				true,
				typeof(void),
				new[]
				{
					new ParameterOfInterest(typeof(BeamHint).MakeByRefType(), true, false, false), new ParameterOfInterest(typeof(BeamHintTextMap).MakeByRefType(), true, false, false),
					new ParameterOfInterest(typeof(BeamHintVisualsInjectionBag), false, false, false)
				})
		};

		/// <summary>
		/// The type of hint this converter will be applied to.
		/// </summary>
		public readonly BeamHintType MatchType;

		/// <summary>
		/// Domain filter for this converter. A match is found if a <see cref="BeamHintHeader.Domain"/> contains this text.
		/// </summary>
		public readonly string DomainSubstring;

		/// <summary>
		/// A regex string used to match against <see cref="BeamHintHeader.Id"/>. The converter will be applied to 
		/// </summary>
		public readonly string IdRegex;

		/// <summary>
		/// <see cref="BeamHintDetailsConfig.Id"/> for the UXML/USS templates used to render this hint.
		/// </summary>
		public readonly string HintDetailConfigId;

		/// <summary>
		/// An alternative <see cref="BeamHintDetailsConfig.Id"/> that we set. If a user creates a <see cref="BeamHintDetailsConfig"/> with this Id in their project,
		/// they can override the <see cref="HintDetailConfigId"/>. 
		/// </summary>
		public readonly string UserOverrideToHintDetailConfigId;

		/// <summary>
		/// Type of the delegate for the converter. Must be one of the converter delegates declared at <see cref="Reflection.BeamHintReflectionCache"/>.
		/// See <see cref="Reflection.BeamHintReflectionCache.DefaultConverter"/> as an example. 
		/// </summary>
		public readonly Type DelegateType;

		/// <summary>
		/// Defines how this converter function maps to <see cref="BeamHintHeader"/>, conversion delegate types (<see cref="Reflection.BeamHintReflectionCache.DefaultConverter"/>) and
		/// <see cref="BeamHintDetailsConfig"/>. 
		/// </summary>
		public BeamHintDetailConverterAttribute(Type delegateType,
												BeamHintType matchType,
												string domainSubstring,
												string idRegex,
												string hintDetailConfigId,
												string userOverrideToHintDetailConfigId = null)
		{
			HintDetailConfigId = hintDetailConfigId;
			DelegateType = delegateType;
			MatchType = matchType;
			DomainSubstring = domainSubstring;
			IdRegex = idRegex;
			UserOverrideToHintDetailConfigId = userOverrideToHintDetailConfigId;
		}

		public AttributeValidationResult IsAllowedOnMember(MemberInfo member)
		{
			var methodInfo = (MethodInfo)member;
			var signatureOfInterests = AcceptedSignatures;

			var matchingMethodSignaturesIndices = signatureOfInterests.FindMatchingMethodSignatures(methodInfo);
			var matchedNoSignatures = matchingMethodSignaturesIndices.TrueForAll(i => i == -1);

			if (matchedNoSignatures)
			{
				var message = new StringBuilder();
				message.AppendLine($"Signatures must match one of the following:");
				message.Append(string.Join("\n", signatureOfInterests.Select(acceptedSignature => acceptedSignature.ToHumanReadableSignature())));

				return new AttributeValidationResult(this,
													 member,
													 ReflectionCache.ValidationResultType.Error,
													 message.ToString());
			}

			return new AttributeValidationResult(this, member, ReflectionCache.ValidationResultType.Valid, $"");
		}
	}
}
