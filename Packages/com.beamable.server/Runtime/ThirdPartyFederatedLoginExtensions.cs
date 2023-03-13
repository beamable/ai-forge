using Beamable.Common;
using Beamable.Common.Api.Auth;
using Beamable.Common.Runtime.Collections;
using System;

namespace Beamable.Server
{
	public static class ThirdPartyFederatedLoginExtensions
	{
		private static readonly ConcurrentDictionary<Type, string> ServiceNamespaceCache = new ConcurrentDictionary<Type, string>();

		private static string GetServiceNamespace<T>() where T : IThirdPartyCloudIdentity, new()
		{
			return ServiceNamespaceCache.GetOrAdd(typeof(T), _ => new T().UniqueName);
		}

		public static Promise<AttachExternalIdentityResponse> AttachIdentity<T>(
			this ISupportsFederatedLogin<T> client,
			string token,
			ChallengeSolution solution = null)
			where T : IThirdPartyCloudIdentity, new()
		{
			var serviceNamespace = GetServiceNamespace<T>();
			var api = client.Provider.GetService<IAuthApi>();
			return api.AttachIdentity(token, client.ServiceName, serviceNamespace, solution);
		}

		public static Promise<DetachExternalIdentityResponse> DetachIdentity<T>(
			this ISupportsFederatedLogin<T> client,
			string userId)
			where T : IThirdPartyCloudIdentity, new()
		{
			var serviceNamespace = GetServiceNamespace<T>();
			var api = client.Provider.GetService<IAuthApi>();
			return api.DetachIdentity(client.ServiceName, userId, serviceNamespace);
		}

		public static Promise<ExternalAuthenticationResponse> AuthorizeExternalIdentity<T>(
			this ISupportsFederatedLogin<T> client,
			string token,
			ChallengeSolution solution = null)
			where T : IThirdPartyCloudIdentity, new()
		{
			var serviceNamespace = GetServiceNamespace<T>();
			var api = client.Provider.GetService<IAuthApi>();
			return api.AuthorizeExternalIdentity(token, client.ServiceName,
												 serviceNamespace,
												 solution);
		}
	}
}
