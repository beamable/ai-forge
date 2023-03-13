using UnityEngine;

namespace Beamable.Service
{
	public class EditorPrefabServiceResolver<TResolvedAs> : IServiceResolver<TResolvedAs>
		where TResolvedAs : MonoBehaviour
	{
		private static TResolvedAs instance;

		public bool CanResolve()
		{
			return !ApplicationLifetime.isQuitting;
		}

		public bool Exists()
		{
			return (instance != null) && !ApplicationLifetime.isQuitting;
		}

		public TResolvedAs Resolve()
		{
			if (instance == null)
			{
				if (ApplicationLifetime.isQuitting)
				{
					Debug.LogError(string.Format("Application is quiting, but something is still calling ServiceManager.Resolve<{0}>, which is going to return null. Use ServiceManager.Exists before ServiceManager.Resolve on cleanup code", typeof(TResolvedAs).ToString()));
					return null;
				}

				instance = Object.FindObjectOfType<TResolvedAs>();

				if (instance == null)
				{
					var serviceType = typeof(TResolvedAs);
					var att = serviceType.GetCustomAttributes(typeof(EditorServiceResolverAttribute), true);
					var editorAttr = att[0] as EditorServiceResolverAttribute;
					var gameObject = GameObject.Instantiate(Resources.Load(editorAttr.userData) as GameObject);
					//GameObject.DontDestroyOnLoad(gameObject);
					ServiceManager.PlaceUnderServiceRoot(gameObject);
					instance = gameObject.GetComponent<TResolvedAs>();
				}
			}
			return instance;
		}

		public void OnTeardown()
		{
			if (ApplicationLifetime.isQuitting)
			{
				return;
			}
			Object.Destroy(instance.gameObject);
			instance = null;
		}
	}
}
