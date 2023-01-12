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
            CoreGameSignals.Instance.onLevelFailed += OnLevelFailed;
        }

        private void UnsubscribeEvents()
        {
            CoreGameSignals.Instance.onPlay -= OnPlay;
            CoreGameSignals.Instance.onLevelFailed -= OnLevelFailed;
        }


        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        #endregion
       
        private IEnumerator InstantiateMissile()
        {
            //Instantiate(missilePrefab, new Vector3(transform.position.x + Random.Range(-2f, 3f),transform.position.y), transform.rotation);
            GameObject missile = PoolSignals.Instance.onGetObject(PoolEnums.Missile);
            missile.transform.position = new Vector3(transform.position.x + Random.Range(-2f, 3f), transform.position.y);
            missile.SetActive(true);
            yield return new WaitForSeconds(2);
            StartCoroutine(InstantiateMissile());
        }
        private void OnPlay()
        {
            StartCoroutine(InstantiateMissile());
        }
        private void OnLevelFailed()
        {
            StopAllCoroutines();
        }
    }
}