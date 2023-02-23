using System;
using System.Collections.Generic;
using Commands;
using Controllers;
using Data.UnityObject;
using Data.ValueObject;
using Enums;
using Signals;
using UnityEngine;

namespace Managers
{
    public class MissileManager : MonoBehaviour
    {
        #region Self Variables

        #region Public Variables
        public PoolEnums ParticleType;
        public bool IsPink = false;

        #endregion

        #region Serialized Variables

        #endregion

        #region Private Variables
        private PlayerData _data;
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
        public PlayerData GetData() => Resources.Load<CD_Player>("Data/CD_Player").Data;

        #region Event Subscription

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            CoreGameSignals.Instance.onPlay += OnPlay;
            CoreGameSignals.Instance.onLevelSuccessful += OnLevelSuccessful;
            CoreGameSignals.Instance.onRestartLevel += OnResetLevel;
            MissileSignals.Instance.onPinkMissileDestroyed += OnPinkMissileDestroyed;
        }

        private void UnsubscribeEvents()
        {
            CoreGameSignals.Instance.onPlay -= OnPlay;
            CoreGameSignals.Instance.onLevelSuccessful -= OnLevelSuccessful;
            CoreGameSignals.Instance.onRestartLevel -= OnResetLevel;
            MissileSignals.Instance.onPinkMissileDestroyed -= OnPinkMissileDestroyed;
        }


        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        #endregion
        public void Explode()
        {
            MissileSignals.Instance.onMissileDestroyed?.Invoke();

            GameObject particle = PoolSignals.Instance.onGetObject?.Invoke(ParticleType);
            particle.transform.position = transform.position;
            particle.gameObject.SetActive(true);
            if (IsPink)
            {
                MissileSignals.Instance.onPinkMissileDestroyed?.Invoke();
            }
            gameObject.SetActive(false);
        }
        private void OnPlay()
        {

        }

        private void OnPinkMissileDestroyed()
        {
            if (IsPink)
            {
                return;
            }
            Explode();
        }
        private void OnLevelSuccessful()
        {
            Explode();
        }
        private void OnResetLevel()
        {

        }
    }
}