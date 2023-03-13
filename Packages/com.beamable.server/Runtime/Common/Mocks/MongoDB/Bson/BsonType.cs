using System;
#if !BEAMABLE_IGNORE_MONGO_MOCKS
namespace MongoDB.Bson
{
	[Serializable]
	public enum BsonType
	{
		EndOfDocument = 0,
		Double = 1,
		String = 2,
		Document = 3,
		Array = 4,
		Binary = 5,
		Undefined = 6,
		ObjectId = 7,
		Boolean = 8,
		DateTime = 9,
		Null = 10, // 0x0000000A
		RegularExpression = 11, // 0x0000000B
		JavaScript = 13, // 0x0000000D
		Symbol = 14, // 0x0000000E
		JavaScriptWithScope = 15, // 0x0000000F
		Int32 = 16, // 0x00000010
		Timestamp = 17, // 0x00000011
		Int64 = 18, // 0x00000012
		Decimal128 = 19, // 0x00000013
		MaxKey = 127, // 0x0000007F
		MinKey = 255, // 0x000000FF
	}
}
#endif
