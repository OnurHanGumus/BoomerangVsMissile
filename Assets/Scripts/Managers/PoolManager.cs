using System.Collections;
using System.Collections.Generic;
using Data.UnityObject;
using Data.ValueObject;
using Enums;
using Signals;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    #region Self Variables

    #region Serialized Variables

    [SerializeField] private List<CD_PoolSettings> pooledSettings = new List<CD_PoolSettings>();

    #endregion

    #region Private Variables

    private Dictionary<PoolEnums, List<GameObject>> _poolDictionary = new Dictionary<PoolEnums, List<GameObject>>();
    private Dictionary<PoolEnums, GameObject> _prefabDictionary = new Dictionary<PoolEnums, GameObject>();

    private int _totalPooledObjectCount = 0;
    private int _createdPooledObjectCount = 0;

    public int CreatedPoolObjectCount
    {
        get => _createdPooledObjectCount;
        set
        {
            _createdPooledObjectCount = value;
            if (_totalPooledObjectCount > 0)
            {
                PoolSignals.Instance.onAPoolObjectCreated?.Invoke((float)_createdPooledObjectCount / _totalPooledObjectCount);
            }
        }
    }

    #endregion

    #endregion

    #region Unity Lifecycle

    private bool _isSubscribed = false;

    private void Awake()
    {
        Init();
        SubscribeEvents();
    }

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    #endregion

    #region Initialization

    private void Init()
    {
        if (pooledSettings == null || pooledSettings.Count == 0)
        {
            CD_PoolSettings defaultSettings = Resources.Load<CD_PoolSettings>("Data/PoolData/DefaultPoolSettings");
            if (defaultSettings != null)
            {
                pooledSettings = new List<CD_PoolSettings> { defaultSettings };
            }
        }

        BuildPrefabDictionary();
        CalculateTotalPooledObjectCount();
        StartCoroutine(WarmupPool());
    }

    private void BuildPrefabDictionary()
    {
        _prefabDictionary.Clear();
        if (pooledSettings == null) return;

        foreach (var setting in pooledSettings)
        {
            if (setting == null || setting.pooledObjects == null) continue;

            foreach (var pooledObj in setting.pooledObjects)
            {
                if (pooledObj.TypeData != null && pooledObj.TypeData.Prefab != null)
                {
                    _prefabDictionary[pooledObj.TypeData.PoolEnums] = pooledObj.TypeData.Prefab;
                }
            }
        }
    }

    private void CalculateTotalPooledObjectCount()
    {
        _totalPooledObjectCount = 0;
        if (pooledSettings == null) return;

        foreach (var setting in pooledSettings)
        {
            if (setting == null || setting.pooledObjects == null) continue;

            foreach (var pooledObj in setting.pooledObjects)
            {
                _totalPooledObjectCount += pooledObj.Amounts;
            }
        }
    }

    private IEnumerator WarmupPool()
    {
        yield return null;

        if (pooledSettings != null)
        {
            foreach (var setting in pooledSettings)
            {
                if (setting == null || setting.pooledObjects == null) continue;

                foreach (var pooledObj in setting.pooledObjects)
                {
                    if (pooledObj.TypeData == null || pooledObj.TypeData.Prefab == null) continue;

                    PoolEnums type = pooledObj.TypeData.PoolEnums;
                    if (!_poolDictionary.ContainsKey(type))
                    {
                        _poolDictionary[type] = new List<GameObject>();
                    }

                    for (int i = 0; i < pooledObj.Amounts; i++)
                    {
                        GameObject obj = Instantiate(pooledObj.TypeData.Prefab, transform);
                        obj.SetActive(false);
                        _poolDictionary[type].Add(obj);
                        CreatedPoolObjectCount++;

                        if (CreatedPoolObjectCount % 10 == 0)
                        {
                            yield return null;
                        }
                    }
                }
            }
        }

        PoolSignals.Instance.onPoolInitialized?.Invoke();
    }

    #endregion

    #region Event Subscriptions

    private void SubscribeEvents()
    {
        if (_isSubscribed) return;
        _isSubscribed = true;

        PoolSignals.Instance.onGetObject += OnGetObject;
        PoolSignals.Instance.onGetObjectAtPosition += OnGetObject;
        PoolSignals.Instance.onGetObjectWithRotation += OnGetObject;
        PoolSignals.Instance.onGetPoolManagerObj += OnGetPoolManagerObj;
        PoolSignals.Instance.onGetPoolTransform += OnGetPoolManagerObj;
        PoolSignals.Instance.onSetParentAsPool += OnSetParentAsPool;

        if (CoreGameSignals.Instance != null)
        {
            CoreGameSignals.Instance.onRestartLevel += OnReset;
            CoreGameSignals.Instance.onReset += OnReset;
        }
    }

    private void UnsubscribeEvents()
    {
        if (!_isSubscribed) return;
        _isSubscribed = false;

        if (PoolSignals.Instance != null)
        {
            PoolSignals.Instance.onGetObject -= OnGetObject;
            PoolSignals.Instance.onGetObjectAtPosition -= OnGetObject;
            PoolSignals.Instance.onGetObjectWithRotation -= OnGetObject;
            PoolSignals.Instance.onGetPoolManagerObj -= OnGetPoolManagerObj;
            PoolSignals.Instance.onGetPoolTransform -= OnGetPoolManagerObj;
            PoolSignals.Instance.onSetParentAsPool -= OnSetParentAsPool;
        }

        if (CoreGameSignals.Instance != null)
        {
            CoreGameSignals.Instance.onRestartLevel -= OnReset;
            CoreGameSignals.Instance.onReset -= OnReset;
        }
    }

    #endregion

    #region Pool Operations

    public GameObject OnGetObject(PoolEnums type)
    {
        if (!_poolDictionary.ContainsKey(type))
        {
            _poolDictionary[type] = new List<GameObject>();
        }

        List<GameObject> poolList = _poolDictionary[type];
        for (int i = 0; i < poolList.Count; i++)
        {
            if (poolList[i] != null && !poolList[i].activeInHierarchy)
            {
                return poolList[i];
            }
        }

        return ExpandPool(type);
    }

    public GameObject OnGetObject(PoolEnums type, Vector3 position)
    {
        GameObject obj = OnGetObject(type);
        if (obj != null)
        {
            obj.transform.position = position;
        }
        return obj;
    }

    public GameObject OnGetObject(PoolEnums type, Vector3 position, Quaternion rotation)
    {
        GameObject obj = OnGetObject(type);
        if (obj != null)
        {
            obj.transform.position = position;
            obj.transform.rotation = rotation;
        }
        return obj;
    }

    private GameObject ExpandPool(PoolEnums type)
    {
        GameObject prefab = GetPrefabForType(type);
        if (prefab == null)
        {
            Debug.LogWarning($"[PoolManager] No prefab found for PoolEnums: {type}");
            return null;
        }

        GameObject expandedObject = Instantiate(prefab, transform);
        expandedObject.SetActive(false);

        if (!_poolDictionary.ContainsKey(type))
        {
            _poolDictionary[type] = new List<GameObject>();
        }
        _poolDictionary[type].Add(expandedObject);

        return expandedObject;
    }

    private GameObject GetPrefabForType(PoolEnums type)
    {
        if (_prefabDictionary.TryGetValue(type, out GameObject prefab) && prefab != null)
        {
            return prefab;
        }

        // Fallback search through pooledSettings if dictionary missed it
        if (pooledSettings != null)
        {
            foreach (var setting in pooledSettings)
            {
                if (setting == null || setting.pooledObjects == null) continue;

                foreach (var pooledObj in setting.pooledObjects)
                {
                    if (pooledObj.TypeData != null && pooledObj.TypeData.PoolEnums == type && pooledObj.TypeData.Prefab != null)
                    {
                        _prefabDictionary[type] = pooledObj.TypeData.Prefab;
                        return pooledObj.TypeData.Prefab;
                    }
                }
            }
        }

        return null;
    }

    public Transform OnGetPoolManagerObj()
    {
        return transform;
    }

    private void OnSetParentAsPool(Transform poolObject)
    {
        if (poolObject != null)
        {
            poolObject.SetParent(transform);
        }
    }

    private void OnReset()
    {
        foreach (var poolPair in _poolDictionary)
        {
            ResetPool(poolPair.Key);
        }
    }

    private void ResetPool(PoolEnums type)
    {
        if (!_poolDictionary.ContainsKey(type) || _poolDictionary[type] == null) return;

        foreach (var obj in _poolDictionary[type])
        {
            if (obj != null)
            {
                obj.transform.localScale = Vector3.one;
                obj.SetActive(false);
            }
        }
    }

    #endregion
}
