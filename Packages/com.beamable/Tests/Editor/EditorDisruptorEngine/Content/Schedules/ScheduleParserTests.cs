using Beamable.Common.Content;
using Beamable.Editor.Models.Schedules;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Beamable.Editor.Tests.Content
{
	public class ScheduleParserTests : ScheduleParser
	{
		[TestCase(0, 1, 0, 0, 1)]
		[TestCase(0, 1, 30, 0, 2)]
		[TestCase(0, 1, 0, 30, 3)]
		[TestCase(0, 1, 30, 30, 4)]
		[TestCase(0, 2, 0, 0, 5)]
		[TestCase(0, 2, 30, 0, 6)]
		[TestCase(0, 2, 0, 30, 7)]
		[TestCase(0, 2, 30, 30, 8)]
		[TestCase(0, 3, 0, 0, 9)]
		[TestCase(0, 3, 30, 0, 10)]
		[TestCase(0, 3, 0, 30, 11)]
		[TestCase(0, 3, 30, 30, 12)]
		[TestCase(5, 12, 15, 45, 13)]
		[TestCase(0, 0, 0, 30, 14)]
		[TestCase(0, 1, 59, 0, 15)]
		public void Listing_Periods_Parser_Test(int fromHour, int toHour, int fromMinute, int toMinute, int expectedResultIndex)
		{
			var expectedResult = GetExpectedResult_DaysMode(expectedResultIndex);
			var result = GetPeriodsSchedulesDefinitions(fromHour, toHour, fromMinute, toMinute, new List<string> { "1" });
			Assert.IsTrue(CompareScheduleDefinitions(result, expectedResult));
		}

		private bool CompareScheduleDefinitions(List<ScheduleDefinition> result, List<ScheduleDefinition> expectedResult)
		{
			if (result.Count != expectedResult.Count)
			{
				Assert.Fail("Different collection sizes (result.Count != expectedResult.Count");
			}

			for (int i = 0; i < result.Count; i++)
			{
				bool isCorrect = result[i].minute.All(expectedResult[i].minute.Contains);
				if (!isCorrect) Assert.Fail("Minutes are not equal");

				isCorrect = result[i].hour.All(expectedResult[i].hour.Contains);
				if (!isCorrect) Assert.Fail("Hours are not equal");
			}
			return true;
		}
		private List<ScheduleDefinition> GetExpectedResult_DaysMode(int expectedResultIndex)
		{
			var definitions = new List<ScheduleDefinition>();
			var allRange = new List<string> { "*" };
			ScheduleDefinition definition;

			switch (expectedResultIndex)
			{
				case 1:
					definition = new ScheduleDefinition(
						allRange,
						allRange,
						new List<string> { "0" },
						allRange,
						allRange,
						allRange,
						new List<string> { "1" });
					definitions.Add(definition);
					break;

				case 2:
					definition = new ScheduleDefinition(
						allRange,
						ConvertIntoRangeList(30, 59),
						new List<string> { "0" },
						allRange,
						allRange,
						allRange,
						new List<string> { "1" });
					definitions.Add(definition);
					break;

				case 3:
					definition = new ScheduleDefinition(
						allRange,
						allRange,
						new List<string> { "0" },
						allRange,
						allRange,
						allRange,
						new List<string> { "1" });
					definitions.Add(definition);

					definition = new ScheduleDefinition(
						allRange,
						ConvertIntoRangeList(0, 29),
						new List<string> { "1" },
						allRange,
						allRange,
						allRange,
						new List<string> { "1" });
					definitions.Add(definition);
					break;

				case 4:
					definition = new ScheduleDefinition(
						allRange,
						ConvertIntoRangeList(30, 59),
						new List<string> { "0" },
						allRange,
						allRange,
						allRange,
						new List<string> { "1" });
					definitions.Add(definition);

					definition = new ScheduleDefinition(
						allRange,
						ConvertIntoRangeList(0, 29),
						new List<string> { "1" },
						allRange,
						allRange,
						allRange,
						new List<string> { "1" });
					definitions.Add(definition);
					break;

				case 5:
					definition = new ScheduleDefinition(
						allRange,
						allRange,
						ConvertIntoRangeList(0, 1),
						allRange,
						allRange,
						allRange,
						new List<string> { "1" });
					definitions.Add(definition);
					break;

				case 6:
					definition = new ScheduleDefinition(
						allRange,
						ConvertIntoRangeList(0, 59),
						ConvertIntoRangeList(0, 1),
						allRange,
						allRange,
						allRange,
						new List<string> { "1" });
					definitions.Add(definition);
					break;

				case 7:
					definition = new ScheduleDefinition(
						allRange,
						allRange,
						ConvertIntoRangeList(0, 1),
						allRange,
						allRange,
						allRange,
						new List<string> { "1" });
					definitions.Add(definition);

					definition = new ScheduleDefinition(
						allRange,
						ConvertIntoRangeList(0, 29),
						new List<string> { "2" },
						allRange,
						allRange,
						allRange,
						new List<string> { "1" });
					definitions.Add(definition);
					break;

				case 8:
					definition = new ScheduleDefinition(
						allRange,
						ConvertIntoRangeList(30, 59),
						new List<string> { "0" },
						allRange,
						allRange,
						allRange,
						new List<string> { "1" });
					definitions.Add(definition);

					definition = new ScheduleDefinition(
						allRange,
						allRange,
						new List<string> { "1" },
						allRange,
						allRange,
						allRange,
						new List<string> { "1" });
					definitions.Add(definition);

					definition = new ScheduleDefinition(
						allRange,
						ConvertIntoRangeList(0, 29),
						new List<string> { "2" },
						allRange,
						allRange,
						allRange,
						new List<string> { "1" });
					definitions.Add(definition);
					break;

				case 9:
					definition = new ScheduleDefinition(
						allRange,
						allRange,
						ConvertIntoRangeList(0, 2),
						allRange,
						allRange,
						allRange,
						new List<string> { "1" });
					definitions.Add(definition);
					break;

				case 10:
					definition = new ScheduleDefinition(
						allRange,
						ConvertIntoRangeList(30, 59),
						new List<string> { "0" },
						allRange,
						allRange,
						allRange,
						new List<string> { "1" });
					definitions.Add(definition);

					definition = new ScheduleDefinition(
						allRange,
						allRange,
						ConvertIntoRangeList(1, 2),
						allRange,
						allRange,
						allRange,
						new List<string> { "1" });
					definitions.Add(definition);
					break;

				case 11:
					definition = new ScheduleDefinition(
						allRange,
						allRange,
						ConvertIntoRangeList(0, 2),
						allRange,
						allRange,
						allRange,
						new List<string> { "1" });
					definitions.Add(definition);

					definition = new ScheduleDefinition(
						allRange,
						ConvertIntoRangeList(0, 29),
						new List<string> { "3" },
						allRange,
						allRange,
						allRange,
						new List<string> { "1" });
					definitions.Add(definition);
					break;

				case 12:
					definition = new ScheduleDefinition(
						allRange,
						ConvertIntoRangeList(30, 59),
						new List<string> { "0" },
						allRange,
						allRange,
						allRange,
						new List<string> { "1" });
					definitions.Add(definition);

					definition = new ScheduleDefinition(
						allRange,
						allRange,
						ConvertIntoRangeList(1, 2),
						allRange,
						allRange,
						allRange,
						new List<string> { "1" });
					definitions.Add(definition);

					definition = new ScheduleDefinition(
						allRange,
						ConvertIntoRangeList(0, 29),
						new List<string> { "3" },
						allRange,
						allRange,
						allRange,
						new List<string> { "1" });
					definitions.Add(definition);
					break;

				case 13:
					definition = new ScheduleDefinition(
						allRange,
						ConvertIntoRangeList(15, 59),
						new List<string> { "5" },
						allRange,
						allRange,
						allRange,
						new List<string> { "1" });
					definitions.Add(definition);

					definition = new ScheduleDefinition(
						allRange,
						allRange,
						ConvertIntoRangeList(6, 11),
						allRange,
						allRange,
						allRange,
						new List<string> { "1" });
					definitions.Add(definition);

					definition = new ScheduleDefinition(
						allRange,
						ConvertIntoRangeList(0, 44),
						new List<string> { "12" },
						allRange,
						allRange,
						allRange,
						new List<string> { "1" });
					definitions.Add(definition);
					break;

				case 14:
					definition = new ScheduleDefinition(
						allRange,
						ConvertIntoRangeList(0, 29),
						new List<string> { "0" },
						allRange,
						allRange,
						allRange,
						new List<string> { "1" });
					definitions.Add(definition);
					break;

				case 15:
					definition = new ScheduleDefinition(
						allRange,
						new List<string> { "59" },
						new List<string> { "0" },
						allRange,
						allRange,
						allRange,
						new List<string> { "1" });
					definitions.Add(definition);
					break;
			}

			return definitions;
		}
		private List<string> ConvertIntoRangeList(int from, int to)
		{
			var tempList = new List<string>();
			for (int i = from; i <= to; i++)
				tempList.Add($"{i}");
			return tempList;
		}
	}
}
