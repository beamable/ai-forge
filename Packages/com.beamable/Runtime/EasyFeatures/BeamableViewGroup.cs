using Beamable;
using Beamable.Common;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Playables;

namespace Beamable.EasyFeatures
{
	/// <summary>
	/// This is a Beamable Component you can add to any <see cref="GameObject"/> you have to gain access to it's API that:
	/// <list type="bullet">
	/// <item>
	/// Can be used to manage which <see cref="BeamContext"/>s are being used to get the data that will populate this view (which player's data should I be displaying?)
	/// </item>
	/// <item>
	/// Can be used to rebuild and modify the UI dynamically as new players are added and removed.
	/// Think about a Party UI in couch co-op games --- it takes X data from each player and displays it as player's "press start to join".
	/// </item>
	/// </list>
	///
	/// This is meant to provide additive behaviour and work with other <see cref="MonoBehaviour"/> scripts that implement <see cref="IBeamableView"/> in the same <see cref="GameObject"/> hierarchy.
	/// </summary>
	public class BeamableViewGroup : MonoBehaviour
	{
		/// <summary>
		/// List of all <see cref="BeamContext"/>s currently being used by this <see cref="BeamableViewGroup"/> to get it's dependencies from.
		/// </summary>
		public BeamContextGroup AllPlayerContexts;

		/// <summary>
		/// List of all <see cref="BeamContext.PlayerCode"/> that are used to identify <see cref="BeamContext"/>s within Beamable's player-centric SDK.
		/// </summary>
		public List<string> AllPlayerCodes;

		/// <summary>
		/// List of all <see cref="IBeamableView"/>s that exist as children of the <see cref="GameObject"/> holding this <see cref="BeamableViewGroup"/>.
		/// If you add/remove <see cref="IBeamableView"/> components from this hierarchy, call <see cref="RebuildManagedViews"/> and then <see cref="Enrich"/> to make sure each <see cref="IBeamableView"/>
		/// sees those changes.
		/// You can also simply add <see cref="IBeamableView"/> views to this list and call <see cref="Enrich"/>. The <see cref="Enrich"/> call will see the newly appended views and enrich them accordingly.
		/// </summary>
		public List<IBeamableView> ManagedViews;

		/// <summary>
		/// Rebuilds the list of managed <see cref="IBeamableView"/>s in this <see cref="BeamableViewGroup"/>'s <see cref="GameObject"/> hierarchy.
		/// In cases where you want to have your <see cref="IBeamableView"/>s be independent of the <see cref="GameObject"/> hierarchy,
		/// you can pass in a list of <see cref="IBeamableView"/>s to append the views controlled by this group.
		/// </summary>
		public void RebuildManagedViews(IEnumerable<IBeamableView> otherViews = null)
		{
			ManagedViews = GetComponentsInChildren(typeof(IBeamableView), true)
						   .Cast<IBeamableView>()
						   .ToList();

			if (otherViews != null)
				ManagedViews.AddRange(otherViews);
		}

		/// <summary>
		/// <see cref="Enrich"/> call that returns void so it can be configured to happen via <see cref="UnityEngine.Events.UnityAction"/>s. 
		/// </summary>
		public virtual async void TriggerEnrich() => await Enrich();

		/// <summary>
		/// Ensures that the <see cref="AllPlayerContexts"/> match the currently set <see cref="AllPlayerCodes"/> and that they are <see cref="BeamContext.OnReady"/>.
		/// Then, goes through all <see cref="ManagedViews"/> and calls either <see cref="IBeamableView.EnrichWithContext(Beamable.BeamContext)"/> or
		/// <see cref="IBeamableView.EnrichWithContext(Beamable.BeamContext, int)"/> based on <see cref="IBeamableView.SupportedMode"/>.
		/// </summary>
		public virtual async Promise Enrich()
		{
			ManagedViews.Sort((v1, v2) => v1.GetEnrichOrder().CompareTo(v2.GetEnrichOrder()));

			// For every view we have, call their appropriate EnrichWithContext function based on their supported mode.  
			foreach (var beamableUIView in ManagedViews)
			{
				switch (beamableUIView)
				{
					case ISyncBeamableView syncBeamableView:
						syncBeamableView.EnrichWithContext(AllPlayerContexts);
						break;
					case IAsyncBeamableView asyncBeamableView:
						await asyncBeamableView.EnrichWithContext(AllPlayerContexts);
						break;
				}
			}
		}

		/// <summary>
		/// Works the same as <see cref="Enrich"/>, but calls and awaits <see cref="RebuildPlayerContexts"/> before enriching.
		/// </summary>
		/// <param name="newPlayerCodes"></param>
		public async Promise EnrichWithPlayerCodes(List<string> newPlayerCodes = null)
		{
			// Rebuild the Player Contexts --- will do nothing if  newPlayerCodes is null or empty.
			await RebuildPlayerContexts(newPlayerCodes);
			await Enrich();
		}

		/// <summary>
		/// Rebuilds the <see cref="AllPlayerCodes"/> and <see cref="AllPlayerContexts"/> codes based on the given <paramref name="playerCodes"/>.
		/// </summary>
		/// <param name="playerCodes">New <see cref="BeamContext.PlayerCode"/> representing the <see cref="BeamContext"/> that this View should get it's data from.</param>
		public async Promise RebuildPlayerContexts(List<string> playerCodes)
		{
			AllPlayerCodes = playerCodes == null || playerCodes.Count == 0 ? AllPlayerCodes : playerCodes;
			AllPlayerContexts = new BeamContextGroup(AllPlayerCodes.Select(playerCode => string.IsNullOrEmpty(playerCode) ? BeamContext.Default : BeamContext.ForPlayer(playerCode)), this);

			foreach (var allPlayerContext in AllPlayerContexts)
				await allPlayerContext.OnReady;
		}
	}

	public class BeamContextGroup : List<BeamContext>
	{
		public BeamContextGroup([NotNull] IEnumerable<BeamContext> collection, BeamableViewGroup owner) : base(collection)
		{
			Owner = owner;
		}

		public BeamContext GetSinglePlayerContext() => this[0];

		public BeamableViewGroup Owner { get; }
	}
}
