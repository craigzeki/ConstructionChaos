using SolidUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GoalRequirement
{
    [SerializeField] public ObjectiveObject RequiredObject;
    [SerializeField] public uint QuantityRequired = 0;
    [SerializeField][ReadOnly] public uint QuantityInZone = 0;

    public bool RequirementMet()
    {
        return QuantityInZone >= QuantityRequired;
    }
}
