using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using Beamable.Common.Shop;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
#pragma warning disable CS0618

namespace Beamable.Common.Announcements
{

	/// <summary>
	/// This type defines a %Beamable %ContentObject subclass for the %AnnouncementsService.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Common.Content.ContentObject script reference
	/// - See Beamable.Api.Announcements.AnnouncementsService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[ContentType("announcements")]
	[System.Serializable]
	[Agnostic]
	public class AnnouncementContent : ContentObject
	{
		/// <summary>
		/// The channel is similar to an inbox in email. You can use whatever value you like. Common examples might be "Primary", "Advertising", or "Seasonal".
		/// </summary>
		[Tooltip("The category of the announcement")]
		[CannotBeBlank]
		public string channel = "main";

		/// <summary>
		/// The title is similar to the subject line of an email.
		/// </summary>
		[Tooltip("The title of the announcement")]
		[CannotBeBlank]
		public string title = "title";

		/// <summary>
		/// A brief description of the announcement.
		/// </summary>
		[Tooltip("A summary of the announcement")]
		[CannotBeBlank]
		public string summary = "summary";

		/// <summary>
		/// The main content of the announcement
		/// </summary>
		[Tooltip("A main body of the announcement")]
		[TextArea(10, 10)]
		[CannotBeBlank]
		public string body = "body";

		/// <summary>
		/// The <see cref="startDate"/>, defined as UTC, specifies when the announcement becomes available for players to see. If no startDate is specified, the announcement will become visible immediately
		/// </summary>
		[Tooltip(ContentObject.TooltipOptional0 + "The startDate, defined as UTC, specifies when the announcement becomes available for players to see. If no startDate is specified, the announcement will become visible immediately " + ContentObject.TooltipStartDate2)]
		[FormerlySerializedAs("start_date")]
		[MustBeDateString]
		[ContentField("start_date")]
		public OptionalString startDate;

		/// <summary>
		/// The <see cref="endDate"/>, defined as UTC, specifies when the announcement stops being available for players to see. If no endDate is specified, the announcement will be visible forever
		/// </summary>
		[Tooltip(ContentObject.TooltipOptional0 + "The endDate, defined as UTC, specifies when the announcement stops being available for players to see. If no endDate is specified, the announcement will be visible forever " + ContentObject.TooltipEndDate2)]
		[FormerlySerializedAs("end_date")]
		[MustBeDateString]
		[ContentField("end_date")]
		public OptionalString endDate;

		/// <summary>
		/// A set of <see cref="AnnouncementAttachment"/> that the player may claim.
		/// This field will be removed in the future, and entirely replaced by the <see cref="gift"/> field.
		/// </summary>
		[Tooltip(ContentObject.TooltipAttachment1)]
		public List<AnnouncementAttachment> attachments;

		/// <summary>
		/// A <see cref="AnnouncementPlayerRewards"/> contains the rewards that a player claim for this announcement.
		/// Please use this field instead of the <see cref="attachments"/> field.
		/// </summary>
		[Tooltip("Players who claim the announcement can receive the rewards listed below")]
		[ContentField("gift")]
		public AnnouncementPlayerRewards gift;

		/// <summary>
		/// If specified, stat requirements will limit the audience of this announcement based on player stats.
		/// </summary>
		[Tooltip(ContentObject.TooltipOptional0 + "If specified, stat requirements will limit the audience of this announcement based on player stats")]
		[ContentField("stat_requirements")]
		public OptionalStats statRequirements;

		/// <summary>
		/// ClientData is a general purpose string to string key value pair system that you can use to add arbitrary metadata to your announcements.
		/// </summary>
		[Tooltip(ContentObject.TooltipOptional0 + "If specified, the client data")]
		public OptionalSerializableDictionaryStringToString clientData;
	}

	/// <summary>
	/// A type of <see cref="PlayerReward"/> that allows a player to claim <see cref="AnnouncementApiReward"/> as well as items and currencies.
	/// </summary>
	[Serializable]
	public class AnnouncementPlayerRewards : PlayerReward<OptionalListOfAnnouncementRewards>
	{
		public override bool HasAnyReward()
		{
			return base.HasAnyReward() || webhooks.GetOrElse(() => null)?.Count > 0;
		}
	}

}
