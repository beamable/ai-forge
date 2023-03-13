using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using NUnit.Framework;

namespace Beamable.Editor.Tests
{
	public class ContentObjectTests
	{
		[Test]
		public void Validate_WellConfigured_Test()
		{
			var sampleValidationCtx = new ValidationContext();

			var testContent = new TestContent(0);
			Assert.DoesNotThrow(() => testContent.Validate(sampleValidationCtx));

			testContent = new TestContent(20);
			Assert.DoesNotThrow(() => testContent.Validate(sampleValidationCtx));

			testContent = new TestContent(-32);
			Assert.Throws<AggregateContentValidationException>(() => testContent.Validate(sampleValidationCtx));
		}

		[Test]
		public void Validate_PoorlyConfigured_Test()
		{
			var sampleValidationCtx = new ValidationContext();

			var content = new PoorlyConfiguredContent("boo!", -32);
			Assert.Throws<AggregateContentValidationException>(() => content.Validate(sampleValidationCtx));
		}
	}

	public class TestContent : ContentObject
	{
		[MustBePositive(AllowZero = true)]
		public int number;

		public TestContent(int number)
		{
			this.number = number;
		}
	}

	public class PoorlyConfiguredContent : ContentObject
	{
		[MustBePositive]
		public string s;

		[MustBePositive]
		public int number;

		public PoorlyConfiguredContent(string s, int number)
		{
			this.s = s;
			this.number = number;
		}
	}
}
