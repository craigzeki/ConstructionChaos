using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A condition that an objective action can have.
/// </summary>
[CreateAssetMenu(fileName = "ObjectiveCondition", menuName = "ScriptableObjects/ObjectiveCondition", order = 1)]
public class ObjectiveCondition : ScriptableObject, IEquatable<ObjectiveCondition>
{
    /// <summary>
    /// The friendly string of the condition.
    /// </summary>
    [SerializeField]
    private string friendlyString;
    public string FriendlyString => friendlyString;

    /// <summary>
    /// Specific Equals opperation for ObjectiveCondition
    /// </summary>
    /// <param name="other">ObjectiveColour to compare with</param>
    /// <returns>True of False (Equal or Not Equal)</returns>
    public bool Equals(ObjectiveCondition other)
    {
        return (
                (friendlyString.Equals(other.friendlyString))
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
        return Equals((ObjectiveCondition)other);
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
            hashCode = (hashCode * 23) + (friendlyString != null ? friendlyString.GetHashCode() : 0);
            return hashCode;
        }
    }
}
