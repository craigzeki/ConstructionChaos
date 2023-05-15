using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Zone : MonoBehaviour
{
    public enum ZONE
    {
        GOAL_ZONE = 0,
        NUM_OF_ZONES
    }

    [SerializeField] private ZONE _zoneType = ZONE.GOAL_ZONE;
    public ZONE ZoneType => _zoneType;

}
