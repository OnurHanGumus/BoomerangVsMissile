using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Commands;
using Controllers;
using Data.UnityObject;
using Data.ValueObject;
using Enums;
using Signals;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
    public class MissileCreatorManager : MonoBehaviour
    {
        #region Self Variables

        #region Public Variables

        #endregion

        #region Serialized Variables
        [SerializeField] private bool isTutorial = true;

        #endregion

        #region Private Variables
        private MissileLevelData _data;
        private int _levelId;
        private int _index = 0;
        private int _destroyedMissileCount = 0;
        private float _lastPosX;

        private float _percentageIndex = 0;
        private List<Range> _rangeList;
        private bool _isLevelFailed = false;

        #endregion

        #endregion

        private void Awake()
        {
            Init();
            SubscribeEvents();
        }

        private void Init()
        {
            _data = GetData();
            _rangeList = new List<Range>(); 
        }

        private void Start()
        {
            isTutorial = LevelSignals.Instance.onGetLevelId() == 0;
        }

        public MissileLevelData GetData() => Resources.Load<CD_MissileCreator>("Data/CD_MissileCreator").Data;

        #region Event Subscription

        private void SubscribeEvents()
        {
            CoreGameSignals.Instance.onPlay += OnPlay;
            CoreGameSignals.Instance.onLevelFailed += OnLevelFailed;
            CoreGameSignals.Instance.onLevelSuccessful += OnLevelSuccess;
            CoreGameSignals.Instance.onRestartLevel += OnRestartLevel;
            MissileSignals.Instance.onMissileDestroyed += OnMissileDestroyed;
            TutorialSignals.Instance.onTutorialSatisfied += OnTutorialSatisfied;
        }

        #endregion

        private IEnumerator InstantiateMissile()
        {
            if (!isTutorial)
            {
                _index++;
            }

            GameObject missile = PoolSignals.Instance.onGetObject((PoolEnums) GetMissileType());
            float posX;
            do
            {
                posX = transform.position.x + Random.Range(-2f, 3f);

            } while ((Mathf.Abs(_lastPosX - posX) <= 0.3f));

            _lastPosX = posX;
            Vector3 missilePos = new Vector3(posX, transform.position.y);
            missile.transform.position = missilePos;
            missile.SetActive(true);
            yield return new WaitForSeconds(_data.MissileData[_levelId].MissileCreateOffset);
            StartCoroutine(InstantiateMissile());
        }

        private int GetMissileType()
        {
            if (_index >= _data.MissileData[_levelId].MissileCount)
            {
                StopAllCoroutines();
            }

            int prefabCount = _data.MissileData[_levelId].MissilePrefabList.Count;
            if (prefabCount <= 1 || _rangeList.Count == 0)
            {
                return 0;
            }

            int rand = Random.Range(0, 100);

            for (int i = 0; i < prefabCount; i++)
            {
                if (i < _rangeList.Count && rand >= _rangeList[i].Start.Value && rand <= _rangeList[i].End.Value)
                {
                    return i;
                }
            }

            return 0;
        }

        private void SetRange()
        {
            _percentageIndex = 0;
            _rangeList.Clear();

            var levelData = _data.MissileData[_levelId];
            int prefabCount = levelData.MissilePrefabList != null ? levelData.MissilePrefabList.Count : 0;
            if (prefabCount == 0)
            {
                return;
            }

            float addedValue = 0f;

            for (int i = 0; i < prefabCount; i++)
            {
                float weight = 1f;
                if (levelData.PercentageList != null && i < levelData.PercentageList.Count)
                {
                    weight = levelData.PercentageList[i];
                }
                addedValue += weight;
            }

            if (addedValue <= 0f)
            {
                addedValue = 1f;
            }

            float unitValue = 100f / addedValue;

            for (int i = 0; i < prefabCount; i++)
            {
                float weight = 1f;
                if (levelData.PercentageList != null && i < levelData.PercentageList.Count)
                {
                    weight = levelData.PercentageList[i];
                }

                int endValue = (int)(_percentageIndex + unitValue * weight);
                _rangeList.Add(new Range((int)_percentageIndex, endValue));
                _percentageIndex = endValue;
            }
        }

        private void OnPlay()
        {
            _levelId = LevelSignals.Instance.onGetCurrentModdedLevel();
            SetRange();
            StartCoroutine(InstantiateMissile());
        }

        private void OnMissileDestroyed(float strength = 0f)
        {
            if (!isTutorial)
            {
                ++_destroyedMissileCount;
            }
            Debug.Log("destroyed missile count: "+_destroyedMissileCount + "\n instantiated missile count: " + _index);
            if (_destroyedMissileCount == _data.MissileData[_levelId].MissileCount)
            {
                if (_isLevelFailed)
                {
                    return;
                }

                CoreGameSignals.Instance.onLevelSuccessful?.Invoke();
                AudioSignals.Instance.onPlaySound(AudioSoundEnums.Win);
                GameObject confeti = PoolSignals.Instance.onGetObject(PoolEnums.Confetti);
                confeti.transform.position = Vector3.zero;
                confeti.SetActive(true);
            }
        }

        private void OnLevelSuccess()
        {
            _percentageIndex = 0;
            _rangeList.Clear();
            ResetSettings();
        }

        private void OnLevelFailed()
        {
            _isLevelFailed = true;
            _percentageIndex = 0;
            _rangeList.Clear();
            ResetSettings();
        }

        private void ResetSettings()
        {
            _index = 0;
            _destroyedMissileCount = 0;
            StopAllCoroutines();
        }

        private void OnRestartLevel()
        {
            _isLevelFailed = false;
            ResetSettings();
        }

        private void OnTutorialSatisfied()
        {
            isTutorial = false;
        }
    }
}