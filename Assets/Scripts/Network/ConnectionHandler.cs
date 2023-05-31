using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionHandler : MonoBehaviour
{
    public static ConnectionHandler Instance;

    [SerializeField] private UnityTransport _unityTransport, _relayTransport;

    [SerializeField] private int _maxNoOfPlayers = 6;

    [SerializeField] private GameObject _gameManagerObject;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Hosts the game using the Unity Relay Service and Netcode for GameObjects
    /// </summary>
    /// <param name="maxPlayers">The maximum number of players allowed in the game</param>
    public async Task<bool> HostGame(bool local = false)
    {
        //Shutdown();

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        }
        if (local)
        {
            // Try to get the local IP address
            string ip = "";
            try
            {
                ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(address => address.AddressFamily == AddressFamily.InterNetwork).ToString();
            }
            catch
            {
                return false;
            }

            // Change the network transport to the Unity Transport
            NetworkManager.Singleton.NetworkConfig.NetworkTransport = _unityTransport;

            // Set the connection data to the local IP address
            _unityTransport.ConnectionData.Address = ip;

            // Display the room code to the user
            string roomCode = IPtoCode(ip);
            MenuUIManager.Instance.SetRoomCode(roomCode);

            bool localConnectionSuccessful = StartNetwork(true);

            if (localConnectionSuccessful)
            {
                try
                {
                    GameManager.Instance?.NetworkObject.Spawn();
                }
                catch (SpawnStateException)
                {
                    // Do nothing if the game manager is already spawned
                }
                catch (NotServerException)
                {

                }
            }
            else
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
                NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
            }

            return localConnectionSuccessful;
        }

        bool connectionSuccessful = await UnityServicesLogin();

        if (connectionSuccessful)
        {
            connectionSuccessful = await CreateRelay();
        }

        if (!connectionSuccessful)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        }

        return connectionSuccessful;
    }

    /// <summary>
    /// Joins the game using the Unity Relay Service and Netcode for GameObjects
    /// </summary>
    /// <param name="roomCode">The room code to join</param>
    public async Task<bool> JoinGame(string roomCode, bool local = false)
    {
        //Shutdown();

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        }
        if (local)
        {
            MenuUIManager.Instance.SetRoomCode(roomCode);

            // Convert the room code to an IP address
            string ip = CodeToIP(roomCode);

            if (ip == null)
            {
                return false;
            }

            // Change the network transport to the Unity Transport
            NetworkManager.Singleton.NetworkConfig.NetworkTransport = _unityTransport;

            // Set the connection data to the IP address
            _unityTransport.ConnectionData.Address = ip;

            bool localLoginSuccessful = StartNetwork(false);

            if (localLoginSuccessful)
            {
                try
                {
                    GameManager.Instance?.NetworkObject.Spawn();
                }
                catch (SpawnStateException)
                {
                    // Do nothing if the game manager is already spawned
                }
                catch (NotServerException)
                {

                }
            }
            else
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
                NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
            }

            return localLoginSuccessful;
        }

        bool loginSuccessful = await UnityServicesLogin();

        if (loginSuccessful)
        {
            loginSuccessful = await JoinRelay(roomCode);
        }

        if (!loginSuccessful)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        }

        return loginSuccessful;
    }

    /// <summary>
    /// Logs into Unity Services Anonymously
    /// </summary>
    private async Task<bool> UnityServicesLogin()
    {
        //bool returnVal = false;

        try
        {
            await UnityServices.InitializeAsync();
        }
        catch
        {
            return false;
        }

        AuthenticationService.Instance.SignedIn += () =>
        {
            // Player has successfully signed in anonymously
            print("Signed in anonymously");
            //returnVal = true;
        };

        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch
        {
            return false;
        }

        return AuthenticationService.Instance.IsSignedIn;
    }

    /// <summary>
    /// Creates a relay server using the Unity Relay Service
    /// </summary>
    private async Task<bool> CreateRelay()
    {
        print("Creating relay");
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(_maxNoOfPlayers - 1);

            string roomCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            MenuUIManager.Instance.SetRoomCode(roomCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

            NetworkManager.Singleton.NetworkConfig.NetworkTransport = _relayTransport;

            _relayTransport.SetRelayServerData(relayServerData);

            return StartNetwork(true);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Joins a relay server using the Unity Relay Service
    /// </summary>
    /// <param name="roomCode">The room code to join</param>
    private async Task<bool> JoinRelay(string roomCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(roomCode);

            MenuUIManager.Instance.SetRoomCode(roomCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

            NetworkManager.Singleton.NetworkConfig.NetworkTransport = _relayTransport;

            _relayTransport.SetRelayServerData(relayServerData);

            return StartNetwork(false);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Starts the network using Netcode for GameObjects
    /// </summary>
    /// <param name="isHost">Whether the device is the host or not</param>
    private bool StartNetwork(bool isHost)
    {
        if (isHost)
        {
            return NetworkManager.Singleton.StartHost();
        }
        else
        {
            return NetworkManager.Singleton.StartClient();
        }
    }

    /// <summary>
    /// Converts an IP address to a room code by converting each part to hex and concatenating them
    /// </summary>
    /// <returns>A string containing the room code</returns>
    string IPtoCode(string ip)
    {
        string[] ipSplit = ip.Split('.');
        string code = "";
        foreach (string s in ipSplit)
        {
            code += int.Parse(s).ToString("X2");
        }
        return code;
    }

    /// <summary>
    /// Converts a room code to an IP address by converting each hex value to an integer and concatenating them
    /// </summary>
    /// <returns>A string containing the IP address</returns>
    string CodeToIP(string code)
    {
        string ip = "";
        for (int i = 0; i < code.Length; i += 2)
        {
            try
            {
                ip += int.Parse(code.Substring(i, 2), System.Globalization.NumberStyles.HexNumber).ToString();
            }
            catch
            {
                return null;
            }

            if (i != code.Length - 2)
            {
                ip += ".";
            }
        }

        return ip;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.CreatePlayerObject = true;
        //response.PlayerPrefabHash = null;
        //response.Position = Vector3.zero;
        //response.Rotation = Quaternion.identity;

        // Always approve the host
        if (request.ClientNetworkId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("Automatically approved the Host for connection");
            response.Approved = true;
            response.Pending = false;
            return;
        }

        if (NetworkManager.Singleton.ConnectedClientsIds.Count() >= _maxNoOfPlayers)
        {
            
            response.Approved = false;
            response.Reason = "Maximum number of players reached";
        }
        else if(GameManager.Instance.CurrentState != GameManager.GAMESTATE.PLAYING_LOBBY)
        {
            response.Approved = false;
            response.Reason = "Game already in progress";
        }
        else
        {
            response.Approved = true;
        }
        response.Pending = false;
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
        Debug.Log("OnClientDisconnectCallback - assumed clientId: = " + clientId.ToString());
        Debug.Log("LocalClientId: " + NetworkManager.Singleton.LocalClientId.ToString());

        if (!NetworkManager.Singleton.IsServer && NetworkManager.Singleton.DisconnectReason != string.Empty)
        {
            Debug.Log($"Approval Declined Reason: {NetworkManager.Singleton.DisconnectReason}");
            
        }
        if ((NetworkManager.Singleton.IsServer) && clientId == NetworkManager.Singleton.LocalClientId)
        {
            GameManager.Instance?.ClientDisconnected(NetworkManager.Singleton.DisconnectReason);
        }
        else if(!NetworkManager.Singleton.IsServer)
        {
            GameManager.Instance?.ClientDisconnected(NetworkManager.Singleton.DisconnectReason);
        }
    }

    private void OnClientConnectedCallback(ulong obj)
    {
        Debug.Log("Client Connected Called");
        GameManager.Instance?.ClientConnected();
    }

    public void Shutdown()
    {
        // If connected to a relay server, disconnect from it
        // If logged in to Unity Services, log out
        try
        {
            AuthenticationService.Instance.SignOut();
        }
        catch
        {
            // Do nothing if not logged in
        }

        // Shut down the network
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
            NetworkManager.Singleton.Shutdown(true);
        }
    }

    public void SpawnManagerNetworkObjects()
    {
        try
        {
            GameManager.Instance.NetworkObject.Spawn();
            LeaderboardUIManager.Instance.NetworkObject.Spawn();
        }
        catch (NotListeningException)
        {
            // Do nothing if NetworkManager is not listening
        }
    }

    public void DespawnManagerNetworkObjects()
    {
        try
        {
            GameManager.Instance.NetworkObject.Despawn(false);
            LeaderboardUIManager.Instance.NetworkObject.Despawn(false);
        }
        catch (System.NullReferenceException)
        {
            // Do nothing if already despawned
        }
    }
}
