using Beamable.Common.Content;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using static Beamable.Common.Constants.Features.Cron;

namespace Beamable.CronExpression
{

	public struct ErrorData
	{
		public bool IsError { get; private set; }
		public string ErrorMessage
		{
			get => _errorMessage;
			set
			{
				IsError = !string.IsNullOrWhiteSpace(value);
				_errorMessage = value;
			}
		}
		private string _errorMessage;

		public ErrorData(string errorMessage) : this()
		{
			ErrorMessage = errorMessage;
		}
	}
	/// <summary>
	///     Converts a Cron Expression into a human readable string
	/// </summary>
	public class ExpressionDescriptor
	{
		private readonly char[] _specialCharacters = { '/', '-', ',', '*' };

		private readonly string[] _24hourTimeFormatTwoLetterISOLanguageName =
		{
			"ru", "uk", "de", "it", "tr", "pl", "ro", "da", "sl", "fi", "sv"
		};

		private readonly string _expression;
		private readonly Options _options;
		private string[] _expressionParts;
		private bool _parsed;
		private readonly bool _use24HourTimeFormat;
		private readonly CultureInfo _culture;
		private CronLocalizationData _localizationData;
		private CronLocalizationDatabase _localizationDatabase;

		/// <summary>
		///     Initializes a new instance of the <see cref="ExpressionDescriptor" /> class
		/// </summary>
		/// <param name="expression">The cron expression string</param>
		public ExpressionDescriptor(string expression) : this(expression, new Options())
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ExpressionDescriptor" /> class
		/// </summary>
		/// <param name="expression">The cron expression string</param>
		/// <param name="options">Options to control the output description</param>
		public ExpressionDescriptor(string expression, Options options)
		{
			_expression = expression;
			_options = options;
			_expressionParts = new string[7];
			_parsed = false;

			_localizationDatabase = AssetDatabase.LoadAssetAtPath<CronLocalizationDatabase>(CRON_LOCALIZATION_DATABASE_ASSET);
			if (_localizationDatabase == null)
			{
				_localizationDatabase = ScriptableObject.CreateInstance<CronLocalizationDatabase>();
				AssetDatabase.CreateAsset(_localizationDatabase, CRON_LOCALIZATION_DATABASE_ASSET);
			}

			if (_options.Locale == null)
				_options.Locale = _localizationDatabase.DefaultLocalization;

			_localizationData = _localizationDatabase.SupportedLocalizations.FirstOrDefault(x => x.Localization == _options.Locale.ConvertCronLocaleToLocale());
			if (_localizationData == null)
				_localizationData = ScriptableObject.CreateInstance<CronLocalizationData>();

			_culture = new CultureInfo(_localizationData.Localization);
			_use24HourTimeFormat = _options.Use24HourTimeFormat ?? _24hourTimeFormatTwoLetterISOLanguageName.Contains(_culture.TwoLetterISOLanguageName);
		}

		/// <summary>
		///     Generates a human readable string for the Cron Expression
		/// </summary>
		/// <param name="type">Which part(s) of the expression to describe</param>
		/// <returns>The cron expression description</returns>
		public string GetDescription(DescriptionTypeEnum type, out ErrorData errorData)
		{
			errorData = new ErrorData();
			var description = string.Empty;
			try
			{
				if (!_parsed)
				{
					var parser = new ExpressionParser(_expression, _options);
					_expressionParts = parser.Parse(out errorData);
					if (errorData.IsError)
						return errorData.ErrorMessage;
					_parsed = true;
				}

				switch (type)
				{
					case DescriptionTypeEnum.FULL:
						description = GetFullDescription();
						break;
					case DescriptionTypeEnum.TIMEOFDAY:
						description = GetTimeOfDayDescription();
						break;
					case DescriptionTypeEnum.HOURS:
						description = GetHoursDescription();
						break;
					case DescriptionTypeEnum.MINUTES:
						description = GetMinutesDescription();
						break;
					case DescriptionTypeEnum.SECONDS:
						description = GetSecondsDescription();
						break;
					case DescriptionTypeEnum.DAYOFMONTH:
						description = GetDayOfMonthDescription();
						break;
					case DescriptionTypeEnum.MONTH:
						description = GetMonthDescription();
						break;
					case DescriptionTypeEnum.DAYOFWEEK:
						description = GetDayOfWeekDescription();
						break;
					case DescriptionTypeEnum.YEAR:
						description = GetYearDescription();
						break;
					default:
						description = GetSecondsDescription();
						break;
				}
			}
			catch (Exception ex)
			{
				description = ex.Message;
				errorData = new ErrorData(ex.Message);
				return description;
			}

			description = string.Concat(_culture.TextInfo.ToUpper(description[0]), description.Substring(1));
			return description;
		}

