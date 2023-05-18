using SolidUtilities;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


/// <summary>
/// Base class for all Ragdoll behaviour
/// </summary>
public class Ragdoll : NetworkBehaviour
{
    [SerializeField]
    [ReadOnly]
    public ulong ClientId;

    public ObjectiveActionReporter ObjectiveActionReporter;

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
    /// Reference to this gameObject's hinge joint
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
    /// Used to store the initial local position - needed to reset player to centre of parent before moving during spawn
    /// </summary>
    private Vector3 _initialLocalPosition;

    /// <summary>
    /// Used to detect when the body part has returned to its original position
    /// </summary>
    [SerializeField] private float _rejoinAnchorDistanceThreshold = 0.005f;

    protected virtual void Awake()
    {
        _originLocalPosition = transform.position;
        _hingeJoint = GetComponent<HingeJoint2D>();
        _initialRotation = transform.rotation;
        _initialLocalPosition = transform.localPosition;
    }

    public void ResetLocalPosition()
    {
        transform.localPosition = _initialLocalPosition;
    }

    /// <summary>
    /// Receives a broadcast message indicating the player has collapsed and sets the internal state accordingly
    /// </summary>
    /// <param name="collapse">True: collapsed   False: restored from collapsed</param>
    /// <remarks>Call using BroadcastMessage</remarks>
    public void OnCollapse((bool collapse, bool breakApart) collapseInfo)
    {
        // Original collapse
        _isActive = !collapseInfo.collapse;

        if (_hingeJoint != null)
        {
            if (collapseInfo.breakApart)
            {
                // Disconnect joints and store previous connected rigidbody
                _hingeJoint.enabled = false;
                _isBroken = true;
            }
        }

        if (_isBroken && !collapseInfo.collapse)
        {
            // Reconnect joints
            StartCoroutine(WaitAndSetRotation());
            _hingeJoint.enabled = true;
            _isBroken = false;
        }
    }

    /// <summary>
    /// Waits until the body part has moved back to its intended position, and then sets rotation.<br/>
    /// Avoids limbs attaching in rotations outside of their joint rotation limits.
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitAndSetRotation()
    {
        yield return new WaitUntil(() => Vector2.Distance(transform.localPosition, _originLocalPosition) < _rejoinAnchorDistanceThreshold);

        transform.rotation = _initialRotation;
    }
}