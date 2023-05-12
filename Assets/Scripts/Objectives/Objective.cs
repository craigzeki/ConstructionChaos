using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Container for storing objective information
/// </summary>
[System.Serializable]
public class Objective : IEquatable<Objective>
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
    /// Constructor for the objective class
    /// </summary>
    /// <param name="action">The required action</param>
    /// <param name="colour">The required colour</param>
    /// <param name="object">The required object</param>
    /// <param name="condition">The required condition</param>
    /// <param name="inverse">Modifier indicating the objective is to avoid instead of do</param>
    public Objective(ObjectiveAction action, ObjectiveColour colour, ObjectiveObject @object, ObjectiveCondition condition, bool inverse)
    {
        this.action = action;
        this.colour = colour;
        this.@object = @object;
        this.condition = condition;
        this.inverse = inverse;
        this.objectiveString = ObjectiveManager.Instance.CreateObjectiveString(this);
    }

    /// <summary>
    /// Specific Equals opperation for Objective
    /// </summary>
    /// <param name="other">Objective to compare with</param>
    /// <returns>True of False (Equal or Not Equal)</returns>
    public bool Equals(Objective other)
    {
        return (
                action.Equals(other.action) &&
                colour.Equals(other.colour) &&
                @object.Equals(other.@object) &&
                condition.Equals(other.condition) &&
                inverse.Equals(other.inverse) &&
                objectiveString.Equals(other.objectiveString)
                );
    }

    /// <summary>
    /// Override of Equals to provide correct equivalence check
    /// </summary>
    /// <param name="other">Object to compare with</param>
    /// <returns>True of False (Equal or Not Equal)</returns>
    public override bool Equals(object other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (other.GetType() != this.GetType()) return base.Equals(other);
        return Equals((Objective)other);
    }


    /// <summary>
    /// Override of GetHashCode to provide correct hash for Dictionary
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// https://stackoverflow.com/questions/3613102/why-use-a-prime-number-in-hashcode<br/>
    /// https://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-overriding-gethashcode
    /// </remarks>
    public override int GetHashCode()
    {
        //unchecked allows overflows to occur and be truncated without throwing an exception
        unchecked
        {
            int hashCode = 17;
            hashCode = (hashCode * 23) + (action != null ? action.GetHashCode() : 0);
            hashCode = (hashCode * 23) + (colour != null ? colour.GetHashCode() : 0);
            hashCode = (hashCode * 23) + (@object != null ? @object.GetHashCode() : 0);
            hashCode = (hashCode * 23) + (condition != null ? condition.GetHashCode() : 0);
            hashCode = (hashCode * 23) + inverse.GetHashCode();
            hashCode = (hashCode * 23) + (objectiveString != null ? objectiveString.GetHashCode() : 0);
            return hashCode;
        }
    }
}
