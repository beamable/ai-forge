using Beamable.Api;
using Beamable.Common;
using Beamable.Common.Api.Announcements;
using Beamable.Common.Api.Notifications;
using Beamable.Common.Dependencies;
using Beamable.Common.Player;
using System;
using System.Linq;

namespace Beamable.Player
{
	[Serializable]
	public class Announcement
	{
		private readonly PlayerAnnouncements _group;

		/// <summary>
		/// The id of the announcement
		/// </summary>
		public string Id;

		/// <summary>
		/// The title of the announcement
		/// </summary>
		public string Title;

		/// <summary>
		/// The channel of the announcement
		/// </summary>
		public string Channel;

		/// <summary>
		/// The main body and message of the announcement
		/// </summary>
		public string Body;

		// public AnnouncementRef ContentRef; // TODO: It would be cool to know which piece of content spawned this.

		/// <summary>
		/// True when the player has marked the announcement as read. Use the <see cref="Read"/> method.
		/// </summary>
		public bool IsRead;

		/// <summary>
		/// True when the player has claimed the associated rewards with an announcement. Use the <see cref="Claim"/> method.
		/// </summary>
		public bool IsClaimed;

		public bool IsIgnored;


		internal Announcement(PlayerAnnouncements group)
		{
			_group = group;
		}

		// TODO: _could_ have custom editor tooling to perform this method.

		/// <summary>
		/// Mark the announcement as "read", so that the <see cref="IsRead"/> becomes true.
		/// </summary>
		/// <returns>A <see cref="Promise"/> representing the work.</returns>
		public Promise Read() => _group.Read(this);

		/// <summary>
		/// Claim the rewards associated with the announcement.
		/// An announcement can only be claimed once, and then the <see cref="IsClaimed"/> field will be true.
		/// </summary>
		/// <returns>A <see cref="Promise"/> representing the work.</returns>
		public Promise Claim() => _group.Claim(this);


		#region auto generated equality members

		private bool Equals(Announcement other)
		{
			return Id == other.Id && Title == other.Title && Channel == other.Channel && Body == other.Body &&
				   IsRead == other.IsRead && IsClaimed == other.IsClaimed && IsIgnored == other.IsIgnored;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((Announcement)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (Id != null ? Id.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Title != null ? Title.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Channel != null ? Channel.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Body != null ? Body.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ IsRead.GetHashCode();
				hashCode = (hashCode * 397) ^ IsClaimed.GetHashCode();
				hashCode = (hashCode * 397) ^ IsIgnored.GetHashCode();
				return hashCode;
			}
		}

		#endregion
	}

	/// <summary>
	/// A readonly list of the player's <see cref="Announcement"/>s.
	/// The data will be updated by the Beamable SDK.
	/// </summary>
	[Serializable]
	public class PlayerAnnouncements : AbsObservableReadonlyList<Announcement>, IBeamableDisposable
	{
		private IPlatformService _platform;
		private IAnnouncementsApi _announcementsApi;
		private PlayerInventory _playerInventory;
		private INotificationService _notifications;
		private ISdkEventService _sdkEventService;

		public PlayerAnnouncements(IPlatformService platform,
								   IAnnouncementsApi announcementsApi,
								   PlayerInventory playerInventory,
								   INotificationService notifications,
								   ISdkEventService sdkEventService)
		{
			_platform = platform;
			_announcementsApi = announcementsApi;
			_playerInventory = playerInventory;
			_notifications = notifications;
			_sdkEventService = sdkEventService;

			// TODO: How do we handle user sign out?

			_sdkEventService.Register(nameof(Announcements), HandleEvent);
			_notifications.Subscribe(_notifications.GetRefreshEventNameForService("announcements"),
									 HandleSubscriptionUpdate);


			var _ = Refresh(); // automatically start.
			IsInitialized = true;
		}

		private void HandleSubscriptionUpdate(object raw)
		{
			var _ = Refresh(); // fire-and-forget.
		}

		private async Promise HandleEvent(SdkEvent evt)
		{
			switch (evt.Event)
			{
				case "read":
					await _announcementsApi.MarkRead(evt.Args[0]);
					await Refresh();
					break;
				case "claim":
					await _announcementsApi.Claim(evt.Args[0]);
					await _playerInventory.Refresh();
					await Refresh();
					break;
				default:
					throw new Exception($"Unhandled event: {evt.Event}");
			}
		}

		protected override async Promise PerformRefresh()
		{
			// end up with a list of announcement...
			await _platform.OnReady;

			// TODO: add a toplevel awaitable thingy so that players can KNOW they have data
			var data = await _announcementsApi.GetCurrent();

			var nextAnnouncements = data.announcements.Select(view => new Announcement(this)
			{
				// TODO: fill in rest of properties.
				Id = view.id,
				Title = view.title,
				Body = view.body,
				Channel = view.channel,
				IsRead = view.isRead,
				IsClaimed = view.isClaimed,
			}).ToList();

			SetData(nextAnnouncements);
		}

		/// <summary>
		/// Get the first <see cref="Announcement"/> that matches the given id.
		/// It is possible that the announcement hasn't been pulled from the Beamable Cloud yet, so
		/// consider using the <see cref="IRefreshable.Refresh"/> method.
		/// </summary>
		/// <param name="id">The id of the announcement to find</param>
		/// <returns>The first matching <see cref="Announcement"/></returns>
		public Announcement GetAnnouncement(string id) => this.FirstOrDefault(a => string.Equals(a.Id, id));

		/// <summary>
		/// <inheritdoc cref="Announcement.Read"/>
		/// </summary>
		/// <param name="announcement">The <see cref="Announcement"/> instance to mark read</param>
		/// <returns>A <see cref="Promise"/> representing the work.</returns>
		public Promise Read(Announcement announcement)
		{
			return _sdkEventService.Add(new SdkEvent(nameof(Announcements), "read", announcement.Id));
		}

		/// <summary>
		/// <inheritdoc cref="Announcement.Claim"/>
		/// </summary>
		/// <param name="announcement">The <see cref="Announcement"/> instance to claim</param>
		/// <returns>A <see cref="Promise"/> representing the work.</returns>
		public Promise Claim(Announcement announcement)
		{
			return _sdkEventService.Add(new SdkEvent(nameof(Announcements), "claim", announcement.Id));
		}

		public Promise OnDispose()
		{
			_platform = null;
			_announcementsApi = null;
			_playerInventory = null;
			_notifications = null;
			_sdkEventService = null;
			return Promise.Success;
		}
	}
}
