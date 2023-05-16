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
    private bool _inverse;
    public bool Inverse => _inverse;

    /// <summary>
    /// The required action
    /// </summary>
    [SerializeField]
    private ObjectiveAction _action;
    public ObjectiveAction Action => _action;

    /// <summary>
    /// The required colour
    /// </summary>
    [SerializeField]
    private ObjectiveColour _colour;
    public ObjectiveColour Colour => _colour;

    /// <summary>
    /// The required object
    /// </summary>
    [SerializeField]
    private ObjectiveObject _object;
    public ObjectiveObject Object => _object;

    /// <summary>
    /// The required condition
    /// </summary>
    [SerializeField]
    private ObjectiveCondition _condition;
    public ObjectiveCondition Condition => _condition;

	/// <summary>
	/// The generated string which is displayed to the player
	/// </summary>
    [SerializeField]
    private string _objectiveString;
    public string ObjectiveString => _objectiveString;


    /// <summary>
    /// The zone (if any) required for the condition
    /// </summary>
    [SerializeField]
    private Zone _zone;
    public Zone Zone => _zone;

    /// <summary>
    /// Constructor for the objective class
    /// </summary>
    /// <param name="action">The required action</param>
    /// <param name="colour">The required colour</param>
    /// <param name="objObject">The required object</param>
    /// <param name="condition">The required condition</param>
    /// <param name="inverse">Modifier indicating the objective is to avoid instead of do</param>
    public Objective(ObjectiveAction action, ObjectiveColour colour, ObjectiveObject objObject, ObjectiveCondition condition, Zone zone, bool inverse)
    {
        this._action = action;
        this._colour = colour;
        this._object = objObject;
        this._condition = condition;
        this._zone = zone;
        this._inverse = inverse;
        
        if(condition == null)
        {
            this._objectiveString = "INVALID OBJECTIVE";
        }
        else
        {
            this._objectiveString = ObjectiveManager.Instance.CreateObjectiveString(this);
        }
        
    }

    /// <summary>
    /// Specific Equals opperation for Objective
    /// </summary>
    /// <param name="other">Objective to compare with</param>
    /// <returns>True of False (Equal or Not Equal)</returns>
    public bool Equals(Objective other)
    {
        if (other is null) return false;
        return (
                (_action != null ? _action.Equals(other._action) : other._action == null) &&
                (_colour != null ? _colour.Equals(other._colour) : other._colour == null) &&
                (_object != null ? _object.Equals(other._object) : other._object == null) &&
                (_condition != null ? _condition.Equals(other._condition) : other._condition == null) &&
                _inverse.Equals(other._inverse) &&
                _objectiveString.Equals(other._objectiveString) &&
                (_zone != null ? _zone.Equals(other.Zone) : other.Zone == null)
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
            hashCode = (hashCode * 23) + (_action != null ? _action.GetHashCode() : 0);
            hashCode = (hashCode * 23) + (_colour != null ? _colour.GetHashCode() : 0);
            hashCode = (hashCode * 23) + (_object != null ? _object.GetHashCode() : 0);
            hashCode = (hashCode * 23) + (_condition != null ? _condition.GetHashCode() : 0);
            hashCode = (hashCode * 23) + _inverse.GetHashCode();
            hashCode = (hashCode * 23) + (_objectiveString != null ? _objectiveString.GetHashCode() : 0);
            hashCode = (hashCode* 23) + (_zone != null ? _zone.GetHashCode() : 0);
            return hashCode;
        }
    }
}
