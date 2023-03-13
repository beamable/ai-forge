
using Beamable.UnityEngineClone.UI.Extensions;
using UnityEngine;
using Gradient = Beamable.UnityEngineClone.UI.Extensions.Gradient;
using GradientMode = Beamable.UnityEngineClone.UI.Extensions.GradientMode;


namespace Beamable.Theme.Objects
{
	[System.Serializable]
	public class GradientStyleObject : StyleObject<Beamable.UnityEngineClone.UI.Extensions.Gradient>
	{

		public GradientMode _gradientMode = GradientMode.Global;

		public GradientDir _gradientDir = GradientDir.Vertical;

		public bool _overwriteAllColor = false;

		public Color _vertex1 = Color.white;

		public Color _vertex2 = Color.white;

		protected override void Apply(Gradient target)
		{
			target.GradientMode = _gradientMode;
			target.GradientDir = _gradientDir;
			target.OverwriteAllColor = _overwriteAllColor;
			target.Vertex1 = _vertex1;
			target.Vertex2 = _vertex2;
		}
	}
}
