using Beamable.Signals;
using System;
using UnityEngine;
using static Beamable.Common.Constants.URLs;

namespace Beamable.Announcements
{
	[Serializable]
	public class ToggleEvent : DeSignal<bool> { }

	[HelpURL(Documentations.URL_DOC_ANNOUNCEMENTS_FLOW)]
	public class AnnouncementSignals : DeSignalTower
	{
		[Header("Flow Events")] public ToggleEvent OnToggleAnnouncement;

		private static bool _toggleState;

		public static bool ToggleState => _toggleState;

		private void Broadcast<TArg>(TArg arg, Func<AnnouncementSignals, DeSignal<TArg>> getter)
		{
			this.BroadcastSignal(arg, getter);
		}

		public void ToggleAnnouncement()
		{
			_toggleState = !_toggleState;
			Broadcast(_toggleState, s => s.OnToggleAnnouncement);
		}

		public void ToggleAnnouncement(bool desiredState)
		{
			if (desiredState == ToggleState)
				return;
			_toggleState = desiredState;
			Broadcast(_toggleState, s => s.OnToggleAnnouncement);
		}
	}
}
