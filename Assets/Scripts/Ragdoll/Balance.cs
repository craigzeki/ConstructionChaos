using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Keeps the body part balanced by rotating it towards the target rotation
/// </summary>
/// <remarks>Requires a Rigidbody2D component</remarks>
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
        if (!IsOwner) return;

        if (IsServer)
            HandleBalance();
        else
            HandleBalanceServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void HandleBalanceServerRpc()
    {
        HandleBalance();
    }

    private void HandleBalance()
    {
        // Move the body part towards its rotation with force
        if(_isActive) _rb.MoveRotation(Mathf.LerpAngle(_rb.rotation, TargetRotation, Force * Time.fixedDeltaTime));
    }
}
