using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using Beamable.Common.Shop;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0618

namespace Beamable.Common.Leaderboards
{
	/// <summary>
	/// This type defines a %Beamable %ContentObject subclass for the %LeaderboardService.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Common.Content.ContentObject script reference
	/// - See Beamable.Api.Leaderboard.LeaderboardService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[ContentType("leaderboards")]
	[System.Serializable]
	[Agnostic]
	public class LeaderboardContent : ContentObject
	{
		[Tooltip(ContentObject.TooltipClientPermission1)]
		public ClientPermissions permissions;

		[Tooltip("Determines whether this leaderboard automatically partitions into smaller leaderboards.")]
		public OptionalBoolean partitioned;

		[MustBePositive]
		[Tooltip("Determines the maximum number of entries in a given leaderboard partition.")]
		public OptionalInt max_entries;

		[Tooltip("Specifies criteria for grouping players together.")]
		public OptionalCohortSettings cohortSettings;
	}

	[System.Serializable]
	public class OptionalCohortSettings : Optional<LeaderboardCohortSettings> { }

	[System.Serializable]
	public class LeaderboardCohortSettings
	{
		[CannotBeEmpty]
		public List<LeaderboardCohort> cohorts;
	}

	[System.Serializable]
	public class LeaderboardCohort
	{
		[MustBeSlugString(MustBeSlugStringConfig.ALLOW_UNDERSCORE)]
		[Tooltip("The id of this cohort. This will appear in the leaderboard id.")]
		public string id;

		[Tooltip("(Optional) Human readable description of what this cohort represents.")]
		public OptionalString description;

		[CannotBeEmpty]
		[Tooltip("Players must qualify for all the specified stats in order to qualify for the cohort.")]
		public List<StatRequirement> statRequirements;
	}
}
