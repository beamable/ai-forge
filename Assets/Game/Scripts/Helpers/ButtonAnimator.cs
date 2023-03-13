using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Helpers
{
    public class ButtonAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float pressedDownScale = 1.05f;
        [SerializeField] private float duration = 0.15f;
        [SerializeField] private Ease easeType;
        private RectTransform _rectTransform;
        private TweenerCore<Vector3, Vector3, VectorOptions> currentTween;
        [SerializeField] private Selectable _selectable;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_selectable == null)
            {
                _selectable = GetComponent<Selectable>();
            }
        }

        private void CancelCurrent()
        {
            if (currentTween != null)
                currentTween.Kill(true);
            currentTween = null;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            CancelCurrent();
            if(_selectable == null || _selectable.interactable)
            {
                currentTween = _rectTransform.DOScale(Vector3.one * pressedDownScale, duration).SetEase(easeType);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            CancelCurrent();
            currentTween =  _rectTransform.DOScale(Vector3.one, duration).SetEase(easeType);
        }
    }
}