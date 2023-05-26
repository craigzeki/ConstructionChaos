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
    public int NextObjectiveIndex = 0;
    public List<Objective> PossibleObjectives = new List<Objective>();
    public string PlayerName = "";

    public NetPlayerData(ulong clientId, NetPlayer netPlayer)
    {
        NetPlayer = netPlayer;
        Objective = null;
        ClientId = clientId;
        CreateRpcParams(clientId);
        Score = 0;
        ColourIndex = 0;
        NextObjectiveIndex = 0;
        PossibleObjectives.Clear();
        PlayerName = "Player " + (clientId + 1).ToString();
    }

    public NetPlayerData(ulong clientId, NetPlayer netPlayer, Objective objective)
    {
        NetPlayer = netPlayer;
        Objective = objective;
        ClientId = clientId;
        CreateRpcParams(clientId);
        Score = 0;
        ColourIndex = 0;
        NextObjectiveIndex = 0;
        PossibleObjectives.Clear();
        PlayerName = "Player " + (clientId + 1).ToString();
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
