using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Common.Inventory;
using Beamable.Common.Player;
using System;
using System.Runtime.CompilerServices;

namespace Beamable.Player
{
	/// <summary>
	/// An item in the player's inventory. The item is uniquely identified by the <see cref="ContentId"/> and <see cref="ItemId"/> as a pair.
	/// <para>
	/// Use the <see cref="Properties"/> dictionary to store runtime instance data about a particular item.
	/// </para>
	/// </summary>
	[Serializable]
	public class PlayerItem : DefaultObservable
	{
		#region equality members
		protected bool Equals(PlayerItem other)
		{
			return ContentId == other.ContentId && ItemId == other.ItemId && CreatedAt == other.CreatedAt && UpdatedAt == other.UpdatedAt && Equals(Properties, other.Properties);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != this.GetType())
			{
				return false;
			}

			return Equals((PlayerItem)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (ContentId != null ? ContentId.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ ItemId.GetHashCode();
				hashCode = (hashCode * 397) ^ CreatedAt.GetHashCode();
				hashCode = (hashCode * 397) ^ UpdatedAt.GetHashCode();
				hashCode = (hashCode * 397) ^ (Properties != null ? Properties.GetHashCode() : 0);
				return hashCode;
			}
		}
		#endregion

		/// <summary>
		/// The content id of <see cref="ItemContent"/> that this item is an instance of.
		/// </summary>
		public string ContentId;

		/// <summary>
		/// The item instance id. This id is unique per player, but not across players.
		/// </summary>
		public long ItemId;

		/// <summary>
		/// The item instance id for federated inventory. This id is across players.
		/// </summary>
		public Optional<string> ProxyId;

		/// <summary>
		/// The epoch timestamp in milliseconds when the item was created.
		/// </summary>
		public long CreatedAt;

		/// <summary>
		/// The epoch timestamp in milliseconds when the item was last modified.
		/// </summary>
		public long UpdatedAt;

		/// <summary>
		/// A hash of the <see cref="ContentId"/> and the <see cref="ItemId"/> that can be used for fast identity checking.
		/// </summary>
		public int UniqueCode;

		/// <summary>
		/// A set of instance level property data for the item.
		/// </summary>
		public SerializableDictionaryStringToString Properties = new SerializableDictionaryStringToString();

		/// <summary>
		/// A reference to the top level <see cref="ItemContent"/> content.
		/// If the item is a sub type of the item content, then this can be cast to the accurate type.
		/// </summary>
		public ItemContent Content;

		/// <summary>
		/// An event that will trigger when this item is removed from the player's inventory.
		/// </summary>
		public event Action OnDeleted;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static int CombineHashCodes(int h1, int h2)
		{
			return (((h1 << 5) + h1) ^ h2);
		}

		internal new void TriggerUpdate() => base.TriggerUpdate();

		public override int GetBroadcastChecksum()
		{
			var hash = 0;
			foreach (var kvp in Properties)
			{
				hash = CombineHashCodes(hash, kvp.Key?.GetHashCode() ?? 1);
				hash = CombineHashCodes(hash, kvp.Value?.GetHashCode() ?? 1 );
			}

			hash = CombineHashCodes(hash, UpdatedAt.GetHashCode());
			// we don't need to care about the content id, item id, or created at, because logically, they should never change.
			return hash;
		}

		internal void TriggerDeletion()
		{
			OnDeleted?.Invoke();
		}
	}

	/// <summary>
	/// A list of <see cref="PlayerItem"/>s grouped by a common type.
	/// </summary>
	[Serializable]
	public class PlayerItemGroup : AbsObservableReadonlyList<PlayerItem>
	{
		private ItemRef _rootRef;
		private readonly PlayerInventory _inventory;
		public Promise OnReady;

		/// <summary>
		/// The scope defines which items in the inventory this group will be able to view.
		/// If the scope is "items", then this group will view every item in the player inventory.
		/// However, if the scope was "items.fish", then the group would only show items that were
		/// instances of "items.fish" and sub types.
		/// </summary>
		public string RootScope => _rootRef?.Id ?? "items";

		public PlayerItemGroup(ItemRef rootRef, PlayerInventory inventory)
		{
			_rootRef = rootRef;
			_inventory = inventory;
			OnReady = Refresh(); // automatically refresh..
		}

		/// <summary>
		/// The inventory group contains some set of items based on the <see cref="ItemRef"/> that was given to the constructor.
		/// Use this method to check if some scope is part of the group. A scope that is more specific than the group scope, belongs
		/// to the group.
		/// For example, if the group scope is "items.X", then the following scopes BELONG
		/// - "items.X"
		/// - "items.X.Y",
		/// - "items.X.Y.Z"
		/// And the following scopes DO NOT BELONG
		/// - "items.Y"
		/// - "items"
		/// - "items.XY"
		/// </summary>
		/// <param name="scope"></param>
		/// <returns></returns>
		public bool IsScopePartOfGroup(string scope)
		{
			if (string.IsNullOrEmpty(scope)) return false;

			var isPrefix = scope.StartsWith(_rootRef.Id);
			if (!isPrefix) return false; // the given scope needs to be at least be a prefix-match

			var hasMore = scope.Length > _rootRef.Id.Length;
			if (!hasMore) return true; // and if its exactly equal, thats fine

			var nextIsDot = scope[_rootRef.Id.Length] == '.';
			return nextIsDot; // but if it has more characters, than the next character MUST be a new sub type
		}


		protected override async Promise PerformRefresh()
		{
			await _inventory.Refresh(RootScope);
			Notify();
		}

		public void Notify()
		{
			var data = _inventory.LocalItems.GetAll(RootScope);
			SetData(data);
		}
	}
}
