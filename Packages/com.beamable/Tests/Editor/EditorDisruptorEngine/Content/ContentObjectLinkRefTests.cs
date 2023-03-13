using Beamable.Common.Announcements;
using Beamable.Common.Content;
using Beamable.Common.Groups;
using Beamable.Common.Inventory;
using Beamable.Common.Leaderboards;
using Beamable.Common.Shop;
using Beamable.Common.Tournaments;
using Beamable.Experimental.Common.Calendars;
using NUnit.Framework;

namespace Beamable.Editor.Tests.Beamable.Content
{
	public class ContentObjectLinkRefTests
	{
		/// <summary>
		/// Tests that these ContentObject subclasses have both a
		/// Ref class and Link class
		/// </summary>
		[Test]
		public void ContentObjects_RefAndLinkExist()
		{
			//
			Assert.IsNotNull(typeof(AnnouncementContent));
			Assert.IsNotNull(typeof(AnnouncementRef));
			Assert.IsNotNull(typeof(AnnouncementLink));
			//
			Assert.IsNotNull(typeof(CalendarContent));
			Assert.IsNotNull(typeof(CalendarRef));
			Assert.IsNotNull(typeof(CalendarLink));
			//
			Assert.IsNotNull(typeof(CurrencyContent));
			Assert.IsNotNull(typeof(CurrencyRef));
			Assert.IsNotNull(typeof(CurrencyLink));
			//
			Assert.IsNotNull(typeof(EmailContent));
			Assert.IsNotNull(typeof(EmailRef));
			Assert.IsNotNull(typeof(EmailLink));
			//
			Assert.IsNotNull(typeof(EventContent));
			Assert.IsNotNull(typeof(EventRef));
			Assert.IsNotNull(typeof(EventLink));
			//
			Assert.IsNotNull(typeof(GroupDonationsContent));
			Assert.IsNotNull(typeof(GroupDonationContentRef));
			Assert.IsNotNull(typeof(GroupDonationContentLink));
			//
			Assert.IsNotNull(typeof(ItemContent));
			Assert.IsNotNull(typeof(ItemRef));
			Assert.IsNotNull(typeof(ItemLink));
			//
			Assert.IsNotNull(typeof(LeaderboardContent));
			Assert.IsNotNull(typeof(LeaderboardRef));
			Assert.IsNotNull(typeof(LeaderboardLink));
			//
			Assert.IsNotNull(typeof(ListingContent));
			Assert.IsNotNull(typeof(ListingRef));
			Assert.IsNotNull(typeof(ListingLink));
			//
			Assert.IsNotNull(typeof(SKUContent));
			Assert.IsNotNull(typeof(SKURef));
			Assert.IsNotNull(typeof(SKULink));
			//
			Assert.IsNotNull(typeof(SimGameType));
			Assert.IsNotNull(typeof(SimGameTypeRef));
			Assert.IsNotNull(typeof(SimGameTypeLink));
			//
			Assert.IsNotNull(typeof(StoreContent));
			Assert.IsNotNull(typeof(StoreRef));
			Assert.IsNotNull(typeof(StoreLink));
			//
			Assert.IsNotNull(typeof(TournamentContent));
			Assert.IsNotNull(typeof(TournamentRef));
			Assert.IsNotNull(typeof(TournamentLink));
			//
			Assert.IsNotNull(typeof(VipContent));
			Assert.IsNotNull(typeof(VipRef));
			Assert.IsNotNull(typeof(VipLink));
			//
		}
	}
}
