using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Beamable.Theme.Appliers
{

	[System.Serializable]
	public abstract class StyleApplier<TComponent>
	   where TComponent : UIBehaviour
	{
		// this state exists because all appliers _likely_ have a set of components to apply styles onto.
		// _probably_, we will write nice custom editors that make use of this information...
		public List<TComponent> Components = new List<TComponent>();

		public virtual void ApplyAll(ThemeObject theme)
		{
			Components.ForEach(c =>
			{
				if (c != null) Apply(theme, c);
			});
		}

		public abstract void Apply(ThemeObject theme, TComponent component);



	}
}
