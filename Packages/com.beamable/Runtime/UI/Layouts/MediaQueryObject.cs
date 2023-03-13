using Beamable.UI.Scripts;
using System;
using System.Reflection;
using UnityEngine;

namespace Beamable.UI.Layouts
{
	public enum MediaQueryOperation
	{
		GREATER_THAN,
		LESS_THAN
	}

	public enum MediaQueryDimension
	{
		WIDTH,
		HEIGHT,
		ASPECT,
		KEYBOARD_HEIGHT
	}

	public delegate void MediaQueryCallback(MediaSourceBehaviour query, bool output);

	// This will be deprecated and removed very soon!!!
	// [CreateAssetMenu(
	//    fileName = "Media Query",
	//    menuName = BeamableConstants.MENU_ITEM_PATH_ASSETS_BEAMABLE + "/" +
	//    "Media Query",
	//    order = BeamableConstants.MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_1)]
	public class MediaQueryObject : ScriptableObject
	{
		public MediaQueryDimension Dimension;
		public MediaQueryOperation Operation;
		public float Threshold;

		private float GetDimensionValue()
		{
			Vector2 screen = GetScreen();
			switch (Dimension)
			{
				case MediaQueryDimension.WIDTH:
					return screen.x;
				case MediaQueryDimension.HEIGHT:
					return screen.y;
				case MediaQueryDimension.ASPECT:
					return screen.x / screen.y;
				case MediaQueryDimension.KEYBOARD_HEIGHT:
					return MobileUtilities.GetKeyboardHeight(false);
				default:
					throw new Exception("Unknown dimension");
			}
		}

		private float GetDimensionValue(RectTransform transform)
		{
			switch (Dimension)
			{
				case MediaQueryDimension.WIDTH:
					return transform.rect.width;
				case MediaQueryDimension.HEIGHT:
					return transform.rect.height;
				case MediaQueryDimension.ASPECT:
					Rect rect = transform.rect;
					return rect.width / rect.height;
				default:
					throw new Exception("Dimension value not supported on transform");
			}
		}

		private Vector2 GetScreen()
		{
			// IF WE ARE IN EDITOR, THEN WE WANT TO GET THE GAME-SCREEN SIZE, NOT THE EDITOR SCREEN SIZE...
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				return GetMainGameViewSize();
			}
#endif
			return new Vector2(Screen.width, Screen.height);
		}

		private bool CompareDimensionAndThreshold(float dimensionValue, float thresholdValue)
		{
			switch (Operation)
			{
				case MediaQueryOperation.LESS_THAN:
					return dimensionValue < thresholdValue;
				case MediaQueryOperation.GREATER_THAN:
					return dimensionValue > thresholdValue;
				default:
					throw new Exception("unknown operation");
			}
		}

		public bool CalculateScreen()
		{
			float dimensionValue = GetDimensionValue();
			return CompareDimensionAndThreshold(dimensionValue, Threshold);
		}

		public bool Calculate(RectTransform target)
		{
			float dimensionValue = GetDimensionValue(target);
			return CompareDimensionAndThreshold(dimensionValue, Threshold);
		}

		private static Vector2 GetMainGameViewSize()
		{
			var T = Type.GetType("UnityEditor.GameView,UnityEditor");
			MethodInfo GetSizeOfMainGameView =
				T.GetMethod("GetSizeOfMainGameView",
							BindingFlags.NonPublic | BindingFlags.Static);

			object Res = GetSizeOfMainGameView.Invoke(null, null);
			return (Vector2)Res;
		}
	}
}
