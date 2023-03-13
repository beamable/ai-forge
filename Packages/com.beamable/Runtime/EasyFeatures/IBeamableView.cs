using Beamable.Common;
using System;
using UnityEngine;

namespace Beamable.EasyFeatures
{
	/// <summary>
	/// This is the common interface that the <see cref="BeamableViewGroup"/> talks too when it's <see cref="BeamContext"/> are configured on start or <see cref="BeamableViewGroup.Enrich"/>
	/// (or <see cref="BeamableViewGroup.EnrichWithPlayerCodes"/>) gets called via code.
	///
	/// The underlying type should control one View and be a <see cref="MonoBehaviour"/>.
	/// In a game-specific way, this means one Scene/Prefab --- any group of Visual game objects and components that together solve the problem of "rendering my specific game's interactivity layer (UX)".
	///
	/// Every <see cref="IBeamableView"/> will have nested interface types (such as <see cref="LeaderboardView.IViewDependencies"/> that must implement <see cref="IBeamableViewDeps"/>) declaring
	/// it's dependencies. It expects these dependencies to have been registered with <see cref="BeamContextSystemAttribute"/> and <see cref="Beamable.Common.Dependencies.RegisterBeamableDependenciesAttribute"/>.
	/// In your user code, you can ignore this part and not declare any <see cref="IBeamableViewDeps"/>. Instead, you can depend directly on your game-specific types if you wish so, making this process simpler.
	///
	/// To enable customization of the <see cref="IBeamableView"/>s that ship with Beamable, we declare these interfaces as we believe there's value provided by their existence.
	/// Our future marketplace users may wish to do so too.
	/// </summary>
	public interface IBeamableView
	{
		/// <summary>
		/// Defines if the view is visible in the scene.
		/// </summary>
		bool IsVisible { get; set; }

		/// <summary>
		/// <see cref="BeamableViewGroup"/> sorts it's managed <see cref="IBeamableView"/> by this value before calling Enrich.
		/// </summary>
		int GetEnrichOrder();
	}

	/// <summary>
	/// Implement this interface if your enrich method can be synchronous OR a fire-and-forget (async void) method.
	/// </summary>
	public interface ISyncBeamableView : IBeamableView
	{
		/// <summary>
		/// Gets called by <see cref="BeamableViewGroup"/> on start or <see cref="BeamableViewGroup.Enrich"/> (or <see cref="BeamableViewGroup.EnrichWithPlayerCodes"/>) gets called via code.
		/// This version is called once per <see cref="BeamableViewGroup.AllPlayerContexts"/> when the <see cref="SupportedMode"/> is set to <see cref="BeamableViewGroup.PlayerCountMode.MultiplayerUI"/>.
		/// If you don't explicitly support <see cref="BeamableViewGroup.PlayerCountMode.MultiplayerUI"/>, throw a <see cref="NotSupportedException"/> from this implementation.
		/// </summary>
		/// <param name="currentContext">The <see cref="BeamContext"/> at the current <paramref name="playerIndex"/>.</param>
		/// <param name="playerIndex">The index for this <see cref="BeamContext"/> in <see cref="BeamableViewGroup.AllPlayerContexts"/>.</param>
		void EnrichWithContext(BeamContextGroup managedPlayers);
	}

	/// <summary>
	/// Implement this interface if your enrich should be awaited on before releasing control to the next <see cref="IBeamableView"/> in <see cref="BeamableViewGroup.ManagedViews"/>.
	/// </summary>
	public interface IAsyncBeamableView : IBeamableView
	{
		/// <summary>
		/// Gets called by <see cref="BeamableViewGroup"/> on start or <see cref="BeamableViewGroup.Enrich"/> (or <see cref="BeamableViewGroup.EnrichWithPlayerCodes"/>) gets called via code.
		/// This version is called once per <see cref="BeamableViewGroup.AllPlayerContexts"/> when the <see cref="SupportedMode"/> is set to <see cref="BeamableViewGroup.PlayerCountMode.MultiplayerUI"/>.
		/// If you don't explicitly support <see cref="BeamableViewGroup.PlayerCountMode.MultiplayerUI"/>, throw a <see cref="NotSupportedException"/> from this implementation.
		/// </summary>
		/// <param name="managedPlayers">The <see cref="BeamContext"/> at the current <paramref name="mainPlayerIndex"/>.</param>
		/// <param name="mainPlayerIndex">The index for this <see cref="BeamContext"/> in <see cref="BeamableViewGroup.AllPlayerContexts"/>.</param>
		Promise EnrichWithContext(BeamContextGroup managedPlayers);
	}

	/// <summary>
	/// This is just a simple tag interface that all views must declare.
	/// These are what the views should use <see cref="BeamContext.ServiceProvider"/> to get.
	/// By doing this, you can easily swap out implementations via <see cref="BeamContextSystemAttribute"/>s and <see cref="Beamable.Common.Dependencies.RegisterBeamableDependenciesAttribute"/>,
	/// adding data to existing systems keeping the UI.
	/// </summary>
	public interface IBeamableViewDeps { }
}
