using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class ConnectionHandler : MonoBehaviour
{
    public static ConnectionHandler Instance;

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
    public async void HostGame(int maxPlayers)
    {
        await UnityServicesLogin();

        CreateRelay(maxPlayers);
    }

    /// <summary>
    /// Joins the game using the Unity Relay Service and Netcode for GameObjects
    /// </summary>
    /// <param name="roomCode">The room code to join</param>
    public async void JoinGame(string roomCode)
    {
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

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();
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

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            //! More error handling is needed here to catch the error when the room code is invalid
            print(e.Message);
        }
    }
}
