using System;
using System.Collections.Generic;
using Data.UnityObject;
using Data.ValueObject;
using Enums;
using Extensions;
using Signals;
using UnityEngine;

namespace Managers
{
    public class BoomerangStoreManager : MonoSingleton<BoomerangStoreManager>
    {
        #region Self Variables

        #region Serialized Variables
        [SerializeField] private CD_Boomerangs cdBoomerangs;
        #endregion

        #region Private Variables
        private const string SaveFileName = "SaveFile.es3";
        private const string SelectedItemKey = "SelectedItem";
        private const string BuyItemKey = "BuyItem";

        private List<int> _ownershipList = new List<int>();
        private int _selectedItemId = 0;
        #endregion

        #endregion

        #region Public Properties
        public CD_Boomerangs Config => cdBoomerangs;
        public int SelectedItemId => _selectedItemId;
        public BoomerangItemData CurrentBoomerangData
        {
            get
            {
                if (cdBoomerangs == null)
                {
                    cdBoomerangs = Resources.Load<CD_Boomerangs>("Data/CD_Boomerangs");
                }
                return cdBoomerangs != null ? cdBoomerangs.GetBoomerang(_selectedItemId) : null;
            }
        }
        #endregion

        protected override void Awake()
        {
            base.Awake();
            Init();
            SubscribeEvents();
        }

        private void Init()
        {
            if (cdBoomerangs == null)
            {
                cdBoomerangs = Resources.Load<CD_Boomerangs>("Data/CD_Boomerangs");
            }

            LoadData();
        }

        #region Event Subscription
        private void SubscribeEvents()
        {
            SaveSignals.Instance.onInitializeBoughtItems += OnInitializeBoughtItems;
        }

        private void UnsubscribeEvents()
        {
            if (SaveSignals.Instance != null)
            {
                SaveSignals.Instance.onInitializeBoughtItems -= OnInitializeBoughtItems;
            }
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }
        #endregion

        private void Start()
        {
            // Broadcast initial equipped weapon
            BoomerangSignals.Instance.onSelectBoomerang?.Invoke(_selectedItemId);
        }

        private void LoadData()
        {
            // 1. Load ownership list
            try
            {
                if (ES3.KeyExists(BuyItemKey, SaveFileName))
                {
                    _ownershipList = ES3.Load<List<int>>(BuyItemKey, SaveFileName, new List<int>());
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BoomerangStoreManager] Failed to load {BuyItemKey}: {e.Message}");
            }

            EnsureOwnershipListLength();

            // 2. Load equipped item index
            try
            {
                if (ES3.KeyExists(SelectedItemKey, SaveFileName))
                {
                    _selectedItemId = ES3.Load<int>(SelectedItemKey, SaveFileName, 0);
                }
                else
                {
                    _selectedItemId = 0;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BoomerangStoreManager] Failed to load {SelectedItemKey}: {e.Message}");
                _selectedItemId = 0;
            }

            // Fallback: if selected item is invalid or not owned, fallback to 0
            if (!IsOwned(_selectedItemId))
            {
                _selectedItemId = 0;
                _ownershipList[0] = 1;
            }
        }

        private void EnsureOwnershipListLength()
        {
            int requiredCount = (cdBoomerangs != null && cdBoomerangs.Boomerangs != null) 
                ? Mathf.Max(5, cdBoomerangs.Boomerangs.Count) 
                : 5;

            if (_ownershipList == null)
            {
                _ownershipList = new List<int>();
            }

            while (_ownershipList.Count < requiredCount)
            {
                _ownershipList.Add(0);
            }

            // Starter boomerang (index 0) is always owned
            if (_ownershipList[0] < 1)
            {
                _ownershipList[0] = 1;
            }
        }

        private void OnInitializeBoughtItems(List<int> levels)
        {
            if (levels == null || levels.Count == 0)
            {
                return;
            }

            for (int i = 0; i < levels.Count; i++)
            {
                if (i < _ownershipList.Count)
                {
                    if (levels[i] > 0)
                    {
                        _ownershipList[i] = levels[i];
                    }
                }
                else
                {
                    _ownershipList.Add(levels[i]);
                }
            }

            EnsureOwnershipListLength();
        }

        public bool IsOwned(int id)
        {
            EnsureOwnershipListLength();
            if (id >= 0 && id < _ownershipList.Count)
            {
                return _ownershipList[id] >= 1;
            }
            return id == 0;
        }

        public bool IsEquipped(int id)
        {
            return _selectedItemId == id;
        }

        public bool CanAfford(int id)
        {
            BoomerangItemData data = cdBoomerangs != null ? cdBoomerangs.GetBoomerang(id) : null;
            if (data == null)
            {
                return false;
            }

            int currentMoney = ScoreSignals.Instance.onGetMoney != null ? ScoreSignals.Instance.onGetMoney() : 0;
            return currentMoney >= data.Price;
        }

        public bool TryBuyBoomerang(int id)
        {
            if (IsOwned(id))
            {
                SelectBoomerang(id);
                return true;
            }

            BoomerangItemData data = cdBoomerangs != null ? cdBoomerangs.GetBoomerang(id) : null;
            if (data == null)
            {
                return false;
            }

            int currentMoney = ScoreSignals.Instance.onGetMoney != null ? ScoreSignals.Instance.onGetMoney() : 0;
            if (currentMoney < data.Price)
            {
                return false;
            }

            // Deduct money
            ScoreSignals.Instance.onScoreDecrease?.Invoke(ScoreTypeEnums.Money, data.Price);

            // Mark owned
            EnsureOwnershipListLength();
            _ownershipList[id] = 1;

            // Save ownership
            try
            {
                ES3.Save(BuyItemKey, _ownershipList, SaveFileName);
                SaveSignals.Instance.onBuyItem?.Invoke(_ownershipList, SaveLoadStates.BuyItem, SaveFiles.SaveFile);
            }
            catch (Exception e)
            {
                Debug.LogError($"[BoomerangStoreManager] Failed to save {BuyItemKey}: {e.Message}");
            }

            // Automatically equip purchased boomerang
            SelectBoomerang(id);

            return true;
        }

        public void SelectBoomerang(int id)
        {
            if (!IsOwned(id))
            {
                return;
            }

            _selectedItemId = id;

            // Save selected item
            try
            {
                ES3.Save(SelectedItemKey, _selectedItemId, SaveFileName);
            }
            catch (Exception e)
            {
                Debug.LogError($"[BoomerangStoreManager] Failed to save {SelectedItemKey}: {e.Message}");
            }

            // Notify systems
            BoomerangSignals.Instance.onSelectBoomerang?.Invoke(_selectedItemId);
            BoomerangSignals.Instance.onBoomerangStatsChanged?.Invoke();
        }

        public BoomerangItemData GetBoomerangData(int id)
        {
            return cdBoomerangs != null ? cdBoomerangs.GetBoomerang(id) : null;
        }
    }
}
