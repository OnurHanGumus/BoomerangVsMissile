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
        #endregion

        #endregion

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _data = GetData();
            
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
            MissileSignals.Instance.onMissileDestroyed += OnMissileDestroyed;
        }

        private void UnsubscribeEvents()
        {
            CoreGameSignals.Instance.onPlay -= OnPlay;
            CoreGameSignals.Instance.onLevelFailed -= OnLevelFailed;
            CoreGameSignals.Instance.onLevelSuccessful -= OnLevelSuccess;
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
            int rand = Random.Range(0, _levelId + 1);
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

        private void OnPlay()
        {
            _levelId = LevelSignals.Instance.onGetCurrentModdedLevel();
            StartCoroutine(InstantiateMissile());
        }

        private void OnMissileDestroyed()
        {
            ++_destroyedMissileCount;
            if (_destroyedMissileCount == _data.MissileData[_levelId].MissileCount)
            {
                CoreGameSignals.Instance.onLevelSuccessful?.Invoke();
            }
        }
        private void OnLevelSuccess()
        {
            _indeks = 0;
            _destroyedMissileCount = 0;
            StopAllCoroutines();
        }
        private void OnLevelFailed()
        {
            _indeks = 0;
            _destroyedMissileCount = 0;
            StopAllCoroutines();
        }
    }
}