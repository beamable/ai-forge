using System;
using System.Collections.Generic;
using System.Linq;

namespace Beamable.Common.Api.Events
{
	public interface IEventsApi : ISupportsGet<EventsGetResponse>
	{
		/// <summary>
		/// Claim the earned rewards for an event.
		/// This method will throw an exception if the player has never submitted a score to the event.
		/// </summary>
		/// <param name="eventId">The runtime id of the event you'd like to claim</param>
		/// <returns>A promise representing the rewards the player earned</returns>
		Promise<EventClaimResponse> Claim(string eventId);

		/// <summary>
		/// Submit a score for the current player. Note that this is only allowed
		/// if the event has the write_self permission.
		/// </summary>
		/// <param name="eventId">Full ID of the event, including timestamp suffix.</param>
		/// <param name="score">The score to submit (or score delta if incremental).</param>
		/// <param name="incremental">If incremental is true, add to the existing score, otherwise set it absolutely.</param>
		/// <param name="stats">Optional key-value mapping of stats to apply to the score.</param>
		/// <returns>Promise indicating success or failure.</returns>
		Promise<Unit> SetScore(string eventId, double score, bool incremental = false, IDictionary<string, object> stats = null);
	}

	[Serializable]
	public class EventsGetResponse
	{
		public List<EventView> running;
		public List<EventView> done;

		public void Init()
		{
			foreach (var view in running)
			{
				view.Init();
			}
			foreach (var view in done)
			{
				view.Init();
			}
		}
	}

	public static class EventViewListExtensions
	{
		/// <summary>
		/// Try to find the first event with the requested id in a set of <see cref="EventView"/>.
		/// If no event exists, the method will return false, and the out parameter will be null.
		/// </summary>
		/// <param name="events">some set of events</param>
		/// <param name="id">the runtime id of the event you are looking for</param>
		/// <param name="eventView">an out parameter that will be set to the found event, or set to null if no match is found.</param>
		/// <returns>True if the event is found, false otherwise.</returns>
		public static bool TryFindEventById(this IEnumerable<EventView> events, string id, out EventView eventView)
		{
			eventView = events.FirstOrDefault(e => string.Equals(e?.id, id));
			return eventView != null;
		}
	}

	[Serializable]
	public class EventClaimResponse
	{
		public EventView view;
		public string gameRspJson;
	}

	[Serializable]
	public class EventView
	{
		public string id;
		public string name;
		public string leaderboardId;
		public double score;
		public long rank;
		public long secondsRemaining;
		public DateTime endTime;
		public List<EventReward> scoreRewards;
		public List<EventReward> rankRewards;
		public EventPlayerGroupState groupRewards;

		public EventPhase currentPhase;
		public List<EventPhase> allPhases;

		public void Init()
		{
			endTime = DateTime.UtcNow.AddSeconds(secondsRemaining);
		}
	}

	[Serializable]
	public class EventPlayerGroupState
	{
		public double groupScore;
		public long groupRank;
		public List<EventReward> scoreRewards;
		public List<EventReward> rankRewards;
		public string groupId;
	}

	[Serializable]
	public class EventReward
	{
		public List<EventCurrency> currencies;
		public List<EventItem> items;
		public double min;
		public double max;
		public bool earned;
		public bool claimed;
	}

	[Serializable]
	public class EventCurrency
	{
		public string id;
		public long amount;
	}

	[Serializable]
	public class EventItem
	{
		public string id;
		public Dictionary<string, string> properties;
	}

	[Serializable]
	public class EventPhase
	{
		public string name;
		public long durationSeconds;
		public List<EventRule> rules;
	}

	[Serializable]
	public class EventRule
	{
		public string rule;
		public string value;
	}
}
