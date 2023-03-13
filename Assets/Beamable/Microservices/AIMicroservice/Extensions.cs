using System.Collections.Generic;
using System.Linq;
using Beamable.Common;
using Beamable.Common.Api.Inventory;
using Beamable.Microservices.Storage;

namespace Beamable.Microservices
{
    internal static class Extensions
    {
        public static FederatedInventoryProxyState ToFederatedInventoryProxyState(
            this IEnumerable<AiInventoryItem> items)
        {
            var itemsMap = items
                .GroupBy(x => x.ContentId)
                .Select(x => new
                {
                    contentId = x.Key,
                    items = x.Select(xx => new FederatedItemProxy
                    {
                        proxyId = xx.ItemId,
                        properties = xx.Properties.Select(p => new ItemProperty
                        {
                            name = p.Key,
                            value = p.Value
                        }).ToList()
                    }).ToList()
                })
                .ToDictionary(x => x.contentId, x => x.items);

            return new FederatedInventoryProxyState
            {
                currencies = new Dictionary<string, long>(),
                items = itemsMap
            };
        }
    }
}