using UnityEditor;

#if !UNITY_UNIFIED_IAP && !UNITY_PURCHASING
using System.Linq;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
#endif

namespace Beamable.Editor.Modules.Purchasing
{
	[InitializeOnLoad]
	internal class BeamablePurchaserImporter
	{

		private const string BEAMABLE_PURCHASING = "BEAMABLE_PURCHASING";
		private const string UNITY_PURCHASING_PACKAGE_NAME = "com.unity.purchasing";
		private const string UNITY_PURCHASING_PACKAGE_LEGACY_VERSION = "2";

		static void EnableBeamablePurchasing() => PlayerSettingsHelper.EnableFlag(BEAMABLE_PURCHASING);
		static void DisableBeamablePurchasing() => PlayerSettingsHelper.DisableFlag(BEAMABLE_PURCHASING);

		static BeamablePurchaserImporter()
		{
#if BEAMABLE_DISABLE_AUTO_PURCHASER
         return;
#endif

			/*
			 * 3 things are possible.
			 *
			 * 1. There is no Purchasing library installed.
			 * 2. The 2x Purchasing flow is installed.
			 * 3. The 3x Purchasing flow is installed.
			 *
			 * If the 2x flow is installed, then the UNITY_PURCHASING compiler directive will exist.
			 * If the 3x flow is installed, then the PackageManager will report it
			 */

#if UNITY_UNIFIED_IAP || UNITY_PURCHASING
         // THIS IS THE 2X FLOW.
         EnableBeamablePurchasing();
#else
			// WE MIGHT BE IN THE 3X FLOW, or the NO FLOW.

			ListRequest request;
			void Check()
			{
				if (!request.IsCompleted) return;

				EditorApplication.update -= Check;
				if (request.Status != StatusCode.Success) return;

				var hasPackage = request.Result.Any(x =>
				   x.name.Equals(UNITY_PURCHASING_PACKAGE_NAME) &&
				   !x.version.StartsWith(UNITY_PURCHASING_PACKAGE_LEGACY_VERSION));
				if (hasPackage)
				{
					// THIS IS THE 3X FLOW
					EnableBeamablePurchasing();
				}
				else
				{
					// THIS IS THE NO FLOW
					DisableBeamablePurchasing();
				}
			}
			request = Client.List(offlineMode: true);
			EditorApplication.update += Check;
#endif

		}

	}
}
