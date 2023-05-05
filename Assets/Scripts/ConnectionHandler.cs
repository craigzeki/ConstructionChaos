using System.Collections;
using System.Collections.Generic;
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

    [SerializeField] private UnityTransport unityTransport, relayTransport;

    [SerializeField] private FollowCam followCam;

    [SerializeField] private GameObject _serverInputHandlerObject;

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
    public async void HostGame(int maxPlayers, bool local = false)
    {
        if (local)
        {
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

            //! The IP Address will be 127.0.0.1 if the device is not connected to a network
            // TODO: Test if this causes the game not to work and implemented a fix

            NetworkManager.Singleton.NetworkConfig.NetworkTransport = unityTransport;

            // Set the connection data to the local IP address
            unityTransport.ConnectionData.Address = ip;

            AsyncOperation loadScene = SceneManager.LoadSceneAsync("LobbyEnv", LoadSceneMode.Additive);

            loadScene.completed += (AsyncOperation op) =>
            {
                GameObject.Instantiate(_serverInputHandlerObject);

                // Start the server
                NetworkManager.Singleton.StartHost();

                //followCam.SetFollowTarget(NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.transform.GetChild(0));

                // Display the room code to the user
                string roomCode = IPtoCode(ip);
                MenuUIManager.Instance.SetRoomCode(roomCode);
            };

            return;
        }

        await UnityServicesLogin();

        CreateRelay(maxPlayers);
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

            // 
            NetworkManager.Singleton.NetworkConfig.NetworkTransport = unityTransport;

            // Set the connection data to the IP address
            unityTransport.ConnectionData.Address = ip;

            AsyncOperation loadScene = SceneManager.LoadSceneAsync("LobbyEnv", LoadSceneMode.Additive);

            loadScene.completed += (AsyncOperation op) =>
            {
                // Start the client
                NetworkManager.Singleton.StartClient();

                //followCam.SetFollowTarget(NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.transform.GetChild(0));
            };

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
    /// <param name="maxPlayers">The maximum number of players allowed in the game</param>
    private async void CreateRelay(int maxPlayers)
    {
        print("Creating relay");
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);

            string roomCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            MenuUIManager.Instance.SetRoomCode(roomCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

            NetworkManager.Singleton.NetworkConfig.NetworkTransport = relayTransport;

            relayTransport.SetRelayServerData(relayServerData);

            AsyncOperation loadScene = SceneManager.LoadSceneAsync("LobbyEnv", LoadSceneMode.Additive);

            loadScene.completed += (AsyncOperation op) =>
            {
                GameObject.Instantiate(_serverInputHandlerObject);

                NetworkManager.Singleton.StartHost();

                //followCam.SetFollowTarget(NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.transform.GetChild(0));
            };
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

            NetworkManager.Singleton.NetworkConfig.NetworkTransport = relayTransport;

            relayTransport.SetRelayServerData(relayServerData);

            AsyncOperation loadScene = SceneManager.LoadSceneAsync("LobbyEnv", LoadSceneMode.Additive);

            loadScene.completed += (AsyncOperation op) =>
            {
                NetworkManager.Singleton.StartClient();

                //followCam.SetFollowTarget(NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.transform.GetChild(0));
            };
        }
        catch (RelayServiceException e)
        {
            //! More error handling is needed here to catch the error when the room code is invalid
            print(e.Message);
        }
    }

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
