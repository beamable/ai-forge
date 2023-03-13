using Beamable.Service;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// HidePoolService manages HidePools so users can control their lifecycle while still accessing the HidePool statically.
// Ultimately the better solution would be to have HidePools explicitly placed in scenes and owned entirely by their
// users, thus removing the need for HidePoolService entirely.  This, however, would require much larger refactors of
// AudioSourceManager and FXParticleConfig.  It is still a worthy long-range effort, but for now HidePoolService fills
// the gap.
namespace Beamable.Pooling
{
	[EditorServiceResolver(typeof(EditorSingletonServiceResolver<HidePoolService>))]
	public class HidePoolService
	{
		private Dictionary<string, HidePool> _hidePools = new Dictionary<string, HidePool>();

		public HidePool GetHidePool(string context, string sceneName = null, bool forceRecreate = false)
		{
			HidePool hidePool = null;
			if (_hidePools.TryGetValue(context, out hidePool) && (hidePool != null))
			{
				if (forceRecreate)
				{
					GameObject.Destroy(hidePool.gameObject);
				}
				else
				{
					return hidePool;
				}
			}

			if (sceneName != null)
			{
				var originalScene = SceneManager.GetActiveScene();
				SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
				hidePool = CreateHidePool(context);
				SceneManager.SetActiveScene(originalScene);
			}
			else
			{
				hidePool = CreateHidePool(context);
			}

			return hidePool;
		}

		private HidePool CreateHidePool(string context)
		{
			GameObject go = new GameObject(context);
			var hidePool = go.AddComponent<HidePool>();
			_hidePools[context] = hidePool;
			return hidePool;
		}

		public void DestroyHidePool(string context)
		{
			HidePool hidePool = null;
			if (_hidePools.TryGetValue(context, out hidePool))
			{
				GameObject.Destroy(hidePool.gameObject);
				_hidePools.Remove(context);
			}
		}
	}
}
