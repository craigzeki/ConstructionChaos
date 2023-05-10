using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Grab : Ragdoll
{
    //[SerializeField] KeyCode _mouseButton;
    [SerializeField] LayerMask _grabableLayerMask;
    private bool _hold = false;
    private FixedJoint2D _joint;

    void FixedUpdate()
    {
        if (!IsOwner) return;

        if (IsServer)
            HandleGrab(CharacterInputHandler.CharacterInputData);
        else
            HandleGrabServerRpc(CharacterInputHandler.CharacterInputData);
    }

    [ServerRpc(RequireOwnership = false)]
    private void HandleGrabServerRpc(CharacterInputData characterInputData, ServerRpcParams serverRpcParams = default)
    {
        HandleGrab(characterInputData);
    }

    private void HandleGrab(CharacterInputData characterInputData)
    {
        // Player is trying to grab, and is allowed to
        if((characterInputData.IsGrabbingLeft || characterInputData.IsGrabbingRight) && _isActive)
        {
            // Set hold to true
            _hold = true;
        }
        else
        {
            // Player has let go, destroy the joint between player and item
            _hold = false;
            Destroy(_joint);
            _joint = null;
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        //if (!IsOwner) return;
        if (!IsServer) return;
        if (!_isActive) return; // Not active or collapsed
        if ((1 << collision.gameObject.layer) != _grabableLayerMask) return; // Item is not on the grabable layer
        if (_hold && _joint == null) // Allowed to grab something and not already holding anything
        {
            // Get the colliding objects rigidbody and create a joint between us and it
            Rigidbody2D rb = collision.transform.GetComponent<Rigidbody2D>();
            if(rb != null)
            {
                _joint = transform.gameObject.AddComponent(typeof(FixedJoint2D)) as FixedJoint2D;
                _joint.connectedBody = rb;
            }
            else
            {
                // No rigidbody found so can't create a joint
                // Could try and create a rigidbody first and then a joint
                // Or could implement another method of grabing which doesn't use physics
                // For now.... do nothing
            }
        }
    }

    
}