		/// <summary>
		///     Generates the FULL description
		/// </summary>
		/// <returns>The FULL description</returns>
		private string GetFullDescription()
		{
			string description;

			try
			{
				var timeSegment = GetTimeOfDayDescription();
				var dayOfMonthDesc = GetDayOfMonthDescription();
				var monthDesc = GetMonthDescription();
				var dayOfWeekDesc = GetDayOfWeekDescription();
				var yearDesc = GetYearDescription();

				description = $"{timeSegment}{dayOfMonthDesc}{dayOfWeekDesc}{monthDesc}{yearDesc}";

				description = TransformVerbosity(description, _options.Verbose);
			}
			catch (Exception ex)
			{
				description = _localizationData.AnErrorOccuredWhenGeneratingTheExpressionD;
				if (_options.ThrowExceptionOnParseError) throw new FormatException(description, ex);
			}

			return description;
		}

		/// <summary>
		///     Generates a description for only the TIMEOFDAY portion of the expression
		/// </summary>
		/// <returns>The TIMEOFDAY description</returns>
		private string GetTimeOfDayDescription()
		{
			var secondsExpression = _expressionParts[0];
			var minuteExpression = _expressionParts[1];
			var hourExpression = _expressionParts[2];

			var description = new StringBuilder();

			//handle special cases first
			if (minuteExpression.IndexOfAny(_specialCharacters) == -1 &&
				hourExpression.IndexOfAny(_specialCharacters) == -1 &&
				secondsExpression.IndexOfAny(_specialCharacters) == -1)
			{
				//specific time of day (i.e. 10 14)
				description.Append(_localizationData.AtSpace)
					.Append(FormatTime(hourExpression, minuteExpression, secondsExpression));
			}
			else if (secondsExpression == "" && minuteExpression.Contains("-") && !minuteExpression.Contains(",") &&
					 hourExpression.IndexOfAny(_specialCharacters) == -1)
			{
				//minute range in single hour (i.e. 0-10 11)
				var minuteParts = minuteExpression.Split('-');
				description.Append(string.Format(_localizationData.EveryMinuteBetweenX0AndX1,
					FormatTime(hourExpression, minuteParts[0]), FormatTime(hourExpression, minuteParts[1])));
			}
			else if (secondsExpression == "" && hourExpression.Contains(",") && hourExpression.IndexOf('-') == -1 &&
					 minuteExpression.IndexOfAny(_specialCharacters) == -1)
			{
				//hours list with single minute (o.e. 30 6,14,16)
				var hourParts = hourExpression.Split(',');
				description.Append(_localizationData.At);
				for (var i = 0; i < hourParts.Length; i++)
				{
					description.Append(" ").Append(FormatTime(hourParts[i], minuteExpression));

					if (i < hourParts.Length - 2) description.Append(",");

					if (i == hourParts.Length - 2) description.Append(_localizationData.SpaceAnd);
				}
			}
			else
			{
				//default time description
				var secondsDescription = GetSecondsDescription();
				var minutesDescription = GetMinutesDescription();
				var hoursDescription = GetHoursDescription();

				description.Append(secondsDescription);

				if (description.Length > 0 && minutesDescription.Length > 0) description.Append(", ");

				description.Append(minutesDescription);

				if (description.Length > 0 && hourExpression.Length > 0) description.Append(", ");

				description.Append(hoursDescription);
			}

			return description.ToString();
		}

