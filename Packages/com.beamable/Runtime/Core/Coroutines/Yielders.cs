using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Coroutines
{
	public static class Yielders
	{
		static Dictionary<float, WaitForSeconds> _timeInterval = new Dictionary<float, WaitForSeconds>(20);

		static WaitForEndOfFrame _endOfFrame = new WaitForEndOfFrame();


		/// <summary>
		/// A single re-usable instance of WaitForEndOfFrame to avoid allocation.
		/// <b>WARNING!</b> Do no use this in a Unity Batch Mode context, like CI/CD.
		/// </summary>
		public static WaitForEndOfFrame EndOfFrame => Application.isBatchMode ? null : _endOfFrame;
		// XXX: Hey, are your tests not working in CI? This might be why. yield returning "null" is ever so slightly different than "endOfFrame"

		static WaitForFixedUpdate _fixedUpdate = new WaitForFixedUpdate();

		/// <summary>
		/// A single re-usable instance of WaitForFixedUpdate to avoid allocation.
		/// </summary>
		public static WaitForFixedUpdate FixedUpdate => _fixedUpdate;

		/// <summary>
		/// Create or get an existing instance of a WaitForSeconds yielder.
		/// Use this to avoid allocation.
		/// </summary>
		/// <param name="seconds">The number of seconds to wait</param>
		/// <returns>An instance of WaitForSeconds with the given seconds wait time.</returns>
		public static WaitForSeconds Seconds(float seconds)
		{
			if (!_timeInterval.ContainsKey(seconds))
				_timeInterval.Add(seconds, new WaitForSeconds(seconds));
			return _timeInterval[seconds];
		}
	}
}
