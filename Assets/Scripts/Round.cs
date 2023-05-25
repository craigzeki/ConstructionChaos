using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Round
{
    [SerializeField] public GameObject RoundPrefab;
    [SerializeField] public uint RoundDurationSeconds = 180;
    [SerializeField] public bool ShowLeaderBoardAfterRound = false;
}
