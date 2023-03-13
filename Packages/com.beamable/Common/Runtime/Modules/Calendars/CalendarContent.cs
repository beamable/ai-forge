using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using Beamable.Experimental.Common.Api.Calendars;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
#pragma warning disable CS0618

namespace Beamable.Experimental.Common.Calendars
{
	/// <summary>
	/// This type defines a %Beamable %ContentObject subclass for the %CalendarsService.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See Beamable.Common.Content.ContentObject script reference
	/// - See Beamable.Experimental.Api.Calendars.CalendarsService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[ContentType("calendars")]
	[System.Serializable]
	[Agnostic]
	public class CalendarContent : ContentObject
	{
		[Tooltip(ContentObject.TooltipStartDate1 + ContentObject.TooltipStartDate2)]
		[FormerlySerializedAs("start_date")]
		[MustBeDateString]
		[ContentField("start_date")]
		public OptionalString startDate;

		[HideInInspector] // this is a legacy entitlements setup we don't support. But we can't delete it because scala requires it.
		[Obsolete]
		public List<RewardCalendarDay> days;
	}
}