		/// <summary>
		///     Generates a description for only the SECONDS portion of the expression
		/// </summary>
		/// <returns>The SECONDS description</returns>
		private string GetSecondsDescription()
		{
			var description = GetSegmentDescription(_expressionParts[0], _localizationData.EverySecond, s => s,
				s => string.Format(_localizationData.EveryX0Seconds, s), s => _localizationData.SecondsX0ThroughX1PastTheMinute,
				s =>
				{
					var i = 0;
					if (int.TryParse(s, out i))
						return s == "0" ? string.Empty :
							_localizationData.AtX0SecondsPastTheMinute;
					return _localizationData.AtX0SecondsPastTheMinute;
				}, s => _localizationData.ComaX0ThroughX1);

			return description;
		}

		/// <summary>
		///     Generates a description for only the MINUTE portion of the expression
		/// </summary>
		/// <returns>The MINUTE description</returns>
		private string GetMinutesDescription()
		{
			var secondsExpression = _expressionParts[0];
			var description = GetSegmentDescription(_expressionParts[1], _localizationData.EveryMinute, s => s,
				s => string.Format(_localizationData.EveryX0Minutes, s), s => _localizationData.MinutesX0ThroughX1PastTheHour,
				s =>
				{
					var i = 0;
					if (int.TryParse(s, out i))
						return s == "0" && secondsExpression == "" ? string.Empty : _localizationData.AtX0MinutesPastTheHour;
					return _localizationData.AtX0MinutesPastTheHour;
				}, s => _localizationData.ComaX0ThroughX1);

			return description;
		}

		/// <summary>
		///     Generates a description for only the HOUR portion of the expression
		/// </summary>
		/// <returns>The HOUR description</returns>
		private string GetHoursDescription()
		{
			var expression = _expressionParts[2];
			var description = GetSegmentDescription(expression, _localizationData.EveryHour, s => FormatTime(s, "0"),
				s => string.Format(_localizationData.EveryX0Hours, s), s => _localizationData.BetweenX0AndX1,
				s => _localizationData.AtX0, s => _localizationData.ComaX0ThroughX1);

			return description;
		}

		/// <summary>
		///     Generates a description for only the DAYOFWEEK portion of the expression
		/// </summary>
		/// <returns>The DAYOFWEEK description</returns>
		private string GetDayOfWeekDescription()
		{
			string description = null;

			if (_expressionParts[5] == "*")
				// DOW is specified as * so we will not generate a description and defer to DOM part.
				// Otherwise, we could get a contradiction like "on day 1 of the month, every day"
				// or a dupe description like "every day, every day".
				description = string.Empty;
			else
				description = GetSegmentDescription(_expressionParts[5], _localizationData.ComaEveryDay, s =>
				{
					var exp = s.Contains("#") ? s.Remove(s.IndexOf("#")) :
						s.Contains("L") ? s.Replace("L", string.Empty) : s;

					return _culture.DateTimeFormat.GetDayName((DayOfWeek)Convert.ToInt32(exp));
				}, s => string.Format(_localizationData.ComaEveryX0DaysOfTheWeek, s), s => _localizationData.ComaX0ThroughX1, s =>
				{
					string format = null;
					if (s.Contains("#"))
					{
						var dayOfWeekOfMonthNumber = s.Substring(s.IndexOf("#") + 1);
						string dayOfWeekOfMonthDescription = null;
						switch (dayOfWeekOfMonthNumber)
						{
							case "1":
								dayOfWeekOfMonthDescription = _localizationData.First;
								break;
							case "2":
								dayOfWeekOfMonthDescription = _localizationData.Second;
								break;
							case "3":
								dayOfWeekOfMonthDescription = _localizationData.Third;
								break;
							case "4":
								dayOfWeekOfMonthDescription = _localizationData.Fourth;
								break;
							case "5":
								dayOfWeekOfMonthDescription = _localizationData.Fifth;
								break;
						}

						format = string.Concat(_localizationData.ComaOnThe, dayOfWeekOfMonthDescription,
							_localizationData.SpaceX0OfTheMonth);
					}
					else if (s.Contains("L"))
					{
						format = _localizationData.ComaOnTheLastX0OfTheMonth;
					}
					else
					{
						format = _localizationData.ComaOnlyOnX0;
					}

					return format;
				}, s => _localizationData.ComaX0ThroughX1);

			return description;
		}

