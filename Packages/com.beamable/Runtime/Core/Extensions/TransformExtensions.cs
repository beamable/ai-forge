using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Extensions
{
	public static class TransformExtensions
	{
		// this is really the only way you should ever create a component on an object..
		public static T FindOrCreate<T>(this GameObject root) where T : Component
		{
			T com = root.GetComponent<T>();
			if (com == null)
				com = root.AddComponent<T>();
			return com;
		}

		public static T FindOrCreate<T>(this MonoBehaviour root) where T : Component
		{
			return root.gameObject.FindOrCreate<T>();
		}

		public static void SetLayerRecursive(this Transform trans, int layer)
		{
			trans.gameObject.layer = layer;
			for (int i = 0; i < trans.childCount; ++i)
				trans.GetChild(i).SetLayerRecursive(layer);
		}

		public static Transform FindChildRecursive(this Transform transform, string name)
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				Transform child = transform.GetChild(i);
				if (child.name == name)
				{
					return child;
				}
				else
				{
					Transform recursiveSearch = FindChildRecursive(child, name);
					if (recursiveSearch != null)
					{
						return recursiveSearch;
					}
				}
			}
			return null;
		}

		public static void Reset(this Transform t)
		{
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
			t.localScale = Vector3.one;
		}

		public static void DestroyAllChildren(this Transform t)
		{
			List<GameObject> destroyList = new List<GameObject>();
			for (int i = 0; i < t.childCount; i++)
			{
				destroyList.Add(t.GetChild(i).gameObject);
			}
			for (int i = 0; i < destroyList.Count; i++)
			{
				GameObject.Destroy(destroyList[i]);
			}
		}

		public static void SetActiveAllChildren(this Transform t, bool isActive)
		{
			for (int i = 0; i < t.childCount; i++)
			{
				t.GetChild(i).gameObject.SetActive(isActive);
			}
		}

		public static void ClearChildren(this Transform trans)
		{
			while (trans.childCount > 0)
			{
				Transform t = trans.GetChild(0);
				t.SetParent(null);
				GameObject.DestroyImmediate(t.gameObject);
			}
		}

		// get trans children recursively
		public static List<GameObject> GetChildrenRecursive(this GameObject root)
		{
			List<GameObject> objs = new List<GameObject>();
			if (root != null)
				GetChildrenRecursive(root, ref objs);
			return objs;
		}

		private static void GetChildrenRecursive(GameObject transformForSearch, ref List<GameObject> objs)
		{
			foreach (Transform trans in transformForSearch.transform)
			{
				objs.Add(trans.gameObject);
				GetChildrenRecursive(trans.gameObject, ref objs);
			}
		}

		public static void AnchorX(this RectTransform xform, float at)
		{
			xform.anchorMin = new Vector2(at, xform.anchorMin.y);
			xform.anchorMax = new Vector2(at, xform.anchorMax.y);
		}

		public static void AnchorY(this RectTransform xform, float at)
		{
			xform.anchorMin = new Vector2(xform.anchorMin.x, at);
			xform.anchorMax = new Vector2(xform.anchorMax.x, at);
		}
	}
}
