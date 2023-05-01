using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Container for storing objective information
/// </summary>
[System.Serializable]
public class Objective
{
	/// <summary>
	/// Modifier indicating the objective is to avoid instead of do
	/// </summary>
    [SerializeField]
    private bool inverse;
    public bool Inverse => inverse;

    /// <summary>
    /// The required action
    /// </summary>
    [SerializeField]
    private ObjectiveAction action;
    public ObjectiveAction Action => action;

    /// <summary>
    /// The required colour
    /// </summary>
    [SerializeField]
    private ObjectiveColour colour;
    public ObjectiveColour Colour => colour;

    /// <summary>
    /// The required object
    /// </summary>
    [SerializeField]
    private ObjectiveObject @object;
    public ObjectiveObject Object => @object;

    /// <summary>
    /// The required condition
    /// </summary>
    [SerializeField]
    private ObjectiveCondition condition;
    public ObjectiveCondition Condition => condition;

	/// <summary>
	/// The generated string which is displayed to the player
	/// </summary>
    [SerializeField]
    private string objectiveString;
    public string ObjectiveString => objectiveString;

    /// <summary>
    /// The assigned player
    /// </summary>
    [SerializeField]
    private uint player;
    public uint Player => player;

    /// <summary>
    /// Constructor for the objective class
    /// </summary>
    /// <param name="action">The required action</param>
    /// <param name="colour">The required colour</param>
    /// <param name="object">The required object</param>
    /// <param name="condition">The required condition</param>
    /// <param name="inverse">Modifier indicating the objective is to avoid instead of do</param>
    /// <param name="player">The assigned player</param>
    public Objective(ObjectiveAction action, ObjectiveColour colour, ObjectiveObject @object, ObjectiveCondition condition, bool inverse, uint player)
    {
        this.action = action;
        this.colour = colour;
        this.@object = @object;
        this.condition = condition;
        this.inverse = inverse;
        this.player = player;
        this.objectiveString = ObjectiveManager.Instance.CreateObjectiveString(this);
    }
}
