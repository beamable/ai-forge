using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Auth;
using Beamable.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;
using AuthService = Beamable.Api.Auth.AuthService;

namespace Beamable.Platform.Tests
{
	public class MockPlatformRequester : IBeamableRequester
	{
		public AuthService AuthApi { get; set; }
		public IAccessToken AccessToken { get; set; }

		public string Cid => AccessToken.Cid;
		public string Pid => AccessToken.Pid;

		public delegate Promise<T> RequestJsonFunction<T>(Method method, string uri, JsonSerializable.ISerializable body,
		   bool includeAuthHeader = true);

		public delegate Promise<T> RequestFunction<T>(Method method, string uri, object body = null,
		   bool includeAuthHeader = true, Func<string, T> parser = null);

		public RequestJsonFunction<object> RequestJsonResult;
		public RequestFunction<object> RequestResult;

		private Dictionary<Type, RequestJsonFunction<object>> _jsonFunctions = new Dictionary<Type, RequestJsonFunction<object>>();

		public void Reset()
		{
			_jsonFunctions.Clear();
		}

		public void RegisterMockRequestJson<T>(RequestJsonFunction<T> function)
		{
			_jsonFunctions.Add(typeof(T), (method, uri, body, header) => function(method, uri, body, header).Map(o => (object)o));
		}

		public Promise<T> RequestJson<T>(Method method, string uri, JsonSerializable.ISerializable body, bool includeAuthHeader = true)
		{
			if (_jsonFunctions.ContainsKey(typeof(T)))
			{
				var function = _jsonFunctions[typeof(T)];
				return function(method, uri, body, includeAuthHeader).Map(o => (T)o);
			}

			throw new NotImplementedException();

		}

		public Promise<T> Request<T>(Method method, string uri, object body = null, bool includeAuthHeader = true, Func<string, T> parser = null, bool useCache = false)
		{
			return RequestResult(method, uri, body, includeAuthHeader, parser != null ? new Func<string, object>(str => parser(str)) : null).Map(res => (T)res);
		}

		public Promise<T> RequestForm<T>(string uri, WWWForm form, bool includeAuthHeader = true)
		{
			throw new NotImplementedException();
		}

		public Promise<T> RequestForm<T>(string uri, WWWForm form, Method method, bool includeAuthHeader = true)
		{
			throw new NotImplementedException();
		}

		public IBeamableRequester WithAccessToken(TokenResponse token)
		{
			throw new NotImplementedException();
		}

		public string EscapeURL(string url)
		{
			throw new NotImplementedException();
		}
	}
}
