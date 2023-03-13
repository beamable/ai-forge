using Beamable.Api;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Auth;
using Beamable.Serialization;
using Beamable.Serialization.SmallerJSON;
using Core.Platform.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using AccessToken = Beamable.Api.AccessToken;

namespace Beamable.Platform.Tests
{
	public class MockAccessToken : IAccessToken
	{
		public string Token
		{
			get;
			set;
		} = "test";
		public string RefreshToken
		{
			get;
			set;
		} = "test";
		public DateTime ExpiresAt
		{
			get;
			set;
		} = DateTime.UtcNow + TimeSpan.FromMinutes(2);

		public string Cid
		{
			get;
			set;
		} = "test";

		public string Pid
		{
			get;
			set;
		} = "test";
	}


	public abstract class MockPlatformRouteBase
	{
		public Method Method;
		public string Uri;
		public bool IncludeAuthHeader = true;
		public string Token;
		protected object _response;
		protected RequesterException _errorResponse;

		public int CallCount { get; protected set; }
		public bool Called => CallCount > 0;

		public abstract Promise<T> Invoke<T>(object body, bool includeAuth, IAccessToken token);
		public abstract string Invoke(object body, bool includeAuth, IAccessToken token);

		public abstract bool MatchesRequest<T>(Method method, string uri, string token, object body, bool includeAuthHeader);
	}

	public class MockPlatformRoute<T> : MockPlatformRouteBase
	{
		private List<BodyMatcher> _matchers = new List<BodyMatcher>();

		private List<UriMatcher> _uriMatchers = new List<UriMatcher>();

		private delegate bool BodyMatcher(object body);

		private delegate bool UriMatcher(string uri);

		public MockPlatformRoute<T> WithResponse(T response)
		{
			_response = response;
			return this;
		}

		public MockPlatformRoute<T> WithResponse(RequesterException err)
		{
			_errorResponse = err;
			return this;
		}

		public MockPlatformRoute<T> WithRawResponse(string response)
		{
			_response = response;
			return this;
		}

		public MockPlatformRoute<T> WithNoAuthHeader(bool use = false)
		{
			IncludeAuthHeader = use;
			return this;
		}

		public MockPlatformRoute<T> WithToken(string token)
		{
			Token = token;
			return this;
		}

		public MockPlatformRoute<T> WithBodyMatch<TRequest>(Func<TRequest, bool> matcher)
		{
			_matchers.Add(body =>
			{
				if (body is TRequest req)
				{
					return matcher(req);
				}

				return false;
			});
			return this;
		}

		public MockPlatformRoute<T> WithoutJsonField(string fieldName)
		{
			_matchers.Add(body =>
			{
				var json = body is string bodyJson ? bodyJson : JsonUtility.ToJson(body);
				var dict = Beamable.Serialization.SmallerJSON.Json.Deserialize(json) as ArrayDict;

				try
				{
					var _ = dict.JsonPath(fieldName);
					return _ == null;
				}
				catch (Exception)
				{
					return true;
				}
			});
			return this;
		}

		public MockPlatformRoute<T> WithJsonFieldMatch(string jsonPath, Func<object, bool> matcher)
		{
			_matchers.Add(body =>
			{
				var json = body is string bodyJson ? bodyJson : JsonUtility.ToJson(body);
				var dict = Beamable.Serialization.SmallerJSON.Json.Deserialize(json) as ArrayDict;

				try
				{
					var actual = dict.JsonPath(jsonPath);
					return matcher(actual);
				}
				catch (Exception)
				{
					return false;
				}
			});
			return this;
		}

		public MockPlatformRoute<T> WithJsonFieldMatch(string jsonPath, object expectedValue)
		{
			_matchers.Add(body =>
			{
				var json = body is string bodyJson ? bodyJson : JsonUtility.ToJson(body);
				var dict = Beamable.Serialization.SmallerJSON.Json.Deserialize(json) as ArrayDict;

				try
				{
					var actual = dict.JsonPath(jsonPath);
					return expectedValue.Equals(actual);
				}
				catch (Exception)
				{
					return false;
				}
			});
			return this;
		}

		public override Promise<T1> Invoke<T1>(object body, bool includeAuth, IAccessToken token)
		{
			CallCount++;
			if (_errorResponse != null)
			{
				throw _errorResponse;
			}
			return Promise<T1>.Successful((T1)_response);
		}

		public override string Invoke(object body, bool includeAuth, IAccessToken token)
		{
			CallCount++;
			if (_errorResponse != null)
			{
				throw _errorResponse;
			}
			return _response.ToString();
		}

