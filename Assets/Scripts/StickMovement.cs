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

    [SerializeField] private Animator _anim;

    private Coroutine _walkLeft, _walkRight;
    private bool _collapse = false;

    #region Inputs

    private bool _jumpValue = false;
    private float _moveHorizontalAxis = 0f;
    private float _moveVerticalAxis = 0f;

    public void Jump(InputAction.CallbackContext value)
    {
        if (!value.performed) return;

        _jumpValue = value.ReadValueAsButton();
        Debug.Log("_jumpValue: " + _jumpValue.ToString());
        
    }

    public void Move(InputAction.CallbackContext value)
    {
        _moveHorizontalAxis = value.ReadValue<float>();
        Debug.Log("_horizontalAxis: " + _moveHorizontalAxis.ToString());
    }

    public void Collapse(InputAction.CallbackContext value)
    {
        
        _moveVerticalAxis = value.ReadValue<float>();
        Debug.Log("_verticalAxis: " + _moveVerticalAxis.ToString());
    }

    #endregion


    // Update is called once per frame
    void Update()
    {
        if(!Mathf.Approximately(_moveVerticalAxis, 0f))
        {
            bool newCollapse = _moveVerticalAxis > _verticalMinValue ? false : _moveVerticalAxis < (-_moveVerticalAxis) ? true : _collapse;
            if (newCollapse != _collapse)
            {
                _collapse = newCollapse;
                gameObject.BroadcastMessage("OnCollapse", _collapse);
            }
            Debug.Log("_collapse: " + _collapse.ToString());
        }
        //if (Input.GetKeyDown(KeyCode.E)) _collapse = !_collapse;

        if (!Mathf.Approximately(_moveHorizontalAxis, 0f) && !_collapse)
        {
            if (_moveHorizontalAxis > 0)
            {
                _anim.Play("WalkRight");
                if (_walkLeft != null) StopCoroutine(_walkLeft);
                _walkRight = StartCoroutine(MoveRight(_stepWait));
            }
            else
            {
                _anim.Play("WalkLeft");
                if (_walkRight != null) StopCoroutine(_walkRight);
                _walkLeft = StartCoroutine(MoveLeft(_stepWait));
            }
        }
        else
        {
            if (_walkLeft != null) StopCoroutine(_walkLeft);
            if (_walkRight != null) StopCoroutine(_walkRight);
            _anim.Play("Idle");
        }

        
        

    }

    private void FixedUpdate()
    {
        if (IsOnGround() && !_collapse && _jumpValue)
        {
            _bodyRB.AddForce(Vector2.up * _jumpForce);
            Debug.Log("Jumped");
            _jumpValue= false;
        }
    }

    private bool IsOnGround()
    {
        bool isOnGround = false;

        foreach (Transform t in _groundPositions)
        {
            isOnGround |= Physics2D.OverlapCircle(t.position, _positionRadius, _groundLayerMask);
        }

        return isOnGround;
    }

    IEnumerator MoveRight(float seconds)
    {
        _leftLegRB.AddForce(Vector2.right * (_speed * 1000) * Time.deltaTime);
        yield return new WaitForSeconds(seconds);
        _rightLegRB.AddForce(Vector2.right * (_speed * 1000) * Time.deltaTime);
        yield return new WaitForSeconds(seconds);
    }
    
    IEnumerator MoveLeft(float seconds)
    {
        _rightLegRB.AddForce(Vector2.left * (_speed * 1000) * Time.deltaTime);
        yield return new WaitForSeconds(seconds);
        _leftLegRB.AddForce(Vector2.left * (_speed * 1000) * Time.deltaTime);
        yield return new WaitForSeconds(seconds);
    }

#if UNITY_EDITOR
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
