using System.Collections.Generic;
using Enums;
using Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers.UI
{
    public class LevelSelectionPanelController : MonoBehaviour
    {
        #region Self Variables

        #region Public Variables
        public CanvasGroup PanelCanvasGroup => canvasGroup;
        #endregion

        #region Serialized Variables
        [Header("Panel References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform contentRect;
        [SerializeField] private RectTransform viewportRect;
        [SerializeField] private LevelSelectionTileUI tilePrefab;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button prevPageButton;
        [SerializeField] private Button nextPageButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI progressSubtitleText;
        [SerializeField] private TextMeshProUGUI pageRangeText;

        [Header("Level Configuration")]
        [SerializeField] private int totalLevels = 1000;
        [SerializeField] private int levelsPerPage = 50;
        #endregion

        #region Private Variables
        private readonly List<LevelSelectionTileUI> _tiles = new List<LevelSelectionTileUI>();
        private int _maxReachedLevelId;
        private int _currentLevelId;
        private int _currentPageIndex;
        private int _totalPages;
        #endregion

        #endregion

        private void Awake()
        {
            Init();
            SubscribeEvents();
        }

        private void Init()
        {
            _totalPages = Mathf.Max(1, Mathf.CeilToInt((float)totalLevels / levelsPerPage));
            CreateTiles();
        }

        private void CreateTiles()
        {
            int tileCount = Mathf.Min(totalLevels, levelsPerPage);
            _tiles.Capacity = tileCount;

            for (int i = 0; i < tileCount; i++)
            {
                LevelSelectionTileUI tile = Instantiate(tilePrefab, contentRect);
                tile.Initialize(OnLevelSelected);
                _tiles.Add(tile);
            }
        }

        #region Event Subscription
        private void SubscribeEvents()
        {
            UISignals.Instance.onOpenPanel += OnOpenPanel;
            closeButton.onClick.AddListener(OnCloseButtonClicked);
            prevPageButton.onClick.AddListener(OnPrevPageClicked);
            nextPageButton.onClick.AddListener(OnNextPageClicked);
        }

        private void UnsubscribeEvents()
        {
            UISignals.Instance.onOpenPanel -= OnOpenPanel;
            closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            prevPageButton.onClick.RemoveListener(OnPrevPageClicked);
            nextPageButton.onClick.RemoveListener(OnNextPageClicked);
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }
        #endregion

        private void OnOpenPanel(UIPanels panel)
        {
            if (panel != UIPanels.LevelSelectionPanel)
            {
                return;
            }

            RefreshAndFocusCurrentLevel();
        }

        private void RefreshAndFocusCurrentLevel()
        {
            _maxReachedLevelId = LevelSignals.Instance.onGetMaxReachedLevelId();
            _currentLevelId = LevelSignals.Instance.onGetLevelId();

            int unlockedDisplayCount = Mathf.Clamp(_maxReachedLevelId + 1, 1, totalLevels);
            progressSubtitleText.text = $"UNLOCKED: {unlockedDisplayCount} / {totalLevels}";

            int clampedLevel = Mathf.Clamp(_currentLevelId, 0, totalLevels - 1);
            int targetPage = clampedLevel / levelsPerPage;
            ShowPage(targetPage, true);
        }

        private void ShowPage(int pageIndex, bool focusCurrentLevel)
        {
            _currentPageIndex = Mathf.Clamp(pageIndex, 0, _totalPages - 1);

            int startLevel = _currentPageIndex * levelsPerPage;
            int endLevel = Mathf.Min(totalLevels, startLevel + levelsPerPage);

            pageRangeText.text = $"{startLevel + 1} - {endLevel}";
            prevPageButton.interactable = _currentPageIndex > 0;
            nextPageButton.interactable = _currentPageIndex < _totalPages - 1;

            for (int i = 0; i < _tiles.Count; i++)
            {
                int levelIndex = startLevel + i;
                if (levelIndex < totalLevels)
                {
                    _tiles[i].gameObject.SetActive(true);
                    bool isUnlocked = levelIndex <= _maxReachedLevelId;
                    bool isSelected = levelIndex == _currentLevelId;
                    bool isMaxReached = levelIndex == _maxReachedLevelId;
                    _tiles[i].Bind(levelIndex, isUnlocked, isSelected, isMaxReached);
                }
                else
                {
                    _tiles[i].gameObject.SetActive(false);
                }
            }

            if (focusCurrentLevel && _currentLevelId >= startLevel && _currentLevelId < endLevel)
            {
                ScrollToLocalTile(_currentLevelId - startLevel);
            }
            else
            {
                ResetScrollToTop();
            }
        }

        private void ScrollToLocalTile(int localIndex)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);

            float tileCenterY = -_tiles[localIndex].RectTransform.anchoredPosition.y;
            float viewportHeight = viewportRect.rect.height;
            float contentHeight = contentRect.rect.height;
            float maxScrollY = Mathf.Max(0f, contentHeight - viewportHeight);
            float targetScrollY = Mathf.Clamp(tileCenterY - (viewportHeight * 0.5f), 0f, maxScrollY);

            scrollRect.StopMovement();
            contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, targetScrollY);
        }

        private void ResetScrollToTop()
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);

            scrollRect.StopMovement();
            contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, 0f);
        }

        private void OnPrevPageClicked()
        {
            AudioSignals.Instance.onPlaySound(AudioSoundEnums.Click);
            PitchSignals.Instance.onPlayPitch(PitchEnums.Click);
            ShowPage(_currentPageIndex - 1, false);
        }

        private void OnNextPageClicked()
        {
            AudioSignals.Instance.onPlaySound(AudioSoundEnums.Click);
            PitchSignals.Instance.onPlayPitch(PitchEnums.Click);
            ShowPage(_currentPageIndex + 1, false);
        }

        private void OnLevelSelected(int levelIndex)
        {
            AudioSignals.Instance.onPlaySound(AudioSoundEnums.Click);
            PitchSignals.Instance.onPlayPitch(PitchEnums.Click);

            LevelSignals.Instance.onPreviousLevelOpened(levelIndex);
            UISignals.Instance.onClosePanel(UIPanels.LevelSelectionPanel);
            UISignals.Instance.onOpenPanel(UIPanels.StartPanel);
        }

        private void OnCloseButtonClicked()
        {
            AudioSignals.Instance.onPlaySound(AudioSoundEnums.Click);
            PitchSignals.Instance.onPlayPitch(PitchEnums.Click);

            UISignals.Instance.onClosePanel(UIPanels.LevelSelectionPanel);
            UISignals.Instance.onOpenPanel(UIPanels.StartPanel);
        }
    }
}
