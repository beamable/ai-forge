using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using UnityEngine;

namespace Beamable.Server
{
	[Serializable]
	public class StorageDocument
	{
		[SerializeField]
		[BsonRepresentation(BsonType.ObjectId)]
		[BsonId]
		[BsonIgnoreIfDefault]
		[BsonIgnoreIfNull]
		private string _id = null; // MongoDb driver will auto-set this.

		/// <summary>
		/// The unique Mongo ID of the instance. This is automatically created when the object is first sent to the database.
		/// </summary>
		[BsonIgnore]
		public string Id => _id;
	}
}
