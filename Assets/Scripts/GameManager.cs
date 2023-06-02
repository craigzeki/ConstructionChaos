using SolidUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;
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
    
    [SerializeField] private float _minLoadScreenTime = 2f;
    [SerializeField] private LayerMask _groundLayers = new LayerMask();
    public LayerMask GroundLayers => _groundLayers;
    [SerializeField] private GameObject _lobbyPrefab;
    [SerializeField] private List<Round> _rounds = new List<Round>();
    [SerializeField] private List<Color> _playerColours = new List<Color>();
    public List<Color> PlayerColours => _playerColours;
    [SerializeField][ReadOnly] private List<int> _playerColourIndexes = new List<int>();
    [SerializeField] private List<String> _playerNames = new List<string>();
    [SerializeField][ReadOnly] private List<String> _remainingNames = new List<string>();

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

    [SerializeField][ReadOnly] private List<int> _roundOrder = new List<int>();
    private int _roundIndex = 0;
    private int _nextRound = 0;
    private GameObject _currentRound;
    public NetPlayer LocalPlayer;
    //private int _playerColourIndex = 0;
    private bool _roundTimerRunning = false;
    private bool _roundWon = false;
    private Coroutine _roundTimerCoroutine;
    private bool _clientConnected = false;
    private bool _leaderboardReady = false;
    private const string NETWORK_ERROR_TEXT = "Oh No!\nA network error occurred!";
    private Controls _controls;

    public static GameManager Instance
    {
        get
        {
            if(_instance == null) _instance = FindObjectOfType<GameManager>();
            return _instance;
        }
    }

    public GAMESTATE CurrentState { get => _currentState; }

    public bool TrueIsServer = false;

    private void Awake()
    {
        _controls = new Controls();
        _controls.Gameplay.Enable();
        _controls.Gameplay.Escape.performed += EscapeButtonPressed;
        _controls.Gameplay.Escape.canceled += EscapeButtonPressed;

        _currentState = GAMESTATE.START;
    }

    public override void OnNetworkSpawn()
    {
        TrueIsServer = IsServer;
        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        TrueIsServer = false;
        base.OnNetworkDespawn();
    }

    private void Update()
    {
        DoStateCyclicActions();
    }

    public void StartGameManager()
    {
        if(_currentState == GAMESTATE.START) DoStateTransition(GAMESTATE.MENU);
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
        if(TrueIsServer)
        {
            if (netPlayer == null) return false;

            NetPlayerData objectivePlayerData = new NetPlayerData(clientId, netPlayer);

            if (!PlayerData.TryAdd(clientId, objectivePlayerData)) return false;

            SetPlayerColour(objectivePlayerData);

            OnPlayerSpawned?.Invoke(this, clientId);

            // send the objective - will be null for the host (joins as the scene is loading - but will be updated when the scene loads in)
            netPlayer.SetObjectiveStringClientRpc(PlayerData[clientId].Objective?.ObjectiveString, PlayerData[clientId].ClientRpcParams);
            // send the player colour
            //netPlayer.SetPlayerColourClientRpc(PlayerData[clientId].ColourIndex, PlayerData[clientId].ClientRpcParams);
            netPlayer.PlayerColorIndex.Value = PlayerData[clientId].ColourIndex;
            netPlayer.LocalPlayerName.Value = GetPlayerName(clientId);

            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                MenuUIManager.Instance.SetPlayerNameText(PlayerData[clientId].PlayerName);
            }

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

        if(PlayerData.TryGetValue(clientId, out NetPlayerData playerData))
        {
            // Add the colour index back to the stack
            _playerColourIndexes.Add(playerData.ColourIndex);
            // If we have a unique name stored, restore this to the list
            if (!playerData.DefaultNameUsed) ReAddPlayerName(clientId);
        }

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

    private void SetPlayerColour(NetPlayerData playerData)
    {
        if (_playerColourIndexes.Count <= 0) return;
        if (playerData == null) return;
        playerData.ColourIndex = _playerColourIndexes[0];
        _playerColourIndexes.RemoveAt(0);

        // Old method
        //if (_playerColourIndex >= _playerColours.Count) return;
        //if (playerData == null) return;

        //playerData.ColourIndex = _playerColourIndex;
        //_playerColourIndex++;
    }

    public Color GetPlayerColour(int colourIndex)
    {
        return colourIndex < _playerColours.Count ? _playerColours[colourIndex] : Color.white;
    }

    /// <summary>
    /// Perform the state's cyclic actions<BR/>
    /// Called every Update()
    /// </summary>
    private void DoStateCyclicActions()
    {
        switch (_currentState)
        {
            case GAMESTATE.START:
                break;
            case GAMESTATE.MENU:
                break;
            case GAMESTATE.LOADING_LOBBY:

                if (_loadingTimerComplete && _clientConnected)
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
                if(!_roundTimerRunning || _roundWon)
                {
                    // Round timer has expired - load the leaderboard if configured
                    if (_rounds[_roundIndex].ShowLeaderBoardAfterRound && (_nextRound < _roundOrder.Count))
                    {
                        // If not the last round, and configured, show the Leaderboard
                        DoStateTransition(GAMESTATE.MIDPOINT_LEADERBOARD);
                    }
                    else
                    {
                        // Move to next round (also handles move to final leaderboard)
                        LoadRound();
                    }
                }
                break;
            case GAMESTATE.MIDPOINT_LEADERBOARD:
                break;
            case GAMESTATE.FINAL_LEADERBOARD:
                if (_leaderboardReady && !TrueIsServer)
                {
                    ConnectionHandler.Instance.Shutdown();
                    _leaderboardReady = false;
                }
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
                
                if (_currentState != GAMESTATE.START)
                {
                    SuperGameManager.Instance.ReloadEntireGame();
                    //try
                    //{
                    //    NetworkObject.Despawn(false);
                    //}
                    //catch (NullReferenceException)
                    //{
                    //    // Do nothing
                    //}
                }
                else
                {
                    //Debug.Log("MenuEntry: Menu Canvas = true");
                    MenuUIManager.Instance?.ToggleCanvas(MenuUIManager.Instance.MainMenuCanvas, true);
                    _clientConnected = false;

                    // Reset players
                    PlayerData.Clear();

                    // reload all colours
                    _playerColourIndexes.Clear();
                    for (int i = 0; i < _playerColours.Count; i++)
                    {
                        _playerColourIndexes.Add(i);
                    }

                    _remainingNames.Clear();
                    _remainingNames = new(_playerNames);
                }
                
                break;
            case GAMESTATE.LOADING_LOBBY:
                StartCoroutine(LoadRound(_lobbyPrefab, _minLoadScreenTime));
                break;
            case GAMESTATE.PLAYING_LOBBY:
                MenuUIManager.Instance.ToggleCanvas(MenuUIManager.Instance.LobbyCanvas, true);
                // Now that scene has loaded, move players to spawn points
                MovePlayersToSpawnPoints();
                // Now that scene has loaded, inform the player of their objective
                SendPlayerObjectiveStrings();
                break;
            case GAMESTATE.LOADING_ROUND:
                StartCoroutine(LoadRound(_rounds[_roundIndex].RoundPrefab, _minLoadScreenTime));
                break;
            case GAMESTATE.PLAYING_ROUND:
                _roundWon = false;
                // Now that scene has loaded, move players to spawn points
                MovePlayersToSpawnPoints();
                // Now that scene has loaded, inform the player of their objective
                SendPlayerObjectiveStrings();
                // Reset the round timer
                if(_roundTimerCoroutine != null) StopCoroutine(_roundTimerCoroutine);
                _roundTimerCoroutine = StartCoroutine(RoundTimer(_rounds[_roundIndex].RoundDurationSeconds));
                MenuUIManager.Instance.ToggleCanvas(MenuUIManager.Instance.GameCanvas, true);
                break;
            case GAMESTATE.MIDPOINT_LEADERBOARD:
                MenuUIManager.Instance.ToggleCanvas(LeaderboardUIManager.Instance.LeaderboardCanvas, true);
                break;
            case GAMESTATE.FINAL_LEADERBOARD:
                LeaderboardUIManager.Instance.FinalLeaderboard = true;
                MenuUIManager.Instance.ToggleCanvas(LeaderboardUIManager.Instance.LeaderboardCanvas, true);
                break;
            case GAMESTATE.DISCONNECTED:
                MenuUIManager.Instance.ToggleCanvas(MenuUIManager.Instance.DisconnectedCanvas, true);
                // Shutdowmn the network manager so it can be used again
                ConnectionHandler.Instance.Shutdown();
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
                if(TrueIsServer)
                {
                    // ! Do not change PlayerData here, as the host has already loaded in
                    // ! Instead change it in Menu Entry Transition
                    _roundOrder.Clear();
                    for (int i = 0; i < _rounds.Count; i++)
                    {
                        _roundOrder.Add(i);
                    }
                    _roundOrder.Shuffle();

                    _nextRound = 0;
                    _roundIndex = _roundOrder[_nextRound];
                    
                }
                break;
            case GAMESTATE.LOADING_LOBBY:
                Debug.Log("LoadingLobbyExit: Menu Canvas = false");
                MenuUIManager.Instance.ToggleCanvas(MenuUIManager.Instance.LoadingCanvas, false);
                break;
            case GAMESTATE.PLAYING_LOBBY:
                if (TrueIsServer)
                {
                    Destroy(_currentRound);
                }
                MenuUIManager.Instance.ToggleCanvas(MenuUIManager.Instance.LobbyCanvas, false);
                break;
            case GAMESTATE.LOADING_ROUND:
                MenuUIManager.Instance.ToggleCanvas(MenuUIManager.Instance.LoadingCanvas, false);
                break;
            case GAMESTATE.PLAYING_ROUND:
                MenuUIManager.Instance.ToggleCanvas(MenuUIManager.Instance.GameCanvas, false);
                ArrowManager.Instance.RemoveAllArrows();
                ArrowManager.Instance.ClearAllLists();
                if (TrueIsServer)
                {
                    Destroy(_currentRound);
                }
                break;
            case GAMESTATE.MIDPOINT_LEADERBOARD:
                MenuUIManager.Instance.ToggleCanvas(LeaderboardUIManager.Instance.LeaderboardCanvas, false);
                break;
            case GAMESTATE.FINAL_LEADERBOARD:
                MenuUIManager.Instance.ToggleCanvas(LeaderboardUIManager.Instance.LeaderboardCanvas, false);
                LeaderboardUIManager.Instance.FinalLeaderboard = false;
                if (TrueIsServer)
                {
                    ConnectionHandler.Instance.Shutdown();
                }
                break;
            case GAMESTATE.DISCONNECTED:
                MenuUIManager.Instance.ToggleCanvas(MenuUIManager.Instance.DisconnectedCanvas, false);
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
                // Do not come here from Menu!
                if (_currentState == GAMESTATE.MENU) return false;
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
                // only come here from Playing Lobby or Mid Point Leaderboard
                if ((_currentState != GAMESTATE.PLAYING_LOBBY) && (_currentState != GAMESTATE.PLAYING_ROUND) && (_currentState != GAMESTATE.MIDPOINT_LEADERBOARD)) return false;
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
                // do not come here from Start
                if (_currentState == GAMESTATE.START) return false;
                break;
            case GAMESTATE.NUM_OF_STATES:
            default:
                return false;
        }

        // do the transition
        DoExitTransition(_currentState);
        DoEntryTransition(newState);
        _currentState = newState;

        if(TrueIsServer)
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
        if (TrueIsServer)
        {
            // Let everyone know the scene was loaded
            OnSceneLoaded?.Invoke(this, EventArgs.Empty);
        }
        // State transition is controlled in DoStateCyclicActions to allow for min time to be accounted for also
    }

    IEnumerator LoadRound(GameObject roundToLoad, float minWaitDuration)
    {
        _loadingTimerComplete = false;
        MenuUIManager.Instance.ToggleCanvas(MenuUIManager.Instance.LoadingCanvas, true);
        if(TrueIsServer)
        {
            yield return new WaitForEndOfFrame();
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

    /// <summary>
    /// Only to be called by the Editor script for GameManager
    /// </summary>
    public void LoadRoundButton()
    {
        if (!TrueIsServer) return;

        if((_currentState != GAMESTATE.PLAYING_ROUND) && (_currentState != GAMESTATE.PLAYING_LOBBY)) return;
        if ((_currentState == GAMESTATE.PLAYING_ROUND) && _rounds[_roundIndex].ShowLeaderBoardAfterRound && (_nextRound < _roundOrder.Count))
        {
            // If not the last round, and configured, show the Leaderboard
            DoStateTransition(GAMESTATE.MIDPOINT_LEADERBOARD);
        }
        else
        {
            // Move to next round (also handles move to final leaderboard)
            LoadRound();
        }
        
    }

    /// <summary>
    /// Function allows UI to move on from Leaderboard
    /// </summary>
    public void LoadNextRound()
    {
        if (!TrueIsServer) return;
        if ((_currentState == GAMESTATE.MIDPOINT_LEADERBOARD) || (_currentState == GAMESTATE.PLAYING_LOBBY))
        {
            LoadRound();
        }
        else if(_currentState == GAMESTATE.FINAL_LEADERBOARD)
        {
            DoStateTransition(GAMESTATE.MENU);
        }
    }

    public void LoadMenu()
    {
        if (CurrentState == GAMESTATE.MENU)
        {
            QuitApp();
        }
        else
        {
            ConnectionHandler.Instance.Shutdown();
            SuperGameManager.Instance.ReloadEntireGame();
        }
    }

    public void ClientDisconnected(string reason)
    {
        MenuUIManager.Instance.SetNetworkErrorText(NETWORK_ERROR_TEXT);

        if (reason != string.Empty)
        {
            MenuUIManager.Instance.AppendNetworkErrorText("\n" + reason);
        }

        
        DoStateTransition(GAMESTATE.DISCONNECTED);
    }

    public void ClientConnected()
    {
        _clientConnected = true;
    }

    /// <summary>
    /// Function allows GameManager to move on the round
    /// </summary>
    private void LoadRound()
    {
        if ((_currentState != GAMESTATE.PLAYING_ROUND) && (_currentState != GAMESTATE.PLAYING_LOBBY) && (_currentState != GAMESTATE.MIDPOINT_LEADERBOARD)) return;

        if (!TrueIsServer) return;

        if (_nextRound >= _roundOrder.Count)
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
        if(TrueIsServer)
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
        if (TrueIsServer)
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
        if(!TrueIsServer)
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

    IEnumerator RoundTimer(uint durationInSeconds)
    {
        GameUIManager.Instance.UpdateTimerUI(durationInSeconds);
        _roundTimerRunning = true;
        while(durationInSeconds > 0)
        {
            yield return new WaitForSeconds(1);
            durationInSeconds--;
            GameUIManager.Instance.UpdateTimerUI(durationInSeconds);
        }
        _roundTimerRunning = false;
        _roundTimerCoroutine = null;
    }

    public void LeaderboardReady()
    {
        _leaderboardReady = true;
    }

    private string GetPlayerName(ulong clientId)
    {
        if(_remainingNames == null) return PlayerData[clientId].PlayerName;
        if(_remainingNames.Count <= 0) return PlayerData[clientId].PlayerName;

        int index = UnityEngine.Random.Range((int)0, (int)_remainingNames.Count);
        string playerName = _remainingNames[index];
        _remainingNames.RemoveAt(index);
        PlayerData[clientId].PlayerName = playerName;
        PlayerData[clientId].DefaultNameUsed = false;
        return playerName;
    }

    private void ReAddPlayerName(ulong clientId)
    {
        if (_remainingNames == null) return;
        if (!PlayerData[clientId].DefaultNameUsed) _remainingNames.Add(PlayerData[clientId].PlayerName);
    }

    public void RoundWon()
    {
        if(_currentState == GAMESTATE.PLAYING_ROUND) _roundWon = true;
    }

    public void QuitApp()
    {
        ConnectionHandler.Instance.Shutdown();
        Application.Quit();
    }

    private void EscapeButtonPressed(InputAction.CallbackContext value)
    {
        if(value.ReadValueAsButton())
        {
            LoadMenu();
        }
        
    }
}
