using System.Collections.Generic;

namespace Beamable.EasyFeatures
{
	/// <summary>
	/// This interface defines a simple control flow API.
	/// It simply exists to enforce a minimal API that all our Easy Features must provide in order to have their fast path available to customers.
	/// There are 2 ways customers are expected to interact with classes implementing this interface:
	/// <list type="bullet">
	/// <item>
	/// <b>Drag-Drop Test</b>: Customers are expected to simply drag and drop the prefab onto the hierarchy or open the scene.
	/// Upon entering play mode, the Feature should just work based on the configurations setup in the script implementing this interface.
	/// To achieve this, our Easy Feature prefabs and scenes should be configured by default to <see cref="RunOnEnable"/> set to true.
	/// </item>
	/// <item>
	/// <b>Integration into Game-Specific Control Flow</b>: Customers will eventually want to integrate this into their own project's control flow.
	/// To do so, they should disable the <see cref="RunOnEnable"/> flag in the prefab or scene and manually call the <see cref="Run"/> method when they
	/// want the Feature to run as it did when entering play mode.
	/// Classes implementing this interface can have a lightly granular API for feature setup so that users can re-use, but it's not the focus.
	/// This aims to be a tiny example of how to use the Feature's Game Systems and their BeamableViewGroups together.  
	/// </item>
	/// </list> 
	/// </summary>
	public interface IBeamableFeatureControl
	{
		IEnumerable<BeamableViewGroup> ManagedViewGroups { get; }

		bool RunOnEnable { get; }

		void OnEnable();

		void Run();
	}
}
