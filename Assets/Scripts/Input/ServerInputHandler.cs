using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ServerInputHandler : NetworkBehaviour
{
    /// <summary>
    /// Dictionary of CharacterInputHandler for each player referenced by their ClientId
    /// </summary>
    public Dictionary<ulong, CharacterInputHandler> _characterInputHandlers = new Dictionary<ulong, CharacterInputHandler>();

    /// <summary>
    /// Instance of the ServerInputHandler
    /// </summary>
    public static ServerInputHandler Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void AddPlayer(ulong clientId, GameObject playerObject)
    {
        if (_characterInputHandlers.ContainsKey(clientId)) return;
        var characterInputHandler = playerObject.GetComponent<CharacterInputHandler>();
        _characterInputHandlers.Add(clientId, characterInputHandler);
    }

    public void RemovePlayer(ulong clientId)
    {
        if (!_characterInputHandlers.ContainsKey(clientId)) return;
        _characterInputHandlers.Remove(clientId);
    }

    [ServerRpc]
    public void UpdateInputDataServerRpc(CharacterInputData characterInputData, ServerRpcParams serverRpcParams = default)
    {
        if (!_characterInputHandlers.ContainsKey(serverRpcParams.Receive.SenderClientId)) return;
        _characterInputHandlers[serverRpcParams.Receive.SenderClientId].UpdateInputData(characterInputData);
    }
}
