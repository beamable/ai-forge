using Beamable.Common.Api.Auth;
using Beamable.Common.Api.Inventory;
using Beamable.Common.Content;
using Beamable.Common.Dependencies;
using BeamableReflection;
using System;
using System.Collections.Generic;

namespace Beamable.Common
{
#if BEAMABLE_DEVELOPER || DB_MICROSERVICE
	public class ExampleCloudIdentity : IThirdPartyCloudIdentity
	{
		public string UniqueName => "Example";
	}
#endif


	[Preserve]
	public interface IThirdPartyCloudIdentity
	{
		string UniqueName { get; }
	}

	public interface IFederatedLogin<in T> where T : IThirdPartyCloudIdentity, new()
	{
		Promise<FederatedAuthenticationResponse> Authenticate(string token, string challenge, string solution);
	}

	public interface IFederatedInventory<in T> : IFederatedLogin<T> where T : IThirdPartyCloudIdentity, new()
	{
		Promise<FederatedInventoryProxyState> GetInventoryState(string id);

		Promise<FederatedInventoryProxyState> StartInventoryTransaction(
			string id,
			string transaction,
			Dictionary<string, long> currencies,
			List<ItemCreateRequest> newItems);
	}

	[Serializable]
	public class FederatedAuthenticationResponse : ExternalAuthenticationResponse
	{
		// exists for typing purposes.
	}

	[Serializable]
	public class FederatedInventoryCurrency
	{
		public string name;
		public long value;
	}

	[Serializable]
	public class FederatedInventoryProxyState
	{
		public Dictionary<string, long> currencies;
		public Dictionary<string, List<FederatedItemProxy>> items;
	}

	[Serializable]
	public class FederatedItemProxy
	{
		public string proxyId;
		public List<ItemProperty> properties;
	}

	public interface IHaveServiceName
	{
		string ServiceName { get; }
	}

	public interface ISupportsFederatedLogin<T> : IHaveServiceName where T : IThirdPartyCloudIdentity, new()
	{
		IDependencyProvider Provider { get; }
	}

	public interface ISupportsFederatedInventory<T> : ISupportsFederatedLogin<T>
		where T : IThirdPartyCloudIdentity, new()
	{

	}
}
