using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private float _stepWait = 0.5f;
    [SerializeField] private float _jumpForce = 10f;
    [SerializeField] private float _positionRadius;
    [SerializeField] private LayerMask _groundLayerMask;
    [SerializeField] private List<Transform> _groundPositions;
    [SerializeField] private Rigidbody2D _leftLegRB;
    [SerializeField] private Rigidbody2D _rightLegRB;
    [SerializeField] private Rigidbody2D _bodyRB;
    [SerializeField] private float _verticalMinValue = 0.5f;
    [SerializeField] private uint _timeBetweenJumpsInPhysicsFrames = 5; //required to prevent compunded jumps

    [SerializeField] private Animator _anim;

    private Coroutine _walkLeft, _walkRight;
    private bool _collapse = false;
    private uint _framesToNextJump = 0;

    protected override void Awake()
    {
        //not calling base.Awake() as do not need the ragdoll effects, only access to set the collapse feature

        // Get all the ragdoll parts and set the input handler
        Ragdoll[] ragdolls = GetComponentsInChildren<Ragdoll>();

        for (int i = 1; i < ragdolls.Length; i++)
        {
            ragdolls[i].CharacterInputHandler = CharacterInputHandler;
        }
    }

    private void FixedUpdate()
    {
        if (IsServer)
            HandleMovementAndJump(CharacterInputHandler.CharacterInputData);
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
                _collapse = newCollapse;
                gameObject.BroadcastMessage("OnCollapse", (_collapse, true));
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

        // Handle jumping
        if (_framesToNextJump > 0)
        {
            _framesToNextJump--;
        }
        else
        {
            // Check if user can and is requesting to jump
            if (IsOnGround() && !_collapse && characterInputData.JumpValue)
            {
                // Do jump
                _bodyRB.AddForce(Vector2.up * _jumpForce);
                Debug.Log("Jumped");
                //_jumpValue= false;
            }
            _framesToNextJump = _timeBetweenJumpsInPhysicsFrames;
        }
    }

    /// <summary>
    /// Check if the user is contacting the ground
    /// </summary>
    /// <returns>True: Player is on the ground<br/>False: Player is not on the ground</returns>
    private bool IsOnGround()
    {
        bool isOnGround = false;

        // Check each ground point, if any are contacting the ground, set isOnGround = true
        foreach (Transform t in _groundPositions)
        {
            isOnGround |= Physics2D.OverlapCircle(t.position, _positionRadius, _groundLayerMask);
        }

        return isOnGround;
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
