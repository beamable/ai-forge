using Beamable.Editor.UI.Components;
using System;
using System.Collections.Generic;

namespace Beamable.Editor.UI.Validation
{
	public abstract class ValidableVisualElement<T> : BeamableVisualElement
	{
		public Action PostVerification;

		private readonly List<ValidationRule<T>> _rules = new List<ValidationRule<T>>();

		protected ValidableVisualElement(string commonPath) : base(commonPath)
		{

		}

		public void RegisterRule(ValidationRule<T> rule)
		{
			_rules.Add(rule);
		}

		protected void InvokeValidationCheck(T value)
		{
			foreach (ValidationRule<T> rule in _rules)
			{
				rule.Validate(value);
			}

			PostVerification?.Invoke();
		}
	}
}
