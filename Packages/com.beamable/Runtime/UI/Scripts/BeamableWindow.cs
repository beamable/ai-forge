using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Gradient = Beamable.UnityEngineClone.UI.Extensions.Gradient;

namespace Beamable.UI.Scripts
{
	public class BeamableWindow : UIBehaviour
	{
		public Image HeaderImage;
		public Gradient HeaderGradient;
		public LayoutElement HeaderElement;
		public RectTransform WindowTransform;
	}
}
