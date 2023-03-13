using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Beamable.CronExpression
{
	/// <summary>
	///     Cron Expression Parser
	/// </summary>
	public class ExpressionParser
	{
		private readonly string _expression;
		private readonly Options _options;
		private readonly CultureInfo _en_culture;
		private readonly Regex _regex = new Regex(@"^\s*($|#|\w+\s*=|(\?|\*|(?:[0-5]?\d)(?:(?:-|\,)(?:[0-5]?\d))?(?:,(?:[0-5]?\d)(?:(?:-|\,)(?:[0-5]?\d))?)*)\s+(\?|\*|(?:[0-5]?\d)(?:(?:-|\,)(?:[0-5]?\d))?(?:,(?:[0-5]?\d)(?:(?:-|\,)(?:[0-5]?\d))?)*)\s+(\?|\*|(?:[01]?\d|2[0-3])(?:(?:-|\,)(?:[01]?\d|2[0-3]))?(?:,(?:[01]?\d|2[0-3])(?:(?:-|\,)(?:[01]?\d|2[0-3]))?)*)\s+(\?|\*|(?:0?[1-9]|[12]\d|3[01])(?:(?:-|\,)(?:0?[1-9]|[12]\d|3[01]))?(?:,(?:0?[1-9]|[12]\d|3[01])(?:(?:-|\,)(?:0?[1-9]|[12]\d|3[01]))?)*)\s+(\?|\*|(?:[1-9]|1[012])(?:(?:-|\,)(?:[1-9]|1[012]))?(?:L|W)?(?:,(?:[1-9]|1[012])(?:(?:-|\,)(?:[1-9]|1[012]))?(?:L|W)?)*|\?|\*|(?:(?:-))?(?:,(?:(?:-))?)*)\s+(\?|\*|(?:[1-7])(?:(?:-|\,|#)(?:[1-7]))?(?:L)?(?:,(?:[1-7])(?:(?:-|\,|#)(?:[1-7]))?(?:L)?)*|\?|\*)(|\s)+(\?|\*|(?:|\d{4})(?:(?:-|\,)(?:|\d{4}))?(?:,(?:|\d{4})(?:(?:-|\,)(?:|\d{4}))?)*))$");

		/// <summary>
		///     Initializes a new instance of the <see cref="ExpressionParser" /> class
		/// </summary>
		/// <param name="expression">The cron expression string</param>
		/// <param name="options">Parsing options</param>
		public ExpressionParser(string expression, Options options)
		{
			_expression = expression;
			_options = options;
			_en_culture = new CultureInfo("en-US"); //Default to English
		}

		/// <summary>
		///     Parses the cron expression string
		/// </summary>
		/// <returns>A 7 part string array, one part for each component of the cron expression (seconds, minutes, etc.)</returns>
		public string[] Parse(out ErrorData errorData)
		{
			// Initialize all elements of parsed array to empty strings
			errorData = new ErrorData();
			var parsed = new string[7].Select(el => "").ToArray();

			if (string.IsNullOrEmpty(_expression))
			{
				throw new FormatException($"Error: Field 'expression' not found.");
			}

			var expressionPartsTemp = _expression.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			if (expressionPartsTemp.Length != 7)
				throw new FormatException($"Error: Expression has {expressionPartsTemp.Length} parts. Exactly 7 parts are required.");

			if (!IsValidFormat)
			{
				errorData.ErrorMessage = "Error: CRON validation is not passing. CRON supports only numbers [0-9] and special characters [,-*]";
				return null;
			}

			if (expressionPartsTemp.Length == 7)
				parsed = expressionPartsTemp;

			NormalizeExpression(parsed);
			return parsed;
		}

		/// <summary>
		///     Converts cron expression components into consistent, predictable formats.
		/// </summary>
		/// <param name="expressionParts">A 7 part string array, one part for each component of the cron expression</param>
		private void NormalizeExpression(string[] expressionParts)
		{
			// Convert ? to * only for DOM and DOW
			expressionParts[3] = expressionParts[3].Replace("?", "*");
			expressionParts[5] = expressionParts[5].Replace("?", "*");

			// Convert 0/, 1/ to */
			if (expressionParts[0].StartsWith("0/"))
				// Seconds
				expressionParts[0] = expressionParts[0].Replace("0/", "*/");

			if (expressionParts[1].StartsWith("0/"))
				// Minutes
				expressionParts[1] = expressionParts[1].Replace("0/", "*/");

			if (expressionParts[2].StartsWith("0/"))
				// Hours
				expressionParts[2] = expressionParts[2].Replace("0/", "*/");

			if (expressionParts[3].StartsWith("1/"))
				// DOM
				expressionParts[3] = expressionParts[3].Replace("1/", "*/");

			if (expressionParts[4].StartsWith("1/"))
				// Month
				expressionParts[4] = expressionParts[4].Replace("1/", "*/");

			if (expressionParts[5].StartsWith("1/"))
				// DOW
				expressionParts[5] = expressionParts[5].Replace("1/", "*/");

			if (expressionParts[6].StartsWith("1/"))
				// Years
				expressionParts[6] = expressionParts[6].Replace("1/", "*/");

			// Adjust DOW based on dayOfWeekStartIndexZero option
			expressionParts[5] = Regex.Replace(expressionParts[5], @"(^\d)|([^#/\s]\d)", t =>
			{
				//skip anything preceeding by # or /
				var dowDigits =
					Regex.Replace(t.Value, @"\D", ""); // extract digit part (i.e. if "-2" or ",2", just take 2)
				var dowDigitsAdjusted = dowDigits;

				if (_options.DayOfWeekStartIndexZero)
				{
					// "7" also means Sunday so we will convert to "0" to normalize it
					if (dowDigits == "7") dowDigitsAdjusted = "0";
				}
				else
				{
					// If dayOfWeekStartIndexZero==false, Sunday is specified as 1 and Saturday is specified as 7.
					// To normalize, we will shift the  DOW number down so that 1 becomes 0, 2 becomes 1, and so on.
					dowDigitsAdjusted = (int.Parse(dowDigits) - 1).ToString();
				}

				return t.Value.Replace(dowDigits, dowDigitsAdjusted);
			});

			// Convert DOM '?' to '*'
			if (expressionParts[3] == "?") expressionParts[3] = "*";

			// Convert 0 second to (empty)
			if (expressionParts[0] == "0") expressionParts[0] = string.Empty;

			// If time interval is specified for seconds or minutes and next time part is single item, make it a "self-range" so
			// the expression can be interpreted as an interval 'between' range.
			//     For example:
			//     0-20/3 9 * * * => 0-20/3 9-9 * * * (9 => 9-9)
			//     */5 3 * * * => */5 3-3 * * * (3 => 3-3)
			if (expressionParts[2].IndexOfAny(new[] { '*', '-', ',', '/' }) == -1 &&
				(Regex.IsMatch(expressionParts[1], @"\*|\/") || Regex.IsMatch(expressionParts[0], @"\*|\/")))
				expressionParts[2] += $"-{expressionParts[2]}";

			// Loop through all parts and apply global normalization
			for (var i = 0; i < expressionParts.Length; i++)
			{
				// convert all '*/1' to '*'
				if (expressionParts[i] == "*/1") expressionParts[i] = "*";

				/* Convert Month,DOW,Year step values with a starting value (i.e. not '*') to between expressions.
                   This allows us to reuse the between expression handling for step values.
        
                   For Example:
                    - month part '3/2' will be converted to '3-12/2' (every 2 months between March and December)
                    - DOW part '3/2' will be converted to '3-6/2' (every 2 days between Tuesday and Saturday)
                */

				if (expressionParts[i].Contains("/") && expressionParts[i].IndexOfAny(new[] { '*', '-', ',' }) == -1)
				{
					string stepRangeThrough = null;
					switch (i)
					{
						case 4:
							stepRangeThrough = "12";
							break;
						case 5:
							stepRangeThrough = "6";
							break;
						case 6:
							stepRangeThrough = "9999";
							break;
						default:
							stepRangeThrough = null;
							break;
					}

					if (stepRangeThrough != null)
					{
						var parts = expressionParts[i].Split('/');
						expressionParts[i] = $"{parts[0]}-{stepRangeThrough}/{parts[1]}";
					}
				}
			}
		}

		private bool IsValidFormat => _regex.IsMatch(_expression);
	}
}
