using Beamable.Common.Dependencies;
using UnityEngine.Purchasing.Extension;

namespace Beamable.Purchasing.Steam
{
	public class SteamPurchasingModule : AbstractPurchasingModule, IStoreConfiguration
	{
		private readonly IDependencyProvider _provider;

		public SteamPurchasingModule(IDependencyProvider provider)
		{
			_provider = provider;
		}
		public override void Configure()
		{
			RegisterStore(SteamStore.Name, new SteamStore(_provider));
		}
	}
}

