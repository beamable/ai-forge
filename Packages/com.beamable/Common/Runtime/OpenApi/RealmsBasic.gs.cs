
namespace Beamable.Api.Autogenerated.Realms
{
	using Beamable.Api.Autogenerated.Models;
	using Beamable.Common;
	using Beamable.Common.Content;
	using IBeamableRequester = Beamable.Common.Api.IBeamableRequester;
	using Method = Beamable.Common.Api.Method;

	public partial interface IRealmsApi
	{
		/// <param name="gsReq">The <see cref="CreateProjectRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="CommonResponse"/></returns>
		Promise<CommonResponse> PostProjectBeamable(CreateProjectRequest gsReq);
		/// <param name="alias"></param>
		/// <param name="includeAuthHeader">By default, every request will include an authorization header so that the request acts on behalf of the current user. When the includeAuthHeader argument is false, the request will not include the authorization header for the current user.</param>
		/// <returns>A promise containing the <see cref="AliasAvailableResponse"/></returns>
		Promise<AliasAvailableResponse> GetCustomerAliasAvailable(string alias, [System.Runtime.InteropServices.DefaultParameterValueAttribute(true)][System.Runtime.InteropServices.OptionalAttribute()] bool includeAuthHeader);
		/// <param name="includeAuthHeader">By default, every request will include an authorization header so that the request acts on behalf of the current user. When the includeAuthHeader argument is false, the request will not include the authorization header for the current user.</param>
		/// <returns>A promise containing the <see cref="ProjectView"/></returns>
		Promise<ProjectView> GetProject([System.Runtime.InteropServices.DefaultParameterValueAttribute(true)][System.Runtime.InteropServices.OptionalAttribute()] bool includeAuthHeader);
		/// <param name="gsReq">The <see cref="CreateProjectRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="CommonResponse"/></returns>
		Promise<CommonResponse> PostProject(CreateProjectRequest gsReq);
		/// <param name="gsReq">The <see cref="UnarchiveProjectRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="CommonResponse"/></returns>
		Promise<CommonResponse> PutProject(UnarchiveProjectRequest gsReq);
		/// <param name="gsReq">The <see cref="ArchiveProjectRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="CommonResponse"/></returns>
		Promise<CommonResponse> DeleteProject(ArchiveProjectRequest gsReq);
		/// <returns>A promise containing the <see cref="GetGameResponse"/></returns>
		Promise<GetGameResponse> GetGames();
		/// <returns>A promise containing the <see cref="RealmConfigResponse"/></returns>
		Promise<RealmConfigResponse> GetConfig();
		/// <param name="gsReq">The <see cref="RealmConfigSaveRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="CommonResponse"/></returns>
		Promise<CommonResponse> PutConfig(RealmConfigSaveRequest gsReq);
		/// <param name="gsReq">The <see cref="RenameProjectRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="CommonResponse"/></returns>
		Promise<CommonResponse> PutProjectRename(RenameProjectRequest gsReq);
		/// <returns>A promise containing the <see cref="ServicePlansResponse"/></returns>
		Promise<ServicePlansResponse> GetPlans();
		/// <param name="gsReq">The <see cref="CreatePlanRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="CommonResponse"/></returns>
		Promise<CommonResponse> PostPlans(CreatePlanRequest gsReq);
		/// <param name="includeAuthHeader">By default, every request will include an authorization header so that the request acts on behalf of the current user. When the includeAuthHeader argument is false, the request will not include the authorization header for the current user.</param>
		/// <returns>A promise containing the <see cref="RealmConfiguration"/></returns>
		Promise<RealmConfiguration> GetClientDefaults([System.Runtime.InteropServices.DefaultParameterValueAttribute(true)][System.Runtime.InteropServices.OptionalAttribute()] bool includeAuthHeader);
		/// <returns>A promise containing the <see cref="CustomerViewResponse"/></returns>
		Promise<CustomerViewResponse> GetCustomer();
		/// <param name="gsReq">The <see cref="NewCustomerRequest"/> instance to use for the request</param>
		/// <param name="includeAuthHeader">By default, every request will include an authorization header so that the request acts on behalf of the current user. When the includeAuthHeader argument is false, the request will not include the authorization header for the current user.</param>
		/// <returns>A promise containing the <see cref="NewCustomerResponse"/></returns>
		Promise<NewCustomerResponse> PostCustomer(NewCustomerRequest gsReq, [System.Runtime.InteropServices.DefaultParameterValueAttribute(true)][System.Runtime.InteropServices.OptionalAttribute()] bool includeAuthHeader);
		/// <returns>A promise containing the <see cref="LaunchMessageListResponse"/></returns>
		Promise<LaunchMessageListResponse> GetLaunchMessage();
		/// <param name="gsReq">The <see cref="CreateLaunchMessageRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="CommonResponse"/></returns>
		Promise<CommonResponse> PostLaunchMessage(CreateLaunchMessageRequest gsReq);
		/// <param name="gsReq">The <see cref="RemoveLaunchMessageRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="CommonResponse"/></returns>
		Promise<CommonResponse> DeleteLaunchMessage(RemoveLaunchMessageRequest gsReq);
		/// <param name="includeAuthHeader">By default, every request will include an authorization header so that the request acts on behalf of the current user. When the includeAuthHeader argument is false, the request will not include the authorization header for the current user.</param>
		/// <returns>A promise containing the <see cref="EmptyResponse"/></returns>
		Promise<EmptyResponse> GetIsCustomer([System.Runtime.InteropServices.DefaultParameterValueAttribute(true)][System.Runtime.InteropServices.OptionalAttribute()] bool includeAuthHeader);
		/// <returns>A promise containing the <see cref="CustomerResponse"/></returns>
		Promise<CustomerResponse> GetAdminCustomer();
		/// <param name="rootPID"></param>
		/// <returns>A promise containing the <see cref="GetGameResponse"/></returns>
		Promise<GetGameResponse> GetGame(string rootPID);
		/// <param name="gsReq">The <see cref="NewGameRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="CommonResponse"/></returns>
		Promise<CommonResponse> PostGame(NewGameRequest gsReq);
		/// <param name="gsReq">The <see cref="UpdateGameHierarchyRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="CommonResponse"/></returns>
		Promise<CommonResponse> PutGame(UpdateGameHierarchyRequest gsReq);
		/// <param name="contentManifestIds"></param>
		/// <param name="promotions"></param>
		/// <param name="sourcePid"></param>
		/// <returns>A promise containing the <see cref="PromoteRealmResponseOld"/></returns>
		Promise<PromoteRealmResponseOld> GetProjectPromote(string sourcePid, [System.Runtime.InteropServices.DefaultParameterValueAttribute(null)][System.Runtime.InteropServices.OptionalAttribute()] Beamable.Common.Content.Optional<string[]> contentManifestIds, [System.Runtime.InteropServices.DefaultParameterValueAttribute(null)][System.Runtime.InteropServices.OptionalAttribute()] Beamable.Common.Content.Optional<string[]> promotions);
		/// <param name="gsReq">The <see cref="PromoteRealmRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="PromoteRealmResponseOld"/></returns>
		Promise<PromoteRealmResponseOld> PostProjectPromote(PromoteRealmRequest gsReq);
		/// <param name="includeAuthHeader">By default, every request will include an authorization header so that the request acts on behalf of the current user. When the includeAuthHeader argument is false, the request will not include the authorization header for the current user.</param>
		/// <returns>A promise containing the <see cref="CustomersResponse"/></returns>
		Promise<CustomersResponse> GetCustomers([System.Runtime.InteropServices.DefaultParameterValueAttribute(true)][System.Runtime.InteropServices.OptionalAttribute()] bool includeAuthHeader);
		/// <param name="contentManifestIds"></param>
		/// <param name="promotions"></param>
		/// <param name="sourcePid"></param>
		/// <returns>A promise containing the <see cref="PromoteRealmResponse"/></returns>
		Promise<PromoteRealmResponse> GetPromotion(string sourcePid, [System.Runtime.InteropServices.DefaultParameterValueAttribute(null)][System.Runtime.InteropServices.OptionalAttribute()] Beamable.Common.Content.Optional<string[]> contentManifestIds, [System.Runtime.InteropServices.DefaultParameterValueAttribute(null)][System.Runtime.InteropServices.OptionalAttribute()] Beamable.Common.Content.Optional<string[]> promotions);
		/// <param name="gsReq">The <see cref="PromoteRealmRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="PromoteRealmResponse"/></returns>
		Promise<PromoteRealmResponse> PostPromotion(PromoteRealmRequest gsReq);
	}
	public partial class RealmsApi : IRealmsApi
	{
		/// <param name="gsReq">The <see cref="CreateProjectRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="CommonResponse"/></returns>
		public virtual Promise<CommonResponse> PostProjectBeamable(CreateProjectRequest gsReq)
		{
			string gsUrl = "/basic/realms/project/beamable";
			// make the request and return the result
			return _requester.Request<CommonResponse>(Method.POST, gsUrl, Beamable.Serialization.JsonSerializable.ToJson(gsReq), true, Beamable.Serialization.JsonSerializable.FromJson<CommonResponse>);
		}
		/// <param name="alias"></param>
		/// <param name="includeAuthHeader">By default, every request will include an authorization header so that the request acts on behalf of the current user. When the includeAuthHeader argument is false, the request will not include the authorization header for the current user.</param>
		/// <returns>A promise containing the <see cref="AliasAvailableResponse"/></returns>
		public virtual Promise<AliasAvailableResponse> GetCustomerAliasAvailable(string alias, [System.Runtime.InteropServices.DefaultParameterValueAttribute(true)][System.Runtime.InteropServices.OptionalAttribute()] bool includeAuthHeader)
		{
			string gsUrl = "/basic/realms/customer/alias/available";
			string gsQuery = "?";
			System.Collections.Generic.List<string> gsQueries = new System.Collections.Generic.List<string>();
			gsQueries.Add(string.Concat("alias=", alias.ToString()));
			gsQuery = string.Concat(gsQuery, string.Join("&", gsQueries));
			gsUrl = string.Concat(gsUrl, gsQuery);
			// make the request and return the result
			return _requester.Request<AliasAvailableResponse>(Method.GET, gsUrl, default(object), includeAuthHeader, Beamable.Serialization.JsonSerializable.FromJson<AliasAvailableResponse>);
		}
		/// <param name="includeAuthHeader">By default, every request will include an authorization header so that the request acts on behalf of the current user. When the includeAuthHeader argument is false, the request will not include the authorization header for the current user.</param>
		/// <returns>A promise containing the <see cref="ProjectView"/></returns>
		public virtual Promise<ProjectView> GetProject([System.Runtime.InteropServices.DefaultParameterValueAttribute(true)][System.Runtime.InteropServices.OptionalAttribute()] bool includeAuthHeader)
		{
			string gsUrl = "/basic/realms/project";
			// make the request and return the result
			return _requester.Request<ProjectView>(Method.GET, gsUrl, default(object), includeAuthHeader, Beamable.Serialization.JsonSerializable.FromJson<ProjectView>);
		}
		/// <param name="gsReq">The <see cref="CreateProjectRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="CommonResponse"/></returns>
		public virtual Promise<CommonResponse> PostProject(CreateProjectRequest gsReq)
		{
			string gsUrl = "/basic/realms/project";
			// make the request and return the result
			return _requester.Request<CommonResponse>(Method.POST, gsUrl, Beamable.Serialization.JsonSerializable.ToJson(gsReq), true, Beamable.Serialization.JsonSerializable.FromJson<CommonResponse>);
		}
		/// <param name="gsReq">The <see cref="UnarchiveProjectRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="CommonResponse"/></returns>
		public virtual Promise<CommonResponse> PutProject(UnarchiveProjectRequest gsReq)
		{
			string gsUrl = "/basic/realms/project";
			// make the request and return the result
			return _requester.Request<CommonResponse>(Method.PUT, gsUrl, Beamable.Serialization.JsonSerializable.ToJson(gsReq), true, Beamable.Serialization.JsonSerializable.FromJson<CommonResponse>);
		}
		/// <param name="gsReq">The <see cref="ArchiveProjectRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="CommonResponse"/></returns>
		public virtual Promise<CommonResponse> DeleteProject(ArchiveProjectRequest gsReq)
		{
			string gsUrl = "/basic/realms/project";
			// make the request and return the result
			return _requester.Request<CommonResponse>(Method.DELETE, gsUrl, Beamable.Serialization.JsonSerializable.ToJson(gsReq), true, Beamable.Serialization.JsonSerializable.FromJson<CommonResponse>);
		}
		/// <returns>A promise containing the <see cref="GetGameResponse"/></returns>
		public virtual Promise<GetGameResponse> GetGames()
		{
			string gsUrl = "/basic/realms/games";
			// make the request and return the result
			return _requester.Request<GetGameResponse>(Method.GET, gsUrl, default(object), true, Beamable.Serialization.JsonSerializable.FromJson<GetGameResponse>);
		}
		/// <returns>A promise containing the <see cref="RealmConfigResponse"/></returns>
		public virtual Promise<RealmConfigResponse> GetConfig()
		{
			string gsUrl = "/basic/realms/config";
			// make the request and return the result
			return _requester.Request<RealmConfigResponse>(Method.GET, gsUrl, default(object), true, Beamable.Serialization.JsonSerializable.FromJson<RealmConfigResponse>);
		}
		/// <param name="gsReq">The <see cref="RealmConfigSaveRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="CommonResponse"/></returns>
		public virtual Promise<CommonResponse> PutConfig(RealmConfigSaveRequest gsReq)
		{
			string gsUrl = "/basic/realms/config";
			// make the request and return the result
			return _requester.Request<CommonResponse>(Method.PUT, gsUrl, Beamable.Serialization.JsonSerializable.ToJson(gsReq), true, Beamable.Serialization.JsonSerializable.FromJson<CommonResponse>);
		}
		/// <param name="gsReq">The <see cref="RenameProjectRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="CommonResponse"/></returns>
		public virtual Promise<CommonResponse> PutProjectRename(RenameProjectRequest gsReq)
		{
			string gsUrl = "/basic/realms/project/rename";
			// make the request and return the result
			return _requester.Request<CommonResponse>(Method.PUT, gsUrl, Beamable.Serialization.JsonSerializable.ToJson(gsReq), true, Beamable.Serialization.JsonSerializable.FromJson<CommonResponse>);
		}
		/// <returns>A promise containing the <see cref="ServicePlansResponse"/></returns>
		public virtual Promise<ServicePlansResponse> GetPlans()
		{
			string gsUrl = "/basic/realms/plans";
			// make the request and return the result
			return _requester.Request<ServicePlansResponse>(Method.GET, gsUrl, default(object), true, Beamable.Serialization.JsonSerializable.FromJson<ServicePlansResponse>);
		}
		/// <param name="gsReq">The <see cref="CreatePlanRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="CommonResponse"/></returns>
		public virtual Promise<CommonResponse> PostPlans(CreatePlanRequest gsReq)
		{
			string gsUrl = "/basic/realms/plans";
			// make the request and return the result
			return _requester.Request<CommonResponse>(Method.POST, gsUrl, Beamable.Serialization.JsonSerializable.ToJson(gsReq), true, Beamable.Serialization.JsonSerializable.FromJson<CommonResponse>);
		}
		/// <param name="includeAuthHeader">By default, every request will include an authorization header so that the request acts on behalf of the current user. When the includeAuthHeader argument is false, the request will not include the authorization header for the current user.</param>
		/// <returns>A promise containing the <see cref="RealmConfiguration"/></returns>
		public virtual Promise<RealmConfiguration> GetClientDefaults([System.Runtime.InteropServices.DefaultParameterValueAttribute(true)][System.Runtime.InteropServices.OptionalAttribute()] bool includeAuthHeader)
		{
			string gsUrl = "/basic/realms/client/defaults";
			// make the request and return the result
			return _requester.Request<RealmConfiguration>(Method.GET, gsUrl, default(object), includeAuthHeader, Beamable.Serialization.JsonSerializable.FromJson<RealmConfiguration>);
		}
		/// <returns>A promise containing the <see cref="CustomerViewResponse"/></returns>
		public virtual Promise<CustomerViewResponse> GetCustomer()
		{
			string gsUrl = "/basic/realms/customer";
			// make the request and return the result
			return _requester.Request<CustomerViewResponse>(Method.GET, gsUrl, default(object), true, Beamable.Serialization.JsonSerializable.FromJson<CustomerViewResponse>);
		}
		/// <param name="gsReq">The <see cref="NewCustomerRequest"/> instance to use for the request</param>
		/// <param name="includeAuthHeader">By default, every request will include an authorization header so that the request acts on behalf of the current user. When the includeAuthHeader argument is false, the request will not include the authorization header for the current user.</param>
		/// <returns>A promise containing the <see cref="NewCustomerResponse"/></returns>
		public virtual Promise<NewCustomerResponse> PostCustomer(NewCustomerRequest gsReq, [System.Runtime.InteropServices.DefaultParameterValueAttribute(true)][System.Runtime.InteropServices.OptionalAttribute()] bool includeAuthHeader)
		{
			string gsUrl = "/basic/realms/customer";
			// make the request and return the result
			return _requester.Request<NewCustomerResponse>(Method.POST, gsUrl, Beamable.Serialization.JsonSerializable.ToJson(gsReq), includeAuthHeader, Beamable.Serialization.JsonSerializable.FromJson<NewCustomerResponse>);
		}
		/// <returns>A promise containing the <see cref="LaunchMessageListResponse"/></returns>
		public virtual Promise<LaunchMessageListResponse> GetLaunchMessage()
		{
			string gsUrl = "/basic/realms/launch-message";
			// make the request and return the result
			return _requester.Request<LaunchMessageListResponse>(Method.GET, gsUrl, default(object), true, Beamable.Serialization.JsonSerializable.FromJson<LaunchMessageListResponse>);
		}
		/// <param name="gsReq">The <see cref="CreateLaunchMessageRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="CommonResponse"/></returns>
		public virtual Promise<CommonResponse> PostLaunchMessage(CreateLaunchMessageRequest gsReq)
		{
			string gsUrl = "/basic/realms/launch-message";
			// make the request and return the result
			return _requester.Request<CommonResponse>(Method.POST, gsUrl, Beamable.Serialization.JsonSerializable.ToJson(gsReq), true, Beamable.Serialization.JsonSerializable.FromJson<CommonResponse>);
		}
		/// <param name="gsReq">The <see cref="RemoveLaunchMessageRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="CommonResponse"/></returns>
		public virtual Promise<CommonResponse> DeleteLaunchMessage(RemoveLaunchMessageRequest gsReq)
		{
			string gsUrl = "/basic/realms/launch-message";
			// make the request and return the result
			return _requester.Request<CommonResponse>(Method.DELETE, gsUrl, Beamable.Serialization.JsonSerializable.ToJson(gsReq), true, Beamable.Serialization.JsonSerializable.FromJson<CommonResponse>);
		}
		/// <param name="includeAuthHeader">By default, every request will include an authorization header so that the request acts on behalf of the current user. When the includeAuthHeader argument is false, the request will not include the authorization header for the current user.</param>
		/// <returns>A promise containing the <see cref="EmptyResponse"/></returns>
		public virtual Promise<EmptyResponse> GetIsCustomer([System.Runtime.InteropServices.DefaultParameterValueAttribute(true)][System.Runtime.InteropServices.OptionalAttribute()] bool includeAuthHeader)
		{
			string gsUrl = "/basic/realms/is-customer";
			// make the request and return the result
			return _requester.Request<EmptyResponse>(Method.GET, gsUrl, default(object), includeAuthHeader, Beamable.Serialization.JsonSerializable.FromJson<EmptyResponse>);
		}
		/// <returns>A promise containing the <see cref="CustomerResponse"/></returns>
		public virtual Promise<CustomerResponse> GetAdminCustomer()
		{
			string gsUrl = "/basic/realms/admin/customer";
			// make the request and return the result
			return _requester.Request<CustomerResponse>(Method.GET, gsUrl, default(object), true, Beamable.Serialization.JsonSerializable.FromJson<CustomerResponse>);
		}
		/// <param name="rootPID"></param>
		/// <returns>A promise containing the <see cref="GetGameResponse"/></returns>
		public virtual Promise<GetGameResponse> GetGame(string rootPID)
		{
			string gsUrl = "/basic/realms/game";
			string gsQuery = "?";
			System.Collections.Generic.List<string> gsQueries = new System.Collections.Generic.List<string>();
			gsQueries.Add(string.Concat("rootPID=", rootPID.ToString()));
			gsQuery = string.Concat(gsQuery, string.Join("&", gsQueries));
			gsUrl = string.Concat(gsUrl, gsQuery);
			// make the request and return the result
			return _requester.Request<GetGameResponse>(Method.GET, gsUrl, default(object), true, Beamable.Serialization.JsonSerializable.FromJson<GetGameResponse>);
		}
		/// <param name="gsReq">The <see cref="NewGameRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="CommonResponse"/></returns>
		public virtual Promise<CommonResponse> PostGame(NewGameRequest gsReq)
		{
			string gsUrl = "/basic/realms/game";
			// make the request and return the result
			return _requester.Request<CommonResponse>(Method.POST, gsUrl, Beamable.Serialization.JsonSerializable.ToJson(gsReq), true, Beamable.Serialization.JsonSerializable.FromJson<CommonResponse>);
		}
		/// <param name="gsReq">The <see cref="UpdateGameHierarchyRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="CommonResponse"/></returns>
		public virtual Promise<CommonResponse> PutGame(UpdateGameHierarchyRequest gsReq)
		{
			string gsUrl = "/basic/realms/game";
			// make the request and return the result
			return _requester.Request<CommonResponse>(Method.PUT, gsUrl, Beamable.Serialization.JsonSerializable.ToJson(gsReq), true, Beamable.Serialization.JsonSerializable.FromJson<CommonResponse>);
		}
		/// <param name="contentManifestIds"></param>
		/// <param name="promotions"></param>
		/// <param name="sourcePid"></param>
		/// <returns>A promise containing the <see cref="PromoteRealmResponseOld"/></returns>
		public virtual Promise<PromoteRealmResponseOld> GetProjectPromote(string sourcePid, [System.Runtime.InteropServices.DefaultParameterValueAttribute(null)][System.Runtime.InteropServices.OptionalAttribute()] Beamable.Common.Content.Optional<string[]> contentManifestIds, [System.Runtime.InteropServices.DefaultParameterValueAttribute(null)][System.Runtime.InteropServices.OptionalAttribute()] Beamable.Common.Content.Optional<string[]> promotions)
		{
			string gsUrl = "/basic/realms/project/promote";
			string gsQuery = "?";
			System.Collections.Generic.List<string> gsQueries = new System.Collections.Generic.List<string>();
			gsQueries.Add(string.Concat("sourcePid=", sourcePid.ToString()));
			if (((promotions != default(OptionalStringArray))
						&& promotions.HasValue))
			{
				gsQueries.Add(string.Concat("promotions=", promotions.Value.ToString()));
			}
			if (((contentManifestIds != default(OptionalStringArray))
						&& contentManifestIds.HasValue))
			{
				gsQueries.Add(string.Concat("contentManifestIds=", contentManifestIds.Value.ToString()));
			}
			gsQuery = string.Concat(gsQuery, string.Join("&", gsQueries));
			gsUrl = string.Concat(gsUrl, gsQuery);
			// make the request and return the result
			return _requester.Request<PromoteRealmResponseOld>(Method.GET, gsUrl, default(object), true, Beamable.Serialization.JsonSerializable.FromJson<PromoteRealmResponseOld>);
		}
		/// <param name="gsReq">The <see cref="PromoteRealmRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="PromoteRealmResponseOld"/></returns>
		public virtual Promise<PromoteRealmResponseOld> PostProjectPromote(PromoteRealmRequest gsReq)
		{
			string gsUrl = "/basic/realms/project/promote";
			// make the request and return the result
			return _requester.Request<PromoteRealmResponseOld>(Method.POST, gsUrl, Beamable.Serialization.JsonSerializable.ToJson(gsReq), true, Beamable.Serialization.JsonSerializable.FromJson<PromoteRealmResponseOld>);
		}
		/// <param name="includeAuthHeader">By default, every request will include an authorization header so that the request acts on behalf of the current user. When the includeAuthHeader argument is false, the request will not include the authorization header for the current user.</param>
		/// <returns>A promise containing the <see cref="CustomersResponse"/></returns>
		public virtual Promise<CustomersResponse> GetCustomers([System.Runtime.InteropServices.DefaultParameterValueAttribute(true)][System.Runtime.InteropServices.OptionalAttribute()] bool includeAuthHeader)
		{
			string gsUrl = "/basic/realms/customers";
			// make the request and return the result
			return _requester.Request<CustomersResponse>(Method.GET, gsUrl, default(object), includeAuthHeader, Beamable.Serialization.JsonSerializable.FromJson<CustomersResponse>);
		}
		/// <param name="contentManifestIds"></param>
		/// <param name="promotions"></param>
		/// <param name="sourcePid"></param>
		/// <returns>A promise containing the <see cref="PromoteRealmResponse"/></returns>
		public virtual Promise<PromoteRealmResponse> GetPromotion(string sourcePid, [System.Runtime.InteropServices.DefaultParameterValueAttribute(null)][System.Runtime.InteropServices.OptionalAttribute()] Beamable.Common.Content.Optional<string[]> contentManifestIds, [System.Runtime.InteropServices.DefaultParameterValueAttribute(null)][System.Runtime.InteropServices.OptionalAttribute()] Beamable.Common.Content.Optional<string[]> promotions)
		{
			string gsUrl = "/basic/realms/promotion";
			string gsQuery = "?";
			System.Collections.Generic.List<string> gsQueries = new System.Collections.Generic.List<string>();
			gsQueries.Add(string.Concat("sourcePid=", sourcePid.ToString()));
			if (((promotions != default(OptionalStringArray))
						&& promotions.HasValue))
			{
				gsQueries.Add(string.Concat("promotions=", promotions.Value.ToString()));
			}
			if (((contentManifestIds != default(OptionalStringArray))
						&& contentManifestIds.HasValue))
			{
				gsQueries.Add(string.Concat("contentManifestIds=", contentManifestIds.Value.ToString()));
			}
			gsQuery = string.Concat(gsQuery, string.Join("&", gsQueries));
			gsUrl = string.Concat(gsUrl, gsQuery);
			// make the request and return the result
			return _requester.Request<PromoteRealmResponse>(Method.GET, gsUrl, default(object), true, Beamable.Serialization.JsonSerializable.FromJson<PromoteRealmResponse>);
		}
		/// <param name="gsReq">The <see cref="PromoteRealmRequest"/> instance to use for the request</param>
		/// <returns>A promise containing the <see cref="PromoteRealmResponse"/></returns>
		public virtual Promise<PromoteRealmResponse> PostPromotion(PromoteRealmRequest gsReq)
		{
			string gsUrl = "/basic/realms/promotion";
			// make the request and return the result
			return _requester.Request<PromoteRealmResponse>(Method.POST, gsUrl, Beamable.Serialization.JsonSerializable.ToJson(gsReq), true, Beamable.Serialization.JsonSerializable.FromJson<PromoteRealmResponse>);
		}
	}
}