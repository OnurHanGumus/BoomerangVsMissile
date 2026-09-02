using DG.Tweening;
using Data.UnityObject;
using Data.ValueObject;
using Signals;
using UnityEngine;

namespace Controllers.Camera
{
    public class CameraShakeController : MonoBehaviour
    {
        #region Self Variables

        #region Private Variables

        private PlayerData _playerData;
        private Vector3 _originalLocalPos;
        private Tween _shakeTween;

        #endregion

        #endregion

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBindToMainCamera()
        {
            if (UnityEngine.Camera.main != null && UnityEngine.Camera.main.GetComponent<CameraShakeController>() == null)
            {
                UnityEngine.Camera.main.gameObject.AddComponent<CameraShakeController>();
            }
        }

        private void Awake()
        {
            Init();
            SubscribeEvents();
        }

        private void Init()
        {
            _playerData = Resources.Load<CD_Player>("Data/CD_Player").Data;
            _originalLocalPos = transform.localPosition;
        }

        private void SubscribeEvents()
        {
            MissileSignals.Instance.onMissileDestroyed += TriggerImpactShake;
            CoreGameSignals.Instance.onRestartLevel += ResetCamera;
            CoreGameSignals.Instance.onLevelFailed += ResetCamera;
            CoreGameSignals.Instance.onPlay += ResetCamera;
        }

        public void TriggerImpactShake()
        {
            if (_playerData == null)
            {
                _playerData = Resources.Load<CD_Player>("Data/CD_Player").Data;
            }

            _shakeTween?.Kill();
            transform.localPosition = _originalLocalPos;

            _shakeTween = transform.DOShakePosition(
                _playerData.ShakeDuration,
                _playerData.ShakeStrength,
                _playerData.ShakeVibrato,
                _playerData.ShakeRandomness,
                false,
                true
            ).SetUpdate(true).OnComplete(() =>
            {
                transform.localPosition = _originalLocalPos;
            });
        }

        public void TriggerCustomShake(float duration, float strength)
        {
            _shakeTween?.Kill();
            transform.localPosition = _originalLocalPos;

            _shakeTween = transform.DOShakePosition(
                duration,
                strength,
                _playerData != null ? _playerData.ShakeVibrato : 14,
                90f,
                false,
                true
            ).SetUpdate(true).OnComplete(() =>
            {
                transform.localPosition = _originalLocalPos;
            });
        }

        private void ResetCamera()
        {
            _shakeTween?.Kill();
            transform.localPosition = _originalLocalPos;
        }
    }
}
