using Enums;
using Signals;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers.UI
{
    public class LevelSelectionButtonController : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables
        [SerializeField] private Button button;
        #endregion

        #endregion

        private void Awake()
        {
            SubscribeEvents();
        }

        #region Event Subscription
        private void SubscribeEvents()
        {
            button.onClick.AddListener(OnButtonClicked);
        }

        private void UnsubscribeEvents()
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }
        #endregion

        public void OnButtonClicked()
        {
            AudioSignals.Instance.onPlaySound(AudioSoundEnums.Click);
            PitchSignals.Instance.onPlayPitch(PitchEnums.Click);

            UISignals.Instance.onOpenPanel(UIPanels.LevelSelectionPanel);
            UISignals.Instance.onClosePanel(UIPanels.StartPanel);
        }
    }
}
