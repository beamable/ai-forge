using Beamable.Common;
using Beamable.Common.Api.Notifications;
using Beamable.Common.Content;
using Beamable.Common.Player;
using Beamable.Experimental.Api.Lobbies;
using Beamable.Serialization.SmallerJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Player
{
	/// <summary>
	/// Experimental API around managing a player's lobby state.
	/// </summary>
	[Serializable]
	public class PlayerLobby : Observable<Lobby>, IDisposable
	{
		public enum LobbyEvent
		{
			LobbyCreated,
			LobbyDisbanded,
			DataChanged,
			PlayerJoined,
			PlayerLeft,
			PlayerKicked,
			HostPlayerChanged,
			None
		}

		public event Action<Exception> OnExceptionThrown;

		private readonly ILobbyApi _lobbyApi;
		private readonly INotificationService _notificationService;

		public ObservableChangeEvent<LobbyEvent, string> ChangeData;

		public PlayerLobby(ILobbyApi lobbyApi, INotificationService notificationService)
		{
			_lobbyApi = lobbyApi;
			_notificationService = notificationService;
		}

		private static string UpdateName(string lobbyId) => $"lobbies.update.{lobbyId}";

		public override Lobby Value
		{
			get => base.Value;
			set
			{
				if (base.Value != null)
				{
					_notificationService.Unsubscribe(UpdateName(base.Value.lobbyId), OnRawUpdate);
				}

				if (value != null)
				{
					_notificationService.Subscribe(UpdateName(value.lobbyId), OnRawUpdate);
				}

				base.Value = value;
			}
		}

		/// <summary>
		/// Checks if the player is in a lobby.
		/// </summary>
		public bool IsInLobby => Value != null;

		/// <inheritdoc cref="Lobby.lobbyId"/>
		/// <para>This references the data in the <see cref="State"/> field, which is the player's current lobby.</para>
		public string Id => SafeAccess(Value?.lobbyId);

		/// <inheritdoc cref="Lobby.name"/>
		/// <para>This references the data in the <see cref="State"/> field, which is the player's current lobby.</para>
		public string Name => SafeAccess(Value?.name);

		/// <inheritdoc cref="Lobby.description"/>
		/// <para>This references the data in the <see cref="State"/> field, which is the player's current lobby.</para>
		public string Description => SafeAccess(Value?.description);

		/// <inheritdoc cref="Lobby.Restriction"/>
		/// <para>This references the data in the <see cref="State"/> field, which is the player's current lobby.</para>
		public LobbyRestriction Restriction => SafeAccess(Value.Restriction);

		/// <inheritdoc cref="Lobby.host"/>
		/// <para>This references the data in the <see cref="State"/> field, which is the player's current lobby.</para>
		public string Host => SafeAccess(Value?.host);

		/// <inheritdoc cref="Lobby.players"/>
		/// <para>This references the data in the <see cref="State"/> field, which is the player's current lobby.</para>
		public List<LobbyPlayer> Players => SafeAccess(Value?.players);

		/// <inheritdoc cref="Lobby.passcode"/>
		/// <para>This references the data in the <see cref="State"/> field, which is the player's current lobby.</para>
		public string Passcode => SafeAccess(Value?.passcode);

		/// <inheritdoc cref="Lobby.maxPlayers"/>
		/// <para>This references the data in the <see cref="State"/> field, which is the player's current lobby.</para>
		public int MaxPlayers => SafeAccess(Value.maxPlayers);

		private T SafeAccess<T>(T value)
		{
			if (!IsInLobby)
			{
				throw new NotInLobby();
			}

			return value;
		}

		/// <inheritdoc cref="ILobbyApi.FindLobbies"/>
		public Promise<LobbyQueryResponse> FindLobbies()
		{
			return _lobbyApi.FindLobbies();
		}

		/// <inheritdoc cref="ILobbyApi.FindLobbiesOfType"/>
		public Promise<LobbyQueryResponse> FindLobbiesOfType(string matchType, int limit = 100, int skip = 0)
		{
			return _lobbyApi.FindLobbiesOfType(matchType, limit, skip);
		}

		/// <inheritdoc cref="ILobbyApi.CreateLobby"/>
		public async Promise Create(string name,
									LobbyRestriction restriction,
									string gameTypeId,
									string description = null,
									List<Tag> playerTags = null,
									int? maxPlayers = null,
									int? passcodeLength = null,
									List<string> statsToInclude = null)
		{
			Value = await _lobbyApi.CreateLobby(
				name,
				restriction,
				gameTypeId,
				description,
				playerTags,
				maxPlayers,
				passcodeLength,
				statsToInclude);
		}

		/// <inheritdoc cref="ILobbyApi.CreateLobby"/>
		public async Promise Create(string name,
									LobbyRestriction restriction,
									SimGameTypeRef gameTypeRef = null,
									string description = null,
									List<Tag> playerTags = null,
									int? maxPlayers = null,
									int? passcodeLength = null,
									List<string> statsToInclude = null)
		{
			Value = await _lobbyApi.CreateLobby(
				name,
				restriction,
				gameTypeRef,
				description,
				playerTags,
				maxPlayers,
				passcodeLength,
				statsToInclude);
		}

		/// <inheritdoc cref="ILobbyApi.UpdateLobby"/>
		public async Promise Update(string lobbyId,
									LobbyRestriction restriction,
									string newHost,
									string name = null,
									string description = null,
									string gameType = null,
									int? maxPlayers = null)
		{
			Value = await _lobbyApi.UpdateLobby(lobbyId,
												restriction,
												newHost,
												name,
												description,
												gameType,
												maxPlayers);
		}

		/// <inheritdoc cref="ILobbyApi.JoinLobby"/>
		public async Promise Join(string lobbyId, List<Tag> playerTags = null)
		{
			Value = await _lobbyApi.JoinLobby(lobbyId, playerTags);
		}

		/// <inheritdoc cref="ILobbyApi.JoinLobbyByPasscode"/>
		public async Promise JoinByPasscode(string passcode, List<Tag> playerTags = null)
		{
			Value = await _lobbyApi.JoinLobbyByPasscode(passcode, playerTags);
		}

		/// <inheritdoc cref="ILobbyApi.AddPlayerTags"/>
		public async Promise AddTags(List<Tag> tags, bool replace = false)
		{
			Value = await _lobbyApi.AddPlayerTags(Value.lobbyId, tags, replace: replace);
		}

		/// <inheritdoc cref="ILobbyApi.RemovePlayerTags"/>
		public async Promise RemoveTags(List<string> tags)
		{
			Value = await _lobbyApi.RemovePlayerTags(Value.lobbyId, tags);
		}

		public async Promise KickPlayer(string playerId)
		{
			Value = await _lobbyApi.KickPlayer(Value.lobbyId, playerId);
		}

		/// <summary>
		/// Leave the lobby if the player is in a lobby.
		/// </summary>
		public async Promise Leave()
		{
			if (Value == null)
			{
				return;
			}

			try
			{
				await _lobbyApi.LeaveLobby(Value.lobbyId);
			}
			finally
			{
				Value = null;
			}
		}

		protected override async Promise PerformRefresh()
		{
			if (Value == null) return; // nothing to do.

			try
			{
				Value = await _lobbyApi.GetLobby(Value.lobbyId);
			}
			catch (Exception e)
			{
				Debug.Log(e.Message);
				Value = null;
				OnExceptionThrown?.Invoke(e);
			}
		}

		public void Dispose()
		{
			Value = null;
		}

		private void OnRawUpdate(object message)
		{
			ChangeData = ParseEvent(message);
			var _ = Refresh();
		}

		protected override void ResetChangeData()
		{
			ChangeData = new ObservableChangeEvent<LobbyEvent, string> { Event = LobbyEvent.None, Data = String.Empty };
		}

		private ObservableChangeEvent<LobbyEvent, string> ParseEvent(object message)
		{
			ObservableChangeEvent<LobbyEvent, string> changeEvent = new ObservableChangeEvent<LobbyEvent, string>();

			if (message is ArrayDict arrayDict)
			{
				if (arrayDict.TryGetValue("event", out object eventData))
				{
					string value = (string)eventData;
					changeEvent.Event = Enum.TryParse(value, true, out LobbyEvent lobbyEvent)
						? lobbyEvent
						: LobbyEvent.None;
				}

				if (arrayDict.TryGetValue("playerId", out object playerId))
				{
					string value = (string)playerId;
					changeEvent.Data = value;
				}
			}

			return changeEvent;
		}
	}
}
