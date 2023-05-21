using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TypeReferences;
using UnityEngine;

/// <summary>
/// An action that can be performed on an objective object.
/// </summary>
[CreateAssetMenu(fileName = "ObjectiveAction", menuName = "ScriptableObjects/ObjectiveAction", order = 1)]
public class ObjectiveAction : ScriptableObject, IEquatable<ObjectiveAction>
{
    /// <summary>
    /// The friendly string of the action. Must be in the format "... <COLOUR> <OBJECT> <CONDITION>".
    /// </summary>
    [SerializeField]
    private string _friendlyString;
    public string FriendlyString => _friendlyString;

    /// <summary>
    /// The amount of time that the player has to perform this action for in order for it to count
    /// </summary>
    [SerializeField]
    private uint _requiredPerformanceTime = 3;
    public uint RequiredPerformanceTime => _requiredPerformanceTime;

    /// <summary>
    /// A list of all the possible conditions that can be applied to this action.
    /// </summary>
    [SerializeField]
    private List<ObjectiveCondition> _possibleConditions = new List<ObjectiveCondition>();
    public List<ObjectiveCondition> PossibleConditions => _possibleConditions;

    [Inherits(typeof(ObjectiveActionBehaviour))]
    [SerializeField] private List<TypeReference> _actionBehaviours = new List<TypeReference>();
    public List<TypeReference> ActionBehaviours => _actionBehaviours;

    /// <summary>
    /// Specific Equals opperation for ObjectiveAction
    /// </summary>
    /// <param name="other">ObjectiveAction to compare with</param>
    /// <returns>True of False (Equal or Not Equal)</returns>
    public bool Equals(ObjectiveAction other)
    {
        if (other is null) return false;
        return (
                (_friendlyString.Equals(other._friendlyString)) &&
                (_requiredPerformanceTime == other._requiredPerformanceTime) &&
                Enumerable.SequenceEqual(_possibleConditions.OrderBy(i => i.FriendlyString), other._possibleConditions.OrderBy(i => i.FriendlyString)) &&
                Enumerable.SequenceEqual(_actionBehaviours.OrderBy(i => i.Type.Name), other._actionBehaviours.OrderBy(i => i.Type.Name))
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
        return Equals((ObjectiveAction)other);
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
            hashCode = (hashCode * 23) + (_friendlyString != null ? _friendlyString.GetHashCode() : 0);
            hashCode = (hashCode * 32) + _requiredPerformanceTime.GetHashCode();
            foreach(ObjectiveCondition objectiveCondition in _possibleConditions)
            {
                hashCode = (hashCode * 23) + objectiveCondition.GetHashCode();
            }
            foreach(TypeReference type in _actionBehaviours)
            {
                hashCode = (hashCode * 23) + type.GetHashCode();
            }
            return hashCode;
        }
    }
}
