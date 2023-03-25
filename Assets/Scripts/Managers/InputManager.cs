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


        private float _currentVelocity; //ref type
        private Vector2? _mousePosition; //ref type
        private Vector3 _moveVector; //ref type
        private bool _isPlayerDead = false;
        private Ray _ray;
        private Transform _lastHitTransform;

        private bool _isBoomerangDisapeared = false;
        private bool _isPlayerDrawing = false;
        private bool _isBoomerangOnPlayer = true;
        #endregion

        #endregion


        private void Awake()
        {
            Data = GetInputData();
        }

        private InputData GetInputData() => Resources.Load<CD_Input>("Data/CD_Input").Data;


        #region Event Subscriptions

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            InputSignals.Instance.onEnableInput += OnEnableInput;
            InputSignals.Instance.onDisableInput += OnDisableInput;
            CoreGameSignals.Instance.onPlay += OnPlay;
            CoreGameSignals.Instance.onReset += OnReset;
            BoomerangSignals.Instance.onBoomerangHasReturned += OnBoomerangReturned;
            BoomerangSignals.Instance.onBoomerangDisapeared += OnBoomerangDisapeared;
            BoomerangSignals.Instance.onBoomerangRebuilded += OnBoomerangRebuilded;
            BoomerangSignals.Instance.onBoomerangThrowed += OnBoomerangThrowed;

        }

        private void UnsubscribeEvents()
        {
            InputSignals.Instance.onEnableInput -= OnEnableInput;
            InputSignals.Instance.onDisableInput -= OnDisableInput;
            CoreGameSignals.Instance.onPlay -= OnPlay;
            CoreGameSignals.Instance.onReset -= OnReset;
            BoomerangSignals.Instance.onBoomerangHasReturned -= OnBoomerangReturned;
            BoomerangSignals.Instance.onBoomerangDisapeared -= OnBoomerangDisapeared;
            BoomerangSignals.Instance.onBoomerangRebuilded -= OnBoomerangRebuilded;
            BoomerangSignals.Instance.onBoomerangThrowed -= OnBoomerangThrowed;

        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        #endregion

        private void Update()
        {
            if (!_isBoomerangOnPlayer)
            {
                return;
            }
            if (_isBoomerangDisapeared)
            {
                if(Input.GetMouseButtonUp(0))
                {
                    PlayerSignals.Instance.onAnimationSpeedIncreased?.Invoke();
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
                _ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit hit;
                if (Physics.Raycast(_ray, out hit))
                {
                    if (hit.collider.CompareTag("Clickable"))
                    {
                        if (hit.transform.Equals(_lastHitTransform))
                        {
                            return;
                        }
                        Vector3 hitPoint = hit.point;
                        hitPoint = new Vector3(hitPoint.x, hitPoint.y, 0);
                        InputSignals.Instance.onClicking?.Invoke(hitPoint);
                        _lastHitTransform = hit.transform;
                        AudioSignals.Instance.onPlaySound(AudioSoundEnums.Pitch);
                        _isPlayerDrawing = true;

                    }
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                _isPlayerDrawing = false;
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

        private void OnBoomerangDisapeared()
        {
            _isBoomerangDisapeared = true;
        }

        private void OnBoomerangRebuilded()
        {
            _isBoomerangDisapeared = false;
        }

        private void OnBoomerangReturned()
        {
            _lastHitTransform = null;
            _isBoomerangOnPlayer = true;
        }
        private void OnBoomerangThrowed()
        {
            _isBoomerangOnPlayer = false;
        }

        //private bool IsPointerOverUIElement() //Joystick'i doðru konumlandýrýrsan buna gerek kalmaz
        //{
        //    var eventData = new PointerEventData(EventSystem.current);
        //    eventData.position = Input.mousePosition;
        //    var results = new List<RaycastResult>();
        //    EventSystem.current.RaycastAll(eventData, results);
        //    return results.Count > 0;
        //}

        private void OnReset()
        {
            _isBoomerangOnPlayer = true;
        }

        private void OnChangePlayerLivingState()
        {
            _isPlayerDead = !_isPlayerDead;
        }

    }
}