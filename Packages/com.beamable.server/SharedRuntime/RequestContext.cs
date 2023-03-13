using Beamable.Common;
using Beamable.Common.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Beamable.Server
{
	/// <summary>
	/// This type defines the %Microservice %RequestContext.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Server.Microservice script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class RequestContext : IUserContext
	{
		private readonly long _userId;

		/// <summary>
		/// The customer id that this request originated from
		/// </summary>
		public string Cid { get; }

		/// <summary>
		/// The realm id that this request originated from
		/// </summary>
		public string Pid { get; }

		/// <summary>
		/// The request id. Will be a positive number if the request is user-generated, and negative if the request is an internal Beamable framework message.
		/// </summary>
		public long Id { get; }

		/// <summary>
		/// The HTTP status code of the operation
		/// </summary>
		public int Status { get; }

		/// <summary>
		/// The gamertag of the user that initiated this request. Be aware that this number can be 0 if there was no authorization header on the original request.
		/// </summary>
		public long UserId
		{
			get
			{
				CheckEmptyUser();
				return _userId;
			}
		}

		/// <summary>
		/// The relative url path for the request
		/// </summary>
		public string Path { get; }

		/// <summary>
		/// The HTTP method used to initiate this request, such as "POST", or "GET"
		/// </summary>
		public string Method { get; }

		/// <summary>
		/// The raw body of this request.
		/// </summary>
		public virtual string Body { get; }

		/// <summary>
		/// Permissions associated with the caller of this request.
		/// </summary>
		public HashSet<string> Scopes { get; }

		/// <summary>
		/// HTTP headers associated with this request
		/// </summary>
		public virtual RequestHeaders Headers { get; }

		public bool HasScopes(IEnumerable<string> scopes) => HasScopes(scopes.ToArray());
		public bool HasScopes(params string[] scopes)
		{
			if (Scopes.Contains("*")) return true;
			var missingCount = scopes.Count(required => !Scopes.Contains(required));
			return missingCount == 0;
		}

		public void CheckAdmin()
		{
			if (!HasScopes("*"))
				throw new MissingScopesException(Scopes);
		}

		/// <summary>
		/// If the request is cancelled or times out, calling this method will trigger an exception.
		/// If you have a `while` loop in your client-callable, you <b>must</b> include this
		/// statement in the loop. Otherwise, if your loop never terminates, the service
		/// instance will suffer severe performance issues.
		///
		/// See <see cref="IsCancelled"/> to check if the request has been cancelled.
		/// </summary>
		public virtual void ThrowIfCancelled()
		{
			// no-op.
		}

		/// <summary>
		/// If the request is cancelled or times out, this will return true.
		/// </summary>
		public virtual bool IsCancelled { get; }

		private void CheckEmptyUser()
		{
			if (IsInvalidUser)
				throw new ArgumentException($"This is an empty UserContext. You cannot make user-specific requests with it. " +
											$"Please call Microservice.AssumeUser and use the returning RequestHandlerData to make the request you are attempting to make.");
		}

		public void RequireScopes(params string[] scopes)
		{
			if (!HasScopes(scopes))
				throw new MissingScopesException(Scopes);
		}

		public bool IsEvent => Path?.StartsWith("event/") ?? false;

		/// <summary>
		/// Informs us whether or not this request context is pointing to any valid user (userId >= 0). If it isn't, customer must call <see cref="Microservice.AssumeUser"/> in
		/// custom Microservice's code before making requests that access player-specific data.
		/// </summary>
		public bool IsInvalidUser => _userId < 0;

		public RequestContext(string cid, string pid, long id, int status, long userId, string path, string method, string body, HashSet<string> scopes = null, IDictionary<string, string> headers = null)
		{
			Cid = cid;
			Pid = pid;
			Id = id;
			_userId = userId;
			Path = path;
			Method = method;
			Status = status;
			Body = body;
			Scopes = scopes ?? new HashSet<string>();
			Scopes.RemoveWhere(string.IsNullOrEmpty);
			if (headers != null)
			{
				Headers = new RequestHeaders(headers);
			}
		}

		public RequestContext(string cid, string pid)
		{
			Cid = cid;
			Pid = pid;
			Id = -1;
			_userId = -1;
			Path = "";
			Method = "";
			Status = 0;
			Body = "";
			Scopes = new HashSet<string>();
		}

	}

	public class RequestHeaders : ReadOnlyDictionary<string, string>
	{
		public RequestHeaders(IDictionary<string, string> dictionary = null) : base(dictionary ?? new Dictionary<string, string>())
		{

		}

		/// <summary>
		/// Try to read the X-KS-BEAM-SDK-VERSION header from the request.
		/// The method will return false if the header was not present on the request.
		/// If the method returns true, then the <see cref="beamableVersion"/> variable will be set to the sdk version that initiated this request
		/// </summary>
		/// <param name="beamableVersion">The variable that will be populated with the header information</param>
		/// <returns>true if the header was found</returns>
		public bool TryGetBeamableSdkVersion(out string beamableVersion)
		{
			beamableVersion = null;
			return TryGetValue(Constants.Requester.HEADER_BEAMABLE_VERSION, out beamableVersion);
		}

		/// <summary>
		/// Try to read the X-KS-GAME-VERSION header from the request.
		/// The method will return false if the header was not present on the request.
		/// If the method returns true, then the <see cref="clientVersion"/> variable will be set to the game version that initiated this request.
		/// The game version is usually set in Unity by using the Application.version field
		/// </summary>
		/// <param name="clientVersion">The variable that will be populated with the header information</param>
		/// <returns>true if the header was found</returns>
		public bool TryGetClientGameVersion(out string clientVersion)
		{
			clientVersion = null;
			return TryGetValue(Constants.Requester.HEADER_APPLICATION_VERSION, out clientVersion);
		}

		/// <summary>
		/// Try to read the X-KS-USER-AGENT header from the request.
		/// The method will return false if the header was not present on the request.
		/// If the method returns true, then the <see cref="clientType"/> variable will be set to the type of client that initiated the request.
		/// Usually, this will be "Unity", or "Portal", or null.
		/// </summary>
		/// <param name="clientType">The variable that will be populated with the header information</param>
		/// <returns>true if the header was found</returns>
		public bool TryGetClientType(out string clientType)
		{
			clientType = null;
			return TryGetValue(Constants.Requester.HEADER_ENGINE_TYPE, out clientType);
		}

		/// <summary>
		/// Try to read the X-KS-USER-AGENT-VERSION header from the request.
		/// The method will return false if the header was not present on the request.
		/// If the method returns true, then the <see cref="clientEngineVersion"/> variable will be set to the version of the client type.
		/// This version number is specific to the value returned from the <see cref="TryGetClientType"/> method.
		/// </summary>
		/// <param name="clientEngineVersion">The variable that will be populated with the header information</param>
		/// <returns>true if the header was found</returns>
		public bool TryGetClientEngineVersion(out string clientEngineVersion)
		{
			clientEngineVersion = null;
			return TryGetValue(Constants.Requester.HEADER_UNITY_VERSION, out clientEngineVersion);
		}
	}
}
