using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This class represents an interactible zone on the map
/// </summary>
public class Zone : NetworkBehaviour, IEquatable<Zone>
{
    public enum ZONE : int
    {
        NO_ZONE = 0,
        GOAL_ZONE,
        LOCATION_ZONE,
        NUM_OF_ZONES
    }

    /// <summary>
    /// The friendly string which is used to construct the objective string
    /// </summary>
    [SerializeField] private string _friendlyString = "";
    public string FriendlyString => _friendlyString;

    [SerializeField] private uint _points;
    public uint Points => _points;

    /// <summary>
    /// The Zone type
    /// </summary>
    [SerializeField] private ZONE _zoneType = ZONE.GOAL_ZONE;
    public ZONE ZoneType => _zoneType;

    [SerializeField] private bool _isOnGround = false;
    public bool IsOnGround => _isOnGround;


    /// <summary>
    /// Specific Equals opperation for Zone
    /// </summary>
    /// <param name="other">Zone to compare with</param>
    /// <returns>True of False (Equal or Not Equal)</returns>
    public bool Equals(Zone other)
    {
        if (other is null) return false;
        return (
                (_friendlyString.Equals(other.FriendlyString)) &&
                (_points == other.Points) &&
                (_zoneType == other.ZoneType) &&
                (_isOnGround == other.IsOnGround)
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
        return Equals((Zone)other);
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
            hashCode = (hashCode * 23) + (_points.GetHashCode());
            hashCode = (hashCode * 32) + _zoneType.GetHashCode();
            hashCode = (hashCode * 32) + _isOnGround.GetHashCode();
            return hashCode;
        }
    }
}
