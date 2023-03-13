using Beamable.Server.Api;
using Beamable.Server.Api.Announcements;
using Beamable.Server.Api.Calendars;
using Beamable.Server.Api.Chat;
using Beamable.Server.Api.CloudData;
using Beamable.Server.Api.Commerce;
using Beamable.Server.Api.Content;
using Beamable.Server.Api.Events;
using Beamable.Server.Api.Groups;
using Beamable.Server.Api.Inventory;
using Beamable.Server.Api.Leaderboards;
using Beamable.Server.Api.Mail;
using Beamable.Server.Api.Notifications;
using Beamable.Server.Api.Payments;
using Beamable.Server.Api.RealmConfig;
using Beamable.Server.Api.Social;
using Beamable.Server.Api.Stats;
using Beamable.Server.Api.Tournament;

namespace Beamable.Server
{
	/// <summary>
	/// This type defines the %Microservice main entry point for %Beamable %Microservice features.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Server.Microservice script reference
	///
	/// ### Example
	/// This demonstrates example usage from WITHIN a custom %Beamable %Microservice.
	///
	/// ```
	/// [ClientCallable]
	/// private async void MyMicroserviceMethod()
	/// {
	///
	///   // Example usage
	///   var announcementsService = Services.Announcements;
	///   var result = await announcementsService.GetCurrent();
	///
	///   // Others...
	///   var AuthService = Services.Auth;
	///   var CalendarsService = Services.Calendars;
	///   var ContentService = Services.Content;
	///   var EventsService = Services.Events;
	///   var GroupsService = Services.Groups;
	///   var InventoryService = Services.Inventory;
	///   var LeaderboardsService = Services.Leaderboards;
	///   var RealmConfigurationService = Services.RealmConfig;
	///   var SocialService = Services.Social;
	///   var StatsService = Services.Stats;
	///   var TournamentService = Services.Tournament;
	///   var TrialDataService = Services.TrialData;
	///
	/// }
	///
	/// ```
	///
	///
	/// #### Alternative API Links
	/// - See Beamable.API for the main %Client script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public interface IBeamableServices
	{
		/// <summary>
		/// %Microservice entry point for the <a target="_blank" href="https://docs.beamable.com/docs/announcements-feature-overview">Announcements</a> feature
		/// </summary>
		IMicroserviceAnnouncementsApi Announcements { get; }

		/// <summary>
		/// %Microservice entry point for the <a target="_blank" href="https://docs.beamable.com/docs/identity">Identity</a> feature
		/// </summary>
		IMicroserviceAuthApi Auth { get; }

		/// <summary>
		/// %Microservice entry point for the Calendars feature
		/// </summary>
		IMicroserviceCalendarsApi Calendars { get; }

		/// <summary>
		/// %Microservice entry point for the <a target="_blank" href="https://docs.beamable.com/docs/content-feature-overview">Content</a> feature
		/// </summary>
		IMicroserviceContentApi Content { get; }

		/// <summary>
		/// %Microservice entry point for the <a target="_blank" href="https://docs.beamable.com/docs/events-feature-overview">Events</a> feature
		/// </summary>
		IMicroserviceEventsApi Events { get; }

		/// <summary>
		/// %Microservice entry point for the <a target="_blank" href="https://docs.beamable.com/docs/groups-feature-overview">Groups</a> feature
		/// </summary>
		IMicroserviceGroupsApi Groups { get; }

		/// <summary>
		/// %Microservice entry point for the <a target="_blank" href="https://docs.beamable.com/docs/inventory-feature-overview">Inventory</a> feature
		/// </summary>
		IMicroserviceInventoryApi Inventory { get; }

		/// <summary>
		/// %Microservice entry point for the <a target="_blank" href="https://docs.beamable.com/docs/leaderboards-feature-overview">Leaderboards</a> feature
		/// </summary>
		IMicroserviceLeaderboardsApi Leaderboards { get; }

		/// <summary>
		/// %Microservice entry point for the <a target="_blank" href="https://docs.beamable.com/docs/mail-feature-overview">Mail</a> feature
		/// </summary>
		IMicroserviceMailApi Mail { get; }

		/// <summary>
		/// %Microservice entry point for the <a target="_blank" href="https://docs.beamable.com/docs/notifications-feature">Mail</a> feature
		/// </summary>
		IMicroserviceNotificationsApi Notifications { get; }

		/// <summary>
		/// %Microservice entry point for the <a target="_blank" href="https://docs.beamable.com/docs/realm-configuration-feature">Realm Configuration</a> feature
		/// </summary>
		IMicroserviceRealmConfigService RealmConfig { get; }

		/// <summary>
		/// %Microservice entry point for the <a target="_blank" href="https://docs.beamable.com/docs/social-networking">Social</a> feature
		/// </summary>
		IMicroserviceSocialApi Social { get; }

		/// <summary>
		/// %Microservice entry point for the <a target="_blank" href="https://docs.beamable.com/docs/stats-feature-overview">Stats</a> feature
		/// </summary>
		IMicroserviceStatsApi Stats { get; }

		/// <summary>
		/// %Microservice entry point for the <a target="_blank" href="https://docs.beamable.com/docs/tournaments-feature-overview">Tournaments</a> feature
		/// </summary>
		IMicroserviceTournamentApi Tournament { get; }

		/// <summary>
		/// %Microservice entry point for the <a target="_blank" href="https://docs.beamable.com/docs/cloud-save-feature-overview">Cloud Saving</a> feature
		/// </summary>
		IMicroserviceCloudDataApi TrialData { get; }

		/// <summary>
		/// %Microservice entry point for the <a target="_blank" href="https://docs.beamable.com/reference/commerce-overview">Commerce</a> feature
		/// </summary>
		IMicroserviceCommerceApi Commerce { get; }

		/// <summary>
		/// %Microservice entry point for the <a target="_blank" href="https://docs.beamable.com/reference/chat-overview">Chat</a> feature
		/// </summary>
		IMicroserviceChatApi Chat { get; }

		/// <summary>
		/// %Microservice entry point for payment operations.
		/// </summary>
		IMicroservicePaymentsApi Payments { get; }
	}
}
