// unset

using Beamable.Api;
using Beamable.Common.Player;

namespace Beamable.Player
{
	public class ObservableAccessToken : Observable<AccessToken>
	{
		public static implicit operator ObservableAccessToken(AccessToken data) => new ObservableAccessToken { Value = data };
	}
}
