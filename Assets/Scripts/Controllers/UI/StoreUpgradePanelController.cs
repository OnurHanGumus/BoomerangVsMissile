using System.Collections.Generic;
using Data.UnityObject;
using Data.ValueObject;
using Enums;
using Managers;
using Signals;
using UnityEngine;

namespace Controllers.UI
{
    public class StoreUpgradePanelController : MonoBehaviour
    {
        #region Serialized Variables
        [Header("Dynamic Card Generation")]
        [Tooltip("Prefab for an upgrade card. If null, pre-placed cards in Cards Container will be used.")]
        [SerializeField] private StoreUpgradeCardUI cardPrefab;

        [Tooltip("Parent transform where upgrade cards will be placed or instantiated.")]
        [SerializeField] private Transform cardsContainer;

        [Header("Pre-placed Cards (Optional fallback)")]
        [SerializeField] private List<StoreUpgradeCardUI> existingCards = new List<StoreUpgradeCardUI>();
        #endregion

        #region Private Variables
        private readonly List<StoreUpgradeCardUI> _activeCards = new List<StoreUpgradeCardUI>();
        private bool _isInitialized = false;
        #endregion

        private void Awake()
        {
            SubscribeEvents();
        }

        private void Start()
        {
            InitializeCards();
        }

        private void OnEnable()
        {
            if (_isInitialized)
            {
                RefreshAllCards();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        #region Event Subscription
        private void SubscribeEvents()
        {
            BoomerangSignals.Instance.onBoomerangStatsChanged += RefreshAllCards;
            ScoreSignals.Instance.onScoreIncrease += OnScoreChanged;
            ScoreSignals.Instance.onScoreDecrease += OnScoreChanged;
            if (UISignals.Instance != null)
            {
                UISignals.Instance.onOpenPanel += OnOpenPanel;
            }
        }

        private void UnsubscribeEvents()
        {
            if (BoomerangSignals.Instance != null)
            {
                BoomerangSignals.Instance.onBoomerangStatsChanged -= RefreshAllCards;
            }

            if (ScoreSignals.Instance != null)
            {
                ScoreSignals.Instance.onScoreIncrease -= OnScoreChanged;
                ScoreSignals.Instance.onScoreDecrease -= OnScoreChanged;
            }

            if (UISignals.Instance != null)
            {
                UISignals.Instance.onOpenPanel -= OnOpenPanel;
            }
        }

        private void OnOpenPanel(UIPanels panel)
        {
            if (panel == UIPanels.StorePanel)
            {
                RefreshAllCards();
            }
        }

        private void OnScoreChanged(ScoreTypeEnums type, int amount)
        {
            if (type == ScoreTypeEnums.Gem)
            {
                RefreshAllCards();
            }
        }
        #endregion

        public void InitializeCards()
        {
            if (_isInitialized)
            {
                RefreshAllCards();
                return;
            }

            _activeCards.Clear();

            CD_BoomerangUpgrades config = BoomerangUpgradeManager.Instance != null
                ? BoomerangUpgradeManager.Instance.UpgradesConfig
                : Resources.Load<CD_BoomerangUpgrades>("Data/CD_BoomerangUpgrades");

            if (config == null || config.Upgrades == null)
            {
                Debug.LogWarning("[StoreUpgradePanelController] CD_BoomerangUpgrades configuration not found.");
                return;
            }

            Transform container = cardsContainer != null ? cardsContainer : transform;

            // 1. If prefab provided, dynamically instantiate cards for every upgrade definition
            if (cardPrefab != null)
            {
                for (int i = 0; i < config.Upgrades.Count; i++)
                {
                    UpgradeDefinition def = config.Upgrades[i];
                    if (def == null) continue;

                    StoreUpgradeCardUI card = Instantiate(cardPrefab, container);
                    card.gameObject.SetActive(true);
                    card.Setup(def);
                    _activeCards.Add(card);
                }
            }
            // 2. Otherwise use existing child cards if configured or found in container
            else
            {
                if (existingCards != null && existingCards.Count > 0)
                {
                    _activeCards.AddRange(existingCards);
                }
                else
                {
                    _activeCards.AddRange(container.GetComponentsInChildren<StoreUpgradeCardUI>(true));
                }

                // Match pre-placed cards with definitions
                for (int i = 0; i < _activeCards.Count && i < config.Upgrades.Count; i++)
                {
                    if (_activeCards[i] != null && config.Upgrades[i] != null)
                    {
                        _activeCards[i].Setup(config.Upgrades[i]);
                    }
                }
            }

            _isInitialized = true;
            RefreshAllCards();
        }

        public void RefreshAllCards()
        {
            for (int i = 0; i < _activeCards.Count; i++)
            {
                if (_activeCards[i] != null)
                {
                    _activeCards[i].UpdateUI();
                }
            }
        }
    }
}
