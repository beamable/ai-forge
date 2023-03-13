using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Groups;
using Beamable.Common.Dependencies;
using System.Linq;

namespace Beamable.Api.Groups
{
	public class GroupsSubscription : PlatformSubscribable<GroupUser, GroupsView>
	{
		private const string Service = "group-users";
		private readonly IGroupsApi _api;

		private readonly GroupsView _view = new GroupsView();

		public GroupsSubscription(IDependencyProvider provider, IGroupsApi api) : base(provider, Service)
		{
			_api = api;
		}

		public void ForceRefresh()
		{
			Refresh();
		}

		protected override async void OnRefresh(GroupUser data)
		{
			var groupsResponses = data.member.guild.Select(membership => _api.GetGroup(membership.id)).ToList();
			var groups = await Promise.Sequence(groupsResponses);
			_view.Update(data, groups);
			Notify(_view);
		}
	}

	/// <summary>
	/// This type defines the %Client main entry point for the %Groups feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/groups-feature">Groups</a> feature documentation
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class GroupsService : GroupsApi, IHasPlatformSubscriber<GroupsSubscription, GroupUser, GroupsView>
	{
		public GroupsSubscription Subscribable { get; }

		public GroupsService(IPlatformService platform, IBeamableRequester requester, IDependencyProvider provider) : base(platform, requester)
		{
			Subscribable = new GroupsSubscription(provider, this);
		}

		public override Promise<GroupsView> GetCurrent(string scope = "") => Subscribable.GetCurrent(scope);
	}
}
