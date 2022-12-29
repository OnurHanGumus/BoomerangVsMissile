using System;
using System.Collections.Generic;
using Data.UnityObject;
using Data.ValueObject;
using Keys;
using Signals;
using UnityEngine;
using UnityEngine.EventSystems;

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
            PlayerSignals.Instance.onBoomerangHasReturned += OnBoomerangReturned;
        }

        private void UnsubscribeEvents()
        {
            InputSignals.Instance.onEnableInput -= OnEnableInput;
            InputSignals.Instance.onDisableInput -= OnDisableInput;
            CoreGameSignals.Instance.onPlay -= OnPlay;
            CoreGameSignals.Instance.onReset -= OnReset;
            PlayerSignals.Instance.onBoomerangHasReturned -= OnBoomerangReturned;

        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        #endregion

        private void Update()
        {
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
                    if (hit.collider.CompareTag("Missile"))
                    {
                        if (hit.transform.Equals(_lastHitTransform))
                        {
                            return;
                        }
                        Vector3 hitPoint = hit.point;
                        hitPoint = new Vector3(hitPoint.x, hitPoint.y, 0);
                        InputSignals.Instance.onClicking?.Invoke(hitPoint);
                        _lastHitTransform = hit.transform;
                    }
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                InputSignals.Instance.onInputReleased?.Invoke();
            }

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
        }

        private void OnChangePlayerLivingState()
        {
            _isPlayerDead = !_isPlayerDead;
        }

    }
}