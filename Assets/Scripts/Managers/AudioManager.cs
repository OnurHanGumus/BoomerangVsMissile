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

        private void Awake()
        {
            SubscribeEvents();
        }

        #region Event Subscriptions

        private void SubscribeEvents()
        {
            AudioSignals.Instance.onPlaySound += OnPlaySound;
            AudioSignals.Instance.onPlaySoundWithPitch += OnPlaySoundWithPitch;
        }

        private void OnDestroy()
        {
            if (AudioSignals.Instance != null)
            {
                AudioSignals.Instance.onPlaySound -= OnPlaySound;
                AudioSignals.Instance.onPlaySoundWithPitch -= OnPlaySoundWithPitch;
            }
        }

        #endregion

        private void OnPlaySound(AudioSoundEnums id)
        {
            if ((int)id < 0 || (int)id >= sounds.Count || sounds[(int)id] == null) return;
            foreach (var i in sources)
            {
                if (i.isPlaying)
                {
                    continue;
                }
                else
                {
                    i.pitch = 1f;
                    i.PlayOneShot(sounds[(int)id]);
                    break;
                }
            }
        }

        private void OnPlaySoundWithPitch(AudioSoundEnums id, float pitch)
        {
            if ((int)id < 0 || (int)id >= sounds.Count || sounds[(int)id] == null) return;
            foreach (var i in sources)
            {
                if (i.isPlaying)
                {
                    continue;
                }
                else
                {
                    i.pitch = pitch;
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