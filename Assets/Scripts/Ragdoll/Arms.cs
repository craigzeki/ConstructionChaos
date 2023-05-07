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

    private Vector3 _controllerPos = Vector3.zero;
    private Vector3 _delta = Vector3.zero;
    private float _rotationZ = 0f;

    /// <summary>
    /// Calculates the arm rotation value based on which controller is active
    /// </summary>
    private void CalculateArmRotation()
    {
        if(CharacterInputHandler.CharacterInputData.ArmsMovementData.IsMouseController)
        {
            _controllerPos.x = Camera.main.ScreenToWorldPoint(CharacterInputHandler.CharacterInputData.ArmsMovementData.ArmsControllerInput).x;
            _controllerPos.y = Camera.main.ScreenToWorldPoint(CharacterInputHandler.CharacterInputData.ArmsMovementData.ArmsControllerInput).y;
            _controllerPos.z = 0;

            _delta = _controllerPos - transform.position;

            _rotationZ = Mathf.Atan2(_delta.x, -_delta.y) * Mathf.Rad2Deg;
        }
        else
        {
            _rotationZ = Mathf.Atan2(CharacterInputHandler.CharacterInputData.ArmsMovementData.ArmsControllerInput.x, -CharacterInputHandler.CharacterInputData.ArmsMovementData.ArmsControllerInput.y) * Mathf.Rad2Deg;
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        // Not active / collapsed
        if (!_isActive) return;

        if (IsServer)
            HandleArms(CharacterInputHandler.CharacterInputData);
        else
            HandleArmsServerRpc(CharacterInputHandler.CharacterInputData);
    }

    [ServerRpc(RequireOwnership = false)]
    private void HandleArmsServerRpc(CharacterInputData characterInputData, ServerRpcParams serverRpcParams = default)
    {
        HandleArms(characterInputData);
    }

    private void HandleArms(CharacterInputData characterInputData)
    {
        CalculateArmRotation();

        // If player is trying to grab and is a mouse controller
        if((characterInputData.IsGrabbingLeft || characterInputData.IsGrabbingRight) && characterInputData.ArmsMovementData.IsMouseController)
        {
            // Move the amrs
            _rb.MoveRotation(Mathf.LerpAngle(_rb.rotation, _rotationZ, _speed * Time.fixedDeltaTime));
        }
        else if(characterInputData.ArmsMovementData.IsMouseController && characterInputData.ArmsMovementData.ArmsStickReleased)
        {
            // Player is trying to move arms using gamepad, move the arms (no reliance on grab to move
            _rb.MoveRotation(Mathf.LerpAngle(_rb.rotation, _rotationZ, _speed * Time.fixedDeltaTime));
        }
    }
}


