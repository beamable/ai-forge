using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Inventory;
using Beamable.Common.Dependencies;
using Beamable.Serialization.SmallerJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Beamable.Server
{
	// a class that acts as vessel for extension methods for service clients
	public class MicroserviceClients
	{
		private BeamContext _ctx;

		public MicroserviceClients(BeamContext context)
		{
			_ctx = context;
		}

		public TClient GetClient<TClient>() where TClient : MicroserviceClient
		{
			return _ctx.ServiceProvider.GetService<TClient>();
		}
	}



	[Serializable]
	public class MicroserviceClientDataWrapper<T> : ScriptableObject
	{
		public T Data;
	}

	public class MicroserviceClient
	{
		protected IBeamableRequester _requester;
		protected BeamContext _ctx;

		protected MicroserviceClient(IBeamableRequester requester = null)
		{
			_requester = requester;
		}

		protected MicroserviceClient(BeamContext ctx) : this(ctx?.Requester)
		{
			_ctx = ctx;
		}

		public IDependencyProvider Provider => _ctx?.ServiceProvider ?? BeamContext.Default.ServiceProvider;

		protected async Promise<T> Request<T>(string serviceName, string endpoint, string[] serializedFields)
		{
			var requester = _requester ?? await API.Instance.Map(b => b.Requester);
			return await MicroserviceClientHelper.Request<T>(requester, serviceName, endpoint, serializedFields);
		}

		protected string SerializeArgument<T>(T arg) => MicroserviceClientHelper.SerializeArgument(arg);

		protected string CreateUrl(string cid, string pid, string serviceName, string endpoint)
		   => MicroserviceClientHelper.CreateUrl(cid, pid, serviceName, endpoint);
	}


	public static class MicroserviceClientHelper
	{
		[System.Serializable]
		public class ResponseObject
		{
			public string payload;
		}

		[System.Serializable]
		public class RequestObject
		{
			public string payload;
		}

		static string _prefix;
		static readonly StringBuilder _builder = new StringBuilder();

		public static void SetPrefix(string prefix) => _prefix = prefix;

		public static string SerializeArgument(object arg)
		{
			// JSONUtility will serialize objects correctly, but doesn't handle primitives well.
			if (arg == null)
			{
				return "null";
			}

			switch (arg)
			{
				case string prim:
					return Json.IsValidJson(prim) ? "[" + prim + "]" : "\"" + prim + "\"";
				case IDictionary dictionary:
					return Json.Serialize(dictionary, _builder);
				case IEnumerable enumerable:
				{
					var output = new List<string>();
					foreach (var elem in enumerable)
					{
						output.Add(SerializeArgument(elem));
					}

					var outputJson = "[" + string.Join(",", output) + "]";
					return outputJson;
				}
				case bool prim:
					return prim ? "true" : "false";
				case long prim:
					return prim.ToString();
				case double prim:
					return prim.ToString();
				case float prim:
					return prim.ToString();
				case int prim:
					return prim.ToString();
				case Vector2Int prim:
					return JsonUtility.ToJson(new Vector2IntEx(prim));
				case Vector3Int prim:
					return JsonUtility.ToJson(new Vector3IntEx(prim));
				case InventoryView prim:
					return JsonUtility.ToJson(new InventoryViewEx(prim));
			}

			return JsonUtility.ToJson(arg);
		}

		public static T DeserializeResult<T>(string json)
		{
			var type = typeof(T);
			var defaultInstance = default(T);

			if (typeof(Unit).IsAssignableFrom(type))
			{
				return (T)(object)PromiseBase.Unit;
			}

			// Handle ScriptableObject Deserialization (like ContententObject)
			if (typeof(ScriptableObject).IsAssignableFrom(type))
			{
				var so = ScriptableObject.CreateInstance(type);
				JsonUtility.FromJsonOverwrite(json, so);
				return (T)(object)so;
			}

			if (type == typeof(string))
			{
				if (json.StartsWith("\"") && json.EndsWith("\""))
					return (T)(object)Json.Deserialize(json);

				return (T)(object)json;
			}

			switch (defaultInstance)
			{
				case float _:
					return (T)(object)float.Parse(json);
				case long _:
					return (T)(object)long.Parse(json);
				case double _:
					return (T)(object)double.Parse(json);
				case bool _:
					return (T)(object)bool.Parse(json);
				case int _:
					return (T)(object)int.Parse(json);
				case Vector2Int _:
					return (T)(object)Vector2IntEx.DeserializeToVector2(json);
				case Vector3Int _:
					return (T)(object)Vector3IntEx.DeserializeToVector3(json);
				case InventoryView _:
					return (T)(object)InventoryViewEx.DeserializeToInventoryView(json);
			}

			if (typeof(IDictionary).IsAssignableFrom(type))
			{
				var arrayDict = (ArrayDict)Json.Deserialize(json);
				object result = default(T);
				if (typeof(Dictionary<string, string>) == type)
				{
					result = ConvertArrayDictToDictionary<string>(arrayDict);
				}
				else if (typeof(Dictionary<string, double>) == type)
				{
					result = ConvertArrayDictToDictionary<double>(arrayDict);
				}
				else if (typeof(Dictionary<string, float>) == type)
				{
					result = ConvertArrayDictToDictionary<float>(arrayDict);
				}
				else if (typeof(Dictionary<string, int>) == type)
				{
					result = ConvertArrayDictToDictionary<int>(arrayDict);
				}
				else if (typeof(Dictionary<string, bool>) == type)
				{
					result = ConvertArrayDictToDictionary<bool>(arrayDict);
				}
				else if (typeof(Dictionary<string, long>) == type)
				{
					result = ConvertArrayDictToDictionary<long>(arrayDict);
				}
				else
				{
					Debug.LogWarning($"Cannot convert json to Dictionary:\n{json}");
				}

				return (T)result;
			}

			if (json.Equals("null") || json.Length == 0)
				return defaultInstance;

			if (json.StartsWith("[") && json.EndsWith("]"))
			{
				string rawJson = json;
				json = $"{{\"items\": {json}}}";
				var wrapped = JsonUtility.FromJson<JsonUtilityWrappedList<T>>(json);

				// Handle ScriptableObject List Deserialization (like List of ContententObject)
				if (wrapped != null && wrapped.items != null)
				{
					Type arrayType = wrapped.items.GetType();

					if (arrayType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(arrayType))
					{
						Type scriptableType = arrayType.GetGenericArguments()[0];
						if (typeof(ScriptableObject).IsAssignableFrom(scriptableType))
						{
							var obj = Json.Deserialize(rawJson) as IEnumerable<object>;
							object instance = Activator.CreateInstance(arrayType);
							var list = (IList)instance;

							foreach (var element in obj)
							{
								var tmm = ScriptableObject.CreateInstance(scriptableType);
								JsonUtility.FromJsonOverwrite(Json.Serialize(element, new StringBuilder()), tmm);
								list.Add(tmm);
							}

							return (T)list;
						}
					}
				}
				return wrapped.items;
			}

			return JsonUtility.FromJson<T>(json);
		}

		public static Dictionary<string, T> ConvertArrayDictToDictionary<T>(ArrayDict arrayDict)
		{
			var dictionary = new Dictionary<string, T>(arrayDict.Count);
			if (typeof(T) == typeof(int))
			{
				foreach (var pair in arrayDict)
				{
					var intValue = (int)((long)pair.Value % Int32.MaxValue);
					dictionary.Add(pair.Key, (T)(object)intValue);
				}
			}
			else if (typeof(T) == typeof(float))
			{
				foreach (var pair in arrayDict)
				{
					var floatValue = (float)((double)pair.Value);
					dictionary.Add(pair.Key, (T)(object)floatValue);
				}
			}
			else
			{
				foreach (var pair in arrayDict)
				{
					dictionary.Add(pair.Key, (T)pair.Value);
				}
			}

			return dictionary;
		}
		private class JsonUtilityWrappedList<TList>
		{
			public TList items = default;
		}

		public static string CreateUrl(string cid, string pid, string serviceName, string endpoint)
		{
			var prefix = _prefix ?? (_prefix = MicroserviceIndividualization.GetServicePrefix(serviceName));
			var path = $"{prefix}micro_{serviceName}/{endpoint}";
			var url = $"/basic/{cid}.{pid}.{path}";
			return url;
		}

		public static async Promise<T> Request<T>(IBeamableRequester requester, string serviceName, string endpoint, string[] serializedFields)
		{
			var argArray = "[ " + string.Join(",", serializedFields) + " ]";

			T Parser(string json)
			{
				// TODO: Remove this in 0.13.0
				if (!(json?.StartsWith("{\"payload\":\"") ?? false)) return DeserializeResult<T>(json);

#pragma warning disable 618
				Debug.LogWarning($"Using legacy payload string. Redeploy the {serviceName} service without the UseLegacySerialization setting.");
#pragma warning restore 618
				var responseObject = DeserializeResult<ResponseObject>(json);
				var result = DeserializeResult<T>(responseObject.payload);
				return result;
			}

			// var b = await API.Instance;
			// var requester = b.Requester;
			var url = CreateUrl(requester.AccessToken.Cid, requester.AccessToken.Pid, serviceName, endpoint);
			var req = new RequestObject
			{
				payload = argArray
			};
			return await requester.Request<T>(Method.POST, url, req, parser: Parser);
		}


	}

}
