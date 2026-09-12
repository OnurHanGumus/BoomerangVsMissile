using Signals;
using UnityEngine;

namespace Controllers
{
    public class BoomerangSoundController : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables

        [SerializeField] private AudioSource audioSource;

        #endregion

        #endregion

        private void Awake()
        {
            Init();
            SubscribeEvents();
        }

        private void Init()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            DisableSound();
        }

        #region Event Subscription

        private void SubscribeEvents()
        {
            BoomerangSignals.Instance.onBoomerangThrown += OnBoomerangThrown;
            BoomerangSignals.Instance.onBoomerangHasReturned += OnBoomerangHasReturned;
            CoreGameSignals.Instance.onPlay += OnPlay;
            CoreGameSignals.Instance.onLevelSuccessful += OnLevelEnded;
            CoreGameSignals.Instance.onLevelFailed += OnLevelEnded;
            CoreGameSignals.Instance.onRestartLevel += OnRestartLevel;
        }

        private void UnsubscribeEvents()
        {
            if (BoomerangSignals.Instance != null)
            {
                BoomerangSignals.Instance.onBoomerangThrown -= OnBoomerangThrown;
                BoomerangSignals.Instance.onBoomerangHasReturned -= OnBoomerangHasReturned;
            }

            if (CoreGameSignals.Instance != null)
            {
                CoreGameSignals.Instance.onPlay -= OnPlay;
                CoreGameSignals.Instance.onLevelSuccessful -= OnLevelEnded;
                CoreGameSignals.Instance.onLevelFailed -= OnLevelEnded;
                CoreGameSignals.Instance.onRestartLevel -= OnRestartLevel;
            }
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        #endregion

        private void OnBoomerangThrown()
        {
            EnableSound();
        }

        private void OnBoomerangHasReturned()
        {
            DisableSound();
        }

        private void OnPlay()
        {
            DisableSound();
        }

        private void OnLevelEnded()
        {
            DisableSound();
        }

        private void OnRestartLevel()
        {
            DisableSound();
        }

        private void EnableSound()
        {
            if (audioSource == null) return;

            audioSource.enabled = true;
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }

        private void DisableSound()
        {
            if (audioSource == null) return;

            audioSource.Stop();
            audioSource.enabled = false;
        }
    }
}
