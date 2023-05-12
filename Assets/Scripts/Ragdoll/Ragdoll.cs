using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


/// <summary>
/// Base class for all Ragdoll behaviour
/// </summary>
public class Ragdoll : NetworkBehaviour
{
    /// <summary>
    /// A reference to this characters input handler, usually found on the parent of the player character prefab
    /// </summary>
    [SerializeField, Tooltip("A reference to this characters input handler, usually found on the character parent object.")] public CharacterInputHandler CharacterInputHandler;

    /// <summary>
    /// Is the ragdoll 'active' - if not, it is collapsed
    /// </summary>
    protected bool _isActive = true;
    protected bool _isBroken = false;

    private Rigidbody2D _connectedRigidBody;
    private HingeJoint2D _hingeJoint;
    private Rigidbody2D _rigidBody2D;
    private Vector2 _connectedAnchor;
    private Vector2 _anchorOrigin;
    private Quaternion _initialRotation;
    [SerializeField] private float _rejoinAnchorDistanceThreshold = 0.005f;

    protected virtual void Awake()
    {
        _rigidBody2D = GetComponent<Rigidbody2D>();
        if(TryGetComponent<HingeJoint2D>(out _hingeJoint))
        {
            _connectedRigidBody = _hingeJoint.connectedBody;
        }
        
        _initialRotation = transform.rotation;
    }

    /// <summary>
    /// Receives a broadcast message indicating the player has collapsed and sets the internal state accordingly
    /// </summary>
    /// <param name="collapse">True: collapsed   False: restored from collapsed</param>
    /// <remarks>Call using BroadcastMessage</remarks>
    public void OnCollapse((bool collapse, bool breakApart) collapseInfo)
    {

        //original collapse
        _isActive = !collapseInfo.collapse;

        if (_hingeJoint != null)
        {
            if (collapseInfo.breakApart)
            {
                //disconnect joints and store previous connected rigidbody
                //_connectedRigidBody = _hingeJoint.connectedBody;
                //_connectedAnchor = _hingeJoint.connectedAnchor;
                _anchorOrigin = _hingeJoint.anchor;
                //Destroy(_hingeJoint);
                _hingeJoint.enabled = false;
                _isBroken = true;
            }
        }

        if (_isBroken && !collapseInfo.collapse)
        {
            //reconnect joints
            _hingeJoint.connectedBody = _connectedRigidBody;
            _hingeJoint.connectedAnchor = _connectedAnchor;
            _hingeJoint.anchor = _anchorOrigin;
            StartCoroutine(WaitAndSetRotation());
            _hingeJoint.enabled = true;
            _isBroken = false;
        }
    }

    /// <summary>
    /// Waits until the body part has moved back to its intended position, and then sets rotation.
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitAndSetRotation()
    {
        while(Vector2.Distance(_connectedAnchor, _hingeJoint.connectedAnchor) > _rejoinAnchorDistanceThreshold)
        {
            yield return new WaitForEndOfFrame();
        }
        _rigidBody2D.velocity = Vector2.zero;
        _rigidBody2D.angularVelocity = 0f;
        transform.rotation = _initialRotation;
    }

}
