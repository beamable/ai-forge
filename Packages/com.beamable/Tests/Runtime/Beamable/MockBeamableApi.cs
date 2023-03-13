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
using Beamable.Common.Api.Tournaments;
using Beamable.Content;
using Beamable.Experimental;
using Beamable.Experimental.Api.Chat;
using Beamable.Experimental.Api.Matchmaking;
using Beamable.Experimental.Api.Sim;
using Beamable.Player;
using System;
using System.Collections.Generic;

namespace Beamable.Tests.Runtime.Api
{
	public class MockBeamableApi : IBeamableAPI
	{
		public User User { get; set; }
		public AccessToken Token { get; set; }
		public IExperimentalAPI Experimental { get; }
		public AnnouncementsService AnnouncementService { get; set; }
		public MockAuthService MockAuthService { get; set; } = new MockAuthService();
		public IAuthService AuthService => MockAuthService;
		public ChatService ChatService { get; set; }

		public CloudSavingService CloudSavingService { get; set; }
		public ContentService ContentService { get; set; }
		public GameRelayService GameRelayService { get; set; }
		public InventoryService InventoryService { get; set; }
		public LeaderboardService LeaderboardService { get; set; }
		public IBeamableRequester Requester { get; set; }
		public StatsService StatsService { get; set; }
		[Obsolete("Use " + nameof(StatsService) + " Instead")]
		public StatsService Stats => StatsService;
		public SessionService SessionService { get; }
		public IAnalyticsTracker AnalyticsTracker { get; }
		public MailService MailService { get; }
		public PushService PushService { get; }
		public CommerceService CommerceService { get; }
		public PaymentService PaymentService { get; }
		public GroupsService GroupsService { get; }
		public EventsService EventsService { get; }
		public Promise<IBeamablePurchaser> BeamableIAP { get; }
		public MatchmakingService Matchmaking { get; }
		public Promise<IBeamablePurchaser> PaymentDelegate { get; }
		public IConnectivityService ConnectivityService { get; }
		public INotificationService NotificationService { get; }
		public ITournamentApi TournamentsService { get; }

		[Obsolete("Use " + nameof(TournamentsService) + " Instead")]
		public ITournamentApi Tournaments => TournamentsService;

		public ISdkEventService SdkEventService { get; }
		public ICloudDataApi TrialDataService { get; }

#pragma warning disable 67
		public event Action<User> OnUserChanged;
		public event Action<User> OnUserLoggingOut;
		public event Action<bool> OnConnectivityChanged;
#pragma warning restore 67


		public Func<TokenResponse, Promise<Unit>> ApplyTokenDelegate;
		public Func<Promise<ISet<UserBundle>>> GetDeviceUsersDelegate;

		public void UpdateUserData(User user)
		{
			User = user;
			TriggerOnUserChanged(user);
		}

		public void TriggerOnUserChanged(User user)
		{
			OnUserChanged?.Invoke(user);
		}

		public void TriggerOnConnectivityChanged(bool isSuccessful)
		{
			OnConnectivityChanged?.Invoke(isSuccessful);
		}

		public Promise<ISet<UserBundle>> GetDeviceUsers()
		{
			return GetDeviceUsersDelegate();
		}

		public void RemoveDeviceUser(TokenResponse token)
		{
			throw new NotImplementedException();
		}

		public void ClearDeviceUsers()
		{
			throw new NotImplementedException();
		}

		public Promise<Unit> ApplyToken(TokenResponse response)
		{
			var promise = ApplyTokenDelegate(response);
			TriggerOnUserChanged(null);
			return promise;
		}
	}
}
