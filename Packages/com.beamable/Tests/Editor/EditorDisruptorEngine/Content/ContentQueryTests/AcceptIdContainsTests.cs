using Beamable.Common.Content;
using NUnit.Framework;
using UnityEngine;

namespace Beamable.Editor.Tests.Beamable.Content.ContentQueryTests
{
	public class AcceptIdContainsTests : MonoBehaviour
	{
		[Test]
		public void Accepts()
		{
			var query = new ContentQuery
			{
				IdContainsConstraint = "foo"
			};
			var content = new ExampleContent();
			content.SetIdAndVersion("example.containfoosomewhere", "");

			Assert.IsTrue(query.AcceptIdContains(content));
		}

		[Test]
		public void Accepts_CaseInsensitive()
		{
			var query = new ContentQuery
			{
				IdContainsConstraint = "FoOozle"
			};
			var content = new ExampleContent();
			content.SetIdAndVersion("example.containsfOoOZLE", "");

			Assert.IsTrue(query.AcceptIdContains(content));
		}

		[Test]
		public void HandlesNullContent_NoConstraint()
		{
			var query = new ContentQuery
			{
				IdContainsConstraint = null
			};
			ExampleContent content = null;
			Assert.IsTrue(query.AcceptIdContains(content));
		}

		[Test]
		public void HandlesNullContent_IdConstraint()
		{
			var query = new ContentQuery
			{
				IdContainsConstraint = "a"
			};
			ExampleContent content = null;
			Assert.IsFalse(query.AcceptIdContains(content));
		}

		[Test]
		public void Rejects()
		{
			var query = new ContentQuery
			{
				IdContainsConstraint = "bar"
			};
			var content = new ExampleContent();
			content.SetIdAndVersion("example.containfoosomewhere", "");

			Assert.IsFalse(query.AcceptIdContains(content));
		}

		[Test]
		public void RejectsOnTypeParts()
		{
			var query = new ContentQuery
			{
				IdContainsConstraint = "exa"
			};
			var content = new ExampleContent();
			content.SetIdAndVersion("example.containfoosomewhere", "");

			Assert.IsFalse(query.AcceptIdContains(content));
		}
	}
}
