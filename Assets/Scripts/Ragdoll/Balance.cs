using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Balance : Ragdoll
{
    [SerializeField] public float TargetRotation;
    [SerializeField] public float Force;
    private Rigidbody2D _rb;

    

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        //mvoe the body part towards its rotation with force
        if(_isActive) _rb.MoveRotation(Mathf.LerpAngle(_rb.rotation, TargetRotation, Force * Time.fixedDeltaTime));
    }

    
}
