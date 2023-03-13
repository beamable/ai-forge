using System;
#if !BEAMABLE_IGNORE_MONGO_MOCKS

namespace MongoDB.Bson.Serialization.Attributes
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class BsonRepresentationAttribute : BsonSerializationOptionsAttribute
	{
		private BsonType _representation;
		public BsonRepresentationAttribute(BsonType representation) => this._representation = representation;
	}
}
#endif
