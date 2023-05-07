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
    public async void HostGame(bool local = false)
    {
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
                print("Error getting IP address");
                // TODO: Display an error message to the user
                return;
            }

            // Change the network transport to the Unity Transport
            NetworkManager.Singleton.NetworkConfig.NetworkTransport = _unityTransport;

            // Set the connection data to the local IP address
            _unityTransport.ConnectionData.Address = ip;

            // Display the room code to the user
            string roomCode = IPtoCode(ip);
            MenuUIManager.Instance.SetRoomCode(roomCode);

            StartNetwork(true);

            return;
        }

        await UnityServicesLogin();

        CreateRelay();
    }

    /// <summary>
    /// Joins the game using the Unity Relay Service and Netcode for GameObjects
    /// </summary>
    /// <param name="roomCode">The room code to join</param>
    public async void JoinGame(string roomCode, bool local = false)
    {
        if (local)
        {
            // Convert the room code to an IP address
            string ip = CodeToIP(roomCode);

            // Change the network transport to the Unity Transport
            NetworkManager.Singleton.NetworkConfig.NetworkTransport = _unityTransport;

            // Set the connection data to the IP address
            _unityTransport.ConnectionData.Address = ip;

            StartNetwork(false);

            return;
        }

        await UnityServicesLogin();

        JoinRelay(roomCode);
    }

    /// <summary>
    /// Logs into Unity Services Anonymously
    /// </summary>
    private async Task UnityServicesLogin()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            // Player has successfully signed in anonymously
            print("Signed in anonymously");
            return;
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    /// <summary>
    /// Creates a relay server using the Unity Relay Service
    /// </summary>
    private async void CreateRelay()
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

            StartNetwork(true);
        }
        catch (RelayServiceException e)
        {
            print(e.Message);
        }
    }

    /// <summary>
    /// Joins a relay server using the Unity Relay Service
    /// </summary>
    /// <param name="roomCode">The room code to join</param>
    private async void JoinRelay(string roomCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(roomCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

            NetworkManager.Singleton.NetworkConfig.NetworkTransport = _relayTransport;

            _relayTransport.SetRelayServerData(relayServerData);

            StartNetwork(false);
        }
        catch (RelayServiceException e)
        {
            //! More error handling is needed here to catch the error when the room code is invalid
            print(e.Message);
        }
    }

    /// <summary>
    /// Starts the network using Netcode for GameObjects
    /// </summary>
    /// <param name="isHost">Whether the device is the host or not</param>
    private void StartNetwork(bool isHost)
    {
        AsyncOperation loadLobbyScene = SceneManager.LoadSceneAsync(SceneNames.LobbyEnvironment);

        loadLobbyScene.completed += (AsyncOperation op) =>
        {
            if (isHost)
                NetworkManager.Singleton.StartHost();
            else
                NetworkManager.Singleton.StartClient();
        };
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
            ip += int.Parse(code.Substring(i, 2), System.Globalization.NumberStyles.HexNumber).ToString();
            if (i != code.Length - 2)
            {
                ip += ".";
            }
        }

        return ip;
    }
}
