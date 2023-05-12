using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Class to store objective data related to a player
/// </summary>
public class ObjectivePlayerData
{
    public NetPlayer NetPlayer;
    public Objective Objective;

    public ObjectivePlayerData()
    {
    }

    public ObjectivePlayerData(NetPlayer netPlayer)
    {
        NetPlayer = netPlayer;
    }

    public ObjectivePlayerData(NetPlayer netPlayer, Objective objective)
    {
        NetPlayer = netPlayer;
        Objective = objective;
    }
}
