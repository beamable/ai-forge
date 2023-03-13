using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Beamable.Microservices.Storage
{
    internal static class AiInventoryItemCollection
    {
        private static IMongoCollection<AiInventoryItem> _collection;

        private static async ValueTask<IMongoCollection<AiInventoryItem>> Get(IMongoDatabase db)
        {
            if (_collection is null)
            {
                _collection = db.GetCollection<AiInventoryItem>("inventory");
                await _collection.Indexes.CreateOneAsync(
                    new CreateIndexModel<AiInventoryItem>(
                        Builders<AiInventoryItem>.IndexKeys
                            .Ascending(x => x.GamerTag)
                            .Ascending(x => x.ContentId)
                            .Ascending(x => x.ItemId),
                        new CreateIndexOptions { Unique = true }
                    )
                );
            }

            return _collection;
        }

        public static async Task<List<AiInventoryItem>> GetAll(IMongoDatabase db, string gamerTag)
        {
            var collection = await Get(db);
            var mints = await collection
                .Find(x => x.GamerTag == gamerTag)
                .ToListAsync();
            return mints;
        }

        public static async Task Save(IMongoDatabase db, AiInventoryItem item)
        {
            var collection = await Get(db);
            await collection.InsertOneAsync(item);
        }

        public static async Task Delete(IMongoDatabase db, AiInventoryItem item)
        {
            var collection = await Get(db);
            await collection.DeleteOneAsync(x => x.ItemId == item.ItemId);
        }
    }
}