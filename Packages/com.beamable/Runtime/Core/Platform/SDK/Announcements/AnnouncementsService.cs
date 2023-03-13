using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Announcements;
using Beamable.Common.Dependencies;
using System;
using System.Collections.Generic;

namespace Beamable.Api.Announcements
{
	public class AnnouncementsSubscription : PlatformSubscribable<AnnouncementQueryResponse, AnnouncementQueryResponse>
	{
		public AnnouncementsSubscription(IDependencyProvider provider, string service) : base(provider, service)
		{
		}

		protected override void OnRefresh(AnnouncementQueryResponse data)
		{
			foreach (var announcement in data.announcements)
			{
				announcement.endDateTime = DateTime.UtcNow.AddSeconds(announcement.secondsRemaining);
			}
			Notify(data);
		}
	}

	/// <summary>
	/// This type defines the %Client main entry point for the %Announcements feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/announcements-feature">Announcements</a> feature documentation
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class AnnouncementsService : AbsAnnouncementsApi, IHasPlatformSubscriber<AnnouncementsSubscription, AnnouncementQueryResponse, AnnouncementQueryResponse>
	{
		public AnnouncementsSubscription Subscribable { get; }

		public AnnouncementsService(IPlatformService platform, IBeamableRequester requester, IDependencyProvider provider) : base(requester, platform)
		{
			Subscribable = new AnnouncementsSubscription(provider, "announcements");
		}

		public override Promise<EmptyResponse> Claim(List<string> ids)
		{
			return base.Claim(ids).Then(_ =>
			{
				var data = Subscribable.GetLatest();
				if (data == null) return;

				var announcements = data.announcements.FindAll((next) => ids.Contains(next.id));
				if (announcements != null)
				{
					foreach (var announcement in announcements)
					{
						announcement.isRead = true;
						announcement.isClaimed = true;
					}
				}
				Subscribable.Notify(data);
			});
		}

		public override Promise<EmptyResponse> MarkDeleted(List<string> ids)
		{
			return base.MarkDeleted(ids).Then(_ =>
			{
				var data = Subscribable.GetLatest();
				if (data != null)
				{
					data.announcements.RemoveAll((next) => ids.Contains(next.id));
					Subscribable.Notify(data);
				}
			});
		}

		public override Promise<EmptyResponse> MarkRead(List<string> ids)
		{
			return base.MarkRead(ids).Then(_ =>
			{
				var data = Subscribable.GetLatest();
				if (data != null)
				{
					var announcements = data.announcements.FindAll((next) => ids.Contains(next.id));
					if (announcements != null)
					{
						foreach (var announcement in announcements)
						{
							announcement.isRead = true;
						}
					}
					Subscribable.Notify(data);
				}
			});
		}

		public override Promise<AnnouncementQueryResponse> GetCurrent(string scope = "") =>
		   Subscribable.GetCurrent(scope);

	}

}