		public override bool MatchesRequest<T1>(Method method, string uri, string token, object body, bool includeAuthHeader)
		{
			var tokenMatch = string.Equals(Token, token);
			var authMatch = includeAuthHeader == IncludeAuthHeader;
			if (authMatch && !includeAuthHeader)
			{
				tokenMatch = true; // it doesn't matter what the token is, because we aren't going to send it anyway.
			}
			var uriMatch = Uri == null || Uri.Equals(uri);

			var typeMatch = typeof(T1) == typeof(T);
			var methodMatch = Method.Equals(method);

			var simpleChecks = tokenMatch && authMatch && uriMatch && typeMatch && methodMatch;

			if (!simpleChecks)
			{
				return false;
			}

			var uriMatchers = _uriMatchers.Count == 0 || _uriMatchers.All(matcher => matcher(uri));
			if (!uriMatchers)
			{
				return false;
			}

			for (var i = 0; i < _matchers.Count; i++)
			{
				var matcher = _matchers[i];
				var passesBodyMatcher = matcher(body);
				if (!passesBodyMatcher)
				{
					return false;
				}
			}

			return true;
		}

		public MockPlatformRoute<T> WithURIPrefix(string uriPrefix)
		{
			_uriMatchers.Add(uri => uri.StartsWith(uriPrefix));
			return this;
		}

		public MockPlatformRoute<T> WithURIMatcher(Func<string, bool> matcher)
		{
			_uriMatchers.Add(uri => matcher(uri));
			return this;
		}
	}

	public class MockPlatformAPI : IPlatformRequester, IBeamableApiRequester
	{
		//private Dictionary<string, MockPlatformRouteBase> _routes = new Dictionary<string, MockPlatformRouteBase>();

		private List<MockPlatformRouteBase> _routes = new List<MockPlatformRouteBase>();

		public AccessToken Token
		{
			get;
			set;
		} //= new AccessToken(new AccessTokenStorage("test"), "testcid", "testpid", "testtok", "testref", 1000000);

		public Promise RefreshToken()
		{
			return Promise.Success;
		}

		public string TimeOverride
		{
			get;
			set;
		}

		public string Cid
		{
			get;
			set;
		}

		public string Pid
		{
			get;
			set;
		}

		public string Language
		{
			get;
			set;
		}

		IAuthApi IPlatformRequester.AuthService
		{
			set
			{
				AuthService = value;
			}
		}

		public void DeleteToken()
		{
			Token = null;
		}

		public IAuthApi AuthService { get; set; }

		public IAccessToken AccessToken => Token;

		public bool AllMocksCalled => _routes.All(mock => mock.Called);

		public MockPlatformAPI()
		{
			// brand new!
		}

		public MockPlatformAPI(MockPlatformAPI copy)
		{
			_routes = copy._routes; // shallow copy.
		}

		public Promise<T> RequestJson<T>(Method method, string uri, JsonSerializable.ISerializable body, bool includeAuthHeader = true)
		{
			throw new NotImplementedException();
		}

		public MockPlatformRoute<T> MockRequest<T>(Method method, string uri = null)
		{
			var route = new MockPlatformRoute<T>()
			{
				Method = method,
				Uri = uri,
				Token = AccessToken?.Token
			};

			_routes.Add(route);

			return route;
		}

		public Promise<T> Request<T>(Method method, string uri, object body = null, bool includeAuthHeader = true, Func<string, T> parser = null, bool useCache = false)
		{
			// XXX this won't support multiple calls to the same route with different bodies.
			var route = _routes.FirstOrDefault(r => r.MatchesRequest<T>(method, uri, AccessToken?.Token, body, includeAuthHeader));

			if (route != null)
			{
				try
				{
					if (parser == null) return route.Invoke<T>(body, includeAuthHeader, AccessToken);
					var output = route.Invoke(body, includeAuthHeader, AccessToken);
					return Promise<T>.Successful(parser(output));
				}
				catch (RequesterException err)
				{
					return Promise<T>.Failed(err);
				}
			}
			else
			{
				throw new Exception($"No route mock available for call. method=[{method}] uri=[{uri}] includeAuth=[{includeAuthHeader}] body=[{JsonUtility.ToJson(body)}] type=[{typeof(T)}]");
			}
		}

		public Promise<T> RequestForm<T>(string uri, WWWForm form, bool includeAuthHeader = true)
		{
			return Request<T>(Method.POST, uri, form, includeAuthHeader);
		}

		public Promise<T> RequestForm<T>(string uri, WWWForm form, Method method, bool includeAuthHeader = true)
		{
			throw new NotImplementedException();
		}

		public IBeamableRequester WithAccessToken(TokenResponse token)
		{
			var clone = new MockPlatformAPI(this);
			clone.Token = new AccessToken(null, null, null, token.access_token, token.refresh_token, token.expires_in);
			return clone;
		}

		public string EscapeURL(string url)
		{
			return UnityWebRequest.EscapeURL(url);
		}

		public Promise<T> BeamableRequest<T>(SDKRequesterOptions<T> req)
		{
			throw new NotImplementedException();
		}
	}
}
