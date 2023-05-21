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
    [SerializeField] public bool UseQtyAsPercentageInScene = false;
    [SerializeField][ReadOnly] public uint PercentBasedQty = 0;

    public bool RequirementMet()
    {
        return UseQtyAsPercentageInScene ? QuantityInZone >= PercentBasedQty : QuantityInZone >= QuantityRequired;
    }

    public void CalculatePercentBasedQty(int totalInScene)
    {
        QuantityRequired = (uint)Mathf.Clamp((int)QuantityRequired, (int)0, (int)100);
        float percentage = (float)QuantityRequired / (float)100f;
        PercentBasedQty = (uint)Mathf.CeilToInt(percentage * totalInScene);
    }
}
