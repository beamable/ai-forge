using Beamable.Common.Api;
using Beamable.Common.Dependencies;
using Beamable.ConsoleCommands;
using UnityEngine;
using UnityEngine.Scripting;

namespace Beamable.Api.Tournaments
{
	/// <summary>
	/// This type defines the %Tournament feature's console commands.
	///
	/// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
	///
	/// #### Related Links
	/// - See the <a target="_blank" href="https://docs.beamable.com/docs/tournaments-feature">Tournaments</a> feature documentation
	/// - See Beamable.Api.Tournaments.TournamentService script reference
	///
	/// ![img beamable-logo]
	///
	/// </summary>
	[BeamableConsoleCommandProvider]
	public class TournamentConsoleCommands
	{
		private readonly IDependencyProvider _provider;
		private BeamableConsole Console => _provider.GetService<BeamableConsole>();
		private TournamentService Tournaments => _provider.GetService<TournamentService>();
		private IBeamableRequester Requester => _provider.GetService<IBeamableRequester>();

		[Preserve]
		public TournamentConsoleCommands(IDependencyProvider provider)
		{
			_provider = provider;
		}

		[BeamableConsoleCommand("TOURNAMENT-JOIN", "Join a tournament cycle by id.", "TOURNAMENT-JOIN <tournament-id>")]
		protected string JoinTournament(params string[] args)
		{
			if (args.Length == 1)
			{
				string tournamentId = args[0];

				Tournaments.JoinTournament(tournamentId).Then(response =>
				{
					Debug.Log($"Tournament {response.tournamentId} Joined.");
				});

				return $"Joining tournament cycle {tournamentId}...";
			}
			else
			{
				return "Please provide a fully qualified tournament id (e.g. 'tournaments.sample.1')";
			}
		}

		[BeamableConsoleCommand("TOURNAMENT-END", "End a tournament cycle by id. (Admin Only)", "TOURNAMENT-END <tournament-id>")]
		protected string ChangeCycle(params string[] args)
		{
			if (args.Length == 1)
			{
				string tournamentId = args[0];
				Requester.Request<EmptyResponse>(
					Method.PUT, $"/object/tournaments/{tournamentId}/internal/cycle", new TournamentEndCycleRequest(tournamentId)
				).Then(response =>
				{
					Debug.Log($"Tournament {tournamentId} Ended.");
				});

				return $"Ending tournament cycle {tournamentId}...";
			}
			else
			{
				return "Please provide a fully qualified tournament id (e.g. 'tournaments.sample.1')";
			}
		}
	}

	[System.Serializable]
	class TournamentEndCycleRequest
	{
		public string tournamentId;

		public TournamentEndCycleRequest(string tournamentId)
		{
			this.tournamentId = tournamentId;
		}
	}
}
