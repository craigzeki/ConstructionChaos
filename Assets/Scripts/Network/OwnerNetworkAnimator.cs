using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;

/// <summary>
/// Allows the client to dictate the animation state of the player<br/>
/// This doesn't need to be server authoritative because the server doesn't care about the animation state
/// </summary>
public class OwnerNetworkAnimator : NetworkAnimator
{
    /// <summary>
    /// Checks if the server is authoritative over this animator
    /// </summary>
    /// <returns>False</returns>
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
