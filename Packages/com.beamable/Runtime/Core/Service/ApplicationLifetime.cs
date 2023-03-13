using Beamable.Common.Spew;
using UnityEngine;

namespace Beamable.Service
{
	// The purpose of this class is to detect the lifespan of the entire process.  In production, this resolves to the
	// creation of a top level Game Object that can only be destroyed by the whole process getting killed.  In editor
	// we have the added wrinkle of wanting to reset this state when you un-press the Play button.
	//
	// This is not a great model to replicate - for lifetime stuff you're more likely to be interested in the
	// lifetime hooks presented by ServiceManager, which represents a play session and can be destroyed and recreated
	// within the scope of a single process run.  However a few things want their lifespans to exceed this and wrap
	// the lifespan of the entire process, which is what this class is for.
	//
	// Note this uses lots of statics because it does live longer than the ServiceManager, and requires use of a static
	// constructor for hooking into the playmode events for editor use.
	//
	public static class ApplicationLifetime
	{
		private static ApplicationLifetimeDetector detector;
		private static bool _isQuitting = false;

		public static bool isQuitting
		{
			get
			{
				EnsureDetector();
				return _isQuitting;
			}
			set { _isQuitting = value; }
		}

#if UNITY_EDITOR
		static ApplicationLifetime()
		{
			AppLifetimeLogger.Log("Application Lifetime Detection Online.");

         UnityEditor.EditorApplication.playModeStateChanged += (obj) => {
				if (!UnityEditor.EditorApplication.isPlaying)
				{
					AppLifetimeLogger.Log("Resetting lifetime quit flag to false.");
					isQuitting = false;
				}
			};
		}
#endif

		private static void EnsureDetector()
		{
			// Don't check the isQuitting property here, you'll make an infinite loop!
			if (!_isQuitting && (detector == null))
			{
				var gameObject = new GameObject("Application Lifetime Detector");
				detector = gameObject.AddComponent<ApplicationLifetimeDetector>();
			}
		}
	}


	public class ApplicationLifetimeDetector : MonoBehaviour
	{
		void OnApplicationQuit()
		{
			AppLifetimeLogger.Log("Lifetime quit flag set to true.");
			ApplicationLifetime.isQuitting = true;
		}

		void Awake()
		{
			AppLifetimeLogger.Log("Lifetime quit flag set to false.");
			ApplicationLifetime.isQuitting = false;
			DontDestroyOnLoad(gameObject);
		}
	}
}
