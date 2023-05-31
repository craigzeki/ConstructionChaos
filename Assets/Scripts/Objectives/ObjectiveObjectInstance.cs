using SolidUtilities;
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
    private ObjectiveObject _objectiveObject;
    public ObjectiveObject ObjectiveObject => _objectiveObject;

    /// <summary>
    /// The colour of the objective object.
    /// </summary>
    [SerializeField]
    [Dropdown("_objectiveObject.PossibleColours")]
    private ObjectiveColour _objectiveColour;
    public ObjectiveColour ObjectiveColour => _objectiveColour;

    /// <summary>
    /// Can be set to true to exlude this object from the objective manager<br/>
    /// This is useful for objects that you only want to configure for GOAL ZONE<br/>
    /// E.g. The player.
    /// </summary>
    [SerializeField]
    private bool _excludeFromObjectiveManager = false;
    public bool ExcludeFromObjectiveManager => _excludeFromObjectiveManager;

    public NetworkVariable<ObjectiveColour> NetworkObjectiveColour = new NetworkVariable<ObjectiveColour>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // The object should be an actual colour, not any coloured
            while ((_objectiveColour?.FriendlyString == ObjectiveManager.Instance.AnyColourString) || (_objectiveColour == null))
            {
                if (_objectiveObject.PossibleColours.Count == 0) break;
                // Pick a random colour from the list of possible colours and assign it to the objective colour
                _objectiveColour = _objectiveObject.PossibleColours[UnityEngine.Random.Range(0, _objectiveObject.PossibleColours.Count)];
            }

            // Removed for now, the ObjectManager was re-worked to use FindObjectOfType as it needed to get zones before objects
            // and there was no easy way to do this if everything was self registering
            //ObjectiveManager.Instance.RegisterObject(this);

            // For each action, add the configured components to the gameObject
            foreach (ObjectiveAction action in _objectiveObject.PossibleActions)
            {
                // For each action, add it as a component to the object
                foreach (TypeReference typeReference in action.ActionBehaviours)
                {
                    // only add it if it inherits from ObjectActionBehaviour - this is required to be able to set parameters
                    if (typeReference.Type.IsSubclassOf(typeof(ObjectiveActionBehaviour)))
                    {

                        ObjectiveActionBehaviour component = this.gameObject.AddComponent(typeReference) as ObjectiveActionBehaviour;
                        component.Conditions = action.PossibleConditions;
                        // Do not need to set the condition & zone as this will be set during action detection in the ObjectiveActionBehaviour
                        component.Objective = new Objective(action, _objectiveColour, _objectiveObject, null, null, false);
                    }
                    
                    
                }
            }

            NetworkObjectiveColour.Value = _objectiveColour;
        }
        else
        {
            _objectiveColour = NetworkObjectiveColour.Value;
        }

        // Set the colour of the object to the colour of the objective colour
        GetComponent<SpriteRenderer>().color = NetworkObjectiveColour.Value != null ? NetworkObjectiveColour.Value.Colour : Color.white;
    }

    /// <summary>
    /// New euqality check which compares parameter values
    /// </summary>
    /// <param name="other">The ObjectiveObjectInstance instance to compare with</param>
    /// <returns>True: ObjectiveObjectInsances contain identical parameters</returns>
    public bool Equals(ObjectiveObjectInstance other)
    {
        if (other is null) return false;
        return _objectiveObject.Equals(other._objectiveObject) &&
            _objectiveColour.Equals(other._objectiveColour) &&
            (_excludeFromObjectiveManager == other.ExcludeFromObjectiveManager);
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
    /// New equality check which compares parameter values and instance ID<br/>
    /// This is used to compare objects in the ArrowManager to ensure that they are actually separate instances of the same object
    /// </summary>
    /// <param name="other">The ObjectiveObjectInstance instance to compare with</param>
    /// <returns>True: ObjectiveObjectInstances contain identical parameters and are identical instances<br/>
    /// False: ObjectiveObjectInstances contain identical parameters but are not identical instances</returns>
    public bool EqualsWithID(ObjectiveObjectInstance other)
    {
        if (other is null) return false;
        return Equals(other) && GetInstanceID() == other.GetInstanceID();
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
            hashCode = (hashCode * 23) + _objectiveObject.GetHashCode();
            hashCode = (hashCode * 23) + _objectiveColour.GetHashCode();
            hashCode = (hashCode * 23) + _excludeFromObjectiveManager.GetHashCode();
            return hashCode;
        }
    }
}