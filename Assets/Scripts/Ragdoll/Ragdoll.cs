using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Base class for all Ragdoll behaviour
/// </summary>
public class Ragdoll : NetworkBehaviour
{
    /// <summary>
    /// A reference to this characters input handler, usually found on the parent of the player character prefab
    /// </summary>
    [SerializeField, Tooltip("A reference to this characters input handler, usually found on the character parent object.")] public CharacterInputHandler CharacterInputHandler;

    /// <summary>
    /// Is the ragdoll 'active' - if not, it is collapsed
    /// </summary>
    protected bool _isActive = true;
    
    /// <summary>
    /// Rceives a broadcast message indicating the player has collapsed and sets the internal state accordingly
    /// </summary>
    /// <param name="collapse">True: collapsed   False: restored from collapsed</param>
    /// <remarks>Call using BroadcastMessage</remarks>
    public void OnCollapse(bool collapse)
    {
        _isActive = !collapse;
    }
}
