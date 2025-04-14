#region
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
#endregion

public class S_NetworkGameManager : MonoBehaviour
{
    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;

        NetworkManager.Singleton.LogLevel = LogLevel.Developer;
        NetworkManager.Singleton.NetworkConfig.EnableNetworkLogs = true;
    }

    private void OnDisable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
    }

    private void OnClientConnected(ulong p_obj)
    {
        Debug.Log("Player Connected = " + p_obj);
    }

    private void OnClientDisconnect(ulong p_client)
    {
        if (NetworkManager.Singleton.LocalClientId == p_client)
        {
            Debug.Log("Player Disconnected = " + p_client);
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadSceneAsync("MainMenu");
        }
    }

    private void Start()
    {
        NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
        UnityTransport transport = GetComponent<UnityTransport>();

        if (S_RelayManager.instance.IsHost)
        {
            // Host setup
            NetworkManager.Singleton.ConnectionApprovalCallback = ConnectionApproval;
            (byte[] allocationId, byte[] key, byte[] connectionData, string ip, int port) = S_RelayManager.instance.GetHostConnectionInfo();
            transport.SetHostRelayData(ip, (ushort)port, allocationId, key, connectionData, true);
            NetworkManager.Singleton.StartHost();
        }

        else
        {
            // Client setup
            (byte[] allocationId, byte[] key, byte[] connectionData, byte[] hostConnectionData, string ip, int port) = S_RelayManager.instance.GetClientConnectionInfo();
            transport.SetClientRelayData(ip, (ushort)port, allocationId, key, connectionData, hostConnectionData, true);
            NetworkManager.Singleton.StartClient();
        }
    }

    private void Update()
    {
        if (NetworkManager.Singleton.ShutdownInProgress)
        {
            S_GameLobbyManager.instance.LeaveLobby();
        }
    }

    private void ConnectionApproval(NetworkManager.ConnectionApprovalRequest p_request, NetworkManager.ConnectionApprovalResponse p_response)
    {
        p_response.Approved = true;
        p_response.CreatePlayerObject = true;
        p_response.Pending = false;
    }
}