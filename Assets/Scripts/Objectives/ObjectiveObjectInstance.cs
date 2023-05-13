using System;
using System.Collections;
using System.Collections.Generic;
using TypeReferences;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// An instance of an objective object.<br/>
/// This is the object that will be placed in the world.
/// </summary>
public class ObjectiveObjectInstance : NetworkBehaviour, IEquatable<ObjectiveObjectInstance>
{
    /// <summary>
    /// The objective object that this instance is based on.
    /// </summary>
    [SerializeField]
    private ObjectiveObject objectiveObject;
    public ObjectiveObject ObjectiveObject => objectiveObject;

    /// <summary>
    /// The colour of the objective object.
    /// </summary>
    [SerializeField]
    [Dropdown("objectiveObject.PossibleColours")]
    private ObjectiveColour _objectiveColour;
    public ObjectiveColour ObjectiveColour => _objectiveColour;

    public NetworkVariable<ObjectiveColour> NetworkObjectiveColour = new NetworkVariable<ObjectiveColour>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // The object should be an actual colour, not any coloured
            while ((_objectiveColour?.FriendlyString == "any coloured") || (_objectiveColour == null))
            {
                // Pick a random colour from the list of possible colours and assign it to the objective colour
                _objectiveColour = objectiveObject.PossibleColours[UnityEngine.Random.Range(0, objectiveObject.PossibleColours.Count)];
            }

            ObjectiveManager.Instance.RegisterObject(this);

            // For each action, add the configured components to the gameObject
            foreach (ObjectiveAction action in objectiveObject.PossibleActions)
            {
                foreach (TypeReference typeReference in action.ActionBehaviours)
                {
                    this.gameObject.AddComponent(typeReference);
                }
            }

            NetworkObjectiveColour.Value = _objectiveColour;
        }
        else
        {
            _objectiveColour = NetworkObjectiveColour.Value;
        }

        // Set the colour of the object to the colour of the objective colour
        GetComponent<SpriteRenderer>().color = NetworkObjectiveColour.Value.Colour;
    }

    /// <summary>
    /// New euqality check which compares parameter values
    /// </summary>
    /// <param name="other">The ObjectiveObjectInstance instance to compare with</param>
    /// <returns>True: ObjectiveObjectInsances contain identical parameters</returns>
    public bool Equals(ObjectiveObjectInstance other)
    {
        return objectiveObject.Equals(other.objectiveObject) && _objectiveColour.Equals(other._objectiveColour);
    }

    /// <summary>
    /// Overriden equality check to allow calling of our new equality check in the case where the object type matches<br/>
    /// Will call the base (MonoBehaviour) equality check in the case where the types do not match.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public override bool Equals(object other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (other.GetType() != this.GetType()) return base.Equals(other);
        return Equals((ObjectiveObjectInstance)other);
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
            hashCode = (hashCode * 23) + objectiveObject.GetHashCode();
            hashCode = (hashCode * 23) + _objectiveColour.GetHashCode();
            return hashCode;
        }
    }
}