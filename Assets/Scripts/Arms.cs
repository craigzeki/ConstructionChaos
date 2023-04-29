using Newtonsoft.Json.Linq;
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

    private Vector3 _controllerPos = Vector3.zero;
    private Vector3 _delta = Vector3.zero;
    private float _rotationZ = 0f;

    private bool _isActive = true;

    //#region Inputs

    //private Vector2 _controllerInput;
    //private bool _isMouseController = false;
    //private bool _stickReleased = true;
    //private bool _isGrabbing = false;

    ///// <summary>
    ///// Processes the mouse input and sets a rotation angle for the arms to target towards
    ///// </summary>
    ///// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    ///// <remarks>Only use to link to the Input System</remarks>
    //public void MouseMoveArms(InputAction.CallbackContext value)
    //{
    //    _controllerInput = value.ReadValue<Vector2>();
    //    //Debug.Log("_controllerInput (arms) : " + _controllerInput.ToString());
    //    //_controllerPos.x = _controllerInput.x;
    //    //_controllerPos.y = _controllerInput.y;
    //    _controllerPos.x = Camera.main.ScreenToWorldPoint(_controllerInput).x;
    //    _controllerPos.y = Camera.main.ScreenToWorldPoint(_controllerInput).y;
    //    _controllerPos.z = 0;

    //    _delta = _controllerPos - transform.position;

    //    _rotationZ = Mathf.Atan2(_delta.x, -_delta.y) * Mathf.Rad2Deg;
    //    _isMouseController = true;
        
    //}

    ///// <summary>
    ///// Processes the gamepad stick input and sets a rotation angle for the arms to target towards
    ///// </summary>
    ///// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    ///// <remarks>Only use to link to the Input System</remarks>
    //public void StickMoveArms(InputAction.CallbackContext value)
    //{
    //    _controllerInput = value.ReadValue<Vector2>();
    //    //Debug.Log("_controllerInput (arms) : " + _controllerInput.ToString());
    //    _controllerPos.x = _controllerInput.x;
    //    _controllerPos.y = _controllerInput.y;
    //    //_controllerPos.x = Camera.main.ScreenToWorldPoint(_controllerInput).x;
    //    //_controllerPos.y = Camera.main.ScreenToWorldPoint(_controllerInput).y;
    //    _controllerPos.z = 0;

    //    //_delta = _controllerPos - transform.position;

    //    _rotationZ = Mathf.Atan2(_controllerPos.x, -_controllerPos.y) * Mathf.Rad2Deg;
    //    _isMouseController = false;
    //    _stickReleased = Mathf.Approximately(_controllerPos.x, 0f) && Mathf.Approximately(_controllerPos.y, 0f);
    //}


    ///// <summary>
    ///// Processes a button press and sets whether the user is trying to grab
    ///// </summary>
    ///// <param name="value">The InputAction CallbackContext passed from the Input System</param>
    ///// <remarks>Only use to link to the Input System</remarks>
    //public void GrabButton(InputAction.CallbackContext value)
    //{
    //    //if (!value.performed) return;
    //    _isGrabbing = value.ReadValueAsButton();
    //}

    //#endregion

    private void CalculateArmRotation()
    {
        if(InputHandler.Instance.IsMouseController)
        {
            _controllerPos.x = Camera.main.ScreenToWorldPoint(InputHandler.Instance.ArmsControllerInput).x;
            _controllerPos.y = Camera.main.ScreenToWorldPoint(InputHandler.Instance.ArmsControllerInput).y;
            _controllerPos.z = 0;

            _delta = _controllerPos - transform.position;

            _rotationZ = Mathf.Atan2(_delta.x, -_delta.y) * Mathf.Rad2Deg;
        }
        else
        {
            _rotationZ = Mathf.Atan2(InputHandler.Instance.ArmsControllerInput.x, -InputHandler.Instance.ArmsControllerInput.y) * Mathf.Rad2Deg;
        }
    }

    void FixedUpdate()
    {
        //not active / collapsed
        if (!_isActive) return;

        CalculateArmRotation();
        //if player is trying to grab and is a mouse controller
        if(InputHandler.Instance.IsGrabbing && InputHandler.Instance.IsMouseController)
        {
            //move the amrs
            _rb.MoveRotation(Mathf.LerpAngle(_rb.rotation, _rotationZ, _speed * Time.fixedDeltaTime));
        }
        else if(!InputHandler.Instance.IsMouseController && !InputHandler.Instance.ArmsStickReleased)
        {
            //player is trying to move arms using gamepad, move the arms (no reliance on grab to move
            _rb.MoveRotation(Mathf.LerpAngle(_rb.rotation, _rotationZ, _speed * Time.fixedDeltaTime));
        }
        
    }


    /// <summary>
    /// Rceives a broadcast message indicating the player has collapsed and sets the internal state accordingly
    /// </summary>
    /// <param name="collapse">True: collapsed   False: restored from collapsed</param>
    /// <remarks>Call using BroadcastMessage</remarks>
    public void OnCollapse(bool collapse)
    {
        _isActive = !collapse;
    }
}


