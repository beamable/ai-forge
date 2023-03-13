using Beamable.Experimental.Api.Lobbies;
using Beamable.Player;
using System.Linq;

namespace Beamable.EasyFeatures.BasicLobby
{
	public static class LobbyExtensions
	{
		public static string TAG_PLAYER_READY = "playerReady";

		public static bool IsReady(this LobbyPlayer player)
		{
			return player.tags.Any(tag => tag.name == TAG_PLAYER_READY && tag.value == bool.TrueString.ToLower());
		}

		public static LobbyPlayer GetCurrentPlayer(this PlayerLobby lobby, string playerCode)
		{
			return lobby.Players.Find(player => player.playerId == playerCode);
		}
	}
}
