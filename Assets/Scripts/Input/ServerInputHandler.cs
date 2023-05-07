using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Server side input handler on all clients but only runs on the server
/// </summary>
public class ServerInputHandler : NetworkBehaviour
{
    /// <summary>
    /// Dictionary of CharacterInputHandler for each player referenced by their ClientId
    /// </summary>
    private Dictionary<ulong, OLDCharacterInputHandler> _characterInputHandlers = new Dictionary<ulong, OLDCharacterInputHandler>();

    public List<ulong> ClientIds = new List<ulong>();
    public List<OLDCharacterInputHandler> CharacterInputHandlers = new List<OLDCharacterInputHandler>();

    /// <summary>
    /// Instance of the ServerInputHandler
    /// </summary>
    public static ServerInputHandler Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!IsServer) return;
        print("ServerInputHandler.Update()");
        ClientIds = new List<ulong>(_characterInputHandlers.Keys);
        CharacterInputHandlers = new List<OLDCharacterInputHandler>(_characterInputHandlers.Values);
    }

    /// <summary>
    /// Adds a player to the dictionary of CharacterInputHandlers
    /// </summary>
    /// <param name="clientId">The clientId of the player to add</param>
    /// <param name="playerObject">The player object to add</param>
    public void AddPlayer(ulong clientId, GameObject playerObject)
    {
        if (!IsServer) return;
        if (_characterInputHandlers.ContainsKey(clientId)) return;
        print($"Adding player with clientId {clientId} to ServerInputHandler");
        var characterInputHandler = playerObject.GetComponent<OLDCharacterInputHandler>();
        _characterInputHandlers.Add(clientId, characterInputHandler);
    }

    /// <summary>
    /// Removes a player from the dictionary of CharacterInputHandlers
    /// </summary>
    /// <param name="clientId">The clientId of the player to remove</param>
    public void RemovePlayer(ulong clientId)
    {
        if (!IsServer) return;
        if (!_characterInputHandlers.ContainsKey(clientId)) return;
        _characterInputHandlers.Remove(clientId);
    }

    /// <summary>
    /// Updates the input data for a player
    /// </summary>
    /// <param name="characterInputData">The input data to update</param>
    /// <param name="serverRpcParams">The ServerRpcParams</param>
    [ServerRpc(RequireOwnership = false)]
    public void UpdateInputDataServerRpc(CharacterInputData characterInputData, ServerRpcParams serverRpcParams = default)
    {
        print($"Recieved input data from client: {serverRpcParams.Receive.SenderClientId}");
        if (!_characterInputHandlers.ContainsKey(serverRpcParams.Receive.SenderClientId)) return;
        _characterInputHandlers[serverRpcParams.Receive.SenderClientId].UpdateInputData(characterInputData);
    }

    [ContextMenu(nameof(LogPlayers))]
    private void LogPlayers()
    {
        foreach (var keyValuePair in _characterInputHandlers)
        {
            print($"ClientId: {keyValuePair.Key} - CharacterInputHandler: {keyValuePair.Value}");
        }
    }
}
