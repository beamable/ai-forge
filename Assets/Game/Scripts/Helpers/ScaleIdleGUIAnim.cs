using DG.Tweening;
using UnityEngine;

namespace Game.Helpers
{
    public class ScaleIdleGUIAnim : MonoBehaviour
    {
        
        [SerializeField] private float maxScale = 1.2f;
        [SerializeField] private float duration = 0.3f;
        private Tweener _tweener;
        
        void OnEnable()
        {
            if (_tweener != null)
            {
                _tweener.Kill(true);
            }
            transform.localScale = Vector3.one;
            _tweener = transform.DOScale(maxScale, duration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        }
    }
}