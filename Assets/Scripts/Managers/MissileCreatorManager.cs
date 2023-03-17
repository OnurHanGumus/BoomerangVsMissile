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

        #endregion

        #region Private Variables
        private MissileLevelData _data;
        private int _levelId;
        private int _indeks = 0;
        private int _destroyedMissileCount = 0;
        private float _lastPosX;

        private float _percentageIndeks = 0;
        private List<Range> _rangeList;
        private bool _isLevelFailed = false;

        #endregion

        #endregion

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _data = GetData();
            _rangeList = new List<Range>(); 


        }

        private void Start()
        {
        }
        public MissileLevelData GetData() => Resources.Load<CD_Missile>("Data/CD_Missile").Data;

        #region Event Subscription

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            CoreGameSignals.Instance.onPlay += OnPlay;
            CoreGameSignals.Instance.onLevelFailed += OnLevelFailed;
            CoreGameSignals.Instance.onLevelSuccessful += OnLevelSuccess;
            CoreGameSignals.Instance.onRestartLevel += OnRestartLevel;
            MissileSignals.Instance.onMissileDestroyed += OnMissileDestroyed;
        }

        private void UnsubscribeEvents()
        {
            CoreGameSignals.Instance.onPlay -= OnPlay;
            CoreGameSignals.Instance.onLevelFailed -= OnLevelFailed;
            CoreGameSignals.Instance.onLevelSuccessful -= OnLevelSuccess;
            CoreGameSignals.Instance.onRestartLevel -= OnRestartLevel;
            MissileSignals.Instance.onMissileDestroyed -= OnMissileDestroyed;
        }


        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        #endregion

        private IEnumerator InstantiateMissile()
        {
            _indeks++;
            if (_indeks >= _data.MissileData[_levelId].MissileCount)
            {
                StopAllCoroutines();
            }
            int rand = Random.Range(0, 100);

            for (int i = 0; i < _data.MissileData[_levelId].MissilePrefabList.Count; i++)
            {
                if (rand >= _rangeList[i].Start.Value && rand <= _rangeList[i].End.Value)
                {
                    rand = i;
                    break;
                }
            }
            if (rand > _data.MissileData.Count)
            {
                Debug.Log("hiçbiri ýolmadý");
                rand = 0;
            }
            
            //int rand = Random.Range(0, _levelId + 1);
            GameObject missile = PoolSignals.Instance.onGetObject((PoolEnums)rand);
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
        private void SetRange()
        {
            float addedValue = 0f;
            for (int i = 0; i < _data.MissileData[_levelId].MissilePrefabList.Count; i++)
            {
                addedValue += _data.PercentageList[i];
            }
            float unitValue = 100f / addedValue;

            for (int i = 0; i < _data.MissileData[_levelId].MissilePrefabList.Count; i++)
            {
                int endValue = (int)(_percentageIndeks + unitValue * _data.PercentageList[i]);
                _rangeList.Add(new Range((int)_percentageIndeks, endValue));
                _percentageIndeks = endValue;
            }
            Debug.Log("list count: " + _rangeList.Count);
            for (int i = 0; i < _rangeList.Count; i++)
            {
                Debug.Log(_rangeList[i]);

            }
        }
        private void OnPlay()
        {
            _levelId = LevelSignals.Instance.onGetCurrentModdedLevel();
            SetRange();
            StartCoroutine(InstantiateMissile());
        }

        

        private void OnMissileDestroyed()
        {
            ++_destroyedMissileCount;
            if (_destroyedMissileCount == _data.MissileData[_levelId].MissileCount)
            {
                if (_isLevelFailed)
                {
                    return;
                }
                CoreGameSignals.Instance.onLevelSuccessful?.Invoke();
                AudioSignals.Instance.onPlaySound(AudioSoundEnums.Win);
            }
        }
        private void OnLevelSuccess()
        {
            _percentageIndeks = 0;
            _rangeList.Clear();
            ResetSettings();
        }
        private void OnLevelFailed()
        {
            _isLevelFailed = true;
            _percentageIndeks = 0;
            _rangeList.Clear();
            ResetSettings();
        }
        private void ResetSettings()
        {
            _indeks = 0;
            _destroyedMissileCount = 0;
            StopAllCoroutines();
        }

        private void OnRestartLevel()
        {
            _isLevelFailed = false;
        }
    }
}