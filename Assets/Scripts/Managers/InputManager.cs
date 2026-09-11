using System;
using System.Collections.Generic;
using Data.UnityObject;
using Data.ValueObject;
using Keys;
using Signals;
using UnityEngine;
using UnityEngine.EventSystems;
using Enums;
using Controllers;

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

        [Header("Drag Direction Settings")]
        [SerializeField] private float dragThreshold = 0.25f;


        #endregion

        #region Private Variables

        private bool _isPlayerDead = false;
        private Ray _ray;
        private Transform _lastHitTransform;

        private bool _isBoomerangOnPlayer = true;
        private float _chargeTimer = 0f;
        private PlayerData _playerData;
        private Vector3 _clickedPoint = Vector3.zero;

        private Vector3 _dragStartWorldPos;
        private float _clickYOffset = 0f;
        private float _currentReturnSwingDir = 1f;
        private bool _hasSelectedDragDirection = false;
        private float _lastThrowTime = -1f;
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
                if (!_isPlayerDead && !IsPointerOverUIElement() && Input.GetMouseButtonDown(0))
                {
                    if (Time.unscaledTime - _lastThrowTime > 0.08f)
                    {
                        BoomerangSignals.Instance.onEmergencyRecall?.Invoke();
                    }
                }
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
                            _lastHitTransform = hit.transform;
                            _clickYOffset = hitPoint.y - _lastHitTransform.position.y;
                            _dragStartWorldPos = GetWorldPointFromMouse(Input.mousePosition);
                            _hasSelectedDragDirection = false;
                            _currentReturnSwingDir = BoomerangMovementController.CalculateSwingDirection(_clickedPoint.x);
                            _chargeTimer = 0f;
                            InputSignals.Instance.onClicking?.Invoke(hitPoint);
                            AudioSignals.Instance.onPlaySound(AudioSoundEnums.Pitch);

                        }
                    }
                }
                else
                {
                    if (_lastHitTransform != null)
                    {
                        // Target follows the missile position seamlessly during hold
                        _clickedPoint = new Vector3(_lastHitTransform.position.x, _lastHitTransform.position.y + _clickYOffset, 0f);
                        InputSignals.Instance.onClicking?.Invoke(_clickedPoint);
                    }

                    // Drag horizontal calculation relative to press point on missile
                    Vector3 currentWorldPos = GetWorldPointFromMouse(Input.mousePosition);
                    float deltaX = currentWorldPos.x - _dragStartWorldPos.x;

                    if (deltaX > dragThreshold)
                    {
                        _currentReturnSwingDir = -1f; // Drag right -> return direction left
                        _hasSelectedDragDirection = true;
                    }
                    else if (deltaX < -dragThreshold)
                    {
                        _currentReturnSwingDir = 1f; // Drag left -> return direction right
                        _hasSelectedDragDirection = true;
                    }
                    else if (!_hasSelectedDragDirection)
                    {
                        _currentReturnSwingDir = BoomerangMovementController.CalculateSwingDirection(_clickedPoint.x);
                    }

                    // Target already selected; charge arc based on hold duration
                    _chargeTimer += Time.unscaledDeltaTime;
                    float progress = Mathf.Clamp01(_chargeTimer / _playerData.ChargeDuration);
                    InputSignals.Instance.onChargeUpdated?.Invoke(progress, _clickedPoint, _currentReturnSwingDir);
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
                    BoomerangSignals.Instance.onSetChargedArc?.Invoke(chargedWidth, chargedHeight, isFullyCharged, _currentReturnSwingDir);
                    InputSignals.Instance.onChargeEnded?.Invoke();
                    _chargeTimer = 0f;
                    ResetDragState();
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
            ResetDragState();
            _isBoomerangOnPlayer = true;
        }
        private void OnBoomerangThrown()
        {
            _isBoomerangOnPlayer = false;
            _lastThrowTime = Time.unscaledTime;
        }

        private void OnReset()
        {
            _lastHitTransform = null;
            _clickedPoint = Vector3.zero;
            ResetDragState();
            _isBoomerangOnPlayer = true;
        }

        private void ResetDragState()
        {
            _hasSelectedDragDirection = false;
            _currentReturnSwingDir = 1f;
            _clickYOffset = 0f;
            _dragStartWorldPos = Vector3.zero;
        }

        private Vector3 GetWorldPointFromMouse(Vector3 mousePos)
        {
            if (Camera.main == null)
            {
                return Vector3.zero;
            }

            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            Plane plane = new Plane(Vector3.back, Vector3.zero);
            if (plane.Raycast(ray, out float enter))
            {
                Vector3 point = ray.GetPoint(enter);
                point.z = 0f;
                return point;
            }

            return Vector3.zero;
        }

        private void OnChangePlayerLivingState()
        {
            _isPlayerDead = !_isPlayerDead;
        }

    }
}