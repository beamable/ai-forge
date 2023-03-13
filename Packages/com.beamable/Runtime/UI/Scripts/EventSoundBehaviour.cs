using Beamable.Sound;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Beamable.UI.Scripts
{
	public class EventSoundBehaviour : UIBehaviour
	{
		public AudioClip Clip;
		public float Volume = 1;

		public Button Button;

		private Button _lastButton;

		protected override void OnEnable()
		{
			RegisterButton();
		}

		void RegisterButton()
		{
			if (_lastButton == Button) return;

			if (_lastButton != null)
			{
				_lastButton.onClick.RemoveListener(OnClick);
			}

			if (Button != null)
			{
				Button.onClick.AddListener(OnClick);
			}

			_lastButton = Button;
		}

		void OnClick()
		{
			if (Clip == null) return;
			var source = SoundConfiguration.Instance.GetAudioSource();
			source.PlayOneShot(Clip, Volume);
			SoundConfiguration.Instance.RecycleAudioSource(source);
		}

		private void Update()
		{
			if (_lastButton != Button)
			{
				RegisterButton();
			}
		}
	}
}
