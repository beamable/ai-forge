#if !BEAMABLE_IGNORE_MONGO_MOCKS

using System;

namespace MongoDB.Bson.Serialization.Attributes
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class BsonIgnoreIfNullAttribute : Attribute, IBsonMemberMapAttribute
	{
		private bool _value;

		public BsonIgnoreIfNullAttribute() => this._value = true;

		public BsonIgnoreIfNullAttribute(bool value) => this._value = value;

		public bool Value => this._value;

	}
}
#endif
