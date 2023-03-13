using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using Beamable.Common.Shop;
using NUnit.Framework;
using UnityEngine;

namespace Beamable.Tests.Modules.Shop.ListingContentTests
{
	public class ValidateListingTests
	{
		[Test]
		public void StoreWithNoTitles_YieldsNoValidationErrors()
		{
			var listing = ScriptableObject.CreateInstance<ListingContent>();
			listing.offer = new ListingOffer
			{
				titles = new OptionalNonBlankStringList
				{
					HasValue = false
				}
			};

			var anyErrors = listing.HasValidationErrors(new ValidationContext(), out var _);
			Assert.IsFalse(anyErrors);
		}

		[Test]
		public void ActivePeriod_WithNone_WithInvalidDate_YieldsNoValidationErrors()
		{
			var listing = ScriptableObject.CreateInstance<ListingContent>();
			listing.activePeriod = new OptionalPeriod
			{
				HasValue = false,
				Value = new ActivePeriod
				{
					end = new OptionalString { HasValue = false },
					start = "not-a-date"
				}
			};

			var anyErrors = listing.HasValidationErrors(new ValidationContext(), out var _);
			Assert.IsFalse(anyErrors);
		}

		[Test]
		public void ActivePeriod_WithSome_WithInvalidDate_YieldsValidationError()
		{
			var listing = ScriptableObject.CreateInstance<ListingContent>();
			listing.activePeriod = new OptionalPeriod
			{
				HasValue = true,
				Value = new ActivePeriod
				{
					end = new OptionalString { HasValue = false },
					start = "not-a-date"
				}
			};

			var anyErrors = listing.HasValidationErrors(new ValidationContext(), out var errors);
			Assert.IsTrue(anyErrors);

			Assert.AreEqual(1, errors.Count);
			Assert.AreEqual("Error with 'System.String start' on Beamable.Common.Shop.ActivePeriod. date is not in 8601 iso format. yyyy-MM-ddTHH:mm:ssZ", errors[0]);
		}
	}
}
