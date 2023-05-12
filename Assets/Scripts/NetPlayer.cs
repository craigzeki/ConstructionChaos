using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterInputHandler))]
public class NetPlayer : NetworkBehaviour
{
    [SerializeField] private string _objectiveString = "";

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsLocalPlayer)
        {
            // Set the camera to follow the player's head
            FollowCam.Instance.SetFollowTarget(transform.GetChild(0));
        }

        if (IsServer)
        {
            // TODO: Register myself with the Objective Manager
            ObjectiveManager.Instance.RegisterPlayer(OwnerClientId, this);
        }
    }

    [ClientRpc]
    public void SetObjectiveStringClientRpc(string newObjectiveString)
    {
        _objectiveString = newObjectiveString;
    }
}
