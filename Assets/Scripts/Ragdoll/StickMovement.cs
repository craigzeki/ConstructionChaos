using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class StickMovement : MonoBehaviour
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

    void Update()
    {
        //check if user is pressing up or down
        if(!Mathf.Approximately(InputHandler.Instance.MoveVerticalAxis, 0f))
        {
            //check if the axis input is above a tolerance to try and reject unintentional up / down movement
            bool newCollapse = InputHandler.Instance.MoveVerticalAxis >= _verticalMinValue ? false : InputHandler.Instance.MoveVerticalAxis <= (-_verticalMinValue) ? true : _collapse;
            if (newCollapse != _collapse)
            {
                //collapse the player and send a message to all the body parts to do the same
                _collapse = newCollapse;
                gameObject.BroadcastMessage("OnCollapse", _collapse);
            }
            Debug.Log("_collapse: " + _collapse.ToString());
        }
        
        //check if the user is inputting horizontal movement
        if (!Mathf.Approximately(InputHandler.Instance.MoveHorizontalAxis, 0f) && !_collapse)
        {
            if (InputHandler.Instance.MoveHorizontalAxis > 0)
            {
                //move right
                _anim.Play("WalkRight");
                if (_walkLeft != null) StopCoroutine(_walkLeft);
                _walkRight = StartCoroutine(MoveRight(_stepWait));
            }
            else
            {
                //move left
                _anim.Play("WalkLeft");
                if (_walkRight != null) StopCoroutine(_walkRight);
                _walkLeft = StartCoroutine(MoveLeft(_stepWait));
            }
        }
        else
        {
            //idle
            if (_walkLeft != null) StopCoroutine(_walkLeft);
            if (_walkRight != null) StopCoroutine(_walkRight);
            _anim.Play("Idle");
        }
    }

    private void FixedUpdate()
    {
        if (_framesToNextJump > 0)
        {
            _framesToNextJump--;
        }
        else
        {
            //check if user can and is requesting to jump
            if (IsOnGround() && !_collapse && InputHandler.Instance.JumpValue)
            {
                //do jump
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

        //check each ground point, if any are contacting the ground, set isOnGround = true
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
        _leftLegRB.AddForce(Vector2.right * (_speed * 1000) * Time.deltaTime);
        yield return new WaitForSeconds(seconds);
        _rightLegRB.AddForce(Vector2.right * (_speed * 1000) * Time.deltaTime);
        yield return new WaitForSeconds(seconds);
    }

    /// <summary>
    /// Move the legs using force - leg angles are changed in the animator
    /// </summary>
    /// <param name="seconds">time between moving each leg</param>
    /// <returns></returns>
    IEnumerator MoveLeft(float seconds)
    {
        _rightLegRB.AddForce(Vector2.left * (_speed * 1000) * Time.deltaTime);
        yield return new WaitForSeconds(seconds);
        _leftLegRB.AddForce(Vector2.left * (_speed * 1000) * Time.deltaTime);
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
