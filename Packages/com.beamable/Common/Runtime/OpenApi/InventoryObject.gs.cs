
namespace Beamable.Api.Autogenerated.Inventory
{
	using Beamable.Api.Autogenerated.Models;
	using Beamable.Common;
	using Beamable.Common.Content;
	using IBeamableRequester = Beamable.Common.Api.IBeamableRequester;
	using Method = Beamable.Common.Api.Method;

	public partial interface IInventoryApi
	{
		/// <param name="objectId">Gamertag of the player.Underlying objectId type is integer in format int64.</param>
		/// <param name="gsReq">The <see cref="InventoryUpdateRequest"/> instance to use for the request</param>
		/// <param name="includeAuthHeader">By default, every request will include an authorization header so that the request acts on behalf of the current user. When the includeAuthHeader argument is false, the request will not include the authorization header for the current user.</param>
		/// <returns>A promise containing the <see cref="PreviewVipBonusResponse"/></returns>
		Promise<PreviewVipBonusResponse> ObjectPutPreview(long objectId, InventoryUpdateRequest gsReq, [System.Runtime.InteropServices.DefaultParameterValueAttribute(true)][System.Runtime.InteropServices.OptionalAttribute()] bool includeAuthHeader);
		/// <param name="objectId">Gamertag of the player.Underlying objectId type is integer in format int64.</param>
		/// <param name="includeAuthHeader">By default, every request will include an authorization header so that the request acts on behalf of the current user. When the includeAuthHeader argument is false, the request will not include the authorization header for the current user.</param>
		/// <returns>A promise containing the <see cref="MultipliersGetResponse"/></returns>
		Promise<MultipliersGetResponse> ObjectGetMultipliers(long objectId, [System.Runtime.InteropServices.DefaultParameterValueAttribute(true)][System.Runtime.InteropServices.OptionalAttribute()] bool includeAuthHeader);
		/// <param name="objectId">Gamertag of the player.Underlying objectId type is integer in format int64.</param>
		/// <param name="gsReq">The <see cref="EndTransactionRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="CommonResponse"/></returns>
		Promise<CommonResponse> ObjectDeleteTransaction(long objectId, EndTransactionRequest gsReq);
		/// <param name="objectId">Gamertag of the player.Underlying objectId type is integer in format int64.</param>
		/// <param name="scope"></param>
		/// <param name="includeAuthHeader">By default, every request will include an authorization header so that the request acts on behalf of the current user. When the includeAuthHeader argument is false, the request will not include the authorization header for the current user.</param>
		/// <returns>A promise containing the <see cref="InventoryView"/></returns>
		Promise<InventoryView> ObjectGet(long objectId, [System.Runtime.InteropServices.DefaultParameterValueAttribute(null)][System.Runtime.InteropServices.OptionalAttribute()] Beamable.Common.Content.Optional<string> scope, [System.Runtime.InteropServices.DefaultParameterValueAttribute(true)][System.Runtime.InteropServices.OptionalAttribute()] bool includeAuthHeader);
		/// <param name="objectId">Gamertag of the player.Underlying objectId type is integer in format int64.</param>
		/// <param name="gsReq">The <see cref="InventoryQueryRequest"/> instance to use for the request</param>
		/// <param name="includeAuthHeader">By default, every request will include an authorization header so that the request acts on behalf of the current user. When the includeAuthHeader argument is false, the request will not include the authorization header for the current user.</param>
		/// <returns>A promise containing the <see cref="InventoryView"/></returns>
		Promise<InventoryView> ObjectPost(long objectId, InventoryQueryRequest gsReq, [System.Runtime.InteropServices.DefaultParameterValueAttribute(true)][System.Runtime.InteropServices.OptionalAttribute()] bool includeAuthHeader);
		/// <param name="objectId">Gamertag of the player.Underlying objectId type is integer in format int64.</param>
		/// <param name="gsReq">The <see cref="InventoryUpdateRequest"/> instance to use for the request</param>
		/// <param name="includeAuthHeader">By default, every request will include an authorization header so that the request acts on behalf of the current user. When the includeAuthHeader argument is false, the request will not include the authorization header for the current user.</param>
		/// <returns>A promise containing the <see cref="CommonResponse"/></returns>
		Promise<CommonResponse> ObjectPut(long objectId, InventoryUpdateRequest gsReq, [System.Runtime.InteropServices.DefaultParameterValueAttribute(true)][System.Runtime.InteropServices.OptionalAttribute()] bool includeAuthHeader);
		/// <param name="objectId">Gamertag of the player.Underlying objectId type is integer in format int64.</param>
		/// <param name="gsReq">The <see cref="TransferRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="CommonResponse"/></returns>
		Promise<CommonResponse> ObjectPutTransfer(long objectId, TransferRequest gsReq);
	}
	public partial class InventoryApi : IInventoryApi
	{
		/// <param name="objectId">Gamertag of the player.Underlying objectId type is integer in format int64.</param>
		/// <param name="gsReq">The <see cref="InventoryUpdateRequest"/> instance to use for the request</param>
		/// <param name="includeAuthHeader">By default, every request will include an authorization header so that the request acts on behalf of the current user. When the includeAuthHeader argument is false, the request will not include the authorization header for the current user.</param>
		/// <returns>A promise containing the <see cref="PreviewVipBonusResponse"/></returns>
		public virtual Promise<PreviewVipBonusResponse> ObjectPutPreview(long objectId, InventoryUpdateRequest gsReq, [System.Runtime.InteropServices.DefaultParameterValueAttribute(true)][System.Runtime.InteropServices.OptionalAttribute()] bool includeAuthHeader)
		{
			string gsUrl = "/object/inventory/{objectId}/preview";
			gsUrl = gsUrl.Replace("{objectId}", objectId.ToString());
			// make the request and return the result
			return _requester.Request<PreviewVipBonusResponse>(Method.PUT, gsUrl, Beamable.Serialization.JsonSerializable.ToJson(gsReq), includeAuthHeader, Beamable.Serialization.JsonSerializable.FromJson<PreviewVipBonusResponse>);
		}
		/// <param name="objectId">Gamertag of the player.Underlying objectId type is integer in format int64.</param>
		/// <param name="includeAuthHeader">By default, every request will include an authorization header so that the request acts on behalf of the current user. When the includeAuthHeader argument is false, the request will not include the authorization header for the current user.</param>
		/// <returns>A promise containing the <see cref="MultipliersGetResponse"/></returns>
		public virtual Promise<MultipliersGetResponse> ObjectGetMultipliers(long objectId, [System.Runtime.InteropServices.DefaultParameterValueAttribute(true)][System.Runtime.InteropServices.OptionalAttribute()] bool includeAuthHeader)
		{
			string gsUrl = "/object/inventory/{objectId}/multipliers";
			gsUrl = gsUrl.Replace("{objectId}", objectId.ToString());
			// make the request and return the result
			return _requester.Request<MultipliersGetResponse>(Method.GET, gsUrl, default(object), includeAuthHeader, Beamable.Serialization.JsonSerializable.FromJson<MultipliersGetResponse>);
		}
		/// <param name="objectId">Gamertag of the player.Underlying objectId type is integer in format int64.</param>
		/// <param name="gsReq">The <see cref="EndTransactionRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="CommonResponse"/></returns>
		public virtual Promise<CommonResponse> ObjectDeleteTransaction(long objectId, EndTransactionRequest gsReq)
		{
			string gsUrl = "/object/inventory/{objectId}/transaction";
			gsUrl = gsUrl.Replace("{objectId}", objectId.ToString());
			// make the request and return the result
			return _requester.Request<CommonResponse>(Method.DELETE, gsUrl, Beamable.Serialization.JsonSerializable.ToJson(gsReq), true, Beamable.Serialization.JsonSerializable.FromJson<CommonResponse>);
		}
		/// <param name="objectId">Gamertag of the player.Underlying objectId type is integer in format int64.</param>
		/// <param name="scope"></param>
		/// <param name="includeAuthHeader">By default, every request will include an authorization header so that the request acts on behalf of the current user. When the includeAuthHeader argument is false, the request will not include the authorization header for the current user.</param>
		/// <returns>A promise containing the <see cref="InventoryView"/></returns>
		public virtual Promise<InventoryView> ObjectGet(long objectId, [System.Runtime.InteropServices.DefaultParameterValueAttribute(null)][System.Runtime.InteropServices.OptionalAttribute()] Beamable.Common.Content.Optional<string> scope, [System.Runtime.InteropServices.DefaultParameterValueAttribute(true)][System.Runtime.InteropServices.OptionalAttribute()] bool includeAuthHeader)
		{
			string gsUrl = "/object/inventory/{objectId}/";
			gsUrl = gsUrl.Replace("{objectId}", objectId.ToString());
			string gsQuery = "?";
			System.Collections.Generic.List<string> gsQueries = new System.Collections.Generic.List<string>();
			if (((scope != default(OptionalString))
						&& scope.HasValue))
			{
				gsQueries.Add(string.Concat("scope=", scope.Value.ToString()));
			}
			gsQuery = string.Concat(gsQuery, string.Join("&", gsQueries));
			gsUrl = string.Concat(gsUrl, gsQuery);
			// make the request and return the result
			return _requester.Request<InventoryView>(Method.GET, gsUrl, default(object), includeAuthHeader, Beamable.Serialization.JsonSerializable.FromJson<InventoryView>);
		}
		/// <param name="objectId">Gamertag of the player.Underlying objectId type is integer in format int64.</param>
		/// <param name="gsReq">The <see cref="InventoryQueryRequest"/> instance to use for the request</param>
		/// <param name="includeAuthHeader">By default, every request will include an authorization header so that the request acts on behalf of the current user. When the includeAuthHeader argument is false, the request will not include the authorization header for the current user.</param>
		/// <returns>A promise containing the <see cref="InventoryView"/></returns>
		public virtual Promise<InventoryView> ObjectPost(long objectId, InventoryQueryRequest gsReq, [System.Runtime.InteropServices.DefaultParameterValueAttribute(true)][System.Runtime.InteropServices.OptionalAttribute()] bool includeAuthHeader)
		{
			string gsUrl = "/object/inventory/{objectId}/";
			gsUrl = gsUrl.Replace("{objectId}", objectId.ToString());
			// make the request and return the result
			return _requester.Request<InventoryView>(Method.POST, gsUrl, Beamable.Serialization.JsonSerializable.ToJson(gsReq), includeAuthHeader, Beamable.Serialization.JsonSerializable.FromJson<InventoryView>);
		}
		/// <param name="objectId">Gamertag of the player.Underlying objectId type is integer in format int64.</param>
		/// <param name="gsReq">The <see cref="InventoryUpdateRequest"/> instance to use for the request</param>
		/// <param name="includeAuthHeader">By default, every request will include an authorization header so that the request acts on behalf of the current user. When the includeAuthHeader argument is false, the request will not include the authorization header for the current user.</param>
		/// <returns>A promise containing the <see cref="CommonResponse"/></returns>
		public virtual Promise<CommonResponse> ObjectPut(long objectId, InventoryUpdateRequest gsReq, [System.Runtime.InteropServices.DefaultParameterValueAttribute(true)][System.Runtime.InteropServices.OptionalAttribute()] bool includeAuthHeader)
		{
			string gsUrl = "/object/inventory/{objectId}/";
			gsUrl = gsUrl.Replace("{objectId}", objectId.ToString());
			// make the request and return the result
			return _requester.Request<CommonResponse>(Method.PUT, gsUrl, Beamable.Serialization.JsonSerializable.ToJson(gsReq), includeAuthHeader, Beamable.Serialization.JsonSerializable.FromJson<CommonResponse>);
		}
		/// <param name="objectId">Gamertag of the player.Underlying objectId type is integer in format int64.</param>
		/// <param name="gsReq">The <see cref="TransferRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="CommonResponse"/></returns>
		public virtual Promise<CommonResponse> ObjectPutTransfer(long objectId, TransferRequest gsReq)
		{
			string gsUrl = "/object/inventory/{objectId}/transfer";
			gsUrl = gsUrl.Replace("{objectId}", objectId.ToString());
			// make the request and return the result
			return _requester.Request<CommonResponse>(Method.PUT, gsUrl, Beamable.Serialization.JsonSerializable.ToJson(gsReq), true, Beamable.Serialization.JsonSerializable.FromJson<CommonResponse>);
		}
	}
}
