using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ArrowHolder : NetworkBehaviour
{
    public static ArrowHolder Instance;

    [SerializeField] private Transform _playerTrunk;
    public Transform PlayerTrunk => _playerTrunk;

    private void Awake()
    {
        float xScale = 1 / transform.parent.lossyScale.x;
        float yScale = 1 / transform.parent.lossyScale.y;
        transform.lossyScale.Set(xScale, yScale, 1);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsLocalPlayer)
        {
            Instance = this;
        }
    }
}
