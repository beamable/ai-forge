using System;
using UnityEngine;

namespace Beamable.UI.Scripts
{

	public static class MobileUtilities
	{
		private static ScreenOrientation PortraitFilter =
		   ScreenOrientation.Portrait | ScreenOrientation.PortraitUpsideDown;

		/// <summary>
		/// Returns the keyboard height ratio.
		/// </summary>
		public static float GetKeyboardHeightRatio(bool includeInput)
		{
			var totalHeight = IsPortrait()
			   ? Display.main.systemHeight
			   : Display.main.systemWidth;

			return Mathf.Clamp01(((float)GetKeyboardHeight(includeInput)) / totalHeight);
		}

		public static bool IsPortrait()
		{
			return (Screen.orientation & PortraitFilter) > 0;
		}

		/// <summary>
		/// Returns the keyboard height in display pixels.
		/// </summary>
		public static int GetKeyboardHeight(bool includeInput)
		{
			if (!TouchScreenKeyboard.isSupported)
			{
				return 0;
			}

#if UNITY_ANDROID
         using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
         {
            var unityPlayer = unityClass.GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer");
            var view = unityPlayer.Call<AndroidJavaObject>("getView");
            var dialog = unityPlayer.Get<AndroidJavaObject>("b");

            if (view == null || dialog == null)
               return 0;

            var decorHeight = 0;

            if (includeInput)
            {
               var decorView = dialog.Call<AndroidJavaObject>("getWindow").Call<AndroidJavaObject>("getDecorView");

               if (decorView != null)
                  decorHeight = decorView.Call<int>(IsPortrait() ? "getHeight" : "getHeight");
            }

            using (var rect = new AndroidJavaObject("android.graphics.Rect"))
            {
               view.Call("getWindowVisibleDisplayFrame", rect);

               if (IsPortrait())
               {
                  return Display.main.systemHeight - rect.Call<int>("height") + decorHeight;
               }
               else
               {
                  return rect.Call<int>("height") + decorHeight;
               }

            }
         }
#elif UNITY_STANDALONE
         throw new NotImplementedException();
#elif UNITY_WEBGL_API
         return 50; // TODO: This is obviously wrong; I just guessed.
#else
			var height = Mathf.RoundToInt(TouchScreenKeyboard.area.height);
			return height >= Display.main.systemHeight ? 0 : height;
#endif
		}
	}
}
