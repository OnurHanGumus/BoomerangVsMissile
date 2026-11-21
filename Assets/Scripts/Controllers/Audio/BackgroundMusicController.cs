using Enums;
using Signals;
using UnityEngine;

namespace Controllers.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class BackgroundMusicController : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables

        [SerializeField] private float maxVolume = 0.2f;

        #endregion

        #region Private Variables

        private AudioSource _audioSource;

        #endregion

        #endregion

        private void Awake()
        {
            Init();
            SubscribeEvents();
        }

        private void Start()
        {
            float savedVolume = SaveSignals.Instance.onGetMusicVolume(SaveLoadStates.MusicVolume, SaveFiles.GameOptions);
            ApplyVolume(savedVolume);
        }

        private void Init()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.volume = 0f;
        }

        #region Event Subscription

        private void SubscribeEvents()
        {
            AudioSignals.Instance.onChangeMusicVolume += OnChangeMusicVolume;
        }

        private void UnsubscribeEvents()
        {
            AudioSignals.Instance.onChangeMusicVolume -= OnChangeMusicVolume;
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        #endregion

        private void OnChangeMusicVolume(float volume)
        {
            ApplyVolume(volume);
        }

        private void ApplyVolume(float volume)
        {
            _audioSource.volume = Mathf.Clamp(volume, 0f, maxVolume);
        }
    }
}
