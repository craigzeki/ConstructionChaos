using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

public class Arms : MonoBehaviour
{
    [SerializeField] private float _speed = 300f;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private KeyCode _mouseButton;

    private Vector3 _playerPos = Vector3.zero;
    private Vector3 _delta = Vector3.zero;
    private float _rotationZ = 0f;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _playerPos.x = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
        _playerPos.y = Camera.main.ScreenToWorldPoint(Input.mousePosition).y;
        _playerPos.z = 0;

        _delta = _playerPos - transform.position;

        _rotationZ = Mathf.Atan2(_delta.x, -_delta.y) * Mathf.Rad2Deg;
        if(Input.GetKey(_mouseButton))
        {
            _rb.MoveRotation(Mathf.LerpAngle(_rb.rotation, _rotationZ, _speed * Time.fixedDeltaTime));
        }
    }
}
