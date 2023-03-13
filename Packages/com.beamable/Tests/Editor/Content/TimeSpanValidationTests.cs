using Beamable.Common.Content.Validation;
using NUnit.Framework;

namespace Beamable.Editor.Tests.Content
{
	public class TimeSpanValidationTests
	{
		[TestCase("P14D")] // every 14 days
		[TestCase("P2M15D")] // every two and a half months, approximately
		[TestCase("PT1H")] // every hour
		[TestCase("P1DT12H")] // every day and a half
		[TestCase("PT36H")] // another way to write every day and a half
		public void ValidPeriods(string period)
		{
			var valid = MustBeTimeSpanDuration.TryParseTimeSpan(period, out _, out _);
			Assert.IsTrue(valid);
		}

		[TestCase("tuna")]
		[TestCase("P72H")]
		public void InValidPeriods(string period)
		{
			var valid = MustBeTimeSpanDuration.TryParseTimeSpan(period, out _, out _);
			Assert.IsFalse(valid);
		}
	}
}
