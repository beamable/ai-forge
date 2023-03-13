using UnityEngine;
using UnityEngine.UI;

namespace Beamable.UI.Scripts
{
	public class AddressableImageBehaviour : MonoBehaviour
	{
		public Image Renderer;

		public async void Refresh(string address)
		{
			Renderer.sprite = await AddressableSpriteLoader.LoadSprite(address);
		}
	}
}
