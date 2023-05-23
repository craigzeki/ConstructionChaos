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
    public async Task<bool> HostGame(bool local = false)
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
                return false;
            }

            // Change the network transport to the Unity Transport
            NetworkManager.Singleton.NetworkConfig.NetworkTransport = _unityTransport;

            // Set the connection data to the local IP address
            _unityTransport.ConnectionData.Address = ip;

            // Display the room code to the user
            string roomCode = IPtoCode(ip);
            MenuUIManager.Instance.SetRoomCode(roomCode);

            return StartNetwork(true);
        }

        await UnityServicesLogin();

        return await CreateRelay();
    }

    /// <summary>
    /// Joins the game using the Unity Relay Service and Netcode for GameObjects
    /// </summary>
    /// <param name="roomCode">The room code to join</param>
    public async Task<bool> JoinGame(string roomCode, bool local = false)
    {
        if (local)
        {
            // Convert the room code to an IP address
            string ip = CodeToIP(roomCode);

            // Change the network transport to the Unity Transport
            NetworkManager.Singleton.NetworkConfig.NetworkTransport = _unityTransport;

            // Set the connection data to the IP address
            _unityTransport.ConnectionData.Address = ip;

            return StartNetwork(false);
        }

        bool loginSuccessful = await UnityServicesLogin();
        if (loginSuccessful)
        {
            return await JoinRelay(roomCode);
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Logs into Unity Services Anonymously
    /// </summary>
    private async Task<bool> UnityServicesLogin()
    {
        bool returnVal = false;

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
            returnVal = true;
            return;
        };

        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch
        {
            return false;
        }

        return returnVal;
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
        catch (RelayServiceException e)
        {
            print(e.Message);
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

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

            NetworkManager.Singleton.NetworkConfig.NetworkTransport = _relayTransport;

            _relayTransport.SetRelayServerData(relayServerData);

            return StartNetwork(false);
        }
        catch (RelayServiceException e)
        {
            //! More error handling is needed here to catch the error when the room code is invalid
            print(e.Message);
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
            ip += int.Parse(code.Substring(i, 2), System.Globalization.NumberStyles.HexNumber).ToString();
            if (i != code.Length - 2)
            {
                ip += ".";
            }
        }

        return ip;
    }
}
