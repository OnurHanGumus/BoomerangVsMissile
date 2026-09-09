using System;
using System.Collections.Generic;
using Data.UnityObject;
using Data.ValueObject;
using Keys;
using Signals;
using UnityEngine;
using UnityEngine.EventSystems;
using Enums;

namespace Managers
{
    public class InputManager : MonoBehaviour
    {
        #region Self Variables

        #region Public Variables

        [Header("Data")] public InputData Data;

        #endregion

        #region Serialized Variables

        //[SerializeField] FloatingJoystick joystick; //SimpleJoystick paketi eklenmeli


        #endregion

        #region Private Variables

        private bool _isPlayerDead = false;
        private Ray _ray;
        private Transform _lastHitTransform;

        private bool _isBoomerangOnPlayer = true;
        private float _chargeTimer = 0f;
        private PlayerData _playerData;
        private Vector3 _clickedPoint = Vector3.zero;
        #endregion

        #endregion


        private void Awake()
        {
            Data = GetInputData();
            _playerData = GetPlayerData();
            SubscribeEvents();
        }

        private InputData GetInputData() => Resources.Load<CD_Input>("Data/CD_Input").Data;
        private PlayerData GetPlayerData() => Resources.Load<CD_Player>("Data/CD_Player").Data;


        #region Event Subscriptions

        private void SubscribeEvents()
        {
            InputSignals.Instance.onEnableInput += OnEnableInput;
            InputSignals.Instance.onDisableInput += OnDisableInput;
            CoreGameSignals.Instance.onPlay += OnPlay;
            CoreGameSignals.Instance.onReset += OnReset;
            BoomerangSignals.Instance.onBoomerangHasReturned += OnBoomerangReturned;
            BoomerangSignals.Instance.onBoomerangThrown += OnBoomerangThrown;

        }

        #endregion

        private void Update()
        {
            if (!_isBoomerangOnPlayer)
            {
                return;
            }
            if (IsPointerOverUIElement())
            {
                return;
            }
            if (Input.GetMouseButton(0))
            {
                if (_isPlayerDead)
                {
                    return;
                }

                if (_lastHitTransform == null)
                {
                    _ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    RaycastHit hit;
                    if (Physics.Raycast(_ray, out hit))
                    {
                        if (hit.collider.CompareTag("Clickable"))
                        {
                            Vector3 hitPoint = new Vector3(hit.point.x, hit.point.y, 0f);
                            _clickedPoint = hitPoint;
                            InputSignals.Instance.onClicking?.Invoke(hitPoint);
                            _lastHitTransform = hit.transform;
                            _chargeTimer = 0f;
                            AudioSignals.Instance.onPlaySound(AudioSoundEnums.Pitch);

                        }
                    }
                }
                else
                {
                    if (_lastHitTransform != null)
                    {
                        _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;
                        if (Physics.Raycast(_ray, out hit))
                        {
                            if (hit.transform == _lastHitTransform || hit.transform.IsChildOf(_lastHitTransform))
                            {
                                Vector3 hitPoint = new Vector3(hit.transform.position.x, hit.point.y, 0f);
                                _clickedPoint = hitPoint;
                                InputSignals.Instance.onClicking?.Invoke(_clickedPoint);
                            }
                        }
                    }

                    // Target already selected; charge arc based on hold duration
                    _chargeTimer += Time.unscaledDeltaTime;
                    float progress = Mathf.Clamp01(_chargeTimer / _playerData.ChargeDuration);
                    InputSignals.Instance.onChargeUpdated?.Invoke(progress, _clickedPoint);
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (_lastHitTransform != null)
                {
                    float progress = Mathf.Clamp01(_chargeTimer / _playerData.ChargeDuration);
                    float chargedWidth = Mathf.Lerp(_playerData.MinReturnArcWidth, _playerData.MaxReturnArcWidth, progress);
                    float chargedHeight = Mathf.Lerp(_playerData.MinReturnArcHeight, _playerData.MaxReturnArcHeight, progress);
                    bool isFullyCharged = progress >= 0.99f;
                    BoomerangSignals.Instance.onSetChargedArc?.Invoke(chargedWidth, chargedHeight, isFullyCharged);
                    InputSignals.Instance.onChargeEnded?.Invoke();
                    _chargeTimer = 0f;
                }

                InputSignals.Instance.onInputReleased?.Invoke();
            }


        }
        private bool IsPointerOverUIElement()
        {
            var eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            return results.Count > 0;
            //return EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0);
        }
        private void OnEnableInput()
        {

        }

        private void OnDisableInput()
        {

        }

        private void OnPlay()
        {

        }

        private void OnBoomerangReturned()
        {
            _lastHitTransform = null;
            _clickedPoint = Vector3.zero;
            _isBoomerangOnPlayer = true;
        }
        private void OnBoomerangThrown()
        {
            _isBoomerangOnPlayer = false;
        }

        private void OnReset()
        {
            _lastHitTransform = null;
            _clickedPoint = Vector3.zero;
            _isBoomerangOnPlayer = true;
        }

        private void OnChangePlayerLivingState()
        {
            _isPlayerDead = !_isPlayerDead;
        }

    }
}