using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using static UnityEngine.UI.Image;

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

    [SerializeField] private Animator _anim;

    private Coroutine _walkLeft, _walkRight;
    private bool _isOnGround = false;
    private bool _isActive = true;

    // Start is called before the first frame update
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) _isActive = !_isActive;

        if ((Input.GetAxisRaw("Horizontal") != 0) && _isActive)
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

        
        if(IsOnGround() && Input.GetKeyDown(KeyCode.Space) && _isActive)
        {
            _bodyRB.AddForce(Vector2.up * _jumpForce);
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
