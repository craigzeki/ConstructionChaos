using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An object that can be used in an objective.
/// </summary>
[CreateAssetMenu(fileName = "ObjectiveObject", menuName = "ScriptableObjects/ObjectiveObject", order = 1)]
public class ObjectiveObject : ScriptableObject
{
    /// <summary>
    /// The friendly string of the object.
    /// </summary>
    [SerializeField]
    private string friendlyString;
    public string FriendlyString => friendlyString;

    /// <summary>
    /// A list of all the possible colours that this object can be.
    /// </summary>
    [SerializeField]
    private List<ObjectiveColour> possibleColours = new List<ObjectiveColour>();
    public List<ObjectiveColour> PossibleColours => possibleColours;

    /// <summary>
    /// A list of all the possible actions that can be performed on this object.
    /// </summary>
    [SerializeField]
    private List<ObjectiveAction> possibleActions = new List<ObjectiveAction>();
    public List<ObjectiveAction> PossibleActions => possibleActions;
}
