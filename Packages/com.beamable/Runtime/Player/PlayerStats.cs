using Beamable.Api;
using Beamable.Api.Stats;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Content;
using Beamable.Common.Dependencies;
using Beamable.Common.Player;
using Beamable.Coroutines;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Player
{
	/// <summary>
	/// A <see cref="PlayerStat"/> is a named value associated with a player.
	/// </summary>
	[Serializable]
	public class PlayerStat
	{

		/// <summary>
		/// The unique stat key. The stat key is only unique per stat domain.
		/// While it is possible for multiple stat domains to have the same key, it is not recommended.
		/// For example, there can be a stat domain for "client.public" stats, and a stat domain for "client.private", and each domain can
		/// have a stat key called "Alias". 
		/// </summary>
		public string Key { get; }

		/// <summary>
		/// The <see cref="PlayerStats"/> that this stat was created from.
		/// </summary>
		public PlayerStats Group { get; }

		/// <summary>
		/// The value of the stat
		/// </summary>
		public string Value { get; }

		public PlayerStat(string key, string value, PlayerStats group)
		{
			Key = key;
			Value = value;
			Group = group;
		}

		public static implicit operator string(PlayerStat self) => self.Value;

		/// <summary>
		/// Set the value of the stat for the current player.
		/// If multiple calls to this method are issued per frame, all set operations will be baked into one network operation.
		/// </summary>
		/// <param name="nextValue">
		/// The desired stat value. This will override the existing value.
		/// </param>
		/// <returns>
		/// A <see cref="Promise"/> representing the work. The promise won't complete until the stat value has been successfully set.
		/// </returns>
		public Promise Set(string nextValue) => Group.Set(Key, nextValue);

		public override string ToString() => Value;

		#region auto generated equality members

		protected bool Equals(PlayerStat other)
		{
			return Key == other.Key && Value == other.Value;
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

			return Equals((PlayerStat)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Key != null ? Key.GetHashCode() : 0) * 397) ^ (Value != null ? Value.GetHashCode() : 0);
			}
		}
		#endregion

	}

	[Serializable]
	public class SerializableDictionaryStringToPlayerStat : SerializableDictionaryStringToSomething<PlayerStat> { }


	[Serializable]
	public class PlayerStats : AbsObservableReadonlyDictionary<PlayerStat, SerializableDictionaryStringToPlayerStat>, IBeamableDisposable
	{
		private readonly IPlatformService _platform;
		private readonly IUserContext _userContext;
		private readonly StatsService _statService;
		private readonly ISdkEventService _eventService;
		private readonly CoroutineService _coroutineService;

		private readonly SdkEventConsumer _consumer;
		private readonly Coroutine _updateRoutine;

		private Promise _commitSync = new Promise();
		private Dictionary<string, string> _pendingUpdates = new Dictionary<string, string>();


		public PlayerStats(IPlatformService platform, IUserContext userContext, StatsService statService, ISdkEventService eventService, CoroutineService coroutineService)
		{
			_platform = platform;
			_userContext = userContext;
			_statService = statService;
			_eventService = eventService;
			_coroutineService = coroutineService;

			_updateRoutine = _coroutineService.StartNew("playerStatLoop", Update());
			_consumer = _eventService.Register(nameof(PlayerStats), HandleEvent);

			var _ = Refresh(); // automatically start.
			IsInitialized = true;
		}

		private IEnumerator Update()
		{
			while (true)
			{
				yield return null;
				if (_pendingUpdates.Count > 0)
				{
					_eventService.Add(new SdkEvent(nameof(PlayerStats), "commit"));
				}
			}
		}

		private async Promise HandleEvent(SdkEvent evt)
		{
			switch (evt.Event)
			{
				case "set":
					_pendingUpdates[evt.Args[0]] = evt.Args[1];
					await _commitSync;
					break;

				case "commit":
					if (_pendingUpdates.Count == 0) return;

					var network = _statService.SetStats("public", _pendingUpdates);
					_pendingUpdates.Clear();
					await network;
					await Refresh();
					_commitSync?.CompleteSuccess();
					_commitSync = new Promise();

					break;
			}
		}

		protected override async Promise PerformRefresh()
		{
			await _platform.OnReady;

			var stats = await _statService.GetStats("client", "public", "player", _userContext.UserId);

			var nextData = new SerializableDictionaryStringToPlayerStat();
			foreach (var kvp in stats)
			{
				nextData.Add(kvp.Key, new PlayerStat(kvp.Key, kvp.Value, this));
			}

			SetData(nextData);
		}

		/// <summary>
		/// Set the value of a <see cref="PlayerStat"/>.
		/// If multiple calls to this method are issued per frame, all set operations will be baked into one network operation.
		/// </summary>
		/// <param name="key">The stat key</param>
		/// <param name="value">The desired stat value. This will override the existing value.</param>
		/// <returns>
		/// A <see cref="Promise"/> representing the work. The promise won't complete until the set is complete.
		/// </returns>
		public Promise Set(string key, string value)
		{
			return _eventService.Add(new SdkEvent(nameof(PlayerStats), "set", key, value));
		}

		public Promise OnDispose()
		{
			_coroutineService.StopCoroutine(_updateRoutine);
			_eventService.Unregister(_consumer);
			return Promise.Success;
		}
	}
}
