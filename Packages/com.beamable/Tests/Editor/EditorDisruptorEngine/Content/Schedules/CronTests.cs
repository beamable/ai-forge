using Beamable.Common.Content;
using Beamable.CronExpression;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Beamable.Editor.Tests.Content
{
	public class CronTests
	{
		[TestCase("* * * * * * *", "Every Second")]
		[TestCase("8 33 14 * * * *", "At 02:33:08 PM")]
		[TestCase("16 33 14 * * * *", "At 02:33:16 PM")]
		[TestCase("16 33 14 * * 3 *", "At 02:33:16 PM, only on Wednesday")]
		[TestCase("16 33 14 * * 2,5-6 *", "At 02:33:16 PM, only on Tuesday and Friday through Saturday")]
		[TestCase("16 33 14 * * 1,2-3,5,6-7 *", "At 02:33:16 PM, only on Monday, Tuesday through Wednesday, Friday, and Saturday through Sunday")]
		[TestCase("16 33 14 * * 1,2-3,5,6-7 2020", "At 02:33:16 PM, only on Monday, Tuesday through Wednesday, Friday, and Saturday through Sunday, only in 2020")]
		[TestCase("16 33 14 * * 1,2-3,5,6-7 2020-2022", "At 02:33:16 PM, only on Monday, Tuesday through Wednesday, Friday, and Saturday through Sunday, 2020 through 2022")]
		[TestCase("16 33 14 1 11 * 2021", "At 02:33:16 PM, on day 1 of the month, only in November, only in 2021")]
		[TestCase("16 33 14 1-2,4-5,10,12-13 1-3,6-8,10,12 * 2021", "At 02:33:16 PM, on day 1 through 2, 4 through 5, 10, and 12 through 13 of the month, only in January through March, June through August, October, and December, only in 2021")]
		[TestCase("16 33 14 1-2,4-5,10,12-13 1-3,6-8,10,12 * 2021", "O 14:33:16, od 1 do 2, od 4 do 5, 10, i od 12 do 13-ego dnia miesiąca, tylko od styczeń do marzec, od czerwiec do sierpień, październik, i grudzień, tylko 2021", CronLocale.pl_PL)]
		public void CRON_Correct_Description_Result(string cronString, string expectedResult, CronLocale locale = CronLocale.en_US)
		{
			var output = ExpressionDescriptor.GetDescription(cronString, new Options { Locale = locale }, out var errorData);
			Assert.IsFalse(errorData.IsError, "Error flag should be false");
			Assert.IsTrue(output.ToLower().Equals(expectedResult.ToLower()), $"Output is \"{output}\" but should be \"{expectedResult}\"");
		}

		[TestCase("", "Error: Field 'expression' not found.")]
		[TestCase("*", "Error: Expression has 1 parts. Exactly 7 parts are required.")]
		[TestCase("* *", "Error: Expression has 2 parts. Exactly 7 parts are required.")]
		[TestCase("* * *", "Error: Expression has 3 parts. Exactly 7 parts are required.")]
		[TestCase("* * * *", "Error: Expression has 4 parts. Exactly 7 parts are required.")]
		[TestCase("* * * * *", "Error: Expression has 5 parts. Exactly 7 parts are required.")]
		[TestCase("* * * * * *", "Error: Expression has 6 parts. Exactly 7 parts are required.")]
		[TestCase("* * * * * * * *", "Error: Expression has 8 parts. Exactly 7 parts are required.")]
		[TestCase("*/ * * * * * *")]
		[TestCase("* * * * * 8 *")]
		[TestCase("-1 * * * * * *")]
		[TestCase("-1 * * * * * *")]
		[TestCase("61 * * * * * *")]
		[TestCase("* -1 * * * * *")]
		[TestCase("* 61 * * * * *")]
		[TestCase("* * -1 * * * *")]
		[TestCase("* * 24 * * * *")]
		[TestCase("* * * -1 * * *")]
		[TestCase("* * * 0 * * *")]
		[TestCase("* * * 32 * * *")]
		[TestCase("* * * * -1 * *")]
		[TestCase("* * * * 0 * *")]
		[TestCase("* * * * 13 * *")]
		[TestCase("* * * * * 0 *")]
		[TestCase("* * * * * 8 *")]
		[TestCase("* * * * * * -1")]
		[TestCase("* * * * * * 999")]
		[TestCase("* * * * * * 10000")]
		public void CRON_Wrong_Description_Result(string cronString, string expectedResult = "Error: CRON validation is not passing. CRON supports only numbers [0-9] and special characters [,-*]", CronLocale locale = CronLocale.en_US)
		{
			var output = ExpressionDescriptor.GetDescription(cronString, new Options { Locale = locale }, out var errorData);
			Assert.IsTrue(errorData.IsError, "Error flag should be true");
			Assert.IsTrue(output.Equals(expectedResult), $"Output is \"{output}\" but should be \"{expectedResult}\"");
		}

		[Test]
		[TestCaseSource(nameof(ConvertScheduleDefinitionToCRONData))]
		public void Convert_CRON_To_Schedule_Definition(
			List<string> second,
			List<string> minute,
			List<string> hour,
			List<string> dayOfMonth,
			List<string> month,
			List<string> dayOfWeek,
			List<string> year,
			string cronString)
		{
			bool AreEquals(ScheduleDefinition def1, ScheduleDefinition def2)
			{
				return def1.second.SequenceEqual(def2.second) &&
					   def1.minute.SequenceEqual(def2.minute) &&
					   def1.hour.SequenceEqual(def2.hour) &&
					   def1.dayOfMonth.SequenceEqual(def2.dayOfMonth) &&
					   def1.month.SequenceEqual(def2.month) &&
					   def1.dayOfWeek.SequenceEqual(def2.dayOfWeek) &&
					   def1.year.SequenceEqual(def2.year);
			}

			var expectedResult = new ScheduleDefinition
			{
				second = second,
				minute = minute,
				hour = hour,
				dayOfMonth = dayOfMonth,
				month = month,
				dayOfWeek = dayOfWeek,
				year = year
			};
			var output = ExpressionDescriptor.CronToScheduleDefinition(cronString);

			Assert.IsTrue(AreEquals(output, expectedResult), $"Output is \"{output}\" but should be \"{expectedResult}\"");
		}

		[Test]
		[TestCaseSource(nameof(ConvertScheduleDefinitionToCRONData))]
		public void Convert_Schedule_Definition_To_CRON(
			List<string> second,
			List<string> minute,
			List<string> hour,
			List<string> dayOfMonth,
			List<string> month,
			List<string> dayOfWeek,
			List<string> year,
			string expectedResult)
		{
			var scheduleDefinition = new ScheduleDefinition
			{
				second = second,
				minute = minute,
				hour = hour,
				dayOfMonth = dayOfMonth,
				month = month,
				dayOfWeek = dayOfWeek,
				year = year
			};
			var output = ExpressionDescriptor.ScheduleDefinitionToCron(scheduleDefinition);
			Assert.IsTrue(output.Equals(expectedResult), $"Output is \"{output}\" but should be \"{expectedResult}\"");
		}

		public static IEnumerable<TestCaseData> ConvertScheduleDefinitionToCRONData()
		{
			yield return new TestCaseData(
				new List<string> { "30" },
				new List<string> { "2" },
				new List<string> { "5" },
				new List<string> { "*" },
				new List<string> { "*" },
				new List<string> { "*" },
				new List<string> { "*" },
				"30 2 5 * * * *");

			yield return new TestCaseData(
				new List<string> { "30" },
				new List<string> { "2" },
				new List<string> { "5" },
				new List<string> { "1", "2", "3", "6", "7" },
				new List<string> { "*" },
				new List<string> { "*" },
				new List<string> { "*" },
				"30 2 5 1-3,6-7 * * *");

			yield return new TestCaseData(
				new List<string> { "30" },
				new List<string> { "2" },
				new List<string> { "5" },
				new List<string> { "10" },
				new List<string> { "1", "2", "3", "4", "5" },
				new List<string> { "*" },
				new List<string> { "2022" },
				"30 2 5 10 1-5 * 2022");

			yield return new TestCaseData(
				new List<string> { "30" },
				new List<string> { "2" },
				new List<string> { "5" },
				new List<string> { "2", "3", "4", "8", "10", "11", "12" },
				new List<string> { "1", "2", "3", "5", "6", "7", "10" },
				new List<string> { "*" },
				new List<string> { "2022", "2023", "2024" },
				"30 2 5 2-4,8,10-12 1-3,5-7,10 * 2022-2024");
		}
	}
}
