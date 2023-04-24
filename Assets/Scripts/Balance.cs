using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Balance : MonoBehaviour
{
    [SerializeField] public float TargetRotation;
    [SerializeField] public float Force;
    private Rigidbody2D _rb;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        
    }


    void FixedUpdate()
    {
        _rb.MoveRotation(Mathf.LerpAngle(_rb.rotation, TargetRotation, Force * Time.fixedDeltaTime));
    }
}
