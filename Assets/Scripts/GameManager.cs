using SolidUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

/// <summary>
/// The Game Manager
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Possible game states
    /// </summary>
    public enum GAMESTATE
    {
        START = 0,
        MENU,
        LOADING_LOBBY,
        PLAYING_LOBBY,
        LOADING_ROUND,
        PLAYING_ROUND,
        MIDPOINT_LEADERBOARD,
        FINAL_LEADERBOARD,
        DISCONNECTED,
        //Add new states above this line
        NUM_OF_STATES
    }

    /// <summary>
	/// Dictionary which stores the connected players and their associated ObjectivePlayerData, which includes their current objective
	/// </summary>
	public Dictionary<ulong, NetPlayerData> PlayerData = new Dictionary<ulong, NetPlayerData>();


    public event EventHandler<ulong> OnPlayerSpawned;
    public event EventHandler<ulong> OnPlayerDisconnect;
    public event EventHandler<PlayerReconnectData> OnPlayerReconnect;
    public event EventHandler<GAMESTATE> OnGameStateChanged;
    public event EventHandler OnSceneLoaded;


    [SerializeField] [ReadOnly] private GAMESTATE _currentState = GAMESTATE.START;

    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if(_instance == null) _instance = FindObjectOfType<GameManager>();
            return _instance;
        }
    }

    public GAMESTATE CurrentState { get => _currentState; }

    private void Awake()
    {
        DoStateTransition(GAMESTATE.MENU);
    }

    /// <summary>
    /// Called once a player has spawned.<br/>
    /// Adds the player to the PlayerData dictionary.<br/>
    /// Informs subscribers to the OnPlayerSpawned event.
    /// </summary>
    /// <param name="clientId">The client ID of the connected player</param>
    /// <param name="netPlayer">The NetPlayer component of the connected player</param>
    /// <returns></returns>
    public bool RegisterNewPlayer(ulong clientId, NetPlayer netPlayer)
    {
        if (netPlayer == null) return false;

        NetPlayerData objectivePlayerData = new NetPlayerData(clientId, netPlayer);

        if (!PlayerData.TryAdd(clientId, objectivePlayerData)) return false;

        OnPlayerSpawned?.Invoke(this, clientId);

        // send the objective - will be null for the host (joins as the scene is loading - but will be updated when the scene loads in)
        netPlayer.SetObjectiveStringClientRpc(PlayerData[clientId].Objective?.ObjectiveString, PlayerData[clientId].ClientRpcParams);

        return true;
    }

    /// <summary>
	/// To be called if a player disconnects intentionally.<br/>
	/// Removes the player from the PlayerData dictionary.<br/>
	/// Informs subscribers to the OnPlayerDisconnect event.
	/// </summary>
	/// <param name="clientId">The clientId of the disconnecting player</param>
    /// <returns></returns>
	public bool UnRegisterPlayer(ulong clientId)
    {
        //inform all that care so that they can also perform tidy up actions
        OnPlayerDisconnect?.Invoke(this, clientId);

        if (!PlayerData.Remove(clientId)) return false;

        return true;

    }

    /// <summary>
    /// To be called if a player has rejoined following an unintentional disconnection.<br/>
    /// Informs all subscribers to OnPlayerReconnect.<br/>
    /// If the player's old clientID is recognized, it will be replaced with the new one.<br/>
    /// The player's previous Objective will be retained, if not a new one will be created <br/>
    /// </summary>
    /// <param name="currentClientId">The new / current clientId</param>
    /// <param name="previousClientId">The old / previous clientId</param>
    /// <param name="netPlayer">The player's NetPlayer component</param>
    /// <returns>True if successful, False if not sucessful</returns>
    public bool ReRegisterPlayer(ulong currentClientId, ulong previousClientId, NetPlayer netPlayer)
    {
        if (PlayerData.Remove(previousClientId, out NetPlayerData playerData))
        {
            playerData.UpdateRpcParams(currentClientId);
            if (!PlayerData.TryAdd(currentClientId, playerData)) return false;
        }
        else
        {
            if (!PlayerData.TryAdd(currentClientId, new NetPlayerData(currentClientId, netPlayer))) return false;
        }

        //inform all that care so that they can also perform reconnection actions / remappings
        OnPlayerReconnect?.Invoke(this, new PlayerReconnectData(previousClientId, currentClientId));

        return true;
    }

    private void DoEntryTransition(GAMESTATE state)
    {
        switch (state)
        {
            case GAMESTATE.START:
                break;
            case GAMESTATE.MENU:
                break;
            case GAMESTATE.LOADING_LOBBY:
                break;
            case GAMESTATE.PLAYING_LOBBY:
                // TODO move the below to Playing_Round once some rounds are implemented
                // Now that scene has loaded, inform the player of their objective
                // First reset the objective manager so that it will gather all zones / objects
                // Need to consider doing this in the loading screen so that there is no hang to the player
                

                foreach(NetPlayerData netPlayerData in PlayerData.Values)
                {
                    //setup clientRpc params to only send to specific client
                    netPlayerData.NetPlayer?.SetObjectiveStringClientRpc(netPlayerData.Objective?.ObjectiveString, netPlayerData.ClientRpcParams);
                }
                break;
            case GAMESTATE.LOADING_ROUND:
                break;
            case GAMESTATE.PLAYING_ROUND:
                break;
            case GAMESTATE.MIDPOINT_LEADERBOARD:
                break;
            case GAMESTATE.FINAL_LEADERBOARD:
                break;
            case GAMESTATE.DISCONNECTED:
                break;
            case GAMESTATE.NUM_OF_STATES:
            default:
                break;
        }
    }

    private void DoExitTransition(GAMESTATE state)
    {
        switch (state)
        {
            case GAMESTATE.START:
                break;
            case GAMESTATE.MENU:
                break;
            case GAMESTATE.LOADING_LOBBY:
                break;
            case GAMESTATE.PLAYING_LOBBY:
                break;
            case GAMESTATE.LOADING_ROUND:
                break;
            case GAMESTATE.PLAYING_ROUND:
                break;
            case GAMESTATE.MIDPOINT_LEADERBOARD:
                break;
            case GAMESTATE.FINAL_LEADERBOARD:
                break;
            case GAMESTATE.DISCONNECTED:
                break;
            case GAMESTATE.NUM_OF_STATES:
            default:
                break;
        }
    }

    /// <summary>
    /// Performs a state machine transition if transition is allowed.
    /// </summary>
    /// <param name="newState"></param>
    private void DoStateTransition(GAMESTATE newState)
    {
        // check if state is different
        if (newState == _currentState) return;

        // check each transition's guard clauses
        switch (newState)
        {
            case GAMESTATE.START:
                // Not valid to come here (initial state only)
                return;
            case GAMESTATE.MENU:
                // Do not come here from Loading Lobby or Loading Round
                if ((_currentState == GAMESTATE.LOADING_LOBBY) || (_currentState == GAMESTATE.LOADING_ROUND)) return;
                break;
            case GAMESTATE.LOADING_LOBBY:
                // only come here from Menu
                if (_currentState != GAMESTATE.MENU) return;
                break;
            case GAMESTATE.PLAYING_LOBBY:
                // only come here from Loading Lobby
                if (_currentState != GAMESTATE.LOADING_LOBBY) return;
                break;
            case GAMESTATE.LOADING_ROUND:
                // only come here from Playing Lobby
                if (_currentState != GAMESTATE.LOADING_LOBBY) return;
                break;
            case GAMESTATE.PLAYING_ROUND:
                // only come here from Loading Round
                if(_currentState != GAMESTATE.LOADING_ROUND) return;
                break;
            case GAMESTATE.MIDPOINT_LEADERBOARD:
                // only come here from Playing Round
                if (_currentState != GAMESTATE.PLAYING_ROUND) return;
                break;
            case GAMESTATE.FINAL_LEADERBOARD:
                // only come here from Playing Round
                if (_currentState != GAMESTATE.PLAYING_ROUND) return;
                break;
            case GAMESTATE.DISCONNECTED:
                // do not come here from Menu or Start
                if ((_currentState == GAMESTATE.START) || (_currentState == GAMESTATE.MENU)) return;
                break;
            case GAMESTATE.NUM_OF_STATES:
            default:
                return;
        }

        // do the transition
        DoExitTransition(_currentState);
        DoEntryTransition(newState);
        _currentState = newState;
        OnGameStateChanged?.Invoke(this, _currentState);
    }

    public void LoadLobby()
    {
        DoStateTransition(GAMESTATE.LOADING_LOBBY);
    }    

    public void LobbyLoaded()
    {
        ObjectiveManager.Instance.ResetObjectiveManager();
        OnSceneLoaded?.Invoke(this, EventArgs.Empty);
        DoStateTransition(GAMESTATE.PLAYING_LOBBY);
    }
}
