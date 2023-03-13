using Beamable;
using Beamable.EasyFeatures;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace EasyFeaturesIntegrationExamples.FeatureExtensionIntegration
{
	/// <summary>
	/// This is the View responsible updating the <see cref="SearchableLeaderboardPlayerSystem.CurrentAliasFilter"/> whenever a user modifies an <see cref="TMP_InputField"/>.
	/// You can create views like these and leverage PrefabVariants to add them to existing Beamable prefabs while re-using the rest of our logic. 
	/// </summary>
	public class LeaderboardSearchFieldView : MonoBehaviour, ISyncBeamableView
	{
		public BeamableViewGroup OwnerGroup;
		public TMP_InputField Filter;

		public bool IsVisible
		{
			get => gameObject.activeSelf;
			set => gameObject.SetActive(value);
		}

		public int GetEnrichOrder() => int.MaxValue;

		public void EnrichWithContext(BeamContextGroup managedPlayers)
		{
			OwnerGroup = managedPlayers.Owner;

			var currentContext = managedPlayers.GetSinglePlayerContext();

			// Since this is user code, it's fine to depend on user-declared systems directly like this.
			var searchableLeaderboard = currentContext.ServiceProvider.GetService<SearchableLeaderboardPlayerSystem>();

			// Sets the filter's text as it is defined in the system
			Filter.SetTextWithoutNotify(searchableLeaderboard.CurrentAliasFilter);

			// Setup listener to handle changes to the filter input field.
			Filter.onEndEdit.ReplaceOrAddListener(HandleFilterChanged);
		}

		public async void HandleFilterChanged(string newFilter)
		{
			// Get the context and system (assumes a single player/BeamContext)
			var currentContext = OwnerGroup.AllPlayerContexts[0];
			var searchableLeaderboard = currentContext.ServiceProvider.GetService<SearchableLeaderboardPlayerSystem>();
			searchableLeaderboard.CurrentAliasFilter = newFilter;

			// Notifies entire BeamableViewGroup to re-fetch it's data so we update the filtered list of entities.
			await OwnerGroup.Enrich();
		}
	}
}
