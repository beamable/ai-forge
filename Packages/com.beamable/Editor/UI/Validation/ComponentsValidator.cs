using System;
using System.Collections.Generic;

namespace Beamable.Editor.UI.Validation
{
	public class ComponentsValidator
	{
		private readonly Action<bool, string> _onValidation;

		private readonly List<ValidationRule<string>> _stringRules = new List<ValidationRule<string>>();
		private readonly List<ValidationRule<int>> _intRules = new List<ValidationRule<int>>();

		private int RulesCount => _stringRules.Count + _intRules.Count;

		public ComponentsValidator(Action<bool, string> onValidation)
		{
			_onValidation = onValidation;
		}

		public void ForceValidationCheck()
		{
			ValidateOnChange();
		}

		public void RegisterRule(ValidationRule<string> rule, ValidableVisualElement<string> ve)
		{
			ve.RegisterRule(rule);
			ve.PostVerification = ValidateOnChange;
			_stringRules.Add(rule);
		}

		public void RegisterRule(ValidationRule<int> rule, ValidableVisualElement<int> ve)
		{
			ve.RegisterRule(rule);
			ve.PostVerification = ValidateOnChange;
			_intRules.Add(rule);
		}

		private void ValidateOnChange()
		{
			List<string> errorMessages = new List<string>();
			int counter = 0;

			foreach (ValidationRule<string> rule in _stringRules)
			{
				if (rule.Satisfied)
				{
					counter++;
				}
				else
				{
					errorMessages.Add(rule.ErrorMessage);
				}
			}

			foreach (ValidationRule<int> rule in _intRules)
			{
				if (rule.Satisfied)
				{
					counter++;
				}
				else
				{
					errorMessages.Add(rule.ErrorMessage);
				}
			}

			_onValidation?.Invoke(counter == RulesCount, BuildErrorMessage(errorMessages));
		}

		private string BuildErrorMessage(List<string> elements)
		{
			return $"{string.Join("\n", elements)}";
		}
	}
}
