using SolidUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using ZekstersLab.Helpers;

/// <summary>
/// The Game Manager
/// </summary>
public class GameManager : NetworkBehaviour
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

    [SerializeField] private GameObject _loadingCanvas;
    [SerializeField] private float _minLoadScreenTime = 2f;

    [SerializeField] private GameObject _lobbyPrefab;
    [SerializeField] private List<GameObject> _roundPrefabs = new List<GameObject>();


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
    private bool _loadingTimerComplete = false;

    private List<int> _roundOrder = new List<int>();
    private int _roundIndex = 0;
    private int _nextRound = 0;
    private GameObject _currentRound;
    public NetPlayer LocalPlayer;

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
        _loadingCanvas.SetActive(false);

        DoStateTransition(GAMESTATE.MENU);
    }

    private void Update()
    {
        DoStateCyclicActions();
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
        if(IsServer)
        {
            if (netPlayer == null) return false;

            NetPlayerData objectivePlayerData = new NetPlayerData(clientId, netPlayer);

            if (!PlayerData.TryAdd(clientId, objectivePlayerData)) return false;

            OnPlayerSpawned?.Invoke(this, clientId);

            // send the objective - will be null for the host (joins as the scene is loading - but will be updated when the scene loads in)
            netPlayer.SetObjectiveStringClientRpc(PlayerData[clientId].Objective?.ObjectiveString, PlayerData[clientId].ClientRpcParams);

            return true;
        }

        return false;
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

    private void DoStateCyclicActions()
    {
        switch (_currentState)
        {
            case GAMESTATE.START:
                break;
            case GAMESTATE.MENU:
                break;
            case GAMESTATE.LOADING_LOBBY:

                if (_loadingTimerComplete)
                {
                    _loadingTimerComplete = false;
                    RoundLoaded();
                    // Do the state transition
                    DoStateTransition(GAMESTATE.PLAYING_LOBBY);
                }
                break;
            case GAMESTATE.PLAYING_LOBBY:
                
                break;
            case GAMESTATE.LOADING_ROUND:
                if (_loadingTimerComplete)
                {
                    _loadingTimerComplete = false;
                    RoundLoaded();
                    // Do the state transition
                    DoStateTransition(GAMESTATE.PLAYING_ROUND);
                }
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

    private void DoEntryTransition(GAMESTATE state)
    {
        switch (state)
        {
            case GAMESTATE.START:
                break;
            case GAMESTATE.MENU:
                break;
            case GAMESTATE.LOADING_LOBBY:
                StartCoroutine(LoadRound(_lobbyPrefab, _minLoadScreenTime));
                break;
            case GAMESTATE.PLAYING_LOBBY:
                // Now that scene has loaded, move players to spawn points
                MovePlayersToSpawnPoints();
                // Now that scene has loaded, inform the player of their objective
                SendPlayerObjectiveStrings();
                break;
            case GAMESTATE.LOADING_ROUND:
                StartCoroutine(LoadRound(_roundPrefabs[_roundIndex], _minLoadScreenTime));
                break;
            case GAMESTATE.PLAYING_ROUND:
                // Now that scene has loaded, move players to spawn points
                MovePlayersToSpawnPoints();
                // Now that scene has loaded, inform the player of their objective
                SendPlayerObjectiveStrings();
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
                if(IsServer)
                {
                    _roundOrder.Clear();
                    for (int i = 0; i < _roundPrefabs.Count; i++)
                    {
                        _roundOrder.Add(i);
                    }
                    _roundOrder.Shuffle();

                    _nextRound = 0;
                    _roundIndex = _roundOrder[_nextRound];
                }
                break;
            case GAMESTATE.LOADING_LOBBY:
                _loadingCanvas.SetActive(false);
                break;
            case GAMESTATE.PLAYING_LOBBY:
                if (IsServer)
                {
                    Destroy(_currentRound);
                }
                break;
            case GAMESTATE.LOADING_ROUND:
                _loadingCanvas.SetActive(false);
                break;
            case GAMESTATE.PLAYING_ROUND:
                if (IsServer)
                {
                    Destroy(_currentRound);
                }
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
    private bool DoStateTransition(GAMESTATE newState)
    {
        // check if state is different
        if (newState == _currentState) return true;

        // check each transition's guard clauses
        switch (newState)
        {
            case GAMESTATE.START:
                // Not valid to come here (initial state only)
                return false;
            case GAMESTATE.MENU:
                // Do not come here from Loading Lobby or Loading Round
                if ((_currentState == GAMESTATE.LOADING_LOBBY) || (_currentState == GAMESTATE.LOADING_ROUND)) return false;
                break;
            case GAMESTATE.LOADING_LOBBY:
                // only come here from Menu
                if (_currentState != GAMESTATE.MENU) return false;
                break;
            case GAMESTATE.PLAYING_LOBBY:
                // only come here from Loading Lobby
                if (_currentState != GAMESTATE.LOADING_LOBBY) return false;
                break;
            case GAMESTATE.LOADING_ROUND:
                // only come here from Playing Lobby
                if ((_currentState != GAMESTATE.PLAYING_LOBBY) && (_currentState != GAMESTATE.PLAYING_ROUND)) return false;
                break;
            case GAMESTATE.PLAYING_ROUND:
                // only come here from Loading Round
                if(_currentState != GAMESTATE.LOADING_ROUND) return false;
                break;
            case GAMESTATE.MIDPOINT_LEADERBOARD:
                // only come here from Playing Round
                if (_currentState != GAMESTATE.PLAYING_ROUND) return false;
                break;
            case GAMESTATE.FINAL_LEADERBOARD:
                // only come here from Playing Round
                if (_currentState != GAMESTATE.PLAYING_ROUND) return false;
                break;
            case GAMESTATE.DISCONNECTED:
                // do not come here from Menu or Start
                if ((_currentState == GAMESTATE.START) || (_currentState == GAMESTATE.MENU)) return false;
                break;
            case GAMESTATE.NUM_OF_STATES:
            default:
                return false;
        }

        // do the transition
        DoExitTransition(_currentState);
        DoEntryTransition(newState);
        _currentState = newState;

        if(IsServer)
        {
            OnGameStateChanged?.Invoke(this, _currentState);
            // inform all clients
            Debug.Log("Sending state change to Clients");
            SetStateClientRpc(_currentState);

        }
        return true;
    }

    public void LoadLobby()
    {
        DoStateTransition(GAMESTATE.LOADING_LOBBY);
    }    

    private void RoundLoaded()
    {
        if (IsServer)
        {
            // Let everyone know the scene was loaded
            OnSceneLoaded?.Invoke(this, EventArgs.Empty);
        }
        // State transition is controlled in DoStateCyclicActions to allow for min time to be accounted for also
    }

    IEnumerator LoadRound(GameObject roundToLoad, float minWaitDuration)
    {
        _loadingTimerComplete = false;
        _loadingCanvas.SetActive(true);
        if(IsServer)
        {
            SpawnManager.Instance.ResetSpawnManager();
            if (roundToLoad != null)
            {
                _currentRound = Instantiate(roundToLoad);
                _currentRound.GetComponent<NetworkObject>().Spawn();
            }
            
            SpawnManager.Instance.ReloadSpawnPoints();
        }
        yield return new WaitForSeconds(minWaitDuration);
        _loadingTimerComplete = true;
    }

    public void LoadRoundButton()
    {
        if (!IsServer) return;

        if((_currentState != GAMESTATE.PLAYING_ROUND) && (_currentState != GAMESTATE.PLAYING_LOBBY)) return;
        if(_nextRound >= _roundOrder.Count)
        {
            // No more rounds to play, load high score page
            DoStateTransition(GAMESTATE.FINAL_LEADERBOARD);
        }
        else
        {
            _roundIndex = _roundOrder[_nextRound];
            _nextRound++;
            DoStateTransition(GAMESTATE.LOADING_ROUND);
        }
    }

    private void MovePlayersToSpawnPoints()
    {
        if(IsServer)
        {
            foreach (NetPlayerData netPlayerData in PlayerData.Values)
            {
                foreach (Ragdoll ragdoll in netPlayerData.NetPlayer.gameObject.GetComponentsInChildren<Ragdoll>())
                {
                    ragdoll.ResetLocalPosition();
                }
                SpawnManager.Instance.SpawnPlayer(netPlayerData.NetPlayer.gameObject);
                
            }
        }

        if(LocalPlayer != null)
        {
            Camera.main.transform.position = FollowCam.Instance.gameObject.transform.position = LocalPlayer.gameObject.transform.position;
        }
    }


    private void SendPlayerObjectiveStrings()
    {
        if (IsServer)
        {
            foreach (NetPlayerData netPlayerData in PlayerData.Values)
            {
                //setup clientRpc params to only send to specific client
                netPlayerData.NetPlayer?.SetObjectiveStringClientRpc(netPlayerData.Objective?.ObjectiveString, netPlayerData.ClientRpcParams);
            }
        }
    }

    [ClientRpc]
    private void SetStateClientRpc(GameManager.GAMESTATE state, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("Received request to change state to: " + state.ToString());
        if(!IsServer)
        {
            if (!DoStateTransition(state))
            {
                // Synchronization error between Client and Server GameManager - disconnect client
                DoStateTransition(GAMESTATE.DISCONNECTED);
                // TODO Error screen in the dicconnected state
                Debug.Log("ERROR: Game Managers are not synchronized");
            }
        }
        else
        {
            Debug.Log("Ignored state change as we are the server");
        }
        
    }
}
