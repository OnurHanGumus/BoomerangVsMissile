using System;
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
        [SerializeField] private GameObject missilePrefab;
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
            InstantiateMissile();

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
            CoreGameSignals.Instance.onRestartLevel += OnResetLevel;
        }

        private void UnsubscribeEvents()
        {
            CoreGameSignals.Instance.onPlay -= OnPlay;
            CoreGameSignals.Instance.onRestartLevel -= OnResetLevel;
        }


        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        #endregion

        private async Task InstantiateMissile()
        {
            Instantiate(missilePrefab, new Vector3(transform.position.x + Random.Range(-2f, 3f),transform.position.y), transform.rotation);
            await Task.Delay(3000);
            await InstantiateMissile();
        }
        private void OnPlay()
        {

        }
        private void OnResetLevel()
        {

        }
    }
}