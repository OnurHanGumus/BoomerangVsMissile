using Data.UnityObject;
using Data.ValueObject;
using Enums;
using Interfaces;
using Managers;
using Signals;
using UnityEngine;

namespace Controllers.Missile.Abilities
{
    public class RewardMissileAbility : MonoBehaviour, IMissileDeathEffect
    {
        #region Self Variables

        #region Public Variables
        public RewardMissileData Data
        {
            get
            {
                if (_data == null)
                {
                    LoadData();
                }
                return _data;
            }
        }
        #endregion

        #region Serialized Variables
        [Header("Reward Data")]
        [SerializeField] private CD_RewardMissile cdRewardMissile;

        [Header("Golden Glow Pulse")]
        [SerializeField] private float pulseFrequency = 4.5f;
        [SerializeField] private float lightIntensityAmplitude = 1.75f;
        #endregion

        #region Private Variables
        private static readonly int StatCount = System.Enum.GetValues(typeof(BoomerangStatEnums)).Length;

        private MissileManager _manager;
        private RewardMissileData _data;
        private Light _missileLight;
        private float _baseLightIntensity = 5f;
        private float _pulseSeed;
        private bool _isLevelEnded;
        private bool _isSubscribed;
        #endregion

        #endregion

        private void Awake()
        {
            EnsureInitialized();
        }

        public void Initialize(MissileManager manager)
        {
            _manager = manager;
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_manager == null)
            {
                _manager = GetComponent<MissileManager>();
            }

            if (_missileLight == null)
            {
                _missileLight = GetComponentInChildren<Light>(true);
                if (_missileLight != null)
                {
                    _baseLightIntensity = _missileLight.intensity;
                }
            }

            LoadData();
        }

        private void LoadData()
        {
            if (cdRewardMissile == null)
            {
                cdRewardMissile = Resources.Load<CD_RewardMissile>("Data/Missiles/CD_RewardMissile");
            }

            _data = cdRewardMissile != null && cdRewardMissile.Data != null
                ? cdRewardMissile.Data
                : new RewardMissileData();
        }

        #region Event Subscription
        private void OnEnable()
        {
            EnsureInitialized();
            _isLevelEnded = false;
            _pulseSeed = Random.Range(0f, Mathf.PI * 2f);
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
            if (_missileLight != null)
            {
                _missileLight.intensity = _baseLightIntensity;
            }
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            if (_isSubscribed || CoreGameSignals.Instance == null)
            {
                return;
            }

            CoreGameSignals.Instance.onLevelFailed += OnLevelEnded;
            CoreGameSignals.Instance.onLevelSuccessful += OnLevelEnded;
            _isSubscribed = true;
        }

        private void UnsubscribeEvents()
        {
            if (!_isSubscribed)
            {
                return;
            }

            if (CoreGameSignals.Instance != null)
            {
                CoreGameSignals.Instance.onLevelFailed -= OnLevelEnded;
                CoreGameSignals.Instance.onLevelSuccessful -= OnLevelEnded;
            }

            _isSubscribed = false;
        }
        #endregion

        private void OnLevelEnded()
        {
            _isLevelEnded = true;
        }

        private void Update()
        {
            if (_missileLight != null && _missileLight.enabled)
            {
                float wave = Mathf.Sin((Time.time + _pulseSeed) * pulseFrequency);
                _missileLight.intensity = Mathf.Max(0.5f, _baseLightIntensity + (wave * lightIntensityAmplitude));
            }
        }

        public bool OnMissileDeath(Vector3 position, bool isLevelEnd)
        {
            if (isLevelEnd || _isLevelEnded)
            {
                return false;
            }

            RewardMissileData rewardData = Data;
            BoomerangStatEnums chosenStat = SelectRewardStat(rewardData);

            if (PoolSignals.Instance != null)
            {
                GameObject spark = PoolSignals.Instance.onGetObject?.Invoke(PoolEnums.HitSpark);
                if (spark != null)
                {
                    spark.transform.position = position;
                    spark.SetActive(true);
                }
            }

            if (BoomerangSignals.Instance != null)
            {
                BoomerangSignals.Instance.onRewardBuffTriggered?.Invoke(chosenStat, rewardData, position);
            }

            if (AudioSignals.Instance != null)
            {
                AudioSignals.Instance.onPlaySoundWithPitch?.Invoke(AudioSoundEnums.Combo, 1.25f);
            }

            if (PitchSignals.Instance != null)
            {
                PitchSignals.Instance.onPlayPitch?.Invoke(PitchEnums.Combo);
            }

            return false;
        }

        private static BoomerangStatEnums SelectRewardStat(RewardMissileData rewardData)
        {
            if (rewardData != null && rewardData.SelectionMode == RewardSelectionMode.FixedStat)
            {
                return rewardData.FixedStatType;
            }

            int roll = Random.Range(0, StatCount);
            return (BoomerangStatEnums)roll;
        }
    }
}
