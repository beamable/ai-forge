using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Common.Inventory;
using Beamable.Common.Player;
using System;
using UnityEngine;

namespace Beamable.Player
{
	/// <summary>
	/// The <see cref="PlayerCurrency"/> represents one currency of a player. The <see cref="Amount"/> shows the current value of the currency.
	/// </summary>
	[Serializable]
	public class PlayerCurrency : DefaultObservable
	{
		/// <summary>
		/// An event that happens whenever the <see cref="Amount"/> changes
		/// this event happens after <see cref="DefaultObservable.OnUpdated"/>
		/// </summary>
		public event Action<long> OnAmountUpdated;

		/// <summary>
		/// The id of the <see cref="CurrencyContent"/> that this currency is for.
		/// </summary>
		public string CurrencyId;

		[SerializeField]
		private long _amount;

		/// <summary>
		/// The current amount of the currency. This value can change over time, and will get realtime updates from the server.
		/// Use the <see cref="OnAmountUpdated"/> event to be notified when that happens.
		/// <para>
		/// You can set this currency locally, but isn't recommended.
		/// </para>
		/// </summary>
		public long Amount
		{
			get => _amount;
			set
			{
				_amount = value;
				TriggerUpdate();
			}
		}

		/// <summary>
		/// The reference to the <see cref="CurrencyContent"/> that this <see cref="PlayerCurrency"/> is connected to.
		/// </summary>
		[NonSerialized]
		public CurrencyContent Content;

		/// <summary>
		/// Currencies may have a set of properties in dictionary format.
		/// To edit this value, use the <see cref="PlayerInventory.Update()"/> function
		/// </summary>
		public SerializableDictionaryStringToString Properties = new SerializableDictionaryStringToString();


		public PlayerCurrency()
		{
			OnUpdated += () => OnAmountUpdated?.Invoke(Amount);
		}

		public new void TriggerUpdate() => base.TriggerUpdate();

		#region Auto Generated Equality Members

		protected bool Equals(PlayerCurrency other)
		{
			return CurrencyId == other.CurrencyId && Amount == other.Amount;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((PlayerCurrency)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((CurrencyId != null ? CurrencyId.GetHashCode() : 0) * 397) ^ Amount.GetHashCode();
			}
		}

		#endregion

	}


	/// <summary>
	/// The <see cref="PlayerCurrencyGroup"/> is a readonly observable list of currencies for a player.
	/// It represents <b>all</b> currencies for a player.
	/// </summary>
	[Serializable]
	public class PlayerCurrencyGroup : AbsObservableReadonlyList<PlayerCurrency>
	{
		private readonly CurrencyRef _rootRef;
		private readonly PlayerInventory _inventory;
		public Promise OnReady;

		/// <summary>
		/// The scope defines which currencies in the inventory this group will be able to view.
		/// If the scope is "currency", then this group will view every currency in the player inventory.
		/// However, if the scope was "currency.a", then the group would only show items that were
		/// instances of "currency.a" and sub types.
		/// </summary>
		public string RootScope => _rootRef?.Id ?? "currency";

		public PlayerCurrencyGroup(CurrencyRef rootRef, PlayerInventory inventory)
		{
			_rootRef = rootRef;
			_inventory = inventory;
			OnReady = Refresh(); // automatically refresh..
		}

		public void Notify()
		{
			var data = _inventory.LocalCurrencies.GetAll(RootScope);
			SetData(data);
		}

		protected override async Promise PerformRefresh()
		{
			await _inventory.Refresh(RootScope);
			Notify();
		}

		/// <summary>
		/// Get a specific currency.
		/// If the currency doesn't yet exist, it will be returned as a new currency with a value of 0.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public PlayerCurrency GetCurrency(CurrencyRef id)
		{
			return _inventory.GetCurrency(id);
		}

		/// <summary>
		/// Update the currency's <see cref="PlayerCurrency.Amount"/> by the given amount.
		/// Internally, this will trigger an <see cref="PlayerInventory.Update(Beamable.Common.Api.Inventory.InventoryUpdateBuilder,string)"/>
		/// </summary>
		/// <param name="currency">The currency to modify</param>
		/// <param name="amount">The amount the currency will change by. This is incremental. A negative number will decrement the currency.</param>
		/// <returns>A promise representing when the add operation has completed</returns>
		public Promise Add(CurrencyRef currency, long amount)
		{
			return _inventory.Update(b => b.CurrencyChange(currency, amount));
		}

	}
}
