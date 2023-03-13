using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Pooling;
using Beamable.Serialization;

namespace Beamable.Api
{
	public static class BeamableRequesterExtensions
	{
		public static Promise<T> RequestJson<T>(this IBeamableRequester requester, Method method, string uri, JsonSerializable.ISerializable body,
		   bool includeAuthHeader = true)
		{
			var jsonFields = JsonSerializable.Serialize(body);

			using (var pooledBuilder = StringBuilderPool.StaticPool.Spawn())
			{
				var json = Serialization.SmallerJSON.Json.Serialize(jsonFields, pooledBuilder.Builder);
				return requester.Request<T>(method, uri, json, includeAuthHeader);
			}
		}
	}
}
