using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// A colour that an objective object can be.
/// </summary>
[CreateAssetMenu(fileName = "ObjectiveColour", menuName = "ScriptableObjects/ObjectiveColour", order = 1)]
public class ObjectiveColour : ScriptableObject, IEquatable<ObjectiveColour>, INetworkSerializable
{
    /// <summary>
    /// The friendly string of the colour.
    /// </summary>
    [SerializeField]
    private string friendlyString;
    public string FriendlyString => friendlyString;

    /// <summary>
    /// The actual colour of the colour.
    /// </summary>
    [SerializeField]
    private Color colour;
    public Color Colour => colour;


    /// <summary>
    /// Specific Equals opperation for ObjectiveColour
    /// </summary>
    /// <param name="other">ObjectiveColour to compare with</param>
    /// <returns>True of False (Equal or Not Equal)</returns>
    public bool Equals(ObjectiveColour other)
    {
        return
        (
            (friendlyString.Equals(other.friendlyString)) &&
            (colour.Equals(other.colour))
        );
    }

    /// <summary>
    /// Override of Equals to provide correct equivalence check
    /// </summary>
    /// <param name="other">Object to compare with</param>
    /// <returns>True of False (Equal or Not Equal)</returns>
    public override bool Equals(object other)
    {
        if(other is null) return false;
        if(ReferenceEquals(this, other)) return true;
        if(other.GetType() != this.GetType()) return base.Equals(other);
        return Equals((ObjectiveColour)other);
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
        // unchecked allows overflows to occur and be truncated without throwing an exception
        unchecked
        {
            int hashCode = 17;
            hashCode = (hashCode * 23) + (friendlyString != null ? friendlyString.GetHashCode() : 0);
            hashCode = (hashCode * 23) + (colour != null ? colour.GetHashCode() : 0);
            return hashCode;
        }
    }

    void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        serializer.SerializeValue(ref friendlyString);
        serializer.SerializeValue(ref colour);
    }
}