		/// <summary>
		///     Generates a description for only the MONTH portion of the expression
		/// </summary>
		/// <returns>The MONTH description</returns>
		private string GetMonthDescription()
		{
			var description = GetSegmentDescription(_expressionParts[4], string.Empty,
				s => new DateTime(DateTime.Now.Year, Convert.ToInt32(s), 1).ToString("MMMM", _culture),
				s => string.Format(_localizationData.ComaEveryX0Months, s),
				s => _localizationData.ComaX0ThroughX1,
				s => _localizationData.ComaOnlyInX0,
				s => _localizationData.ComaX0ThroughX1);

			return description;
		}

		/// <summary>
		///     Generates a description for only the DAYOFMONTH portion of the expression
		/// </summary>
		/// <returns>The DAYOFMONTH description</returns>
		private string GetDayOfMonthDescription()
		{
			string description = null;
			var expression = _expressionParts[3];

			switch (expression)
			{
				case "L":
					description = _localizationData.ComaOnTheLastDayOfTheMonth;
					break;
				case "WL":
				case "LW":
					description = _localizationData.ComaOnTheLastWeekdayOfTheMonth;
					break;
				default:
					var weekDayNumberMatches = new Regex("(\\d{1,2}W)|(W\\d{1,2})");
					if (weekDayNumberMatches.IsMatch(expression))
					{
						var m = weekDayNumberMatches.Match(expression);
						var dayNumber = int.Parse(m.Value.Replace("W", ""));

						var dayString = dayNumber == 1
							? _localizationData.FirstWeekday
							: string.Format(_localizationData.WeekdayNearestDayX0, dayNumber);
						description = string.Format(_localizationData.ComaOnTheX0OfTheMonth, dayString);

						break;
					}
					else
					{
						// Handle "last day offset" (i.e. L-5:  "5 days before the last day of the month")
						var lastDayOffSetMatches = new Regex("L-(\\d{1,2})");
						if (lastDayOffSetMatches.IsMatch(expression))
						{
							var m = lastDayOffSetMatches.Match(expression);
							var offSetDays = m.Groups[1].Value;
							description = string.Format(_localizationData.CommaDaysBeforeTheLastDayOfTheMonth, offSetDays);
							break;
						}

						description = GetSegmentDescription(expression, _localizationData.ComaEveryDay, s => s,
							s => s == "1" ? _localizationData.ComaEveryDay : _localizationData.ComaEveryX0Days,
							s => _localizationData.ComaBetweenDayX0AndX1OfTheMonth, s => _localizationData.ComaOnDayX0OfTheMonth,
							s => _localizationData.ComaX0ThroughX1);
						break;
					}
			}

			return description;
		}

		/// <summary>
		///     Generates a description for only the YEAR portion of the expression
		/// </summary>
		/// <returns>The YEAR description</returns>
		private string GetYearDescription()
		{
			var description = GetSegmentDescription(_expressionParts[6], string.Empty,
				s => Regex.IsMatch(s, @"^\d+$") ? new DateTime(Convert.ToInt32(s), 1, 1).ToString("yyyy") : s,
				s => string.Format(_localizationData.ComaEveryX0Years, s),
				s => _localizationData.ComaX0ThroughX1,
				s => _localizationData.ComaOnlyInYearX0,
				s => _localizationData.ComaX0ThroughX1);

			return description;
		}

