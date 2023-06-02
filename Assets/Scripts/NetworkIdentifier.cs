using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkIdentifier : NetworkBehaviour
{
    public NetworkVariable<uint> NetworkId = new NetworkVariable<uint>();

    override public void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkId.Value = (uint)Random.Range(0, 100000);
        }
    }
}
