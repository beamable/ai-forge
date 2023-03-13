using System;
using System.Text.RegularExpressions;

namespace Beamable.Editor.UI.Validation
{
	public class IsProperDate : ValidationRule<string>
	{
		public override string ErrorMessage => $"{ComponentName} has invalid date";

		public IsProperDate(string componentName) : base(componentName)
		{
		}

		public override void Validate(string value)
		{
			Satisfied = DateTime.TryParse(value, out _);
		}
	}

	public class IsNotEmptyRule : ValidationRule<string>
	{
		public override string ErrorMessage => $"{ComponentName} field can't be empty";

		public IsNotEmptyRule(string componentLabel) : base(componentLabel)
		{
		}

		public override void Validate(string value)
		{
			Satisfied = !string.IsNullOrEmpty(value) && !string.IsNullOrWhiteSpace(value);
		}
	}

	public class PatternMatchRule : ValidationRule<string>
	{
		private readonly string _pattern;

		public override string ErrorMessage => $"{ComponentName} field doesn't match pattern";

		public PatternMatchRule(string pattern, string componentLabel) : base(componentLabel)
		{
			_pattern = pattern;
		}

		public override void Validate(string value)
		{
			Satisfied = Regex.IsMatch(value, _pattern);
		}
	}
}
