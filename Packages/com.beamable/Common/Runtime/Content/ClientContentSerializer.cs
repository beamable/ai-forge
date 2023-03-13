using UnityEngine;

namespace Beamable.Common.Content.Serialization
{
	public class ClientContentSerializer : ContentSerializer<ContentObject>
	{
		private static ClientContentSerializer _instance;

		private static ContentSerializer<ContentObject> Instance => _instance ?? (_instance = new ClientContentSerializer());

		public static string SerializeContent<TContent>(TContent content) where TContent : IContentObject, new() =>
		   Instance.Serialize(content);

		public new static string SerializeProperties<TContent>(TContent content) where TContent : IContentObject =>
		   Instance.SerializeProperties(content);

		protected override TContent CreateInstance<TContent>()
		{
			return ScriptableObject.CreateInstance<TContent>();
		}

		public static TContent DeserializeContent<TContent>(string json, bool disableExceptions = false) where TContent : ContentObject, IContentObject, new() =>
		   Instance.Deserialize<TContent>(json, disableExceptions);
	}
}
