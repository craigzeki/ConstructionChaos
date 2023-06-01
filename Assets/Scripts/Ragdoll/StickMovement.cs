using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.IO.LowLevel.Unsafe;
using Unity.Netcode;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.InputSystem;

public class StickMovement : Ragdoll
{
    [SerializeField] private float _speed = 1.5f;
    [SerializeField] private float _maxVelocity = 23f;
    [SerializeField] private float _stepWait = 0.5f;
    [SerializeField] private float _jumpForce = 10f;
    [SerializeField] private float _positionRadius;
    [SerializeField] private LayerMask _groundLayerMask;
    [SerializeField] private List<Transform> _groundPositions;
    [SerializeField] private Rigidbody2D _leftLegRB;
    [SerializeField] private Rigidbody2D _rightLegRB;
    [SerializeField] private Rigidbody2D _bodyRB;
    [SerializeField] private float _verticalMinValue = 0.5f;
    [SerializeField] private uint _timeBetweenJumpsInPhysicsFrames = 10; //required to prevent compunded jumps

    [SerializeField] private Animator _anim;

    private Coroutine _walkLeft, _walkRight;
    private bool _collapse = false;
    private bool _allowedToJump = true;
    private List<Grab> _grabbers = new List<Grab>();
    private float _maxSqrVelocity;

    protected override void Awake()
    {
        //not calling base.Awake() as do not need the ragdoll effects, only access to set the collapse feature

        // Get all the ragdoll parts and set the input handler
        Ragdoll[] ragdolls = GetComponentsInChildren<Ragdoll>();

        for (int i = 1; i < ragdolls.Length; i++)
        {
            ragdolls[i].CharacterInputHandler = CharacterInputHandler;
            ragdolls[i].StickMovement = this;
        }

        _grabbers = GetComponentsInChildren<Grab>().ToList<Grab>();
        _maxSqrVelocity = _maxVelocity * _maxVelocity;
    }

    private void FixedUpdate()
    {
        if (IsServer)
        {
            HandleMovementAndJump(CharacterInputHandler.CharacterInputData);
            LimitVelocity();
        }
            
    }

    public void DoCollapse(bool collapse, bool breakApart)
    {
        _collapse = collapse;
        gameObject.BroadcastMessage("OnCollapse", (collapse, breakApart));
    }

    private void LimitVelocity()
    {
        _maxSqrVelocity = _maxVelocity * _maxVelocity;
        
        if(_bodyRB.velocity.sqrMagnitude > _maxSqrVelocity)
        {
            _bodyRB.velocity = _bodyRB.velocity.normalized * _maxVelocity;
            //Debug.Log("LIMITED VELOCITY to " + _bodyRB.velocity);
        }
    }

    /// <summary>
    /// Moves the player on the server
    /// </summary>
    private void HandleMovementAndJump(CharacterInputData characterInputData)
    {
        // Handle movement
        // Check if user is pressing up or down
        if(!Mathf.Approximately(characterInputData.MoveVerticalAxis, 0f))
        {
            // Check if the axis input is above a tolerance to try and reject unintentional up / down movement
            bool newCollapse = characterInputData.MoveVerticalAxis >= _verticalMinValue ? false : characterInputData.MoveVerticalAxis <= (-_verticalMinValue) ? true : _collapse;
            if (newCollapse != _collapse)
            {
                // Collapse the player and send a message to all the body parts to do the same
                DoCollapse(newCollapse, false);
            }
            Debug.Log("_collapse: " + _collapse.ToString());
        }
        
        // Check if the user is inputting horizontal movement
        if (!Mathf.Approximately(characterInputData.MoveHorizontalAxis, 0f) && !_collapse)
        {
            if (characterInputData.MoveHorizontalAxis > 0)
            {
                // Move right
                _anim.Play("WalkRight");
                if (_walkLeft != null) StopCoroutine(_walkLeft);
                _walkRight = StartCoroutine(MoveRight(_stepWait));
            }
            else
            {
                // Move left
                _anim.Play("WalkLeft");
                if (_walkRight != null) StopCoroutine(_walkRight);
                _walkLeft = StartCoroutine(MoveLeft(_stepWait));
            }
        }
        else
        {
            // Idle
            if (_walkLeft != null) StopCoroutine(_walkLeft);
            if (_walkRight != null) StopCoroutine(_walkRight);
            _anim.Play("Idle");
        }

        // Check if user can and is requesting to jump
        if ((IsOnGround() || IsHoldingLedge()) && !_collapse && _allowedToJump && characterInputData.JumpValue)
        {
            // Do jump
            _bodyRB.AddForce(Vector2.up * _jumpForce);
            Debug.Log("Jumped");
            _allowedToJump = false;

            // If holding the ledge, let go
            ReleaseLedge();

            StartCoroutine(WaitToJumpAgain());
        }
    }

