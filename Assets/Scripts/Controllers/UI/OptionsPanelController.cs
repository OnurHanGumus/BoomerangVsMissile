using Enums;
using Signals;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsPanelController : MonoBehaviour
{
    #region Self Variables
    #region Public Variables
    #endregion
    #region SerializeField Variables
    [SerializeField] private Toggle soundToggle;
    [SerializeField] private Toggle pitchToggle;
    [SerializeField] private AudioSource audioSource;
    #endregion
    #region Private Variables
    private bool _audioSourceActiveness;
    private bool _pitchActiveness;
    #endregion
    #endregion

    private void Start()
    {
        _audioSourceActiveness = SaveSignals.Instance.onGetSoundState(SaveLoadStates.SoundState, SaveFiles.GameOptions) == 1;
        if (soundToggle != null)
        {
            soundToggle.isOn = _audioSourceActiveness;
        }
        SetAudioSource();

        if (pitchToggle != null)
        {
            _pitchActiveness = SaveSignals.Instance.onGetPitchState(SaveLoadStates.PitchState, SaveFiles.GameOptions) == 1;
            pitchToggle.isOn = _pitchActiveness;
            pitchToggle.onValueChanged.AddListener(OnPitchToggleValueChanged);
        }
    }

    public void OnValueChanged()
    {
        if (soundToggle != null)
        {
            _audioSourceActiveness = soundToggle.isOn;
            SaveSignals.Instance.onChangeSoundState?.Invoke(_audioSourceActiveness ? 1 : 0, SaveLoadStates.SoundState, SaveFiles.GameOptions);
            SetAudioSource();
        }
    }

    public void OnPitchValueChanged()
    {
        if (pitchToggle != null)
        {
            _pitchActiveness = pitchToggle.isOn;
            SaveSignals.Instance.onChangePitchState?.Invoke(_pitchActiveness ? 1 : 0, SaveLoadStates.PitchState, SaveFiles.GameOptions);
        }
    }

    private void OnPitchToggleValueChanged(bool isOn)
    {
        OnPitchValueChanged();
    }
    public void OpenSourcesPanel()
    {
        UISignals.Instance.onOpenPanel?.Invoke(UIPanels.SourcesPanel);
    }

    public void CloseOptionsPanel()
    {
        UISignals.Instance.onClosePanel?.Invoke(UIPanels.OptionsPanel);
    }
    private void SetAudioSource()
    {
        //audioSource.enabled = _audioSourceActiveness;
        //AudioListener.pause = _audioSourceActiveness;
        AudioListener.volume = _audioSourceActiveness ? 1 : 0;
    }


}
