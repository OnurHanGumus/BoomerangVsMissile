using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Signals;
using Enums;

public class PoolManager : MonoBehaviour
{
    #region Self Variables

    #region Serialized Variables

    [SerializeField] private GameObject missilePrefab0, missilePrefab1, missilePrefab2, missilePrefab3, missilePrefab4, missilePrefab5, missilePrefab6;
    [SerializeField] private GameObject explosionPrefab, explosionPinkPrefab;
    [SerializeField] private GameObject particlePrefab;

    [SerializeField] private Dictionary<PoolEnums, List<GameObject>> poolDictionary;


    [SerializeField] private int amountMissile = 50;
    [SerializeField] private int amountParticle = 5;



    #endregion
    #region Private Variables
    private int _levelId = 0;
    #endregion
    #endregion
    private void Awake()
    {
        Init();
        SubscribeEvents();
    }
    private void Init()
    {
        _levelId = LevelSignals.Instance.onGetCurrentModdedLevel();
        poolDictionary = new Dictionary<PoolEnums, List<GameObject>>();
        if (missilePrefab0 != null) InitializePool(PoolEnums.Missile0, missilePrefab0, amountMissile);
        if (missilePrefab1 != null) InitializePool(PoolEnums.Missile1, missilePrefab1, amountMissile);
        if (missilePrefab2 != null) InitializePool(PoolEnums.Missile2, missilePrefab2, amountMissile);
        if (missilePrefab3 != null) InitializePool(PoolEnums.Missile3, missilePrefab3, amountMissile);
        if (missilePrefab4 != null) InitializePool(PoolEnums.Missile4, missilePrefab4, amountMissile);
        if (missilePrefab5 != null) InitializePool(PoolEnums.Missile5, missilePrefab5, amountMissile);
        if (missilePrefab6 != null) InitializePool(PoolEnums.Missile6, missilePrefab6, amountMissile);
        if (explosionPrefab != null) InitializePool(PoolEnums.ExplosionStandard, explosionPrefab, amountParticle);
        if (explosionPinkPrefab != null) InitializePool(PoolEnums.ExplosionPink, explosionPinkPrefab, amountParticle);
        if (particlePrefab != null) InitializePool(PoolEnums.Confetti, particlePrefab, amountParticle);
    }



    #region Event Subscriptions

    private void SubscribeEvents()
    {
        PoolSignals.Instance.onGetPoolManagerObj += OnGetPoolManagerObj;
        PoolSignals.Instance.onGetObject += OnGetObject;
        CoreGameSignals.Instance.onRestartLevel += OnReset;

    }

    #endregion

    private void InitializePool(PoolEnums type, GameObject prefab, int size)
    {
        List<GameObject> tempList = new List<GameObject>();
        GameObject tmp;

        for (int i = 0; i < size; i++)
        {
            tmp = Instantiate(prefab, transform);
            tmp.SetActive(false);
            tempList.Add(tmp);
        }
        poolDictionary.Add(type, tempList);
    }

    public GameObject OnGetObject(PoolEnums type)
    {
        if (!poolDictionary.ContainsKey(type) || poolDictionary[type] == null)
        {
            return null;
        }

        for (int i = 0; i < poolDictionary[type].Count; i++)
        {
            if (!poolDictionary[type][i].activeInHierarchy)
            {
                return poolDictionary[type][i];
            }
        }
        return null;
    }

    public Transform OnGetPoolManagerObj()
    {
        return transform;
    }


    private void OnReset()
    {
        //reset
        if (poolDictionary.ContainsKey(PoolEnums.Missile0)) ResetPool(PoolEnums.Missile0);
        if (poolDictionary.ContainsKey(PoolEnums.Missile1)) ResetPool(PoolEnums.Missile1);
        if (poolDictionary.ContainsKey(PoolEnums.Missile2)) ResetPool(PoolEnums.Missile2);
        if (poolDictionary.ContainsKey(PoolEnums.Missile3)) ResetPool(PoolEnums.Missile3);
        if (poolDictionary.ContainsKey(PoolEnums.Missile4)) ResetPool(PoolEnums.Missile4);
        if (poolDictionary.ContainsKey(PoolEnums.Missile5)) ResetPool(PoolEnums.Missile5);
        if (poolDictionary.ContainsKey(PoolEnums.Missile6)) ResetPool(PoolEnums.Missile6);
        if (poolDictionary.ContainsKey(PoolEnums.ExplosionStandard)) ResetPool(PoolEnums.ExplosionStandard);
        if (poolDictionary.ContainsKey(PoolEnums.ExplosionPink)) ResetPool(PoolEnums.ExplosionPink);
    }

    private void ResetPool(PoolEnums type)
    {
        foreach (var i in poolDictionary[type])
        {
            i.SetActive(false);
        }
    }
}
