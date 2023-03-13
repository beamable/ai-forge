// unset

using UnityEngine;

namespace Beamable
{
	public static class MonoBehaviourExtensions
	{
		/// <summary>
		/// Find the first <see cref="BeamableBehaviour.Context"/> in the parent lineage of the current component, or <see cref="BeamContext.Default"/> if no <see cref="BeamableBehaviour"/> components exist
		/// </summary>
		public static BeamContext GetBeamable(this Component self)
		{
			return self.GetComponentInParent<BeamableBehaviour>()?.Context ?? BeamContext.Default;
		}
	}
}
