using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterInputHandler))]
public class NetPlayer : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        ServerInputHandler.Instance.AddPlayer(OwnerClientId, gameObject);
        GetComponent<CharacterInputHandler>().UseInputHandlerEvents = false;
        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        ServerInputHandler.Instance.RemovePlayer(OwnerClientId);
        base.OnNetworkDespawn();
    }
}
