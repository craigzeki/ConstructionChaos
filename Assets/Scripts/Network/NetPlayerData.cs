using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


/// <summary>
/// Class to store objective data related to a player
/// </summary>
public class NetPlayerData
{
    public NetPlayer NetPlayer = null;
    public Objective Objective = null;

    public ClientRpcParams ClientRpcParams = default;
    public ulong ClientId;
    public uint Score = 0;
    public int ColourIndex = 0;

    public NetPlayerData(ulong clientId, NetPlayer netPlayer)
    {
        NetPlayer = netPlayer;
        ClientId = clientId;
        CreateRpcParams(clientId);
    }

    public NetPlayerData(ulong clientId, NetPlayer netPlayer, Objective objective)
    {
        NetPlayer = netPlayer;
        Objective = objective;
        ClientId = clientId;
        CreateRpcParams(clientId);
    }

    private void CreateRpcParams(ulong clientId)
    {
        ClientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };
    }

    public void UpdateRpcParams(ulong clientId)
    {
        ClientRpcParams.Send = new ClientRpcSendParams
        {
            TargetClientIds = new ulong[] { clientId }
        };
    }
}
