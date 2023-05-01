using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An action that can be performed on an objective object.
/// </summary>
[CreateAssetMenu(fileName = "ObjectiveAction", menuName = "ScriptableObjects/ObjectiveAction", order = 1)]
public class ObjectiveAction : ScriptableObject
{
    /// <summary>
    /// The friendly string of the action. Must be in the format "... <COLOUR> <OBJECT> <CONDITION>".
    /// </summary>
    [SerializeField]
    private string friendlyString;
    public string FriendlyString => friendlyString;

    /// <summary>
    /// A list of all the possible conditions that can be applied to this action.
    /// </summary>
    [SerializeField]
    private List<ObjectiveCondition> possibleConditions = new List<ObjectiveCondition>();
    public List<ObjectiveCondition> PossibleConditions => possibleConditions;
}
