using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Arms : Ragdoll
{
    [SerializeField] private float _speed = 300f;
    [SerializeField] private Rigidbody2D _rb;

    private Vector2 _screenOrigin = new Vector2(Screen.width / 2, Screen.height / 2);
    private Vector2 _delta = Vector2.zero;
    private float _rotationZ = 0f;

    /// <summary>
    /// Calculates the arm rotation value based on which controller is active
    /// </summary>
    private void CalculateArmRotation(CharacterInputData characterInputData)
    {
        if(characterInputData.ArmsMovementData.IsMouseController)
        {
            //! This only works because the player is always in the center of the screen
            _delta = characterInputData.ArmsMovementData.ArmsControllerInput - _screenOrigin;

            _rotationZ = Mathf.Atan2(_delta.x, -_delta.y) * Mathf.Rad2Deg;
        }
        else
        {
            _rotationZ = Mathf.Atan2(characterInputData.ArmsMovementData.ArmsControllerInput.x, -characterInputData.ArmsMovementData.ArmsControllerInput.y) * Mathf.Rad2Deg;
        }
    }

    void FixedUpdate()
    {
        // Not active / collapsed
        if (!_isActive) return;

        if (IsServer)
            HandleArms(CharacterInputHandler.CharacterInputData);
    }

    private void HandleArms(CharacterInputData characterInputData)
    {
        CalculateArmRotation(characterInputData);

        // If player is trying to grab and is a mouse controller
        if((characterInputData.IsGrabbingLeft || characterInputData.IsGrabbingRight) && characterInputData.ArmsMovementData.IsMouseController)
        {
            // Move the amrs
            _rb.MoveRotation(Mathf.LerpAngle(_rb.rotation, _rotationZ, _speed * Time.fixedDeltaTime));
        }
        else if(!characterInputData.ArmsMovementData.IsMouseController && !characterInputData.ArmsMovementData.ArmsStickReleased)
        {
            // Player is trying to move arms using gamepad, move the arms (no reliance on grab to move
            _rb.MoveRotation(Mathf.LerpAngle(_rb.rotation, _rotationZ, _speed * Time.fixedDeltaTime));
        }
    }
}


