using System;
using System.Collections.Generic;
using Enums;
using RichTap;
using RichTap.Common;
using RichTap.Source;
using Signals;
using UnityEngine;

namespace Managers
{
    public class PitchManager : MonoBehaviour
    {
        [Serializable]
        public struct PitchPresetMapping
        {
            public PitchEnums pitchType;
            public RichtapPreset preset;
            [Range(1, 255)] public int amplitude;

            public PitchPresetMapping(PitchEnums pitchType, RichtapPreset preset, int amplitude = 255)
            {
                this.pitchType = pitchType;
                this.preset = preset;
                this.amplitude = amplitude;
            }
        }

        #region Serialized Variables

        [SerializeField] private List<PitchPresetMapping> presetMappings = new List<PitchPresetMapping>();

        #endregion

        #region Private Variables

        private readonly Dictionary<PitchEnums, PitchPresetMapping> _mappingLookup = new Dictionary<PitchEnums, PitchPresetMapping>();
        private bool _isPitchEnabled = true;

        #endregion

        public bool IsPitchEnabled => _isPitchEnabled;

        private void Awake()
        {
            InitializeDefaultMappings();
            BuildLookupTable();
            EnsureRichtapInitialized();
            SubscribeEvents();
        }

        private void Start()
        {
            _isPitchEnabled = SaveSignals.Instance.onGetPitchState(SaveLoadStates.PitchState, SaveFiles.GameOptions) == 1;
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        #region Event Subscriptions

        private void SubscribeEvents()
        {
            PitchSignals.Instance.onPlayPitch += OnPlayPitch;
            SaveSignals.Instance.onChangePitchState += OnChangePitchState;
        }

        private void UnsubscribeEvents()
        {
            if (PitchSignals.Instance != null)
            {
                PitchSignals.Instance.onPlayPitch -= OnPlayPitch;
            }

            if (SaveSignals.Instance != null)
            {
                SaveSignals.Instance.onChangePitchState -= OnChangePitchState;
            }
        }

        private void OnChangePitchState(int state, SaveLoadStates saveState, SaveFiles saveFile)
        {
            if (saveState == SaveLoadStates.PitchState)
            {
                _isPitchEnabled = state == 1;
            }
        }

        #endregion

        private void InitializeDefaultMappings()
        {
            if (presetMappings == null || presetMappings.Count == 0)
            {
                presetMappings = new List<PitchPresetMapping>
                {
                    new PitchPresetMapping(PitchEnums.Explosion, RichtapPreset.RT_BOMB, 255),
                    new PitchPresetMapping(PitchEnums.Click, RichtapPreset.RT_CLICK, 255),
                    new PitchPresetMapping(PitchEnums.Combo, RichtapPreset.RT_SUCCESS, 255),
                    new PitchPresetMapping(PitchEnums.Hit, RichtapPreset.RT_THUD, 255),
                    new PitchPresetMapping(PitchEnums.Win, RichtapPreset.RT_VICTORY, 255),
                    new PitchPresetMapping(PitchEnums.Lose, RichtapPreset.RT_FAILURE, 255),
                    new PitchPresetMapping(PitchEnums.ShieldBroke, RichtapPreset.RT_REJECT, 255)
                };
            }
        }

        private void BuildLookupTable()
        {
            _mappingLookup.Clear();
            for (int i = 0; i < presetMappings.Count; i++)
            {
                var mapping = presetMappings[i];
                if (!_mappingLookup.ContainsKey(mapping.pitchType))
                {
                    _mappingLookup.Add(mapping.pitchType, mapping);
                }
            }
        }

        private void EnsureRichtapInitialized()
        {
            try
            {
                // Accessing RichtapManager.Instance ensures the singleton is created and initialized
                var _ = RichtapManager.Instance;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PitchManager] Failed to initialize RichtapManager: {ex.Message}");
            }
        }

        public void PlayPitch(PitchEnums id)
        {
            OnPlayPitch(id);
        }

        private void OnPlayPitch(PitchEnums id)
        {
            if (!_isPitchEnabled)
            {
                return;
            }

            try
            {
                if (!_mappingLookup.TryGetValue(id, out var mapping))
                {
                    // Fallback to RT_CLICK if unmapped
                    mapping = new PitchPresetMapping(id, RichtapPreset.RT_CLICK, 255);
                }

                var effect = RichtapPresetEffect.BuildEffect(mapping.preset, mapping.amplitude);
                effect?.Play();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PitchManager] Error playing pitch effect for {id}: {ex.Message}");
            }
        }
    }
}
