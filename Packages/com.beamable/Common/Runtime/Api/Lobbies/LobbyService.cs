using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Content;
using System.Collections.Generic;

namespace Beamable.Experimental.Api.Lobbies
{
	// TODO: This needs to be implemented for "Real"
	public class LobbyService : ILobbyApi
	{
		private readonly IBeamableRequester _requester;
		private readonly IUserContext _userContext;

		public LobbyService(IBeamableRequester requester, IUserContext userContext)
		{
			_requester = requester;
			_userContext = userContext;
		}

		/// <inheritdoc cref="ILobbyApi.FindLobbies"/>
		public Promise<LobbyQueryResponse> FindLobbies()
		{
			return _requester.Request<LobbyQueryResponse>(
				Method.GET,
				$"/lobbies"
			);
		}

		/// <inheritdoc cref="ILobbyApi.FindLobbiesOfType"/>
		public Promise<LobbyQueryResponse> FindLobbiesOfType(string matchType, int limit = 100, int skip = 0)
		{
			return _requester.Request<LobbyQueryResponse>(
				Method.GET,
				$"/lobbies",
				new LobbyQueryRequest(skip, limit, matchType)
			);
		}

		/// <inheritdoc cref="ILobbyApi.CreateLobby"/>
		public Promise<Lobby> CreateLobby(string name,
										  LobbyRestriction restriction,
										  string gameTypeId = null,
										  string description = null,
										  List<Tag> playerTags = null,
										  int? maxPlayers = null,
										  int? passcodeLength = null,
										  List<string> statsToInclude = null)
		{
			return _requester.Request<Lobby>(
				Method.POST,
				$"/lobbies",
				new CreateLobbyRequest(
					name,
					description,
					restriction.ToString(),
					gameTypeId,
					playerTags,
					maxPlayers,
					passcodeLength)
			);
		}

		/// <inheritdoc cref="ILobbyApi.CreateLobby"/>
		public Promise<Lobby> CreateLobby(string name,
										  LobbyRestriction restriction,
										  SimGameTypeRef gameTypeRef = null,
										  string description = null,
										  List<Tag> playerTags = null,
										  int? maxPlayers = null,
										  int? passcodeLength = null,
										  List<string> statsToInclude = null)
		{
			return _requester.Request<Lobby>(
				Method.POST,
				$"/lobbies",
				new CreateLobbyRequest(
					name,
					description,
					restriction.ToString(),
					gameTypeRef?.Id,
					playerTags,
					maxPlayers,
					passcodeLength)
			);
		}

		/// <inheritdoc cref="ILobbyApi.JoinLobby"/>
		public Promise<Lobby> JoinLobby(string lobbyId, List<Tag> playerTags = null)
		{
			return _requester.Request<Lobby>(
				Method.PUT,
				$"/lobbies/{lobbyId}",
				new JoinLobbyRequest(playerTags)
			);
		}

		/// <inheritdoc cref="ILobbyApi.JoinLobbyByPasscode"/>
		public Promise<Lobby> JoinLobbyByPasscode(string passcode, List<Tag> playerTags = null)
		{
			return _requester.Request<Lobby>(
				Method.PUT,
				$"/lobbies/passcode",
				new JoinByPasscodeRequest(passcode, playerTags)
			);
		}

		/// <inheritdoc cref="ILobbyApi.GetLobby"/>
		public Promise<Lobby> GetLobby(string lobbyId)
		{
			return _requester.Request<Lobby>(
				Method.GET,
				$"/lobbies/{lobbyId}"
			);
		}

		/// <inheritdoc cref="ILobbyApi.LeaveLobby"/>
		public Promise LeaveLobby(string lobbyId)
		{
			return _requester.Request<Unit>(
				Method.DELETE,
				$"/lobbies/{lobbyId}",
				new RemoveFromLobbyRequest(_userContext.UserId.ToString())
			).ToPromise();
		}

		/// <inheritdoc cref="ILobbyApi.AddPlayerTags"/>
		public Promise<Lobby> AddPlayerTags(string lobbyId,
											List<Tag> tags,
											string playerId = null,
											bool replace = false)
		{
			playerId = playerId ?? _userContext.UserId.ToString();
			return _requester.Request<Lobby>(
				Method.PUT,
				$"/lobbies/{lobbyId}/tags",
				new AddTagsRequest(playerId, tags, replace)
			);
		}

		/// <inheritdoc cref="ILobbyApi.RemovePlayerTags"/>
		public Promise<Lobby> RemovePlayerTags(string lobbyId, List<string> tags, string playerId = null)
		{
			playerId = playerId ?? _userContext.UserId.ToString();
			return _requester.Request<Lobby>(
				Method.PUT,
				$"/lobbies/{lobbyId}/tags",
				new RemoveTagsRequest(playerId, tags)
			);
		}

		/// <inheritdoc cref="ILobbyApi.KickPlayer"/>
		public Promise<Lobby> KickPlayer(string lobbyId, string playerId)
		{
			return _requester.Request<Lobby>(
				Method.DELETE,
				$"/lobbies/{lobbyId}",
				new RemoveFromLobbyRequest(playerId)
			);
		}

		/// <inheritdoc cref="ILobbyApi.UpdateLobby"/>
		public Promise<Lobby> UpdateLobby(string lobbyId,
									 LobbyRestriction restriction,
									 string newHost,
									 string name = null,
									 string description = null,
									 string gameType = null,
									 int? maxPlayers = null)
		{
			return _requester.Request<Lobby>(
				Method.PUT,
				$"/lobbies/{lobbyId}/metadata",
				new UpdateLobbyRequest(name, description, restriction.ToString(), newHost, gameType, maxPlayers)
			);
		}
	}
}
