using System;
#if !BEAMABLE_IGNORE_MONGO_MOCKS

namespace MongoDB.Bson.Serialization.Attributes
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	[BsonMemberMapAttributeUsage(AllowMultipleMembers = false)]
	public class BsonIdAttribute : Attribute, IBsonMemberMapAttribute { }
}
#endif
