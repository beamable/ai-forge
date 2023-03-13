using Beamable.Serialization.SmallerJSON;
using NUnit.Framework;
using System.Text;
using Unity.PerformanceTesting;

namespace Beamable.Editor.Tests.SmallerJson
{
	public class InfinityTests
	{
		[Performance]
		[TestCase(32, TestName = "PreventTestWithRegularNumber")]
		[TestCase(double.PositiveInfinity, TestName = "PreventTestWithInvalidNumber")]
		public void PerformanceForJsonDouble(double val)
		{
			void Method()
			{
				var dict = new ArrayDict { ["x"] = val };
				try
				{
					Json.Serialize(dict, new StringBuilder());
				}
				catch
				{
					// it might fail, who cares?
				}
			}

			Measure.Method(Method)
				   .WarmupCount(10)
				   .MeasurementCount(100)
				   .IterationsPerMeasurement(50) // boost this number to really give it a crank...
				   .GC()
				   .Run();
		}

		[TestCase(double.PositiveInfinity, TestName = "infinity-throws")]
		[TestCase(double.NaN, TestName = "nan-throws")]
		[TestCase(double.NegativeInfinity, TestName = "infinity-neg-throws")]
		public void ThrowsException(double val)
		{
			Assert.Throws<CannotSerializeException>(() =>
			{
				var dict = new ArrayDict { ["x"] = val };
				Json.Serialize(dict, new StringBuilder());
			}, "Should be an exception");
		}

		[TestCase(double.MinValue, TestName = "min-is-fine")]
		[TestCase(double.MaxValue, TestName = "min-is-fine")]
		[TestCase(0, TestName = "zero-is-fine")]
		[TestCase(-32, TestName = "negatives-are-fine")]
		[TestCase(32, TestName = "positives-are-fine")]
		public void DoesntThrowException(double val)
		{
			var dict = new ArrayDict { ["x"] = val };
			Json.Serialize(dict, new StringBuilder());
		}

	}

}
