using Beamable.Serialization.SmallerJSON;
using System.Collections.Generic;

namespace PubNubMessaging.Core
{
	#region "Json Pluggable Library"
	public interface IJsonPluggableLibrary
	{
		bool IsArrayCompatible(string jsonString);

		bool IsDictionaryCompatible(string jsonString);

		string SerializeToJsonString(object objectToSerialize);

		List<object> DeserializeToListOfObject(string jsonString);

		object DeserializeToObject(string jsonString);

		IDictionary<string, object> DeserializeToDictionaryOfObject(string jsonString);
	}

	public static class JSONSerializer
	{
		public static IJsonPluggableLibrary JsonPluggableLibrary = new SmallerJSONObjectSerializer();

	}
	public class SmallerJSONObjectSerializer : IJsonPluggableLibrary
	{
		public bool IsArrayCompatible(string jsonString)
		{
			return jsonString.Trim().StartsWith("[");
		}

		public bool IsDictionaryCompatible(string jsonString)
		{
			return jsonString.Trim().StartsWith("{");
		}

		public string SerializeToJsonString(object objectToSerialize)
		{
			string json = global::Beamable.Serialization.SmallerJSON.Json.Serialize(objectToSerialize, SharedStringBuilder.Builder);
			return PubnubCryptoBase.ConvertHexToUnicodeChars(json);
		}

		public List<object> DeserializeToListOfObject(string jsonString)
		{
			return Json.Deserialize(jsonString) as List<object>;
		}

		public object DeserializeToObject(string jsonString)
		{
			return Json.Deserialize(jsonString) as object;
		}

		public IDictionary<string, object> DeserializeToDictionaryOfObject(string jsonString)
		{
			return Json.Deserialize(jsonString) as IDictionary<string, object>;
		}
	}
	#endregion
}

