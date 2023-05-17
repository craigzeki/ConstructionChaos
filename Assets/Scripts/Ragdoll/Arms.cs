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

    void FixedUpdate()
    {
        // Not active / collapsed
        if (!_isActive) return;

        if (IsServer)
            HandleArms(CharacterInputHandler.CharacterInputData);
    }

    private void HandleArms(CharacterInputData characterInputData)
    {
        // If player is trying to grab and is a mouse controller
        if((characterInputData.IsGrabbingLeft || characterInputData.IsGrabbingRight) && characterInputData.ArmsMovementData.IsMouseController)
        {
            // Move the amrs
            _rb.MoveRotation(Mathf.LerpAngle(_rb.rotation, characterInputData.ArmsMovementData.ArmRotation, _speed * Time.fixedDeltaTime));
        }
        else if(!characterInputData.ArmsMovementData.IsMouseController && !characterInputData.ArmsMovementData.ArmsStickReleased)
        {
            // Player is trying to move arms using gamepad, move the arms (no reliance on grab to move
            _rb.MoveRotation(Mathf.LerpAngle(_rb.rotation, characterInputData.ArmsMovementData.ArmRotation, _speed * Time.fixedDeltaTime));
        }
    }
}