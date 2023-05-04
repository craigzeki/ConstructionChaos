using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class Arms : Ragdoll
{
    [SerializeField] private float _speed = 300f;
    [SerializeField] private Rigidbody2D _rb;

    private Vector3 _controllerPos = Vector3.zero;
    private Vector3 _delta = Vector3.zero;
    private float _rotationZ = 0f;

    /// <summary>
    /// Calculates the arm rotation value based on which controller is active
    /// </summary>
    private void CalculateArmRotation()
    {
        if(_characterInputHandler._characterInputData.ArmsMovementData.IsMouseController)
        {
            _controllerPos.x = Camera.main.ScreenToWorldPoint(_characterInputHandler._characterInputData.ArmsMovementData.ArmsControllerInput).x;
            _controllerPos.y = Camera.main.ScreenToWorldPoint(_characterInputHandler._characterInputData.ArmsMovementData.ArmsControllerInput).y;
            _controllerPos.z = 0;

            _delta = _controllerPos - transform.position;

            _rotationZ = Mathf.Atan2(_delta.x, -_delta.y) * Mathf.Rad2Deg;
        }
        else
        {
            _rotationZ = Mathf.Atan2(_characterInputHandler._characterInputData.ArmsMovementData.ArmsControllerInput.x, -_characterInputHandler._characterInputData.ArmsMovementData.ArmsControllerInput.y) * Mathf.Rad2Deg;
        }
    }

    void FixedUpdate()
    {
        //not active / collapsed
        if (!_isActive) return;

        CalculateArmRotation();
        //if player is trying to grab and is a mouse controller
        if((_characterInputHandler._characterInputData.IsGrabbingLeft || _characterInputHandler._characterInputData.IsGrabbingRight) && _characterInputHandler._characterInputData.ArmsMovementData.IsMouseController)
        {
            //move the amrs
            _rb.MoveRotation(Mathf.LerpAngle(_rb.rotation, _rotationZ, _speed * Time.fixedDeltaTime));
        }
        else if(!_characterInputHandler._characterInputData.ArmsMovementData.IsMouseController && !_characterInputHandler._characterInputData.ArmsMovementData.ArmsStickReleased)
        {
            //player is trying to move arms using gamepad, move the arms (no reliance on grab to move
            _rb.MoveRotation(Mathf.LerpAngle(_rb.rotation, _rotationZ, _speed * Time.fixedDeltaTime));
        }
        
    }
}