		/// <summary>
		///     Generates the segment description
		///     <remarks>
		///         Range expressions used the 'ComaX0ThroughX1' resource
		///         However Romanian language has different idioms for
		///         1. 'from number to number' (minutes, seconds, hours, days) => ComaMinX0ThroughMinX1 optional resource
		///         2. 'from month to month' ComaMonthX0ThroughMonthX1 optional resource
		///         3. 'from year to year' => ComaYearX0ThroughYearX1 oprtional resource
		///         therefore <paramref name="getRangeFormat" /> was introduced
		///     </remarks>
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="allDescription"></param>
		/// <param name="getSingleItemDescription"></param>
		/// <param name="getIntervalDescriptionFormat"></param>
		/// <param name="getBetweenDescriptionFormat"></param>
		/// <param name="getDescriptionFormat"></param>
		/// <param name="getRangeFormat">function that formats range expressions depending on cron parts</param>
		/// <returns></returns>
		private string GetSegmentDescription(string expression, string allDescription,
			Func<string, string> getSingleItemDescription, Func<string, string> getIntervalDescriptionFormat,
			Func<string, string> getBetweenDescriptionFormat, Func<string, string> getDescriptionFormat,
			Func<string, string> getRangeFormat)
		{
			string description = null;

			if (string.IsNullOrEmpty(expression))
			{
				description = string.Empty;
			}
			else if (expression == "*")
			{
				description = allDescription;
			}
			else if (expression.IndexOfAny(new[] { '/', '-', ',' }) == -1)
			{
				description = string.Format(getDescriptionFormat(expression), getSingleItemDescription(expression));
			}
			else if (expression.Contains("/"))
			{
				var segments = expression.Split('/');
				description = string.Format(getIntervalDescriptionFormat(segments[1]),
					getSingleItemDescription(segments[1]));

				//interval contains 'between' piece (i.e. 2-59/3 )
				if (segments[0].Contains("-"))
				{
					var betweenSegmentDescription = GenerateBetweenSegmentDescription(segments[0],
						getBetweenDescriptionFormat, getSingleItemDescription);

					if (!betweenSegmentDescription.StartsWith(", ")) description += ", ";

					description += betweenSegmentDescription;
				}
				else if (segments[0].IndexOfAny(new[] { '*', ',' }) == -1)
				{
					var rangeItemDescription = string.Format(getDescriptionFormat(segments[0]),
						getSingleItemDescription(segments[0]));
					//remove any leading comma
					rangeItemDescription = rangeItemDescription.Replace(", ", "");

					description += string.Format(_localizationData.CommaStartingX0, rangeItemDescription);
				}
			}
			else if (expression.Contains(","))
			{
				var segments = expression.Split(',');

				var descriptionContent = string.Empty;
				for (var i = 0; i < segments.Length; i++)
				{
					if (i > 0 && segments.Length > 2)
					{
						descriptionContent += ",";

						if (i < segments.Length - 1) descriptionContent += " ";
					}

					if (i > 0 && segments.Length > 1 && (i == segments.Length - 1 || segments.Length == 2))
						descriptionContent += _localizationData.SpaceAndSpace;

					if (segments[i].Contains("-"))
					{
						var betweenSegmentDescription =
							GenerateBetweenSegmentDescription(segments[i], getRangeFormat, getSingleItemDescription);

						//remove any leading comma
						betweenSegmentDescription = betweenSegmentDescription.Replace(", ", "");

						descriptionContent += betweenSegmentDescription;
					}
					else
					{
						descriptionContent += getSingleItemDescription(segments[i]);
					}
				}

				description = string.Format(getDescriptionFormat(expression), descriptionContent);
			}
			else if (expression.Contains("-"))
			{
				description = GenerateBetweenSegmentDescription(expression, getBetweenDescriptionFormat,
					getSingleItemDescription);
			}

			return description;
		}

