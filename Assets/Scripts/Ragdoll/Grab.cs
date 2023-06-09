using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Grab : Ragdoll
{
    /// <sumamry>
    /// Enum for which hand is being used
    /// </summary>
    [System.Serializable]
    private enum HandType
    {
        LEFT,
        RIGHT
    }

    /// <summary>
    /// Which hand is this script attached to
    /// </summary>
    [SerializeField] private HandType _handType;
    [SerializeField] LayerMask _grabableLayerMask;

    private LayerMask _objectLayerMask;
    [SerializeField] private int _platformIgnoreGrabLayerMask;
    
    private FixedJoint2D _joint;
    public GameObject GrabbedObject
    {
        get
        {
            if (_joint == null) return null;
            return _joint.connectedBody.gameObject;
        }
    }
    public bool ReadyToGrab { get; private set; } = false;
    public bool IsHoldingLedge { get; private set; } = false;
    public bool Release = false;

    protected override void Awake()
    {
        //do nothing - but prevents Ragdoll.Awake from running, which in turn prevents grab collider from detatching during break apart.
    }

    void FixedUpdate()
    {
        if (IsServer)
            HandleGrab(CharacterInputHandler.CharacterInputData);
    }

    private void HandleGrab(CharacterInputData characterInputData)
    {
        // Player is trying to grab, and is allowed to
        if((_isActive) && (!Release) && ((characterInputData.IsGrabbingLeft && _handType == HandType.LEFT) || (characterInputData.IsGrabbingRight && _handType == HandType.RIGHT)))
        {
            // Set hold to true
            ReadyToGrab = true;
        }
        else
        {
            // Player has let go, destroy the joint between player and item
            ReadyToGrab = false;
            IsHoldingLedge = false;
            if (_joint != null)
                if (_joint.attachedRigidbody.gameObject.layer == _platformIgnoreGrabLayerMask)
                    _joint.attachedRigidbody.gameObject.layer = _objectLayerMask;
            Destroy(_joint);
            _joint = null;
            Release = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) return; // Only the server can grab
        if (!_isActive) return; // Not active or collapsed
        if (((1 << collision.gameObject.layer) & _grabableLayerMask) == 0) return; // Item is not on the grabable layer
        if (ReadyToGrab && _joint == null) // Allowed to grab something and not already holding anything
        {
            // Get the colliding objects rigidbody and create a joint between us and it
            Rigidbody2D rb = collision.transform.GetComponent<Rigidbody2D>();
            if(rb != null)
            {
                _joint = collision.gameObject.AddComponent(typeof(FixedJoint2D)) as FixedJoint2D;
                _joint.connectedBody = GetComponent<Rigidbody2D>();
                if (collision.gameObject.tag == "Ledge") IsHoldingLedge = true;
                if (collision.gameObject.layer == LayerMask.NameToLayer("Grabable"))
                {
                    _objectLayerMask = collision.gameObject.layer;
                    collision.gameObject.layer = _platformIgnoreGrabLayerMask;
                }
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