using System;
using UnityEngine.Events;
#if UNITY_DEVELOPER
using System.Linq;
using UnityEngine;
#endif

namespace Beamable.Signals
{
	[Serializable]
	public class DeSignal<T> : UnityEvent<T>
	{
		public void InvokeWithTrace<TSource>(TSource sourceTower, TSource currentTower, T arg)
		   where TSource : DeSignalTower
		{
#if UNITY_DEVELOPER
			var fields = currentTower.GetType().GetFields();
			var field = fields.FirstOrDefault(f => f.GetValue(currentTower) == this);

			var fieldName = field == null ? "unknown " : field.Name;
			var listenerCount = GetPersistentEventCount();
			Debug.Log($"Signal=[{fieldName}]. Source=[{sourceTower.name}] ListenerCount=[{listenerCount}] arg=[{arg}] ", sourceTower);
			for (var i = 0; i < listenerCount; i++)
			{
				Debug.Log($"   Listener {i}: Current=[{currentTower.name}] Target=[{GetPersistentTarget(i).name}] Method=[{GetPersistentMethodName(i)}]", currentTower);
			}
#endif
			Invoke(arg);
		}
	}

	public static class GameObjectExtensions
	{
		public static void BroadcastSignal<T, TArg>(this T self, TArg arg, DeSignal<TArg> signal)
		   where T : DeSignalTower
		{
			if (self == null || self.gameObject == null || !self.gameObject.activeInHierarchy) return;
			if (self.Diagnostic)
			{
				signal?.InvokeWithTrace(self, self, arg);
			}
			else
			{
				signal?.Invoke(arg);
			}
		}

		public static void BroadcastSignal<T, TArg>(this T self, TArg arg, Func<T, DeSignal<TArg>> getter)
		   where T : DeSignalTower
		{
			DeSignalTower.ForAll<T>(tower =>
			{
				if (tower == null || tower.gameObject == null || !tower.gameObject.activeInHierarchy) return;

				var signal = getter(tower);

				if (self.Diagnostic || tower.Diagnostic) // if the sender, or the receiver has diagnostic on...
				{
					signal?.InvokeWithTrace(self, tower, arg);
				}
				else
				{
					signal?.Invoke(arg);
				}
			});
		}
	}
}
