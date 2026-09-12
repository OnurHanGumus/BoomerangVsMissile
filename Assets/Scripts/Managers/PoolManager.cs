using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Signals;
using Enums;

public class PoolManager : MonoBehaviour
{
    #region Self Variables

    #region Serialized Variables

    [SerializeField] private GameObject Missile_1_Default, Missile_2_Small, Missile_3_Exploder, Missile_4_Armored, Missile_5_Cluster, Missile_6_Cluster_Small;
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
        if (Missile_1_Default != null) InitializePool(PoolEnums.Missile_1_Default, Missile_1_Default, amountMissile);
        if (Missile_2_Small != null) InitializePool(PoolEnums.Missile_2_Small, Missile_2_Small, amountMissile);
        if (Missile_3_Exploder != null) InitializePool(PoolEnums.Missile_3_Exploder, Missile_3_Exploder, amountMissile);
        if (Missile_4_Armored != null) InitializePool(PoolEnums.Missile_4_Armored, Missile_4_Armored, amountMissile);
        if (Missile_5_Cluster != null) InitializePool(PoolEnums.Missile_5_Cluster, Missile_5_Cluster, amountMissile);
        if (Missile_6_Cluster_Small != null) InitializePool(PoolEnums.Missile_6_Cluster_Small, Missile_6_Cluster_Small, amountMissile);
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
        if (poolDictionary.ContainsKey(PoolEnums.Missile_1_Default)) ResetPool(PoolEnums.Missile_1_Default);
        if (poolDictionary.ContainsKey(PoolEnums.Missile_2_Small)) ResetPool(PoolEnums.Missile_2_Small);
        if (poolDictionary.ContainsKey(PoolEnums.Missile_3_Exploder)) ResetPool(PoolEnums.Missile_3_Exploder);
        if (poolDictionary.ContainsKey(PoolEnums.Missile_4_Armored)) ResetPool(PoolEnums.Missile_4_Armored);
        if (poolDictionary.ContainsKey(PoolEnums.Missile_5_Cluster)) ResetPool(PoolEnums.Missile_5_Cluster);
        if (poolDictionary.ContainsKey(PoolEnums.Missile_6_Cluster_Small)) ResetPool(PoolEnums.Missile_6_Cluster_Small);
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
