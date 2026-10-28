using System;
using System.Collections.Generic;
using Data.ValueObject;
using DG.Tweening;
using Enums;
using Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers.UI
{
    public class RewardBuffUIController : MonoBehaviour
    {
        #region Self Variables

        #region Private Variables
        private sealed class PopupItem
        {
            public GameObject Root;
            public RectTransform Rect;
            public CanvasGroup Group;
            public TextMeshProUGUI Label;
            public Sequence ActiveSequence;
            public bool InUse;
        }

        private sealed class BuffPillItem
        {
            public GameObject Root;
            public RectTransform Rect;
            public Image AccentStrip;
            public RectTransform FillBarRect;
            public Image FillBarImage;
            public TextMeshProUGUI Label;
            public Tween PunchTween;
            public int LastDisplayedStacks = -1;
            public int LastDisplayedTenths = -1;
        }

        private static readonly Color DamageAccentColor = new Color(1f, 0.58f, 0.12f, 1f);
        private static readonly Color SizeAccentColor = new Color(1f, 0.88f, 0.10f, 1f);
        private static readonly Color SpeedAccentColor = new Color(0.18f, 0.92f, 0.84f, 1f);
        private static readonly Color PillBgColor = new Color(0.07f, 0.09f, 0.14f, 0.84f);
        private static readonly Color BarTrackColor = new Color(0.02f, 0.03f, 0.05f, 0.75f);

        private const int InitialPopupPoolSize = 5;

        private RectTransform _hudRoot;
        private Canvas _rootCanvas;
        private TMP_FontAsset _sharedFont;
        private RectTransform _pillsRowRect;

        private readonly List<PopupItem> _popupPool = new List<PopupItem>(InitialPopupPoolSize);
        private readonly Dictionary<BoomerangStatEnums, BuffPillItem> _pills =
            new Dictionary<BoomerangStatEnums, BuffPillItem>(3);

        private bool _isSubscribed;
        #endregion

        #endregion

        private void Awake()
        {
            Init();
            SubscribeEvents();
        }

        private void Init()
        {
            _hudRoot = GetComponent<RectTransform>();
            if (_hudRoot == null)
            {
                _hudRoot = gameObject.AddComponent<RectTransform>();
            }

            _hudRoot.anchorMin = Vector2.zero;
            _hudRoot.anchorMax = Vector2.one;
            _hudRoot.offsetMin = Vector2.zero;
            _hudRoot.offsetMax = Vector2.zero;
            _hudRoot.pivot = new Vector2(0.5f, 0.5f);
            _hudRoot.localScale = Vector3.one;

            CanvasGroup rootGroup = GetComponent<CanvasGroup>();
            if (rootGroup == null)
            {
                rootGroup = gameObject.AddComponent<CanvasGroup>();
            }
            rootGroup.interactable = false;
            rootGroup.blocksRaycasts = false;

            _rootCanvas = GetComponentInParent<Canvas>();
            ResolveSharedFont();
            BuildPillsRow();
            WarmupPopupPool();
        }

        private void ResolveSharedFont()
        {
            if (transform.parent != null)
            {
                TextMeshProUGUI siblingTmp = transform.parent.GetComponentInChildren<TextMeshProUGUI>(true);
                if (siblingTmp != null && siblingTmp.font != null)
                {
                    _sharedFont = siblingTmp.font;
                    return;
                }
            }

            if (_rootCanvas != null)
            {
                TextMeshProUGUI canvasTmp = _rootCanvas.GetComponentInChildren<TextMeshProUGUI>(true);
                if (canvasTmp != null && canvasTmp.font != null)
                {
                    _sharedFont = canvasTmp.font;
                    return;
                }
            }

            _sharedFont = TMP_Settings.defaultFontAsset;
        }

        private void BuildPillsRow()
        {
            GameObject rowObj = new GameObject("ActiveBuffPillsRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            rowObj.transform.SetParent(_hudRoot, false);

            _pillsRowRect = rowObj.GetComponent<RectTransform>();
            _pillsRowRect.anchorMin = new Vector2(0.5f, 1f);
            _pillsRowRect.anchorMax = new Vector2(0.5f, 1f);
            _pillsRowRect.pivot = new Vector2(0.5f, 1f);
            _pillsRowRect.anchoredPosition = new Vector2(0f, -195f);
            _pillsRowRect.sizeDelta = new Vector2(760f, 64f);

            HorizontalLayoutGroup layout = rowObj.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 14f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            BoomerangStatEnums[] stats = (BoomerangStatEnums[])Enum.GetValues(typeof(BoomerangStatEnums));
            for (int i = 0; i < stats.Length; i++)
            {
                BoomerangStatEnums stat = stats[i];
                _pills[stat] = CreatePillItem(stat);
            }
        }

        private BuffPillItem CreatePillItem(BoomerangStatEnums stat)
        {
            Color accent = GetStatAccentColor(stat);

            GameObject pillObj = new GameObject($"BuffPill_{stat}", typeof(RectTransform), typeof(Image));
            pillObj.transform.SetParent(_pillsRowRect, false);

            RectTransform pillRect = pillObj.GetComponent<RectTransform>();
            pillRect.sizeDelta = new Vector2(228f, 56f);

            Image bgImage = pillObj.GetComponent<Image>();
            bgImage.color = PillBgColor;
            bgImage.raycastTarget = false;

            // Top thin accent line
            GameObject accentObj = new GameObject("TopAccent", typeof(RectTransform), typeof(Image));
            accentObj.transform.SetParent(pillRect, false);
            RectTransform accentRect = accentObj.GetComponent<RectTransform>();
            accentRect.anchorMin = new Vector2(0f, 1f);
            accentRect.anchorMax = new Vector2(1f, 1f);
            accentRect.pivot = new Vector2(0.5f, 1f);
            accentRect.anchoredPosition = Vector2.zero;
            accentRect.sizeDelta = new Vector2(0f, 4f);
            Image accentImage = accentObj.GetComponent<Image>();
            accentImage.color = accent;
            accentImage.raycastTarget = false;

            // Bottom timer track
            GameObject trackObj = new GameObject("TimerTrack", typeof(RectTransform), typeof(Image));
            trackObj.transform.SetParent(pillRect, false);
            RectTransform trackRect = trackObj.GetComponent<RectTransform>();
            trackRect.anchorMin = new Vector2(0f, 0f);
            trackRect.anchorMax = new Vector2(1f, 0f);
            trackRect.pivot = new Vector2(0.5f, 0f);
            trackRect.anchoredPosition = new Vector2(0f, 4f);
            trackRect.sizeDelta = new Vector2(-16f, 8f);
            Image trackImage = trackObj.GetComponent<Image>();
            trackImage.color = BarTrackColor;
            trackImage.raycastTarget = false;

            // Depleting timer fill (anchor-driven so null sprite works cleanly)
            GameObject fillObj = new GameObject("TimerFill", typeof(RectTransform), typeof(Image));
            fillObj.transform.SetParent(trackRect, false);
            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            Image fillImage = fillObj.GetComponent<Image>();
            fillImage.color = accent;
            fillImage.raycastTarget = false;

            // Text label
            GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObj.transform.SetParent(pillRect, false);
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(8f, 14f);
            labelRect.offsetMax = new Vector2(-8f, -5f);

            TextMeshProUGUI tmp = labelObj.GetComponent<TextMeshProUGUI>();
            if (_sharedFont != null)
            {
                tmp.font = _sharedFont;
            }
            tmp.raycastTarget = false;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 13f;
            tmp.fontSizeMax = 21f;
            tmp.color = Color.white;
            tmp.text = string.Empty;

            pillObj.SetActive(false);

            return new BuffPillItem
            {
                Root = pillObj,
                Rect = pillRect,
                AccentStrip = accentImage,
                FillBarRect = fillRect,
                FillBarImage = fillImage,
                Label = tmp
            };
        }

        private void WarmupPopupPool()
        {
            for (int i = 0; i < InitialPopupPoolSize; i++)
            {
                _popupPool.Add(CreatePopupItem(i));
            }
        }

        private PopupItem CreatePopupItem(int index)
        {
            GameObject popupObj = new GameObject(
                $"RewardRevealPopup_{index}",
                typeof(RectTransform),
                typeof(CanvasGroup),
                typeof(TextMeshProUGUI));
            popupObj.transform.SetParent(_hudRoot, false);

            RectTransform rect = popupObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(520f, 90f);

            CanvasGroup group = popupObj.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            TextMeshProUGUI tmp = popupObj.GetComponent<TextMeshProUGUI>();
            if (_sharedFont != null)
            {
                tmp.font = _sharedFont;
            }
            tmp.raycastTarget = false;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.fontSize = 40f;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 24f;
            tmp.fontSizeMax = 42f;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.outlineWidth = 0.22f;
            tmp.outlineColor = new Color32(15, 15, 22, 235);

            popupObj.SetActive(false);

            return new PopupItem
            {
                Root = popupObj,
                Rect = rect,
                Group = group,
                Label = tmp,
                InUse = false
            };
        }

        #region Event Subscription
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
            ClearAllVisuals();
        }

        private void SubscribeEvents()
        {
            if (_isSubscribed)
            {
                return;
            }

            if (BoomerangSignals.Instance != null)
            {
                BoomerangSignals.Instance.onRewardBuffTriggered += OnRewardBuffTriggered;
                BoomerangSignals.Instance.onBuffTimerUpdated += OnBuffTimerUpdated;
            }

            if (CoreGameSignals.Instance != null)
            {
                CoreGameSignals.Instance.onLevelFailed += ClearAllVisuals;
                CoreGameSignals.Instance.onLevelSuccessful += ClearAllVisuals;
                CoreGameSignals.Instance.onRestartLevel += ClearAllVisuals;
                CoreGameSignals.Instance.onReset += ClearAllVisuals;
            }

            _isSubscribed = true;
        }

        private void UnsubscribeEvents()
        {
            if (!_isSubscribed)
            {
                return;
            }

            if (BoomerangSignals.Instance != null)
            {
                BoomerangSignals.Instance.onRewardBuffTriggered -= OnRewardBuffTriggered;
                BoomerangSignals.Instance.onBuffTimerUpdated -= OnBuffTimerUpdated;
            }

            if (CoreGameSignals.Instance != null)
            {
                CoreGameSignals.Instance.onLevelFailed -= ClearAllVisuals;
                CoreGameSignals.Instance.onLevelSuccessful -= ClearAllVisuals;
                CoreGameSignals.Instance.onRestartLevel -= ClearAllVisuals;
                CoreGameSignals.Instance.onReset -= ClearAllVisuals;
            }

            _isSubscribed = false;
        }
        #endregion

        private void OnRewardBuffTriggered(BoomerangStatEnums stat, RewardMissileData data, Vector3 worldPos)
        {
            PopupItem popup = AcquirePopupItem();
            Vector2 anchoredPos = WorldToHudAnchoredPosition(worldPos);

            popup.ActiveSequence?.Kill();
            popup.InUse = true;
            popup.Root.SetActive(true);
            popup.Rect.SetAsLastSibling();
            popup.Rect.anchoredPosition = anchoredPos;
            popup.Rect.localScale = Vector3.zero;
            popup.Group.alpha = 1f;

            popup.Label.color = GetStatAccentColor(stat);
            popup.Label.text = GetRevealPopupText(stat);

            Sequence seq = DOTween.Sequence();
            seq.SetUpdate(true);
            seq.SetLink(popup.Root);
            seq.Append(popup.Rect.DOScale(1.12f, 0.18f).SetEase(Ease.OutBack));
            seq.Append(popup.Rect.DOScale(1.0f, 0.12f).SetEase(Ease.InOutSine));
            seq.Join(popup.Rect.DOAnchorPosY(anchoredPos.y + 115f, 0.9f).SetEase(Ease.OutCubic));
            seq.Insert(0.62f, popup.Group.DOFade(0f, 0.3f).SetEase(Ease.InQuad));
            seq.OnComplete(() =>
            {
                popup.InUse = false;
                if (popup.Root != null)
                {
                    popup.Root.SetActive(false);
                }
            });

            popup.ActiveSequence = seq;
        }

        private void OnBuffTimerUpdated(BoomerangStatEnums stat, int stacks, float remainingTime, float totalDuration)
        {
            if (!_pills.TryGetValue(stat, out BuffPillItem pill) || pill == null || pill.Root == null)
            {
                return;
            }

            if (stacks <= 0 || remainingTime <= 0f)
            {
                pill.PunchTween?.Kill();
                pill.LastDisplayedStacks = -1;
                pill.LastDisplayedTenths = -1;
                if (pill.Root.activeSelf)
                {
                    pill.Root.SetActive(false);
                }
                return;
            }

            bool wasActive = pill.Root.activeSelf;
            if (!wasActive)
            {
                pill.Root.SetActive(true);
            }

            if (!wasActive || stacks != pill.LastDisplayedStacks)
            {
                pill.PunchTween?.Kill();
                pill.Rect.localScale = Vector3.one;
                pill.PunchTween = pill.Rect
                    .DOPunchScale(new Vector3(0.16f, 0.16f, 0f), 0.22f, 8, 1f)
                    .SetUpdate(true)
                    .SetLink(pill.Root);
            }

            float normalized = totalDuration > 0.01f ? Mathf.Clamp01(remainingTime / totalDuration) : 0f;
            pill.FillBarRect.anchorMax = new Vector2(normalized, 1f);

            int tenths = Mathf.CeilToInt(remainingTime * 10f);
            if (stacks != pill.LastDisplayedStacks || tenths != pill.LastDisplayedTenths)
            {
                pill.LastDisplayedStacks = stacks;
                pill.LastDisplayedTenths = tenths;
                float displaySeconds = tenths * 0.1f;
                pill.Label.text = FormatPillText(stat, stacks, displaySeconds);
            }
        }

        private void ClearAllVisuals()
        {
            for (int i = 0; i < _popupPool.Count; i++)
            {
                PopupItem popup = _popupPool[i];
                if (popup == null)
                {
                    continue;
                }
                popup.ActiveSequence?.Kill();
                popup.InUse = false;
                if (popup.Root != null)
                {
                    popup.Root.SetActive(false);
                }
            }

            foreach (var kvp in _pills)
            {
                BuffPillItem pill = kvp.Value;
                if (pill == null)
                {
                    continue;
                }
                pill.PunchTween?.Kill();
                pill.LastDisplayedStacks = -1;
                pill.LastDisplayedTenths = -1;
                if (pill.Root != null)
                {
                    pill.Root.SetActive(false);
                }
            }
        }

        private PopupItem AcquirePopupItem()
        {
            for (int i = 0; i < _popupPool.Count; i++)
            {
                if (!_popupPool[i].InUse)
                {
                    return _popupPool[i];
                }
            }

            PopupItem extra = CreatePopupItem(_popupPool.Count);
            _popupPool.Add(extra);
            return extra;
        }

        private Vector2 WorldToHudAnchoredPosition(Vector3 worldPos)
        {
            UnityEngine.Camera mainCam = UnityEngine.Camera.main;
            if (mainCam == null || _hudRoot == null)
            {
                return Vector2.zero;
            }

            Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);
            if (_rootCanvas == null)
            {
                _rootCanvas = GetComponentInParent<Canvas>();
            }

            UnityEngine.Camera uiCam = (_rootCanvas != null && _rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                ? _rootCanvas.worldCamera
                : null;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_hudRoot, screenPos, uiCam, out Vector2 localPos))
            {
                return Vector2.zero;
            }

            Rect bounds = _hudRoot.rect;
            if (bounds.width > 360f && bounds.height > 360f)
            {
                float halfW = Mathf.Max(80f, (bounds.width * 0.5f) - 180f);
                float halfH = Mathf.Max(80f, (bounds.height * 0.5f) - 180f);
                localPos.x = Mathf.Clamp(localPos.x, -halfW, halfW);
                localPos.y = Mathf.Clamp(localPos.y, -halfH, halfH);
            }

            return localPos;
        }

        private static Color GetStatAccentColor(BoomerangStatEnums stat)
        {
            switch (stat)
            {
                case BoomerangStatEnums.Damage:
                    return DamageAccentColor;
                case BoomerangStatEnums.Size:
                    return SizeAccentColor;
                case BoomerangStatEnums.HyperSpeed:
                    return SpeedAccentColor;
                default:
                    return SizeAccentColor;
            }
        }

        private static string GetRevealPopupText(BoomerangStatEnums stat)
        {
            switch (stat)
            {
                case BoomerangStatEnums.Damage:
                    return "DOUBLE DAMAGE!";
                case BoomerangStatEnums.Size:
                    return "GIANT BLADE!";
                case BoomerangStatEnums.HyperSpeed:
                    return "HYPER SPEED!";
                default:
                    return "POWER UP!";
            }
        }

        private static string FormatPillText(BoomerangStatEnums stat, int stacks, float seconds)
        {
            string stackSuffix = stacks > 1 ? $" x{stacks}" : string.Empty;
            switch (stat)
            {
                case BoomerangStatEnums.Damage:
                    int dmgMult = 1 + stacks;
                    return $"DMG x{dmgMult}  {seconds:0.0}s";
                case BoomerangStatEnums.Size:
                    return $"GIANT{stackSuffix}  {seconds:0.0}s";
                case BoomerangStatEnums.HyperSpeed:
                    return $"SPEED{stackSuffix}  {seconds:0.0}s";
                default:
                    return $"BUFF  {seconds:0.0}s";
            }
        }
    }
}
