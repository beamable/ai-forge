using Beamable.Common.Assistant;
using Beamable.Common.Reflection;
using Beamable.Editor.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Beamable.Editor.Assistant
{

	/// <summary>
	/// The base class defining all <see cref="BeamHintReflectionCache.DefaultConverter"/> (and other similar delegates).
	/// <para/>
	/// Users must inherit from this class to declare their own conversion functions (<see cref="BeamHintDetailConverterAttribute"/>). They will be automatically detected by the
	/// <see cref="BeamHintReflectionCache"/> and have their mapping cached for rendering hint details when needed.
	/// </summary>
	public abstract class BeamHintDetailConverterProvider
	{

		/// <summary>
		/// Converter to handle cases where other <see cref="BeamHintDetailConverterAttribute"/> fail their validations. It also handles <see cref="AttributeValidationResults"/>,
		/// but in a way that guarantees that the converter function matches one of the accepted signatures.
		/// </summary>
		[BeamHintDetailConverter(typeof(BeamHintReflectionCache.DefaultConverter),
								 BeamHintType.Validation, "", "MisconfiguredHintDetailsProvider",
								 "HintDetailsAttributeValidationResultConfig")]
		public static void MisconfiguredHintDetailsAttributeConverter(in BeamHint hint, in BeamHintTextMap textMap, BeamHintVisualsInjectionBag injectionBag)
		{
			var ctx = hint.ContextObject as IEnumerable<AttributeValidationResult>;

			var validationIntro = textMap != null && textMap.TryGetHintIntroText(hint.Header, out var intro) ? intro : hint.Header.Id;

			var validationMsg = new StringBuilder();
			foreach (var attr in ctx)
			{
				var line = $"{attr.Pair.Info.DeclaringType.FullName}.{attr.Pair.Info.Name}";
				validationMsg.AppendLine(line);
			}

			injectionBag.SetLabel(validationIntro + validationMsg, "hintText");
		}


		/// <summary>
		/// Converter that handles <see cref="AttributeValidationResult"/>s as context object and displays a single Label text message.
		/// </summary>
		[BeamHintDetailConverter(typeof(BeamHintReflectionCache.DefaultConverter),
								 BeamHintType.Validation | BeamHintType.Hint, "", BeamHintIds.ATTRIBUTE_VALIDATION_ID_PREFIX,
								 "HintDetailsAttributeValidationResultConfig")]
		public static void AttributeValidationConverter(in BeamHint hint, in BeamHintTextMap textMap, BeamHintVisualsInjectionBag injectionBag)
		{
			var hintId = hint.Header.Id;
			var ctx = hint.ContextObject as IEnumerable<AttributeValidationResult>;

			var validationIntro = textMap != null && textMap.TryGetHintIntroText(hint.Header, out var intro) ? intro : hint.Header.Id;

			var validationMsg = new StringBuilder();
			foreach (var attrValidation in ctx)
			{
				string line;


				// Format the data depending on the hint we are rendering
				if (hintId == BeamHintIds.ID_CLIENT_CALLABLE_UNSUPPORTED_PARAMETERS) { line = $"{attrValidation.Pair.Info.DeclaringType.Name}.{attrValidation.Pair.Info.Name} => {attrValidation.Message}"; }
				else if (hintId == BeamHintIds.ID_CLIENT_CALLABLE_ASYNC_VOID) { line = $"{attrValidation.Pair.Info.DeclaringType.FullName}.{attrValidation.Pair.Info.Name}"; }
				else if (hintId == BeamHintIds.ID_MICROSERVICE_ATTRIBUTE_MISSING) { line = $"{attrValidation.Pair.Info.Name}"; }
				else if (hintId == BeamHintIds.ID_MISCONFIGURED_HINT_DETAILS_PROVIDER) { line = $"{attrValidation.Pair.Info.DeclaringType.FullName}.{attrValidation.Pair.Info.Name}"; }
				else
				{
					var msg = string.IsNullOrEmpty(attrValidation.Message) ? "" : $" => {attrValidation.Message}";
					var type = attrValidation.Pair.Info.ReflectedType == null ? attrValidation.Pair.Info.Name : attrValidation.Pair.Info.ReflectedType.FullName;
					line = $"{type}{msg}";
				}

				validationMsg.AppendLine(line);
			}

			injectionBag.SetLabel(validationIntro + validationMsg, "hintText");
		}


		/// <summary>
		/// Converter that handles <see cref="UniqueNameCollisionData"/>s as context object and displays a single Label text message.
		/// </summary>
		[BeamHintDetailConverter(typeof(BeamHintReflectionCache.DefaultConverter),
								 BeamHintType.Validation | BeamHintType.Hint, "", BeamHintIds.ATTRIBUTE_NAME_COLLISION_ID_PREFIX,
								 "HintDetailsAttributeValidationResultConfig")]
		public static void UniqueNameAttributeValidationConverter(in BeamHint hint, in BeamHintTextMap textMap, BeamHintVisualsInjectionBag injectionBag)
		{
			var hintId = hint.Header.Id;
			var ctx = hint.ContextObject as IEnumerable<UniqueNameCollisionData>;

			var validationIntro = textMap != null && textMap.TryGetHintIntroText(hint.Header, out var intro) ? intro : hint.Header.Id;

			var validationMsg = new StringBuilder();
			foreach (var collisionData in ctx)
			{
				var line = $"{collisionData.Name} => {string.Join(", ", collisionData.CollidedAttributes.Select(pair => pair.Info.Name))}";
				validationMsg.AppendLine(line);
			}

			injectionBag.SetLabel(validationIntro + validationMsg, "hintText");
		}

	}
}
