using Beamable.Signals;
using System;
using UnityEngine;

namespace Beamable.Leaderboards
{
	[Serializable] public class ToggleEvent : DeSignal<bool> { }

	public class LeaderboardSignals : DeSignalTower
	{
		[Header("Flow Events")] public ToggleEvent OnToggleLeaderboard;
		public static bool ToggleState { get; private set; }

		private void Broadcast<TArg>(TArg arg, Func<LeaderboardSignals, DeSignal<TArg>> getter)
		{
			this.BroadcastSignal(arg, getter);
		}

		public void ToggleLeaderboard()
		{
			ToggleState = !ToggleState;
			Broadcast(ToggleState, s => s.OnToggleLeaderboard);
		}

		public void ToggleLeaderboard(bool desiredState)
		{
			if (desiredState == ToggleState) { return; }

			ToggleState = desiredState;
			Broadcast(ToggleState, s => s.OnToggleLeaderboard);
		}
	}
}
