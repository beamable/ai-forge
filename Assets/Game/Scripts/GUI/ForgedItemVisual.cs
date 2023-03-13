using DG.Tweening;
using Game.Data;
using Helpers;
using Helpers.UnityHelpers.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GUI
{
    public class ForgedItemVisual : MonoBehaviour
    {
        [SerializeField] private SoundAnalyzer _soundAnalyzer;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _clip;
        [SerializeField] private Image _image;
        [SerializeField] private float _lerpMultiplier;
        [SerializeField] private Color _forgedColor;

        private readonly Color _baseColor = Color.white;
        private Color _currentColor = Color.white;
        private bool _isDuringCreating = false;
        private float _alpha;
        private float _targetAlpha;

        private void OnEnable()
        {
            _alpha = 0f;
            _image.color = _baseColor.WithAlpha(0f);
            EventBetter.Listen(this, (CreatingItemStarted e) => HandleCreatedItemStarted(e));
            EventBetter.Listen(this, (CreatingItemFinished e) => HandleCreatingItemFinished(e));
        }

        void Update()
        {
            if (_isDuringCreating)
            {
                _soundAnalyzer.UpdateValues();
                // Debug.Log($"{_soundAnalyzer.PitchValue} pitch");
                const float max = 2300f;
                _targetAlpha = EasingFunctions.OutElastic(Mathf.Clamp(_soundAnalyzer.PitchValue / max, 0.03f, 1.0f));
            }

            if(Mathf.Abs(_targetAlpha - _alpha) > 0.001f)
            {
                _alpha = Mathf.Min(Mathf.Lerp(_alpha, _targetAlpha, Time.deltaTime * _lerpMultiplier), 1.0f);
                _image.color = _currentColor.WithAlpha(_alpha);
            }
        }

        private void HandleCreatingItemFinished(CreatingItemFinished creatingItemFinished)
        {
            _isDuringCreating = false;
            _targetAlpha = 1.0f;

            DOTween.To(value => { _audioSource.volume = value; }, _audioSource.volume, 0.0f, 1.0f)
                .OnComplete(_audioSource.Stop);
            DOVirtual.Color(_forgedColor, Color.white, 0.1f, c => _currentColor = c)
                .OnComplete(() =>
                {
                    DOVirtual.Float(1f, 0f, 0.3f, f => _targetAlpha = f).SetDelay(1.8f);
                });
        }

        private void HandleCreatedItemStarted(CreatingItemStarted creatingItemStarted)
        {
            _isDuringCreating = true;
            _audioSource.clip = _clip;
            _audioSource.loop = true;
            _audioSource.Play();
            DOTween.To(value => { _audioSource.volume = value; }, 0.0f, 0.5f, 1.0f);
            DOVirtual.Color(Color.white, _forgedColor, 0.8f, c => _currentColor = c);
        }
    }
}
