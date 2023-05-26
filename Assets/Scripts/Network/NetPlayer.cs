using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// A class which contains all networking methods for the player
/// </summary>
[RequireComponent(typeof(CharacterInputHandler))]
public class NetPlayer : NetworkBehaviour
{
    [SerializeField] private string _objectiveString = "";
    [SerializeField] private List<SpriteRenderer> _partsToColour = new List<SpriteRenderer>();
    [SerializeField][ReadOnly] private Color _playerColour = Color.white;

    public NetworkVariable<int> PlayerColorIndex = new NetworkVariable<int>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        

        if (IsLocalPlayer)
        {
            // Set the camera to follow the player's head
            FollowCam.Instance.SetFollowTarget(transform.GetChild(0));
            GameManager.Instance.LocalPlayer = this;
        }

        if (IsServer)
        {
            // Move to an empty spawn point
            SpawnManager.Instance.SpawnPlayer(gameObject);

            NetworkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
            //_networkObjectiveString.Value = _objectiveString;
        }
        else
        {
            PlayerColorIndex.OnValueChanged += SetPlayerColour;
            SetPlayerColour(0, PlayerColorIndex.Value);
        }
        
        
        
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        PlayerColorIndex.OnValueChanged -= SetPlayerColour;
        GameManager.Instance.UnRegisterPlayer(OwnerClientId);
        if (NetworkManager != null) NetworkManager.OnClientConnectedCallback -= NetworkManager_OnClientConnectedCallback;
    }

    private void OnDisable()
    {
        if (NetworkManager != null) NetworkManager.OnClientConnectedCallback -= NetworkManager_OnClientConnectedCallback;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if(NetworkManager != null) NetworkManager.OnClientConnectedCallback -= NetworkManager_OnClientConnectedCallback;
    }

    /// <summary>
    /// Callback which is called by the Client on the Server (automagically) once the Client has fully spawned and connected
    /// </summary>
    /// <param name="obj"></param>
    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        
        // Register with the Game Manager
        GameManager.Instance.RegisterNewPlayer(OwnerClientId, this);
        
        if(GameManager.Instance.PlayerData.TryGetValue(OwnerClientId, out NetPlayerData playerData))
        {
            SetPlayerColour(0,playerData.ColourIndex);
        }

        // Send the ownerClientId to some objects which need it (to report to the objective system)
        List<Ragdoll> ragdollObjects = GetComponentsInChildren<Ragdoll>().ToList<Ragdoll>();
        ObjectiveActionReporter objectiveActionReporter = GetComponentInChildren<ObjectiveActionReporter>();
        foreach (Ragdoll ragdoll in ragdollObjects)
        {
            ragdoll.ClientId = OwnerClientId;
            ragdoll.ObjectiveActionReporter = objectiveActionReporter;
        }

        if (NetworkManager != null) NetworkManager.OnClientConnectedCallback -= NetworkManager_OnClientConnectedCallback;

    }

    /// <summary>
    /// Receive a new objective from the ObjectiveManager and set this locally on the client.
    /// </summary>
    /// <param name="newObjectiveString"></param>
    [ClientRpc]
    public void SetObjectiveStringClientRpc(string newObjectiveString, ClientRpcParams clientRpcParams = default)
    {
        _objectiveString = newObjectiveString != null ? newObjectiveString : "No Objective";
        GameUIManager.Instance.SetObjectiveText(_objectiveString);
    }

    private void SetPlayerColour(int previous, int current)
    {
        _playerColour = GameManager.Instance.GetPlayerColour(current);
        foreach(SpriteRenderer spriteRenderer in _partsToColour)
        {
            spriteRenderer.color = _playerColour;
        }
    }
}