using Data.UnityObject;
using Data.ValueObject;
using Managers;
using Signals;
using UnityEngine;

namespace Controllers.Boomerang
{
    public class BoomerangSpawner : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables
        [SerializeField] private CD_Boomerangs cdBoomerangs;
        [SerializeField] private Transform catchArea;
        #endregion

        #region Private Variables
        private GameObject _activeBoomerang;
        private PlayerData _playerData;
        private int _currentEquippedId = -1;
        #endregion

        #endregion

        private void Awake()
        {
            Init();
            SubscribeEvents();
        }

        private void Init()
        {
            if (cdBoomerangs == null)
            {
                cdBoomerangs = Resources.Load<CD_Boomerangs>("Data/CD_Boomerangs");
            }
            _playerData = Resources.Load<CD_Player>("Data/CD_Player").Data;

            FindCatchArea();
        }

        private void FindCatchArea()
        {
            if (catchArea == null)
            {
                var catchObj = GameObject.FindWithTag("CatchArea");
                if (catchObj != null)
                {
                    catchArea = catchObj.transform;
                }
            }
        }

        #region Event Subscription
        private void SubscribeEvents()
        {
            BoomerangSignals.Instance.onSelectBoomerang += OnBoomerangSelected;
        }

        private void UnsubscribeEvents()
        {
            if (BoomerangSignals.Instance != null)
            {
                BoomerangSignals.Instance.onSelectBoomerang -= OnBoomerangSelected;
            }
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }
        #endregion

        private void Start()
        {
            int equippedId = BoomerangStoreManager.Instance != null 
                ? BoomerangStoreManager.Instance.SelectedItemId 
                : 0;

            SpawnBoomerang(equippedId);
        }

        private void OnBoomerangSelected(int id)
        {
            SpawnBoomerang(id);
        }

        public void SpawnBoomerang(int id)
        {
            if (_activeBoomerang != null && _currentEquippedId == id)
            {
                return;
            }

            if (cdBoomerangs == null)
            {
                cdBoomerangs = Resources.Load<CD_Boomerangs>("Data/CD_Boomerangs");
            }

            BoomerangItemData data = cdBoomerangs != null ? cdBoomerangs.GetBoomerang(id) : null;
            if (data == null || data.Prefab == null)
            {
                Debug.LogWarning($"[BoomerangSpawner] Boomerang data or prefab for id {id} not found.");
                return;
            }

            // Find and destroy any existing boomerang instance in the scene
            if (_activeBoomerang != null)
            {
                _activeBoomerang.SetActive(false);
                Destroy(_activeBoomerang);
                _activeBoomerang = null;
            }
            else
            {
                // Also check for pre-placed Boomerang GameObject in scene
                var existingMgr = FindFirstObjectByType<BoomerangManager>();
                if (existingMgr != null)
                {
                    existingMgr.gameObject.SetActive(false);
                    Destroy(existingMgr.gameObject);
                }
            }

            // Determine spawn position: CatchArea if present, fallback to PlayerData coords
            FindCatchArea();

            Vector3 spawnPos;
            if (catchArea != null)
            {
                spawnPos = catchArea.position;
            }
            else if (_playerData != null)
            {
                spawnPos = new Vector3(_playerData.BoomerangInitPosX, _playerData.BoomerangInitPosY, 0f);
            }
            else
            {
                spawnPos = new Vector3(0f, -4.028f, 0f);
            }

            // Spawn directly in the Catch Area (NOT parented to player's hand)
            _activeBoomerang = Instantiate(data.Prefab, spawnPos, Quaternion.identity);
            _activeBoomerang.name = "Boomerang";
            _activeBoomerang.transform.SetParent(null); // Keep at root level, never parented to player hand
            _currentEquippedId = id;

            // Apply weapon-specific stats (scale, etc.)
            var mgr = _activeBoomerang.GetComponent<BoomerangManager>();
            if (mgr != null)
            {
                mgr.ApplyStats();
            }
        }
    }
}
