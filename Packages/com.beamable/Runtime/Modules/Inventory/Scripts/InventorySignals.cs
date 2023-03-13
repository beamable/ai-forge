using Beamable.AccountManagement;
using Beamable.Common.Api.Inventory;
using Beamable.Signals;
using System;
using System.Collections.Generic;
// TODO, we can share one toggle event

namespace Beamable.Inventory.Scripts
{

	[System.Serializable]
	public class InventoryUpdateArg
	{
		public List<ItemView> Inventory;
		public InventoryGroup Group;
	}

	public class InventorySignals : DeSignalTower
	{
		public ToggleEvent OnToggleInventory;

		private static bool _toggleState;


		public void ToggleInventory()
		{
			ToggleInventory(!_toggleState);
		}

		public void ToggleInventory(bool desiredState)
		{
			if (_toggleState == desiredState) return;
			_toggleState = desiredState;
			Broadcast(_toggleState, s => s.OnToggleInventory);
		}

		private void Broadcast<TArg>(TArg arg, Func<InventorySignals, DeSignal<TArg>> getter)
		{
			this.BroadcastSignal(arg, getter);
		}

	}
}
