using Beamable.Common.Content.Validation;
using Beamable.Common.Leaderboards;
using Beamable.Common.Shop;
using Beamable.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
#pragma warning disable CS0618

namespace Beamable.Common.Content
{
	/// <summary>
	/// This type defines a methodology for resolving a reference to a %Beamable %ContentObject.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/content-code#contentlink-vs-contentref">ContentLink vs ContentRef</a> documentation
	/// - See Beamable.Common.Content.ContentObject script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[System.Serializable]
	[Agnostic]
	public class EventLink : ContentLink<EventContent> { }

	/// <summary>
	/// This type defines a methodology for resolving a reference to a %Beamable %ContentObject.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/content-code#contentlink-vs-contentref">ContentLink vs ContentRef</a> documentation
	/// - See Beamable.Common.Content.ContentObject script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[System.Serializable]
	[Agnostic]
	public class EventRef : ContentRef<EventContent> { } // TODO: Factor

	/// <summary>
	/// This type defines a %Beamable %ContentObject subclass for the %EventsService.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Common.Content.ContentObject script reference
	/// - See Beamable.Api.Events.EventsService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[ContentType("events")]
	[System.Serializable]
	[Agnostic]
	public class EventContent : ContentObject, ISerializationCallbackReceiver
	{
		[Tooltip(ContentObject.TooltipName1)]
		[CannotBeBlank]
		public new string name;

		[Tooltip(ContentObject.TooltipStartDate1 + ContentObject.TooltipStartDate2)]
		[FormerlySerializedAs("start_date")]
		[ContentField("start_date")]
		[MustBeDateString(nameof(OnScheduleModified))]
		public string startDate;

		[Tooltip(ContentObject.TooltipPartitionSize1)]
		[FormerlySerializedAs("partition_size")]
		[ContentField("partition_size")]
		[MustBePositive]
		public int partitionSize;

		[Tooltip("Specifies criteria for grouping players together.")]
		public OptionalCohortSettings cohortSettings;

		//Hidden, so no tooltip needed
		[FormerlySerializedAs("phases")]
		[SerializeField]
		[HideInInspector]
		[IgnoreContentField]
		private List<EventPhase> legacyPhases;

		[CannotBeEmpty]
		[Tooltip(ContentObject.TooltipPhase1)]
		public PhaseList phases;

		[Tooltip(ContentObject.TooltipScoreReward1)]
		[FormerlySerializedAs("score_rewards")]
		[ContentField("score_rewards")]
		public List<EventPlayerReward> scoreRewards;

		[Tooltip(ContentObject.TooltipRankReward1)]
		[FormerlySerializedAs("rank_rewards")]
		[ContentField("rank_rewards")]
		public List<EventPlayerReward> rankRewards;

		[Tooltip(ContentObject.TooltipStore1)]
		public List<StoreRef> stores;

		[Tooltip(ContentObject.TooltipGroupReward1)]
		[ContentField("group_rewards")]
		public EventGroupRewards groupRewards;

		[Tooltip(ContentObject.TooltipClientPermission1)]
		public ClientPermissions permissions;

		[Tooltip(ContentObject.TooltipEventSchedule)]
		public OptionalEventSchedule schedule;

		public void OnBeforeSerialize()
		{
			// never save the legacy phases...
			legacyPhases = null;
		}

		public void OnAfterDeserialize()
		{
			// if anything is in the legacy phases, move them into the new list.
			if (legacyPhases != null && legacyPhases.Count > 0)
			{
				phases = new PhaseList
				{
					listData = legacyPhases.ToList()
				};
			}

			legacyPhases = null;
		}
		private void OnScheduleModified()
		{
			if (!schedule.HasValue || schedule.Value.definitions.Count == 0)
				return;

			var date = startDate.ParseEventStartDate(out var _);
			schedule.Value.definitions.ToList().ForEach(scheduleDefinition =>
			{
				scheduleDefinition.second = new List<string> { date.Second.ToString() };
				scheduleDefinition.minute = new List<string> { date.Minute.ToString() };
				scheduleDefinition.hour = new List<string> { date.Hour.ToString() };
				scheduleDefinition.OnScheduleModified?.Invoke(schedule.Value);
			});
		}
	}

	[System.Serializable]
	public class PhaseList : DisplayableList<EventPhase>
	{
		public List<EventPhase> listData = new List<EventPhase>();

		protected override IList InternalList => listData;
		public override string GetListPropertyPath() => nameof(listData);
	}

	[System.Serializable]
	public class EventPhase
	{
		[Tooltip(ContentObject.TooltipName1)]
		[CannotBeBlank]
		public string name;

		[Tooltip(ContentObject.TooltipDurationMinutes1)]
		[FormerlySerializedAs("duration_minutes")]
		[MustBePositive]
		[ContentField("duration_minutes")]
		public int durationMinutes;

		[Tooltip(ContentObject.TooltipEventRule1)]
		public List<EventRule> rules;
	}

	[System.Serializable]
	public class EventRule
	{
		[Tooltip(ContentObject.TooltipRule1)]
		[CannotBeBlank]
		public string rule;

		[Tooltip(ContentObject.TooltipValue1)]
		[CannotBeBlank]
		public string value;
	}

	[System.Serializable]
	public class EventPlayerReward
	{
		[Tooltip(ContentObject.TooltipMinimum1)]
		[MustBeNonNegative]
		public double min;

		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.TooltipMaximum1)]
		[MustBeNonNegative]
		public OptionalDouble max;

		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.TooltipCurrency1)]
		public OptionalEventCurrencyList currencies;

		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.TooltipEventItemObtain1)]
		public OptionalEventItemList items;

	}

	[Serializable]
	public class EventGroupRewards
	{
		[Tooltip(ContentObject.TooltipScoreReward1)]
		public List<EventPlayerReward> scoreRewards;
	}


	[System.Serializable]
	public class EventObtain
	{
		[Tooltip(ContentObject.TooltipSymbol1)]
		public string symbol; // TODO: Is this inventory? Is this entitlement?

		[Tooltip(ContentObject.TooltipCount1)]
		public int count;
	}

	[Serializable]
	public class OptionalEventCurrencyList : Optional<List<EventCurrencyObtain>>
	{

	}

	[Serializable]
	public class OptionalEventItemList : Optional<List<EventItemObtain>> { }

	[Serializable]
	public class EventCurrencyObtain
	{
		[Tooltip(ContentObject.TooltipId1)]
		[MustBeCurrency]
		public string id;

		[Tooltip(ContentObject.TooltipAmount1)]
		[MustBePositive]
		public long amount;
	}

	[Serializable]
	public class EventItemObtain
	{
		[Tooltip(ContentObject.TooltipId1)]
		[MustBeItem]
		public string id;

		[Tooltip(ContentObject.TooltipOptional0 + ContentObject.TooltipProperty1)]
		public OptionalSerializableDictionaryStringToString properties;
	}
}
