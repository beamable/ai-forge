using Beamable.Api;
using Beamable.Api.Analytics;
using Beamable.Api.Announcements;
using Beamable.Api.Auth;
using Beamable.Api.CloudSaving;
using Beamable.Api.Commerce;
using Beamable.Api.Connectivity;
using Beamable.Api.Events;
using Beamable.Api.Groups;
using Beamable.Api.Inventory;
using Beamable.Api.Leaderboard;
using Beamable.Api.Mail;
using Beamable.Api.Notification;
using Beamable.Api.Payments;
using Beamable.Api.Sessions;
using Beamable.Api.Stats;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Auth;
using Beamable.Common.Api.CloudData;
using Beamable.Common.Api.Notifications;
using Beamable.Common.Api.Presence;
using Beamable.Common.Api.Tournaments;
using Beamable.Content;
using Beamable.Experimental;
using Beamable.Experimental.Api.Calendars;
using Beamable.Experimental.Api.Chat;
using Beamable.Experimental.Api.Matchmaking;
using Beamable.Experimental.Api.Sim;
using Beamable.Experimental.Api.Social;
using System;
using System.Collections.Generic;

namespace Beamable.Player
{
	public class ApiServices : IBeamableAPI
	{

		public class ExperimentalApiServices : IExperimentalAPI
		{
			private readonly BeamContext _ctx;

			public ExperimentalApiServices(BeamContext ctx)
			{
				_ctx = ctx;
			}
			public ChatService ChatService => _ctx.ServiceProvider.GetService<ChatService>();
			public GameRelayService GameRelayService => _ctx.ServiceProvider.GetService<GameRelayService>();
			public MatchmakingService MatchmakingService => _ctx.ServiceProvider.GetService<MatchmakingService>();
			public SocialService SocialService => _ctx.ServiceProvider.GetService<SocialService>();
			public CalendarsService CalendarService => _ctx.ServiceProvider.GetService<CalendarsService>();
		}

		private readonly BeamContext _ctx;
		public User User => _ctx.AuthorizedUser;
		public AccessToken Token => _ctx.AccessToken;

#pragma warning disable CS0067
		public event Action<User> OnUserChanged;
		public event Action<User> OnUserLoggingOut;
#pragma warning restore CS0067


		private ExperimentalApiServices _experimentalApiServices;
		public IExperimentalAPI Experimental => _experimentalApiServices;
		public AnnouncementsService AnnouncementService => _ctx.ServiceProvider.GetService<AnnouncementsService>();
		public IAuthService AuthService => _ctx.ServiceProvider.GetService<IAuthService>();

		public CloudSavingService CloudSavingService => _ctx.ServiceProvider.GetService<CloudSavingService>();
		public ContentService ContentService => _ctx.ServiceProvider.GetService<ContentService>();
		public InventoryService InventoryService => _ctx.ServiceProvider.GetService<InventoryService>();
		public LeaderboardService LeaderboardService => _ctx.ServiceProvider.GetService<LeaderboardService>();
		public IBeamableRequester Requester => _ctx.ServiceProvider.GetService<IBeamableRequester>();
		public StatsService StatsService => _ctx.ServiceProvider.GetService<StatsService>();

		[Obsolete("Use " + nameof(StatsService) + " instead.")]
		public StatsService Stats => _ctx.ServiceProvider.GetService<StatsService>();
		public SessionService SessionService => _ctx.ServiceProvider.GetService<SessionService>();
		public IAnalyticsTracker AnalyticsTracker => _ctx.ServiceProvider.GetService<IAnalyticsTracker>();
		public MailService MailService => _ctx.ServiceProvider.GetService<MailService>();
		public PushService PushService => _ctx.ServiceProvider.GetService<PushService>();
		public CommerceService CommerceService => _ctx.ServiceProvider.GetService<CommerceService>();
		public PaymentService PaymentService => _ctx.ServiceProvider.GetService<PaymentService>();
		public GroupsService GroupsService => _ctx.ServiceProvider.GetService<GroupsService>();
		public EventsService EventsService => _ctx.ServiceProvider.GetService<EventsService>();
		public Promise<IBeamablePurchaser> BeamableIAP => _ctx.ServiceProvider.GetService<Promise<IBeamablePurchaser>>();
		public IConnectivityService ConnectivityService => _ctx.ServiceProvider.GetService<IConnectivityService>();
		public INotificationService NotificationService => _ctx.ServiceProvider.GetService<INotificationService>();
		public ITournamentApi TournamentsService => _ctx.ServiceProvider.GetService<ITournamentApi>();
		public ICloudDataApi TrialDataService => _ctx.ServiceProvider.GetService<ICloudDataApi>();
		public ITournamentApi Tournaments => _ctx.ServiceProvider.GetService<ITournamentApi>();
		public ISdkEventService SdkEventService => _ctx.ServiceProvider.GetService<ISdkEventService>();
		public IPresenceApi PresenceService => _ctx.ServiceProvider.GetService<IPresenceApi>();
		public IPresenceApi Presence => _ctx.ServiceProvider.GetService<IPresenceApi>();

		private string Cid => _ctx.Cid;
		private string Pid => _ctx.Pid;

		public void UpdateUserData(User user)
		{
			_ctx.AuthorizedUser.Value = user; // TODO: Is this a valid thing to do??
			OnUserChanged?.Invoke(user);
		}

		public async Promise<ISet<UserBundle>> GetDeviceUsers()
		{
			var storage = _ctx.ServiceProvider.GetService<AccessTokenStorage>();
			var tokens = storage.RetrieveDeviceRefreshTokens(Cid, Pid);
			var userPromises = new List<Promise<User>>();
			var userBundles = new HashSet<UserBundle>();
			for (var i = 0; i < tokens.Length; i++)
			{
				var token = tokens[i];
				userPromises.Add(AuthService.GetUser(token).RecoverFromNoConnectivity(_ =>
				{
					var dbid = UnityEngine.Random.Range(int.MinValue, 0);
					return new User() { id = dbid };
				}));
			}

			await Promise.Sequence(userPromises);

			for (var i = 0; i < userPromises.Count; i++)
			{
				var userPromise = userPromises[i];
				var user = userPromise.GetResult();
				if (user != null)
				{
					userBundles.Add(new UserBundle { User = user, Token = tokens[i] });
				}
			}

			return userBundles;
		}

		public void RemoveDeviceUser(TokenResponse token)
		{
			_ctx.ServiceProvider.GetService<AccessTokenStorage>().RemoveDeviceRefreshToken(Cid, Pid, token);
		}

		public void ClearDeviceUsers()
		{
			_ctx.ServiceProvider.GetService<PlatformRequester>().DeleteToken();
			_ctx.ServiceProvider.GetService<AccessTokenStorage>().ClearDeviceRefreshTokens(Cid, Pid);
		}

		public async Promise<Unit> ApplyToken(TokenResponse response)
		{
			await _ctx.ChangeAuthorizedPlayer(response);
			return PromiseBase.Unit;
		}

		public ApiServices(BeamContext ctx)
		{
			_ctx = ctx;
			_experimentalApiServices = new ExperimentalApiServices(ctx);

			_ctx.OnUserLoggingOut += user => OnUserLoggingOut?.Invoke(user);
			_ctx.OnUserLoggedIn += user => OnUserChanged?.Invoke(user);
		}
	}
}
