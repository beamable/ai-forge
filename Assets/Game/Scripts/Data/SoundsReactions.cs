using System;
using Helpers;
using UnityEngine;

namespace Game.Data
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundsReactions : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip[] _creatingItemFinishedSounds;
        [SerializeField] private AudioClip[] _buttonPressedSounds;
        
        private void Awake()
        {
            EventBetter.Listen(this, (CreatingItemFinished e) => HandleCreatingItemFinished(e));
            EventBetter.Listen(this, (ButtonPressed e) => HandleButtonPressed(e));
        }

        private void HandleButtonPressed(ButtonPressed buttonPressed)
        {
            _audioSource.PlayOneShot(_buttonPressedSounds.GetRandom());
        }

        private void HandleCreatingItemFinished(CreatingItemFinished creatingItemFinished)
        {
            if (creatingItemFinished.Successful)
            {
                _audioSource.PlayOneShot(_creatingItemFinishedSounds.GetRandom());
            }
        }
    }
}