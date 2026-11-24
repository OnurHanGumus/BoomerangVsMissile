using Data.UnityObject;
using Data.ValueObject;
using Signals;
using UnityEngine;

namespace Managers
{
    public class LevelEnvironmentManager : MonoBehaviour
    {
        #region Self Variables

        #region Public Variables

        #endregion

        #region Serialized Variables

        #endregion

        #region Private Variables

        private CD_LevelEnvironment _data;
        private Camera _mainCamera;

        #endregion

        #endregion

        private void Awake()
        {
            Init();
        }

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void Init()
        {
            _data = GetData();
            _mainCamera = Camera.main;
        }

        private CD_LevelEnvironment GetData() => Resources.Load<CD_LevelEnvironment>("Data/CD_LevelEnvironment");

        #region Event Subscription

        private void SubscribeEvents()
        {
            LevelSignals.Instance.onLevelLoaded += OnLevelLoaded;
        }

        private void UnsubscribeEvents()
        {
            LevelSignals.Instance.onLevelLoaded -= OnLevelLoaded;
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        #endregion

        private void OnLevelLoaded(int levelIndex)
        {
            int colorIndex = levelIndex % _data.LevelColors.Count;
            LevelEnvironmentColorData colorData = _data.LevelColors[colorIndex];

            RenderSettings.fogColor = colorData.FogColor;
            _mainCamera.clearFlags = CameraClearFlags.SolidColor;
            _mainCamera.backgroundColor = colorData.CameraBackgroundColor;
        }
    }
}
