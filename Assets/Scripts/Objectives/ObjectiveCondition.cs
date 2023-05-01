using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A condition that an objective action can have.
/// </summary>
[CreateAssetMenu(fileName = "ObjectiveCondition", menuName = "ScriptableObjects/ObjectiveCondition", order = 1)]
public class ObjectiveCondition : ScriptableObject
{
    /// <summary>
    /// The friendly string of the condition.
    /// </summary>
    [SerializeField]
    private string friendlyString;
    public string FriendlyString => friendlyString;
}
