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

    /// <summary>
    /// Is the ragdoll 'broken'
    /// </summary>
    protected bool _isBroken = false;

    /// <summary>
    /// Reference to this gameobjects hinge joint
    /// </summary>
    private HingeJoint2D _hingeJoint;

    /// <summary>
    /// Used to store the correct (original) rotation of the body part
    /// </summary>
    private Quaternion _initialRotation;

    /// <summary>
    /// Used to store the correct (original) local position of the body part
    /// </summary>
    private Vector3 _originLocalPosition;

    /// <summary>
    /// Used to detect when the body part has returned to its original position
    /// </summary>
    [SerializeField] private float _rejoinAnchorDistanceThreshold = 0.005f;

    protected virtual void Awake()
    {
        _originLocalPosition = transform.position;
        _hingeJoint = GetComponent<HingeJoint2D>();
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
                _hingeJoint.enabled = false;
                _isBroken = true;
            }
        }

        if (_isBroken && !collapseInfo.collapse)
        {
            //reconnect joints
            StartCoroutine(WaitAndSetRotation());
            _hingeJoint.enabled = true;
            _isBroken = false;
        }
    }

    /// <summary>
    /// Waits until the body part has moved back to its intended position, and then sets rotation.<br/>
    /// Avoids limbs attatching in rotations outside of their joint rotation limits.
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitAndSetRotation()
    {
        while(Vector2.Distance(transform.localPosition, _originLocalPosition) > _rejoinAnchorDistanceThreshold)
        {
            yield return new WaitForEndOfFrame();
        }

        transform.rotation = _initialRotation;
    }

}
