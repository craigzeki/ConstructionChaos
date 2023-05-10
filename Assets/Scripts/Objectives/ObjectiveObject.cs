using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// An object that can be used in an objective.
/// </summary>
[CreateAssetMenu(fileName = "ObjectiveObject", menuName = "ScriptableObjects/ObjectiveObject", order = 1)]
public class ObjectiveObject : ScriptableObject, IEquatable<ObjectiveObject>
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


    /// <summary>
    /// Specific Equals opperation for ObjectiveObject
    /// </summary>
    /// <param name="other">ObjectiveColour to compare with</param>
    /// <returns>True of False (Equal or Not Equal)</returns>
    public bool Equals(ObjectiveObject other)
    {
        return (
                (friendlyString.Equals(other.friendlyString)) &&
                Enumerable.SequenceEqual(possibleColours.OrderBy(i => i.FriendlyString), other.possibleColours.OrderBy(i => i.FriendlyString)) &&
                Enumerable.SequenceEqual(possibleActions.OrderBy(i => i.FriendlyString), other.possibleActions.OrderBy(i => i.FriendlyString))
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
        return Equals((ObjectiveObject)other);
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
            foreach(ObjectiveColour objectiveColour in possibleColours)
            {
                hashCode = (hashCode * 23) + objectiveColour.GetHashCode();
            }
            foreach(ObjectiveAction objectiveAction in possibleActions)
            {
                hashCode = (hashCode*23) + objectiveAction.GetHashCode();
            }
            return hashCode;
        }
    }
}
