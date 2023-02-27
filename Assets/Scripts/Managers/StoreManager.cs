using System;
using System.Collections.Generic;
using Commands;
using Controllers;
using Data.UnityObject;
using Data.ValueObject;
using Extentions;
using Keys;
using Signals;
using UnityEngine;
using Enums;
using System.Collections;
using TMPro;
using DG.Tweening;

namespace Managers
{
    public class StoreManager : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables

        [SerializeField] private List<TextMeshProUGUI> levelTxt;
        [SerializeField] private List<TextMeshProUGUI> upgradeTxt;



        #endregion
        private ItemPricesData _data;
        private List<int> _itemLevels;

        #endregion



        private void Awake()
        {
            Init();
        }


        private void Init()
        {
            _data = GetData();
        }
        private ItemPricesData GetData() => Resources.Load<CD_Prices>("Data/CD_Prices").Data;
        private void Start()
        {
            if (_itemLevels.Count.Equals(0))
            {
                _itemLevels = new List<int>() { 1, 0, 0, 0 };
            }
            UpdateTexts();

        }

        #region Event Subscription

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            SaveSignals.Instance.onInitializeBuyedItems += OnGetStoreLevels;
        }

        private void UnsubscribeEvents()
        {
            SaveSignals.Instance.onInitializeBuyedItems -= OnGetStoreLevels;

        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        #endregion

        public void BuyItem(int id)
        {
            if (_itemLevels[id] >= 1)
            {
                BoomerangSignals.Instance.onSelectBoomerang?.Invoke(id);
                return;
            }


            if (ScoreSignals.Instance.onGetGem() > _data.prices[id])
            {
                ScoreSignals.Instance.onScoreDecrease(ScoreTypeEnums.Gem, _data.prices[id]);
                _itemLevels[id] = _itemLevels[id] + 1;
                SaveSignals.Instance.onBuyItem?.Invoke(_itemLevels, SaveLoadStates.BuyItem, SaveFiles.SaveFile);
                UpdateTexts();
                //AudioSignals.Instance.onPlaySound?.Invoke(Enums.AudioSoundEnums.Click);
                BoomerangSignals.Instance.onSelectBoomerang?.Invoke(id);

            }
            else
            {
                return;
            }
        }

        private void OnGetStoreLevels(List<int> levels)
        {
            SetList(levels);
            UpdateTexts();
        }

        private void SetList(List<int> levels)
        {
            if (levels.Count.Equals(0))
            {
                return;
            }

            _itemLevels = levels;
        }

        private void UpdateTexts()
        {
            for (int i = 0; i < _itemLevels.Count; i++)//textleri initialize et
            {
                if (_itemLevels[i] < 1)
                {

                    //levelTxt[i].text = "LEVEL " + (itemLevels[i] + 1).ToString();
                    upgradeTxt[i].text = _data.prices[i].ToString() + "$";
                }
                else
                {
                    //levelTxt[i].text = "LEVEL " + (itemLevels[i] + 1).ToString();
                    upgradeTxt[i].text = "BUYED";
                }
            }
        }
    }
}