using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Beamable.Microservices.Storage
{
    internal class AiInventoryItem
    {
        [BsonElement("_id")] public ObjectId ID { get; set; } = ObjectId.GenerateNewId();
        public string ItemId { get; set; } = Guid.NewGuid().ToString();
        public string GamerTag { get; set; }
        public string ContentId { get; set; }
        public Dictionary<string, string> Properties { get; set; }
    }
}