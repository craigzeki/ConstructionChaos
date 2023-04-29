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

    void FixedUpdate()
    {
        //mvoe the body part towards its rotation with force
        if(_isActive) _rb.MoveRotation(Mathf.LerpAngle(_rb.rotation, TargetRotation, Force * Time.fixedDeltaTime));
    }

    /// <summary>
    /// Rceives a broadcast message indicating the player has collapsed and sets the internal state accordingly
    /// </summary>
    /// <param name="collapse">True: collapsed   False: restored from collapsed</param>
    /// <remarks>Call using BroadcastMessage</remarks>
    public void OnCollapse(bool collapse)
    {
        _isActive = !collapse;
    }
}
