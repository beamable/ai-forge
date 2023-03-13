using UnityEngine;
using UnityEngine.UI;

namespace Beamable.UI.Scripts
{
	public class ScrollButton : MonoBehaviour
	{
		public ScrollRect ScrollRect;

		public void Scroll(float amount)
		{
			if (ScrollRect.horizontal)
			{
				ScrollRect.velocity = Vector2.right * amount;
			}
			else
			{
				ScrollRect.velocity = Vector2.down * amount;
			}
		}

	}
}
