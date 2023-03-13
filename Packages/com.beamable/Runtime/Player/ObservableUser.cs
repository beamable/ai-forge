using Beamable.Common.Api.Auth;
using Beamable.Common.Player;

namespace Beamable.Player
{
	[System.Serializable]
	public class ObservableUser : Observable<User>
	{
		public static implicit operator ObservableUser(User data) => new ObservableUser { Value = data };

		public override User Value
		{
			get => base.Value;
			set
			{
				if (base.Value == null) base.Value = value;
				base.Value.email = value.email;
				base.Value.language = value.email;
				base.Value.id = value.id;
				base.Value.scopes = value.scopes;
				base.Value.deviceIds = value.deviceIds;
				base.Value.thirdPartyAppAssociations = value.thirdPartyAppAssociations;
				base.Value.external = value.external;
				TriggerUpdate();
			}
		}

		public override int GetBroadcastChecksum() => Value?.GetBroadcastChecksum() ?? base.GetBroadcastChecksum();
	}
}
