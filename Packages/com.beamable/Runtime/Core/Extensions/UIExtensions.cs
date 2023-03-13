using UnityEngine;

namespace Beamable.Extensions
{
	public static class UIExtensions
	{
		////////////////////////////////////////////////////////////
		// CanvasGroup Extension Methods
		////////////////////////////////////////////////////////////

		public static void SetVisible(this CanvasGroup group, bool visible)
		{
			if (visible)
			{
				group.alpha = 1;
				group.interactable = true;
				group.blocksRaycasts = true;
			}
			else
			{
				group.alpha = 0;
				group.interactable = false;
				group.blocksRaycasts = false;
			}
		}
	}
}
