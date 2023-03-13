using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Api.Analytics
{

	public class LoginPanicEvent : CoreEvent
	{

		public LoginPanicEvent(string step, string error, bool retry)
		: base("loading", "login_panic", new Dictionary<string, object>
		{
			["step"] = step,
			["error"] = error,
			["retry"] = retry
		})
		{
			if (retry)
				Debug.LogError("LOGIN PANIC (retry): " + step + " " + error);
			else
				Debug.LogError("LOGIN PANIC (terminal): " + step + " " + error);
		}
	}
}
