using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Events;
using Beamable.Common.Dependencies;
using System;

namespace Beamable.Api.Events
{
	public class EventSubscription : PlatformSubscribable<EventsGetResponse, EventsGetResponse>
	{
		public EventSubscription(IDependencyProvider provider) : base(provider, AbsEventsApi.SERVICE_NAME)
		{
		}

		public void ForceRefresh()
		{
			Refresh();
		}

		protected override void OnRefresh(EventsGetResponse data)
		{
			data.Init();
			Notify(data);
		}
	}

	/// <summary>
	/// This type defines the %Client main entry point for the %Events feature.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/events-feature">Events</a> feature documentation
	/// - See Beamable.API script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	public class EventsService : AbsEventsApi, IHasPlatformSubscriber<EventSubscription, EventsGetResponse, EventsGetResponse>
	{
		public EventSubscription Subscribable { get; }

		public EventsService(IPlatformService platform, IBeamableRequester requester, IDependencyProvider provider) : base(requester, platform)
		{
			Subscribable = new EventSubscription(provider);
		}

		/// <summary>
		/// Try to claim the earned rewards for an event.
		/// The claim could fail due to a variety of reasons, but this method will safely return even if an error occured.
		/// In that event, the response's <see cref="TryEventClaimResponse.isClaimed"/> will be false to let you know the claim
		/// didn't happen.
		/// If you want to see the exact failure of the claim, use <see cref="Claim"/> instead.
		/// </summary>
		/// <param name="eventId">The runtime id of the event you'd like to claim</param>
		/// <returns>A <see cref="TryEventClaimResponse"/> containing the claim response, and a boolean flag to let you know if the claim was successful.</returns>
		public async Promise<TryEventClaimResponse> TryClaim(string eventId)
		{
			var res = new TryEventClaimResponse { isClaimed = true };
			try
			{
				res.response = await Claim(eventId);
			}
			catch
			{
				res.isClaimed = false;
			}

			return res;
		}

		/// <summary>
		/// Claim the earned rewards for an event.
		/// This method will an exception if the player has never submitted a score to the event.
		/// For a safer Claim method, use <see cref="TryClaim"/>
		/// </summary>
		/// <param name="eventId">The runtime id of the event you'd like to claim</param>
		/// <returns>A promise representing the rewards the player earned</returns>
		/// <exception cref="PlayerNotInEventException">If the player has never submitted a score for this event, you'll get an error</exception>
		/// <exception cref="PlatformRequesterException">If you pass a bad eventId, you'll get an exception with "EventNotFound" </exception>
		/// <exception cref="PlatformRequesterException">If there are no pending claims, you'll get an exception with "NoPendingClaims" </exception>
		public override async Promise<EventClaimResponse> Claim(string eventId)
		{
			try
			{
				var res = await base.Claim(eventId);
				await Subscribable.Refresh();
				return res;
			}
			catch (PlatformRequesterException ex)
				when (ex.Error.status == 400 && ex.Error.error == "EventNotFound")
			{
				var events = await Subscribable.GetCurrent();
				if (events.done.TryFindEventById(eventId, out _) || events.running.TryFindEventById(eventId, out _))
				{
					throw new PlayerNotInEventException(eventId);
				}
				throw;
			}
		}

		public override Promise<EventsGetResponse> GetCurrent(string scope = "") => Subscribable.GetCurrent(scope);
	}

	public class TryEventClaimResponse
	{
		/// <summary>
		/// The contents of the claim operation, or null if there was an error with the claim
		/// </summary>
		public EventClaimResponse response;

		/// <summary>
		/// True if the claim event occured; or false if there was an error with the claim
		/// </summary>
		public bool isClaimed;
	}

	public class PlayerNotInEventException : Exception
	{
		public string EventId { get; }

		public PlayerNotInEventException(string eventId)
		: base($"You cannot claim rewards on an event that the player hasn't submitted a score for. Make sure the player has set a score on the event before calling claim. event-id=[{eventId}]")
		{
			EventId = eventId;
		}
	}

}
