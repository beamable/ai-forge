using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Signals
{
	public class DeSignalTower : MonoBehaviour
	{
		private static Dictionary<Type, List<DeSignalTower>> _all = new Dictionary<Type, List<DeSignalTower>>();
		public bool Diagnostic;

		private void OnEnable()
		{
			// register to the knick-knacks.
			var selfType = GetType();

			if (!_all.ContainsKey(selfType))
			{
				_all.Add(selfType, new List<DeSignalTower>());
			}

			_all[selfType].Add(this);

			OnAfterEnable();
		}

		private void OnDisable()
		{
			var selfType = GetType();

			if (!_all.ContainsKey(selfType))
			{
				_all.Add(selfType, new List<DeSignalTower>());
			}

			_all[selfType].Remove(this);

			OnAfterDisable();
		}

		protected virtual void OnAfterEnable()
		{

		}

		protected virtual void OnAfterDisable()
		{

		}

		public static void ForAll<TSignalTower>(Action<TSignalTower> action)
		   where TSignalTower : DeSignalTower
		{
			if (!_all.ContainsKey(typeof(TSignalTower)))
			{
				return;
			}

			var set = _all[typeof(TSignalTower)];
			List<DeSignalTower> callbackBuffer = new List<DeSignalTower>();
			callbackBuffer.AddRange(set);
			foreach (var next in callbackBuffer)
			{
				action(next as TSignalTower);
			}
		}
	}
}
