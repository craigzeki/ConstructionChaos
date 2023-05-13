using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterInputHandler))]
public class NetPlayer : NetworkBehaviour
{
    [SerializeField] private string _objectiveString = "";
   //[SerializeField] private NetworkVariable<FixedString128Bytes> _networkObjectiveString = new NetworkVariable<FixedString128Bytes>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsLocalPlayer)
        {
            // Set the camera to follow the player's head
            FollowCam.Instance.SetFollowTarget(transform.GetChild(0));
            //_objectiveString = _networkObjectiveString.ToString();
            //_networkObjectiveString.OnValueChanged += OnObjectiveStringChanged;
        }

        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
            //_networkObjectiveString.Value = _objectiveString;
        }
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        // Register with the Game Manager
        GameManager.Instance.RegisterNewPlayer(OwnerClientId, this);
    }

    //private void OnObjectiveStringChanged(FixedString128Bytes previous, FixedString128Bytes current)
    //{
    //    _objectiveString = current.ToString();
    //}

    /// <summary>
    /// Receive a new objective from the ObjectiveManager and set this locally on the client.
    /// </summary>
    /// <param name="newObjectiveString"></param>
    [ClientRpc]
    public void SetObjectiveStringClientRpc(string newObjectiveString, ClientRpcParams clientRpcParams = default)
    {
        _objectiveString = newObjectiveString != null ? newObjectiveString : "No Objective";
    }
}