    private bool IsHoldingLedge()
    {
        bool result = false;
        foreach(Grab grabber in _grabbers)
        {
            result |= grabber.IsHoldingLedge;
        }
        return result;
    }

    private void ReleaseLedge()
    {
        foreach(Grab grabber in _grabbers)
        {
            if (grabber.IsHoldingLedge) grabber.Release = true;
        }
    }

    /// <summary>
    /// Check if the user is contacting the ground
    /// </summary>
    /// <returns>True: Player is on the ground<br/>False: Player is not on the ground</returns>
    public bool IsOnGround(Collider2D other = null)
    {
        // Check each ground point, if any are contacting the ground, set isOnGround = true
        foreach (Transform t in _groundPositions)
        {
            //! This needs to get all of the colliders because it will always return your own colliders
            //! The other option is to set the legs and feet to a different layer to the rest of the body
            // Get all the colliders in the ground position
            Collider2D[] colliders = Physics2D.OverlapCircleAll(t.position, _positionRadius, _groundLayerMask);

            foreach (Collider2D collider in colliders)
            {
                // If the collider is attached to the player, ignore it
                if (collider.transform.parent == transform)
                    continue;
                
                // If the collider is the one we are checking against, return true
                if (other != null && collider == other) return true;

                // If we are checking against a specific collider but are colliding with another collider then don't return true
                else if (other != null && collider != null) continue;

                // If we are not checking against a specific collider but have collided, return true
                else if (other == null && collider != null) return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Wait for a set number of physics frames before allowing the player to jump again
    /// </summary>
    private IEnumerator WaitToJumpAgain()
    {
        for (int i = 0; i < _timeBetweenJumpsInPhysicsFrames; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        _allowedToJump = true;
    }

    /// <summary>
    /// Move the legs using force - leg angles are changed in the animator
    /// </summary>
    /// <param name="seconds">time between moving each leg</param>
    /// <returns></returns>
    IEnumerator MoveRight(float seconds)
    {
        _leftLegRB.AddForce(Vector2.right * (_speed * 1000) * Time.fixedDeltaTime);
        yield return new WaitForSeconds(seconds);
        _rightLegRB.AddForce(Vector2.right * (_speed * 1000) * Time.fixedDeltaTime);
        yield return new WaitForSeconds(seconds);
    }

    /// <summary>
    /// Move the legs using force - leg angles are changed in the animator
    /// </summary>
    /// <param name="seconds">time between moving each leg</param>
    /// <returns></returns>
    IEnumerator MoveLeft(float seconds)
    {
        _rightLegRB.AddForce(Vector2.left * (_speed * 1000) * Time.fixedDeltaTime);
        yield return new WaitForSeconds(seconds);
        _leftLegRB.AddForce(Vector2.left * (_speed * 1000) * Time.fixedDeltaTime);
        yield return new WaitForSeconds(seconds);
    }

#if UNITY_EDITOR
    //if we are in the editor draw the gizmos for the ground contact points
    private void OnDrawGizmos()
    {
        Handles.color = Color.red;
        foreach (Transform t in _groundPositions)
        {
            Handles.DrawWireDisc(t.position, new Vector3(0, 0, 1), _positionRadius);
        }
    }
#endif
}
