using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class Arms : MonoBehaviour
{
    [SerializeField] private float _speed = 300f;
    [SerializeField] private Rigidbody2D _rb;
    //[SerializeField] private KeyCode _mouseButton;

    private Vector3 _controllerPos = Vector3.zero;
    private Vector3 _delta = Vector3.zero;
    private float _rotationZ = 0f;

    private bool _isActive = true;

    #region Inputs

    private Vector2 _controllerInput;
    private bool _isMouseController = false;
    private bool _stickReleased = true;
    private bool _isGrabbing = false;

    public void MouseMoveArms(InputAction.CallbackContext value)
    {
        _controllerInput = value.ReadValue<Vector2>();
        //Debug.Log("_controllerInput (arms) : " + _controllerInput.ToString());
        //_controllerPos.x = _controllerInput.x;
        //_controllerPos.y = _controllerInput.y;
        _controllerPos.x = Camera.main.ScreenToWorldPoint(_controllerInput).x;
        _controllerPos.y = Camera.main.ScreenToWorldPoint(_controllerInput).y;
        _controllerPos.z = 0;

        _delta = _controllerPos - transform.position;

        _rotationZ = Mathf.Atan2(_delta.x, -_delta.y) * Mathf.Rad2Deg;
        _isMouseController = true;
    }

    public void StickMoveArms(InputAction.CallbackContext value)
    {
        _controllerInput = value.ReadValue<Vector2>();
        //Debug.Log("_controllerInput (arms) : " + _controllerInput.ToString());
        _controllerPos.x = _controllerInput.x;
        _controllerPos.y = _controllerInput.y;
        //_controllerPos.x = Camera.main.ScreenToWorldPoint(_controllerInput).x;
        //_controllerPos.y = Camera.main.ScreenToWorldPoint(_controllerInput).y;
        _controllerPos.z = 0;

        //_delta = _controllerPos - transform.position;

        _rotationZ = Mathf.Atan2(_controllerPos.x, -_controllerPos.y) * Mathf.Rad2Deg;
        _isMouseController = false;
        _stickReleased = Mathf.Approximately(_controllerPos.x, 0f) && Mathf.Approximately(_controllerPos.y, 0f);
    }

    public void GrabButton(InputAction.CallbackContext value)
    {
        //if (!value.performed) return;
        _isGrabbing = value.ReadValueAsButton();
    }

    #endregion

    void FixedUpdate()
    {
        if (!_isActive) return;

        
        if(_isGrabbing && _isMouseController)
        {
            _rb.MoveRotation(Mathf.LerpAngle(_rb.rotation, _rotationZ, _speed * Time.fixedDeltaTime));
        }
        else if(!_isMouseController && !_stickReleased)
        {
            _rb.MoveRotation(Mathf.LerpAngle(_rb.rotation, _rotationZ, _speed * Time.fixedDeltaTime));
        }
    }

    public void OnCollapse(bool collapse)
    {
        _isActive = !collapse;
    }
}


