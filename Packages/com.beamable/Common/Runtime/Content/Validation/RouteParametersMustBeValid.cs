using System.Collections.Generic;
using System.Linq;

namespace Beamable.Common.Content.Validation
{
	public class RouteParametersMustBeValid : ValidationAttribute
	{
		public override void Validate(ContentValidationArgs args)
		{
			var field = args.ValidationField;
			var obj = args.Content;

			if (typeof(RouteParameters) != field.FieldType)
				throw new ContentValidationException(obj, field, $"{nameof(RouteParametersMustBeValid)} is only valid on {nameof(RouteParameters)}");

			var fieldValue = field.GetValue() as RouteParameters;
			if (fieldValue == null || fieldValue.Parameters == null)
				throw new ContentValidationException(obj, field, $"No route parameters exist yet");


			var errors = new List<string>();

			foreach (var parameter in fieldValue.Parameters)
			{
				if (!parameter.variableReference.HasValue) continue;

				var variableName = parameter.variableReference.Value.Name;
				var variable = fieldValue.ApiContent.Variables.FirstOrDefault(v => v.Name.Equals(variableName));
				if (variable == null)
				{
					errors.Add($"{parameter.Name} references unknown variable {variableName}");
					continue;
				}

				if (!string.Equals(parameter.TypeName, variable.TypeName))
				{
					errors.Add($"{parameter.Name} is a {parameter.TypeName} but references variable {variableName} which is a {variable.TypeName}");
				}
			}

			if (errors.Count > 0)
			{
				var combinedMessage = string.Join("\n  ", errors);
				throw new ContentValidationException(obj, field, combinedMessage);
			}
		}
	}
}
