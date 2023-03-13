using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Beamable.UI.Scripts
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(RectTransform))]
	public class PoolableScrollView : UIBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler,
									  IDragHandler, IScrollHandler, ICanvasElement
	{
		public interface IItem
		{
			float Height
			{
				get;
			}
		}

		public interface IContentProvider
		{
			RectTransform Spawn(IItem item, out int order);
			void Despawn(IItem item, RectTransform transform);
		}

		public event Action OnPositionChanged;

#pragma warning disable CS0649
		[SerializeField] private RectOffset _padding = new RectOffset();
		[SerializeField] private float _elasticity = 0.1f;
		[SerializeField] private bool _inertia = true;
		[SerializeField] private float _decelerationRate = 0.135f;
		[SerializeField] private float _spacing = 0.0f;
		[SerializeField] private bool _trackLastElement;
		[SerializeField] private float _trackSpeedFactor = 4.0f;
		[SerializeField] private TextAnchor _childAlignment;
#pragma warning restore CS0649

		private readonly Dictionary<IItem, (RectTransform rect, int order)> _itemRects =
			new Dictionary<IItem, (RectTransform, int)>();

		private bool _dirty;
		private bool _dirtyContent;
		private bool _dragging;
		private float _velocity;
		private float _position;
		private float _positionOld;
		private float _positionLimit;
		private float _positionMoveTo;
		private IContentProvider _provider;

		private List<IItem> _items;
		private float _itemsHeight;

		private RectTransform _viewRect;

		private Vector2 _dragStart;
		private float _dragStartPosition;

		private IItem _lastElement;
		private bool _lowerLimit = true;
		private float _moveToSpeed;

		public float Velocity
		{
			get => _velocity;
			set => _velocity = value;
		}

		public float Position
		{
			get => _position;
			set => _position = value;
		}

		public float PositionMoveTo
		{
			get => _positionMoveTo;
			set
			{
				_positionMoveTo = value;
				_moveToSpeed = Math.Abs(_position - _positionMoveTo) * _trackSpeedFactor;
			}
		}

		public RectTransform ViewRect
		{
			get
			{
				ValidateRect();
				return _viewRect;
			}
		}

		public float ItemsHeight => _itemsHeight;

		protected override void Awake()
		{
			base.Awake();

			ValidateRect();

			if (_provider != null && _items != null)
			{
				_itemsHeight = EvaluateContentHeight();

				UpdateContent();
				_positionLimit = Mathf.Max(0.0f, _itemsHeight - _viewRect.rect.height);
			}
		}

		public void SetDirty()
		{
			_dirty = true;
			_positionLimit = Mathf.Max(0.0f, _itemsHeight - _viewRect.rect.height);
		}

		public void SetDirtyContent()
		{
			_dirty = true;
			_dirtyContent = true;
		}

		public void SetContentProvider(IContentProvider provider)
		{
			_provider = provider;
		}

		public void SetContent(List<IItem> items, bool forceTrackLastElement = false, bool forceResetPosition = false)
		{
			if (_provider == null)
				return;

			ValidateRect();

			foreach (var rt in _itemRects.Where(r => r.Key != null))
			{
				_provider.Despawn(rt.Key, rt.Value.Item1);
			}

			_itemRects.Clear();

			_items = items;
			_itemsHeight = EvaluateContentHeight();

			_positionLimit = Mathf.Max(0.0f, _itemsHeight - _viewRect.rect.height);

			if (_trackLastElement && _items.Count > 0 && (_lowerLimit || forceTrackLastElement))
			{
				bool animateContent = _lastElement != null && !EqualItems(_lastElement, _items[_items.Count - 1]) &&
									  _items.Exists(i => EqualItems(i, _lastElement));
				_lastElement = _items[_items.Count - 1];
				_positionMoveTo = _positionLimit;
				_position = Mathf.Max(0, _positionLimit - _lastElement.Height);
				_moveToSpeed = _lastElement.Height * _trackSpeedFactor;

				if (!animateContent)
				{
					_position = _positionMoveTo;
				}
			}

			UpdateContent();
		}

		public void SetPosition(float position)
		{
			_position = position;
			_positionOld = position;
			_positionMoveTo = position;
			UpdateContent();
		}

		protected override void OnRectTransformDimensionsChange()
		{
			base.OnRectTransformDimensionsChange();

			if (_viewRect != null && _trackLastElement && _lowerLimit)
			{
				_positionLimit = Mathf.Max(0.0f, _itemsHeight - _viewRect.rect.height);
				_positionMoveTo = _positionLimit;
				_position = _positionLimit;
				_positionOld = _positionLimit;
				_moveToSpeed = 500 * _trackSpeedFactor;
			}

			_dirty = true;
		}

		private void UpdateContent()
		{
			if (_items == null)
				return;

			ValidateRect();
			if (_dirtyContent)
			{
				_itemsHeight = EvaluateContentHeight();
			}

			float limit = _viewRect.rect.height;
			float pos = -_position + _padding.top;

			if (_itemsHeight < limit && (_childAlignment == TextAnchor.LowerCenter ||
										 _childAlignment == TextAnchor.LowerLeft ||
										 _childAlignment == TextAnchor.LowerRight))
			{
				pos += limit - _itemsHeight;
			}

			foreach (IItem item in _items)
			{
				if (pos + item.Height < 0 || pos > limit)
				{
					if (_itemRects.TryGetValue(item, out (RectTransform rect, int order) itemRectOrder))
					{
						_provider.Despawn(item, itemRectOrder.rect);
						_itemRects.Remove(item);
					}
				}
				else
				{
					if (!_itemRects.TryGetValue(item, out (RectTransform rect, int order) itemRectOrder))
					{
						itemRectOrder.rect = _provider.Spawn(item, out int order);
						itemRectOrder.order = order;
						_itemRects.Add(item, itemRectOrder);

						itemRectOrder.rect.SetParent(transform, false);
						itemRectOrder.rect.anchorMin = new Vector2(0, 1);
						itemRectOrder.rect.anchorMax = new Vector2(1, 1);
						itemRectOrder.rect.pivot = new Vector2(0, 1);
					}

					itemRectOrder.rect.offsetMin = new Vector2(0, 0);
					itemRectOrder.rect.offsetMax = new Vector2(0, item.Height);
					itemRectOrder.rect.anchoredPosition = new Vector2(_padding.left, -pos);
					itemRectOrder.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
																 itemRectOrder.rect.rect.width - _padding.right -
																 _padding.left);
				}

				pos += item.Height;
				pos += _spacing;
			}

			// fix for order in gear list view (sort by cached order)
			RectTransform[] sortedItems = _itemRects.OrderBy(kv => kv.Value.order)
													.Select(kv => kv.Value.rect).ToArray();

			for (int i = 0; i < sortedItems.Length; i++)
			{
				sortedItems[i].SetSiblingIndex(i);
			}

			_dirty = false;
			_dirtyContent = false;
		}

		private float EvaluateContentHeight()
		{
			float result = -_spacing;
			foreach (IItem item in _items)
			{
				result += _spacing;
				result += item.Height;
			}

			result += _padding.top + _padding.bottom;

			return result;
		}

		private void LateUpdate()
		{
			float deltaTime = Time.unscaledDeltaTime;

			_positionLimit = Mathf.Max(0.0f, _itemsHeight - _viewRect.rect.height);
			if (Mathf.Abs(_position) < 0.5f)
			{
				_position = 0;
			}

			if (Mathf.Abs(_position - _positionLimit) < 0.5f)
			{
				_position = _positionLimit;
			}

			float offset = 0;
			if (_position < 0)
			{
				offset = _position;
			}

			if (_position > _positionLimit)
			{
				offset = _position - _positionLimit;
			}

			if (!_dragging)
			{
				if (offset != 0)
				{
					_position = Mathf.SmoothDamp(_position, _position - offset, ref _velocity, _elasticity,
												 Mathf.Infinity, deltaTime);
					_positionMoveTo = _position;
					UpdateContent();

					if (_position - _positionLimit > -20)
					{
						_lowerLimit = true;
					}
				}
				else if (_velocity != 0)
				{
					_velocity *= Mathf.Pow(_decelerationRate, deltaTime);
					if (Mathf.Abs(_velocity) < 1)
						_velocity = 0;

					_position += _velocity * deltaTime;
					_positionMoveTo = _position;
					UpdateContent();
				}
				else if (!Mathf.Approximately(_position, _positionMoveTo))
				{
					float dp = _positionMoveTo - _position;

					_position += Mathf.Min(_moveToSpeed * deltaTime, dp);
					UpdateContent();
				}
			}

			if (_dragging && _inertia)
			{
				float newVelocity = (_position - _positionOld) / deltaTime;
				_velocity = Mathf.Lerp(_velocity, newVelocity, deltaTime * 10);
			}

			if (!Mathf.Approximately(_positionOld, _position))
			{
				OnPositionChanged?.Invoke();
			}

			_positionOld = _position;

			if (_dirty)
			{
				UpdateContent();
			}
		}

		public void OnInitializePotentialDrag(PointerEventData eventData)
		{
			_velocity = 0;
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
				return;

			if (!IsActive())
				return;

			RectTransformUtility.ScreenPointToLocalPointInRectangle(_viewRect, eventData.position,
																	eventData.pressEventCamera, out _dragStart);
			_dragStartPosition = _position;
			_dragging = true;
			_lowerLimit = false;
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
				return;

			_dragging = false;
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
				return;

			if (!IsActive())
				return;

			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
				_viewRect, eventData.position, eventData.pressEventCamera, out Vector2 localCursor))
				return;

			Vector2 pointerDelta = localCursor - _dragStart;
			_position = _dragStartPosition + pointerDelta.y;

			float offset = 0;
			if (_position < 0)
			{
				offset = _position;
				_position = 0;
			}

			if (_position > _positionLimit)
			{
				offset = _position - _positionLimit;
				_position = _positionLimit;
				_lowerLimit = true;
			}

			_position += RubberDelta(offset, _viewRect.rect.height);
			_positionMoveTo = _position;

			UpdateContent();
		}

		public void OnScroll(PointerEventData eventData) { }

		public void Rebuild(CanvasUpdate executing) { }

		public void LayoutComplete() { }

		public void GraphicUpdateComplete() { }

		public RectTransform GetRect(IItem item)
		{
			return _itemRects.TryGetValue(item, out (RectTransform rect, int order) rectOrder) ? rectOrder.rect : null;
		}

		private void ValidateRect()
		{
			if (_viewRect == null)
			{
				_viewRect = GetComponent<RectTransform>();
				_viewRect.ForceUpdateRectTransforms();
			}
		}

		private static float RubberDelta(float overStretching, float viewSize)
		{
			return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize *
				   Mathf.Sign(overStretching);
		}

		private bool EqualItems(IItem l, IItem r)
		{
			if (l is IEquatable<IItem> el)
				return el.Equals(r);

			return l == r;
		}
	}
}
