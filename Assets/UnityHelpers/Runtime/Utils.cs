using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Debug = System.Diagnostics.Debug;

namespace Helpers
{
    public static class Utils
    {
        
        private static Camera _camera;
        static readonly Vector3 center = new Vector3(0.5f, 0.5f, 0);

        public static Camera Camera
        {
            get
            {
                if (_camera == null) _camera = Camera.main;
                return _camera;
            }
        }

        private static readonly Dictionary<float, WaitForSeconds> _waitForSecondsMap =
            new Dictionary<float, WaitForSeconds>();

        public static WaitForSeconds GetWait(float time)
        {
            if (_waitForSecondsMap.TryGetValue(time, out var wait)) return wait;
            _waitForSecondsMap[time] = new WaitForSeconds(time);
            return _waitForSecondsMap[time];
        }

        private static PointerEventData _eventDataCurPos;
        private static List<RaycastResult> _results = new List<RaycastResult>();

        public static bool IsOverUI()
        {
            _eventDataCurPos = new PointerEventData(EventSystem.current) {position = Input.mousePosition};
            _results.Clear();
            EventSystem.current.RaycastAll(_eventDataCurPos, _results);
            return _results.Count > 0;
        }

        public static Vector3 GetWorldPositionOfCanvasElement(this RectTransform element)
        {
            var success = RectTransformUtility.ScreenPointToWorldPointInRectangle(element, element.position, Camera, out var result);
            return success ? result : Vector3.positiveInfinity;
        }

        public static void DeleteChildren(this Transform t)
        {
            var childCount = t.childCount;
            for(int i = childCount - 1; i >= 0; i--)
            {
                Object.Destroy(t.GetChild(i).gameObject);
            }
        }

        public static void SetPosFromWorldPos(this RectTransform targetTransform, Vector3 worldPos, Camera cam = null, RectTransform canvasTransform = null)
        {
            cam = cam == null ? Camera.main : cam;
            canvasTransform = canvasTransform == null
                ? targetTransform.GetComponentInParent<Canvas>().GetComponent<RectTransform>()
                : canvasTransform;
            Debug.Assert(cam != null, "Camera != null");
            Debug.Assert(canvasTransform != null, nameof(canvasTransform) + " != null");
            var viewportPosition = cam.WorldToViewportPoint(worldPos);
            var centerBasedViewPortPosition = viewportPosition - center;
            var scale = canvasTransform.sizeDelta;
            targetTransform.localPosition = Vector3.Scale(centerBasedViewPortPosition, scale);
        }

        public static T GetRandom<T>(this IList<T> list)
        {
            var count = list.Count;
            switch (count)
            {
                case 0:
                    return default(T);
                case 1:
                    return list[0];
                default:
                    return list[Random.Range(0, count)];
            }
        }
        
        public static Color WithAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
        public static void ScrollToTop(this ScrollRect scrollRect)
        {
            scrollRect.normalizedPosition = new Vector2(0, 1);
        }
        public static void ScrollToBottom(this ScrollRect scrollRect)
        {
            scrollRect.normalizedPosition = new Vector2(0, 0);
        }
        
    }
}
