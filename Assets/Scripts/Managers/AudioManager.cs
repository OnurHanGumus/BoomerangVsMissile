using Controllers;
using Enums;
using Signals;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class AudioManager : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables
        [SerializeField] private List<AudioSource> sources;
        [SerializeField] private List<AudioClip> sounds;

        #endregion

        #endregion

        #region Event Subscriptions

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            AudioSignals.Instance.onPlaySound += OnPlaySound;
        }

        private void UnsubscribeEvents()
        {
            AudioSignals.Instance.onPlaySound -= OnPlaySound;
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        #endregion

        private void OnPlaySound(AudioSoundEnums id)
        {
            foreach (var i in sources)
            {
                if (i.isPlaying)
                {
                    continue;
                }
                else
                {
                    i.PlayOneShot(sounds[(int)id]);
                    break;
                }
            }
        }
        public void ButtonClickSound()
        {
            OnPlaySound(AudioSoundEnums.Click);
        }
    }
}