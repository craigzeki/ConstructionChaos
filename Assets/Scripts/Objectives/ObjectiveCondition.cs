using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TypeReferences;
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
    private string _friendlyString;
    public string FriendlyString => _friendlyString;

    [SerializeField]
    private bool _duringCountdown = false;
    public bool DuringCountdown => _duringCountdown;

    [SerializeField]
    private bool _requiresObjectToBeInZone = false;
    public bool RequiresObjectToBeInZone => _requiresObjectToBeInZone;


    [SerializeField]
    private Zone.ZONE _requiredZone = Zone.ZONE.NO_ZONE;
    public Zone.ZONE RequiredZone => _requiredZone;

    [SerializeField]
    private bool _requiresZoneToBeOnGround = false;
    public bool RequireZoneToBeOnGround => _requiresZoneToBeOnGround;

    [SerializeField]
    [HideInInspector] // Not currently used - in place for when coloured zones are implemented
    private ZoneColour _requiredZoneColour;
    public ZoneColour RequiredZoneColour => _requiredZoneColour;

    

    /// <summary>
    /// Specific Equals opperation for ObjectiveCondition
    /// </summary>
    /// <param name="other">ObjectiveColour to compare with</param>
    /// <returns>True of False (Equal or Not Equal)</returns>
    public bool Equals(ObjectiveCondition other)
    {
        if (other is null) return false;
        //if (ReferenceEquals(this, other)) return true;
        return (
                (_friendlyString.Equals(other.FriendlyString)) &&
                (_duringCountdown == other.DuringCountdown) &&
                (_requiresObjectToBeInZone == other.RequiresObjectToBeInZone) &&
                (_requiredZone == other.RequiredZone) &&
                (_requiresZoneToBeOnGround == other.RequireZoneToBeOnGround) &&
                (_requiredZoneColour != null ? _requiredZoneColour.Equals(other.RequiredZoneColour) : other.RequiredZoneColour == null)

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
            hashCode = (hashCode * 23) + (_friendlyString != null ? _friendlyString.GetHashCode() : 0);
            hashCode = (hashCode * 23) + (_duringCountdown.GetHashCode());
            hashCode = (hashCode * 23) + (_requiresObjectToBeInZone.GetHashCode());
            hashCode = (hashCode * 23) + (_requiredZone.GetHashCode());
            hashCode = (hashCode * 23) + (_requiresZoneToBeOnGround.GetHashCode());
            hashCode = (hashCode * 23) + (_requiredZoneColour != null ? _requiredZoneColour.GetHashCode() : 0);
            return hashCode;
        }
    }
}
