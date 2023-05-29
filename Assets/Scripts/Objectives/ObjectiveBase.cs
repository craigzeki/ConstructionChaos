using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectiveBase : ScriptableObject
{
    /// <summary>
    /// The friendly string of the action. Must be in the format "... <COLOUR> <OBJECT> <CONDITION>".
    /// </summary>
    [SerializeField]
    protected string _friendlyString;
    public string FriendlyString => _friendlyString;

    [SerializeField]
    protected uint _points;
    public uint Points => _points;
}
