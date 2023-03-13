using Beamable.Api;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Dependencies;
using System.Collections.Generic;

namespace Beamable.Experimental.Api.Chat
{
	public class ChatSubscription : PlatformSubscribable<GetMyRoomsResponse, ChatView>
	{
		private readonly IDependencyProvider _provider;
		private const string SERVICE = "chatV2";
		private readonly ChatView view = new ChatView();

		private const string GroupMembershipEvent = "GROUP.MEMBERSHIP";
		private bool _group_subscribed = false;

		public ChatSubscription(IDependencyProvider provider) : base(provider, SERVICE)
		{
			_provider = provider;
		}

		protected override void OnRefresh(GetMyRoomsResponse data)
		{
			// Subscribe for re-syncs
			if (!_group_subscribed)
			{
				_group_subscribed = true;
				notificationService.Subscribe(GroupMembershipEvent, _ => { Refresh(); });
			}

			view.Update(data.rooms, _provider);
			Notify(view);
		}
	}

	/// <summary>
	/// This type defines the %Client main entry point for the %Chat feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/chat-feature">Chat</a> feature documentation
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class ChatService : ChatApi, IHasPlatformSubscriber<ChatSubscription, GetMyRoomsResponse, ChatView>
	{

		public ChatSubscription Subscribable { get; }

		public ChatService(IPlatformService platform, IBeamableRequester requester, IDependencyProvider provider)
			: base(requester, platform)
		{
			Subscribable = new ChatSubscription(provider);
		}
	}

}
