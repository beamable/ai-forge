using Beamable;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Leaderboards;
using Beamable.Common.Dependencies;
using Beamable.Common.Leaderboards;
using Beamable.EasyFeatures;
using Beamable.Modules.Generics;
using Beamable.Modules.Leaderboards;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Beamable.EasyFeatures.BasicLeaderboard
{
	/// <summary>
	/// This is an example <see cref="IBeamableView"/>. It is based off the <see cref="LeaderboardsPresenter"/>.
	/// </summary>
	public class BasicLeaderboardView : MonoBehaviour, ISyncBeamableView
	{
		public interface ILeaderboardDeps : IBeamableViewDeps
		{
			IEnumerable<BasicLeaderboardViewEntry> Entries { get; }
			IReadOnlyList<long> Ranks { get; }
			IReadOnlyList<double> Scores { get; }
			IReadOnlyList<string> Aliases { get; }
			IReadOnlyList<Sprite> Avatars { get; }

			int PlayerIndexInLeaderboard { get; }
			string PlayerAlias { get; }
			long PlayerRank { get; }
			double PlayerScore { get; }
			Sprite PlayerAvatar { get; }
		}

		[Header("View Configuration")]
		public int EnrichOrder;

		[Header("UI Components")]
		public GenericButton BackButton;

		public GenericButton TopButton;
		public LeaderboardsRankEntriesPresenter RankEntries;
		public LeaderboardsRankEntryPresenter CurrentUserRankEntry;

		[Header("Exposed Events"), Space] public UnityEvent BackButtonAction;
		public UnityEvent TopButtonAction;

		public bool IsVisible
		{
			get => gameObject.activeSelf;
			set => gameObject.SetActive(value);
		}

		public virtual int GetEnrichOrder() => EnrichOrder;

		public virtual void EnrichWithContext(BeamContextGroup managedPlayers)
		{
			var currentContext = managedPlayers.GetSinglePlayerContext();
			var system = currentContext.ServiceProvider.GetService<ILeaderboardDeps>();

			// If there are no players in this leaderboard yet --- simply finish loading.
			if (system.Ranks.Count == 0)
			{
				CurrentUserRankEntry.LoadingIndicator.Toggle(false);
				RankEntries.LoadingIndicator.Toggle(false);
				return;
			}

			Debug.Log($"Player Id: {currentContext.PlayerId} => Rank: {system.PlayerRank} / Score: {system.PlayerScore} ");

			if (BackButton != null)
			{
				BackButton.onClick.RemoveAllListeners();
				BackButton.onClick.AddListener(() => BackButtonAction.Invoke());
			}

			if (TopButton != null)
			{
				TopButton.onClick.RemoveAllListeners();
				TopButton.onClick.AddListener(() => TopButtonAction.Invoke());
			}

			if (CurrentUserRankEntry != null)
			{
				CurrentUserRankEntry.LoadingIndicator.Toggle(true);
				RankEntries.ClearPooledRankedEntries();

				RankEntries.Enrich(system.Entries, system.PlayerRank);
				RankEntries.RebuildPooledRankEntries();

				CurrentUserRankEntry.Enrich(system.PlayerAlias, system.PlayerAvatar, system.PlayerRank, system.PlayerScore, system.PlayerRank);
				CurrentUserRankEntry.RebuildRankEntry();
			}
		}

		public virtual void EnterLoadingState()
		{
			if (CurrentUserRankEntry != null) CurrentUserRankEntry.LoadingIndicator.Toggle(true);
			RankEntries.ClearPooledRankedEntries();
		}

		public struct BasicLeaderboardViewEntry
		{
			public string Alias;
			public long Rank;
			public double Score;
			public Sprite Avatar;
		}
	}
}
