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
    [SerializeField] private Toggle pitchToggle;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private float maxMusicVolume = 0.2f;
    [SerializeField] private AudioSource audioSource;
    #endregion
    #region Private Variables
    private bool _pitchActiveness;
    private float _musicVolume;
    #endregion
    #endregion

    private void Start()
    {
        musicVolumeSlider.minValue = 0f;
        musicVolumeSlider.maxValue = maxMusicVolume;
        _musicVolume = Mathf.Clamp(
            SaveSignals.Instance.onGetMusicVolume(SaveLoadStates.MusicVolume, SaveFiles.GameOptions),
            0f,
            maxMusicVolume);
        musicVolumeSlider.SetValueWithoutNotify(_musicVolume);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        if (pitchToggle != null)
        {
            _pitchActiveness = SaveSignals.Instance.onGetPitchState(SaveLoadStates.PitchState, SaveFiles.GameOptions) == 1;
            pitchToggle.isOn = _pitchActiveness;
            pitchToggle.onValueChanged.AddListener(OnPitchToggleValueChanged);
        }
    }

    private void OnDestroy()
    {
        musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
    }

    private void OnMusicVolumeChanged(float value)
    {
        _musicVolume = Mathf.Clamp(value, 0f, maxMusicVolume);
        AudioSignals.Instance.onChangeMusicVolume(_musicVolume);
        SaveSignals.Instance.onChangeMusicVolume(_musicVolume, SaveLoadStates.MusicVolume, SaveFiles.GameOptions);
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
        UISignals.Instance.onOpenPanel?.Invoke(UIPanels.StartPanel);

    }


}
