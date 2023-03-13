using Beamable.Serialization.SmallerJSON;
using System;
using System.Collections.Generic;

namespace Beamable.Common.Api
{
	/// <summary>
	/// This type defines getting fresh data from an %Api data source (e.g. %Service).
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Api script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public interface ISupportsGet<TData>
	{
		/// <summary>
		/// Manually fetch the available data. If the server hasn't delivered a new update, this method will not return the absolute latest data unless you pass forceRefresh as true.
		/// </summary>
		/// <param name="scope">A scope to narrow down the data to receive.</param>
		/// <returns>A <see cref="Promise"/> that contains the latest data</returns>
		Promise<TData> GetCurrent(string scope = "");
	}

	/// <summary>
	/// This type defines getting fresh data from an %Api data source (e.g. %Service).
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Api script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public interface ISupportGetLatest<out TData>
	{
		/// <summary>
		/// Manually read the currently cached data. This will not trigger any network request.
		/// </summary>
		/// <param name="scope">The scope to look up the data for.</param>
		/// <returns>The currently cached data, or null if no data exists</returns>
		TData GetLatest(string scope = "");
	}

	/// <summary>
	/// This type defines getting %Api data source.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Api script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class BeamableGetApiResource<ScopedRsp>
	{
		private readonly bool _useCache;

		public BeamableGetApiResource(bool useCache = false)
		{
			_useCache = useCache;
		}
		public Promise<ScopedRsp> RequestData(IBeamableRequester requester, IUserContext ctx, string serviceName, string scope)
		{
			return RequestData(requester, CreateRefreshUrl(ctx, serviceName, scope));
		}

		public virtual Promise<ScopedRsp> RequestData(IBeamableRequester requester, string url)
		{
			return requester.Request<ScopedRsp>(Method.GET, url, useCache: _useCache);
		}

		public virtual string CreateRefreshUrl(IUserContext ctx, string serviceName, string scope)
		{
			var queryArgs = "";
			if (!string.IsNullOrEmpty(scope))
			{
				queryArgs = $"?scope={scope}";
			}

			return $"/object/{serviceName}/{ctx.UserId}{queryArgs}";
		}
	}

	/// <summary>
	/// Helper class that can be used to make continuous <see cref="Method.POST"/> requests in order to keep some cached data somewhere.
	/// </summary>
	/// <typeparam name="ScopedRsp">The response type of the Post Request.</typeparam>
	public class BeamableGetApiResourceViaPost<ScopedRsp> : BeamableGetApiResource<ScopedRsp>
	{
		private readonly bool _useCache;
		private readonly Func<string, ScopedRsp> _offlineResponseGenerator;

		/// <summary>
		/// Mapping of each requested scope to the body generated for it.
		/// </summary>
		private Dictionary<string, ArrayDict> ScopesToBodyMap;

		/// <summary>
		/// Last value that passed through <see cref="CreateRefreshUrl"/>.
		/// TODO: Revise this approach when improving/changing PlatformSubscribable code in the Unity Runtime.
		/// </summary>
		private ArrayDict OutgoingBody;

		public BeamableGetApiResourceViaPost(bool useCache = false, Func<string, ScopedRsp> offlineResponseGenerator = null)
		{
			_useCache = useCache;
			_offlineResponseGenerator = offlineResponseGenerator;
			ScopesToBodyMap = new Dictionary<string, ArrayDict>();
		}

		public override async Promise<ScopedRsp> RequestData(IBeamableRequester requester, string url)
		{
			try
			{
				return await requester.Request<ScopedRsp>(Method.POST, url, OutgoingBody, useCache: _useCache);
			}
			catch (NoConnectivityException)
			{
				if (_offlineResponseGenerator != null)
				{
					return _offlineResponseGenerator.Invoke(url);
				}

				throw;
			}
		}

		/// <summary>
		/// Builds a <see cref="Method.POST"/> request's body for the given scope and caches it in <see cref="ScopesToBodyMap"/>.
		/// </summary>
		/// <param name="scope">A ","-separated string with all item types or ids that we want to get OR an empty string. Null is not supported.</param>
		public override string CreateRefreshUrl(IUserContext ctx, string serviceName, string scope)
		{
			if (!ScopesToBodyMap.TryGetValue(scope, out var body))
			{
				body = new ArrayDict()
			   {
				   { "scopes", scope.Split(',')}
			   };
				ScopesToBodyMap.Add(scope, body);
			}

			OutgoingBody = body;

			return $"/object/{serviceName}/{ctx.UserId}";
		}

	}
}
