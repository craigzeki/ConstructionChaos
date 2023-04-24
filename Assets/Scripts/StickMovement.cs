using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StickMovement : MonoBehaviour
{
    [SerializeField] private float _speed = 1.5f;
    [SerializeField] private float _stepWait = 0.5f;
    [SerializeField] private float _jumpForce = 10f;
    [SerializeField] private float _positionRadius;
    [SerializeField] private LayerMask _groundLayerMask;
    [SerializeField] private Transform _position;
    [SerializeField] private Rigidbody2D _leftLegRB;
    [SerializeField] private Rigidbody2D _rightLegRB;
    [SerializeField] private Rigidbody2D _bodyRB;

    [SerializeField] private Animator _anim;

    private Coroutine _walkLeft, _walkRight;
    private bool _isOnGround = false;

    // Start is called before the first frame update
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxisRaw("Horizontal") != 0)
        {
            if (Input.GetAxisRaw("Horizontal") > 0)
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

        _isOnGround = Physics2D.OverlapCircle(_position.position, _positionRadius, _groundLayerMask);
        if(_isOnGround && Input.GetKeyDown(KeyCode.Space))
        {
            _bodyRB.AddForce(Vector2.up * _jumpForce);
        }

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
}
