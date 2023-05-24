using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(HingeJoint2D))]
public class Arms : Ragdoll
{
    [SerializeField] private float _speed = 300f;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Rigidbody2D _connectedRb;
    [SerializeField] private float _releaseThreshold = 0.2f;

    private HingeJoint2D _hingeJoint2d;
    private Vector3 _connectedPosition;
    private float _initialConnectionDistance;
    private float _currentConnectionDistance;
    private Grab _grabber;

    protected override void Awake()
    {
        base.Awake();
        _hingeJoint2d = GetComponent<HingeJoint2D>();
        _connectedRb = _hingeJoint2d.connectedBody;
        _connectedPosition = _connectedRb.gameObject.transform.localPosition;
        _initialConnectionDistance = Vector3.Distance(transform.localPosition, _connectedPosition);
        _grabber = GetComponentInChildren<Grab>();
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

        if(_grabber != null)
        {
            if(_grabber.ReadyToGrab)
            {
                _connectedPosition = _connectedRb.gameObject.transform.localPosition;
                _currentConnectionDistance = Vector3.Distance(transform.localPosition, _connectedPosition);
                Debug.Log("Distance: " + Mathf.Abs(_currentConnectionDistance - _initialConnectionDistance).ToString("##.###"));
                if (Mathf.Abs(_currentConnectionDistance - _initialConnectionDistance) > _releaseThreshold)
                {
                    _grabber.Release = true;
                }

            }
            
        }
        
    }
}