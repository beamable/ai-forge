using Beamable.Common.Content;
using Beamable.Common.Content.Serialization;
using Beamable.Serialization;
using Beamable.Serialization.SmallerJSON;
using System.Collections.Generic;
using System.Text;

namespace Beamable.Editor.Content.SaveRequest
{
	public class ContentSaveRequest //: JsonSerializable.ISerializable
	{
		public List<ContentDefinition> Content;

		public void Serialize(JsonSerializable.IStreamSerializer s)
		{
			s.SerializeList("content", ref Content);
		}
	}

	public class RawValue : IRawJsonProvider
	{
		private readonly string _json;

		public RawValue(string json)
		{
			_json = json;
		}
		public string ToJson()
		{
			return _json;
		}
	}

	public class ContentDefinition : IRawJsonProvider //: JsonSerializable.ISerializable
	{
		public string Id;
		public string Checksum;
		public ContentObject Content;
		public string[] Tags;
		public long LastChanged;

		//      public void Serialize(JsonSerializable.IStreamSerializer s)
		//      {
		//         s.Serialize("id", ref Id);
		//         s.Serialize("checksum", ref Checksum);
		//
		//         var json = ClientContentSerializer.SerializeProperties(Content);
		//         s.SetValue("properties", json);
		//         //s.SerializeInline("properties", ref json);
		//         //s.Serialize("properties", ref Content);
		//      }

		public string ToJson()
		{
			var dict = new ArrayDict
		 {
			{"id", Id},
			{"checksum", Checksum},
			{"tags", Tags},
			{"lastChanged", LastChanged},
			{"properties", new RawValue(ClientContentSerializer.SerializeProperties(Content))},
		 };

			var json = Json.Serialize(dict, new StringBuilder());
			return json;
		}
	}

	public class ContentMeta : JsonSerializable.ISerializable
	{
		public object Data;
		public string Link;
		public List<string> Links;
		public object Text;
		public void Serialize(JsonSerializable.IStreamSerializer s)
		{
			// TODO IMPLEMENT THIS SOMEHOW?
			//s.Serialize("data", ref Data);
		}
	}

	[System.Serializable]
	public class ContentSaveResponse
	{
		public List<ContentReference> content;
	}

	[System.Serializable]
	public class ContentReference
	{
		public string id, version, uri, checksum, visibility;
		public long lastChanged;
		public string[] tags;
	}
}
