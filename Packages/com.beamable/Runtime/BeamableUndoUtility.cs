using System.Diagnostics;
using UnityEngine;

namespace Beamable
{
	public static class BeamableUndoUtility
	{
		[Conditional("UNITY_EDITOR")]
		public static void Undo(Object obj, string message)
		{
#if UNITY_EDITOR
			UnityEditor.Undo.RecordObject(obj, message);
#endif
		}
	}
}
