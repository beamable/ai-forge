using UnityEngine;
using UnityEngine.UI;

namespace Beamable.UI.Scripts
{
	public enum StickyLockState
	{
		LockedTop, LockedLow, Unlocked
	}
	public class StickyScrollElement : MonoBehaviour
	{
		public ScrollRect Scroller;
		public LayoutElement StickyElement;

		public bool IsAbove, IsBelow;
		public StickyLockState LockState = StickyLockState.Unlocked;
		public bool Locked => LockState != StickyLockState.Unlocked;
		private bool _wasAbove, _wasBelow, _appliedLockedAnchors;

		private RectTransform _stickyTransform;
		private LayoutElement _companionLayoutElement;
		// Start is called before the first frame update
		void Start()
		{
			if (Scroller == null)
			{
				Scroller = GetComponentInParent<ScrollRect>();
			}

			if (Scroller == null)
			{
				Debug.LogWarning("Cannot use sticky scroll element that isnt in a scroll rect");
				return;
			}
			if (StickyElement == null)
			{
				Debug.LogWarning("Cannot use sticky scroll element with no assigned sticky element");
				return;
			}

			_stickyTransform = StickyElement.transform as RectTransform;
			Scroller.onValueChanged.AddListener(ScrollChanged);

		}

		// Update is called once per frame
		void LateUpdate()
		{


			if (IsAbove && !_wasAbove)
			{
				Lock(StickyLockState.LockedTop);
			}

			if (IsBelow && !_wasBelow)
			{
				Lock(StickyLockState.LockedLow);
			}

			if ((!IsAbove && _wasAbove) || (!IsBelow && _wasBelow))
			{
				Unlock();
			}

			if (Locked)
			{
				UpdateLockPosition();
			}

			_wasAbove = IsAbove;
			_wasBelow = IsBelow;
		}

		public void Lock(StickyLockState state)
		{

			StickyElement.ignoreLayout = true;
			var companion = GetCompanion();

			companion.ignoreLayout = false;
			_stickyTransform.SetAsLastSibling(); // render last.
			LockState = state;

		}

		public void Unlock()
		{
			StickyElement.ignoreLayout = false;
			var companion = GetCompanion();
			companion.ignoreLayout = true;
			_stickyTransform.SetSiblingIndex(companion.transform.GetSiblingIndex());
			LockState = StickyLockState.Unlocked;
			_appliedLockedAnchors = false;
		}

		void UpdateLockPosition()
		{
			var height = _stickyTransform.rect.height;

			if (!_appliedLockedAnchors)
			{

				_stickyTransform.anchorMax = new Vector2(1, 1);
				_stickyTransform.anchorMin = new Vector2(0, 1);
				_stickyTransform.pivot = new Vector2(.5f, 1);
				_appliedLockedAnchors = true;
			}

			if (LockState == StickyLockState.LockedTop)
			{
				_stickyTransform.offsetMax = new Vector2(
					1,
					-Scroller.content.offsetMax.y);
				_stickyTransform.offsetMin = new Vector2(
					0,
					-Scroller.content.offsetMax.y - height);
			}
			else if (LockState == StickyLockState.LockedLow)
			{
				_stickyTransform.offsetMax = new Vector2(
					1,
					-(Scroller.content.offsetMax.y + Scroller.viewport.rect.height - height));
				_stickyTransform.offsetMin = new Vector2(
					0,
					-(Scroller.content.offsetMax.y + Scroller.viewport.rect.height - height) - height);
			}


		}

		LayoutElement GetCompanion()
		{
			if (_companionLayoutElement == null)
			{
				var gob = new GameObject("stickyElementSpaceFiller", typeof(RectTransform));
				_companionLayoutElement = gob.AddComponent<LayoutElement>();
				_companionLayoutElement.minHeight = StickyElement.minHeight;
				_companionLayoutElement.preferredHeight = StickyElement.preferredHeight;
				_companionLayoutElement.flexibleHeight = StickyElement.flexibleHeight;

				gob.transform.SetParent(StickyElement.transform.parent);
				gob.transform.SetSiblingIndex(_stickyTransform.GetSiblingIndex() + 1);
			}
			return _companionLayoutElement;

		}

		float GetElementTop()
		{
			var movingElement = Locked ? (RectTransform)GetCompanion().transform : _stickyTransform;
			return -movingElement.offsetMax.y;
		}

		float GetElementLow()
		{
			var movingElement = Locked ? (RectTransform)GetCompanion().transform : _stickyTransform;
			return -movingElement.offsetMin.y;
		}

		void ScrollChanged(Vector2 scrollPos)
		{
			if (StickyElement == null) return;

			var scrollTop = Scroller.content.offsetMax.y;
			var scrollLow = scrollTop + Scroller.viewport.rect.height;
			var stickyTop = GetElementTop();
			var stickyLow = GetElementLow();

			IsAbove = scrollTop > stickyTop;
			IsBelow = scrollLow < stickyLow;

		}
	}
}

