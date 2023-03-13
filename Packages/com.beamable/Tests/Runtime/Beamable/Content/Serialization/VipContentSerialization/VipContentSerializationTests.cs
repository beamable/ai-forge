using Beamable.Common.Content.Serialization;
using Beamable.Common.Inventory;
using NUnit.Framework;
using System.Collections.Generic;

namespace Beamable.Tests.Content.Serialization.VipContentSerialization
{
	public class VipContentSerializationTests
	{
		[Test]
		public void SerializeVipContent()
		{
			// ReSharper disable once Unity.IncorrectScriptableObjectInstantiation
			var vip = new VipContent
			{
				currency = new CurrencyRef("currency.gems"),
				tiers = new List<VipTier>
			{
			   new VipTier
			   {
				  name="test",
				  qualifyThreshold = 1,
				  disqualifyThreshold = 1,
				  multipliers = new List<VipBonus>()
			   }
			}
			};
			var json = ClientContentSerializer.SerializeContent(vip);

			var expected = "{\"id\":\"vip.\",\"version\":\"\",\"properties\":{\"currency\":{\"data\":\"currency.gems\"},\"tiers\":{\"data\":[{\"name\":\"test\",\"qualifyThreshold\":1,\"disqualifyThreshold\":1,\"multipliers\":[]}]}}}";
			Assert.AreEqual(expected, json);
		}
	}
}
