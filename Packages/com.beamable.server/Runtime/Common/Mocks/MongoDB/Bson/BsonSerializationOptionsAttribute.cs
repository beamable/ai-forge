using System;
#if !BEAMABLE_IGNORE_MONGO_MOCKS

namespace MongoDB.Bson.Serialization.Attributes
{
	public abstract class BsonSerializationOptionsAttribute : Attribute, IBsonMemberMapAttribute { }
}

#endif
