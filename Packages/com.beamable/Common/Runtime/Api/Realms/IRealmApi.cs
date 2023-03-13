using System.Collections.Generic;

namespace Beamable.Common.Api.Realms
{
	public interface IRealmsApi
	{
		Promise<CustomerView> GetCustomerData();
		Promise<List<RealmView>> GetGames();
		Promise<RealmView> GetRealm();
		Promise<List<RealmView>> GetRealms(RealmView game = null);
		Promise<List<RealmView>> GetRealms(string pid);
	}
}
