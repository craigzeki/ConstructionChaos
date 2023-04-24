using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Balance : MonoBehaviour
{
    [SerializeField] public float TargetRotation;
    [SerializeField] public float Force;
    private Rigidbody2D _rb;

    private bool _isActive = true;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.E)) _isActive = !_isActive;

    }

    void FixedUpdate()
    {
        
        if(_isActive) _rb.MoveRotation(Mathf.LerpAngle(_rb.rotation, TargetRotation, Force * Time.fixedDeltaTime));
    }
}
