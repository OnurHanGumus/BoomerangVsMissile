using Enums;
using Signals;
using TMPro;
using UnityEngine;

namespace Controllers.UI
{
    public class StartPanelController : MonoBehaviour
    {
        #region Self Variables

        #region Public Variables
        public TextMeshProUGUI LevelText => levelText;
        #endregion

        #region Serialized Variables
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("Level Formatting")]
        [Tooltip("Format string for level text, where {0} is the level number.")]
        [SerializeField] private string levelFormat = "Level {0}";

        [Tooltip("If true, treats internal index 0 as Level 1 (human-friendly 1-based indexing).")]
        [SerializeField] private bool useOneBasedLevel = true;

        [Tooltip("If true, converts the output text to uppercase (e.g. 'LEVEL 22').")]
        [SerializeField] private bool toUpper = true;
        #endregion

        #region Private Variables
        private int _lastDisplayedLevel = -1;
        #endregion

        #endregion

        private void Awake()
        {
            Init();
            SubscribeEvents();
        }

        private void Start()
        {
            UpdateLevelDisplay();
        }

        private void OnEnable()
        {
            UpdateLevelDisplay();
        }

        private void Init()
        {
            if (levelText == null)
            {
                Transform existing = transform.Find("LevelText (TMP)");
                if (existing != null)
                {
                    levelText = existing.GetComponent<TextMeshProUGUI>();
                }
                else
                {
                    levelText = CreateFallbackLevelText();
                }
            }
        }

        private TextMeshProUGUI CreateFallbackLevelText()
        {
            GameObject levelObj = new GameObject("LevelText (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
            levelObj.transform.SetParent(transform, false);

            RectTransform rect = levelObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 680f);
            rect.sizeDelta = new Vector2(600f, 90f);
            rect.localScale = Vector3.one;

            TextMeshProUGUI tmp = levelObj.GetComponent<TextMeshProUGUI>();

            // Inherit font & material from start button text if available
            TextMeshProUGUI siblingTMP = GetComponentInChildren<TextMeshProUGUI>(true);
            if (siblingTMP != null)
            {
                if (siblingTMP.font != null)
                {
                    tmp.font = siblingTMP.font;
                }
                if (siblingTMP.fontSharedMaterial != null)
                {
                    tmp.fontSharedMaterial = siblingTMP.fontSharedMaterial;
                }
            }

            tmp.fontSize = 46f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 28f;
            tmp.fontSizeMax = 52f;

            return tmp;
        }

        #region Event Subscription
        private void SubscribeEvents()
        {
            if (UISignals.Instance != null)
            {
                UISignals.Instance.onOpenPanel += OnOpenPanel;
            }

            if (CoreGameSignals.Instance != null)
            {
                CoreGameSignals.Instance.onNextLevel += OnNextLevel;
                CoreGameSignals.Instance.onRestartLevel += OnRestartLevel;
            }
        }

        private void UnsubscribeEvents()
        {
            if (UISignals.Instance != null)
            {
                UISignals.Instance.onOpenPanel -= OnOpenPanel;
            }

            if (CoreGameSignals.Instance != null)
            {
                CoreGameSignals.Instance.onNextLevel -= OnNextLevel;
                CoreGameSignals.Instance.onRestartLevel -= OnRestartLevel;
            }
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }
        #endregion

        private void OnOpenPanel(UIPanels panel)
        {
            if (panel == UIPanels.StartPanel)
            {
                UpdateLevelDisplay();
            }
        }

        private void OnNextLevel()
        {
            UpdateLevelDisplay();
        }

        private void OnRestartLevel()
        {
            UpdateLevelDisplay();
        }

        public void UpdateLevelDisplay()
        {
            int currentLevelId = GetCurrentLevelId();
            int displayLevel = useOneBasedLevel ? (currentLevelId + 1) : currentLevelId;

            _lastDisplayedLevel = displayLevel;

            if (levelText != null)
            {
                string text = string.Format(levelFormat, displayLevel);
                if (toUpper)
                {
                    text = text.ToUpperInvariant();
                }
                levelText.text = text;
            }
        }

        private int GetCurrentLevelId()
        {
            if (LevelSignals.Instance != null && LevelSignals.Instance.onGetLevelId != null)
            {
                return LevelSignals.Instance.onGetLevelId();
            }

            try
            {
                if (ES3.KeyExists("Level", "SaveFile.es3"))
                {
                    return ES3.Load<int>("Level", "SaveFile.es3", 0);
                }

                if (ES3.KeyExists("Level"))
                {
                    return ES3.Load<int>("Level", 0);
                }
            }
            catch
            {
                // Fallback to initial level
            }

            return 0;
        }
    }
}