		/// <summary>
		///     Generates the between segment description
		/// </summary>
		/// <param name="betweenExpression"></param>
		/// <param name="getBetweenDescriptionFormat"></param>
		/// <param name="getSingleItemDescription"></param>
		/// <returns>The between segment description</returns>
		private string GenerateBetweenSegmentDescription(string betweenExpression,
			Func<string, string> getBetweenDescriptionFormat, Func<string, string> getSingleItemDescription)
		{
			var description = string.Empty;
			var betweenSegments = betweenExpression.Split('-');
			var betweenSegment1Description = getSingleItemDescription(betweenSegments[0]);
			var betweenSegment2Description = getSingleItemDescription(betweenSegments[1]);
			//betweenSegment2Description = betweenSegment2Description.Replace(":00", ":59");
			var betweenDescriptionFormat = getBetweenDescriptionFormat(betweenExpression);
			description += string.Format(betweenDescriptionFormat, betweenSegment1Description,
				betweenSegment2Description);

			return description;
		}

		/// <summary>
		///     Given time parts, will contruct a formatted time description
		/// </summary>
		/// <param name="hourExpression">Hours part</param>
		/// <param name="minuteExpression">Minutes part</param>
		/// <returns>Formatted time description</returns>
		private string FormatTime(string hourExpression, string minuteExpression)
		{
			return FormatTime(hourExpression, minuteExpression, string.Empty);
		}

		/// <summary>
		///     Given time parts, will contruct a formatted time description
		/// </summary>
		/// <param name="hourExpression">Hours part</param>
		/// <param name="minuteExpression">Minutes part</param>
		/// <param name="secondExpression">Seconds part</param>
		/// <returns>Formatted time description</returns>
		private string FormatTime(string hourExpression, string minuteExpression, string secondExpression)
		{
			var hour = Convert.ToInt32(hourExpression);

			var period = string.Empty;
			if (!_use24HourTimeFormat)
			{
				period = hour >= 12 ? _localizationData.PMPeriod : _localizationData.AMPeriod;
				if (period.Length > 0)
					// add preceeding space
					period = string.Concat(" ", period);

				if (hour > 12) hour -= 12;
				if (hour == 0) hour = 12;
			}

			var minute = Convert.ToInt32(minuteExpression).ToString();
			var second = string.Empty;
			if (!string.IsNullOrEmpty(secondExpression))
				second = string.Concat(":", Convert.ToInt32(secondExpression).ToString().PadLeft(2, '0'));

			return string.Format("{0}:{1}{2}{3}", hour.ToString().PadLeft(2, '0'), minute.PadLeft(2, '0'), second,
				period);
		}

		/// <summary>
		///     Transforms the verbosity of the expression description by stripping verbosity from original description
		/// </summary>
		/// <param name="description">The description to transform</param>
		/// <param name="isVerbose">If true, will leave description as it, if false, will strip verbose parts</param>
		/// <returns>The transformed description with proper verbosity</returns>
		private string TransformVerbosity(string description, bool useVerboseFormat)
		{
			if (!useVerboseFormat)
			{
				description = description.Replace(_localizationData.ComaEveryMinute, string.Empty);
				description = description.Replace(_localizationData.ComaEveryHour, string.Empty);
				description = description.Replace(_localizationData.ComaEveryDay, string.Empty);
				description = Regex.Replace(description, @"\, ?$", "");
			}

			return description;
		}

		#region Static

		/// <summary>
		///     Generates a human readable string for the Cron Expression
		/// </summary>
		/// <param name="expression">The cron expression string</param>
		/// <returns>The cron expression description</returns>
		public static string GetDescription(string expression, out ErrorData errorData)
		{
			return GetDescription(expression, new Options(), out errorData);
		}

		/// <summary>
		///     Generates a human readable string for the Cron Expression
		/// </summary>
		/// <param name="expression">The cron expression string</param>
		/// <param name="options">Options to control the output description</param>
		/// <returns>The cron expression description</returns>
		public static string GetDescription(string expression, Options options, out ErrorData errorData)
		{
			var descriptor = new ExpressionDescriptor(expression, options);
			return descriptor.GetDescription(DescriptionTypeEnum.FULL, out errorData);
		}

		/// <summary>
		///     Generates a human readable string for the schedule definition
		/// </summary>
		/// <param name="scheduleDefinition">Schedule definition</param>
		/// <returns>The cron expression description</returns>
		public static string GetDescription(ScheduleDefinition scheduleDefinition, out ErrorData errorData)
		{
			return GetDescription(scheduleDefinition, new Options(), out errorData);
		}

		/// <summary>
		///     Generates a human readable string for the schedule definition
		/// </summary>
		/// <param name="scheduleDefinition">Schedule definition</param>
		/// <param name="options">Options to control the output description</param>
		/// <returns>The cron expression description</returns>
		public static string GetDescription(ScheduleDefinition scheduleDefinition, Options options, out ErrorData errorData)
		{
			var expression = ScheduleDefinitionToCron(scheduleDefinition);
			return GetDescription(expression, options, out errorData);
		}

		/// <summary>
		///     Converts schedule definition into cron expression
		/// </summary>
		/// <param name="scheduleDefinition">Schedule definition</param>
		/// <returns>The cron expression<returns>
		public static string ScheduleDefinitionToCron(ScheduleDefinition scheduleDefinition)
		{
			string Convert(IReadOnlyList<string> part)
			{
				int ConvertToInt(string text) => int.Parse(text);

				if (part.Contains("*") && part.Count == 1)
					return part[0];

				var converted = string.Empty;
				var dashedStrings = new List<int>();

				for (var i = 0; i < part.Count; i++)
				{
					var from = ConvertToInt(part[i]);
					dashedStrings.Add(from);

					if (i == part.Count - 1)
						break;

					var to = ConvertToInt(part[i + 1]);

					if (from + 1 == to)
						continue;

					converted += dashedStrings.Count == 1 ? i + 1 == part.Count ? $"{dashedStrings[0]}" :
						$"{dashedStrings[0]}," :
						i + 1 == part.Count ? $"{dashedStrings[0]}-{dashedStrings[dashedStrings.Count - 1]}" :
						$"{dashedStrings[0]}-{dashedStrings[dashedStrings.Count - 1]},";

					dashedStrings.Clear();
				}

				if (dashedStrings.Count == 1)
					converted += $"{dashedStrings[0]}";
				else if (dashedStrings.Count != 0)
					converted += $"{dashedStrings[0]}-{dashedStrings[dashedStrings.Count - 1]}";

				return converted;
			}

			var second = Convert(scheduleDefinition.second);
			var minute = Convert(scheduleDefinition.minute);
			var hour = Convert(scheduleDefinition.hour);
			var dayOfMonth = Convert(scheduleDefinition.dayOfMonth);
			var month = Convert(scheduleDefinition.month);
			var dayOfWeek = Convert(scheduleDefinition.dayOfWeek);
			var year = Convert(scheduleDefinition.year);

			var expression = $"{second} {minute} {hour} {dayOfMonth} {month} {dayOfWeek} {year}";
			return expression;
		}

		/// <summary>
		///     Converts cron expression into schedule definition
		/// </summary>
		/// <param name="expression">Cron expression</param>
		/// <returns>Schedule definition</returns>
		public static ScheduleDefinition CronToScheduleDefinition(string expression)
		{
			List<string> Convert(string part)
			{
				var subParts = part.Split(',').ToList();
				var finalList = new List<string>();

				foreach (var subPart in subParts)
				{
					if (!subPart.Contains('-'))
					{
						finalList.Add(subPart);
						continue;
					}

					var range = subPart.Split('-').ToList();
					var from = int.Parse(range[0]);
					var to = int.Parse(range[1]);

					for (int i = from; i <= to; i++)
						finalList.Add($"{i}");
				}

				return finalList;
			}

			var split = expression.Split(' ');

			if (split.Length != 7)
			{
				Debug.LogError("Cron expression should consist of exactly 7 parts!");
				return null;
			}

			var scheduleDefinition = new ScheduleDefinition
			{
				second = Convert(split[0]),
				minute = Convert(split[1]),
				hour = Convert(split[2]),
				dayOfMonth = Convert(split[3]),
				month = Convert(split[4]),
				dayOfWeek = Convert(split[5]),
				year = Convert(split[6])
			};

			return scheduleDefinition;
		}

		#endregion
	}
}
