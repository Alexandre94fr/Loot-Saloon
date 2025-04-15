#region
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
#endregion

public class S_NetworkGameManager : MonoBehaviour
{
    private NetworkManager _networkManagerInstance;

    private void OnEnable()
    {
        // Useless for now
    }

    private void OnDisable()
    {
        _networkManagerInstance.OnClientConnectedCallback -= OnClientConnected;
        _networkManagerInstance.OnClientDisconnectCallback -= OnClientDisconnect;
    }

    private void OnClientConnected(ulong p_obj)
    {
        Debug.Log("Player Connected = " + p_obj);
    }

    private void OnClientDisconnect(ulong p_client)
    {
        if (_networkManagerInstance.LocalClientId == p_client)
        {
            Debug.Log("Player Disconnected = " + p_client);
            _networkManagerInstance.Shutdown();
            SceneManager.LoadSceneAsync("MainMenu");
        }
    }

    private void Start()
    {
        _networkManagerInstance = NetworkManager.Singleton;

        _networkManagerInstance.OnClientConnectedCallback += OnClientConnected;
        _networkManagerInstance.OnClientDisconnectCallback += OnClientDisconnect;

        _networkManagerInstance.LogLevel = LogLevel.Developer;
        _networkManagerInstance.NetworkConfig.EnableNetworkLogs = true;


        _networkManagerInstance.NetworkConfig.ConnectionApproval = true;
        UnityTransport transport = GetComponent<UnityTransport>();

        if (S_RelayManager.instance.IsHost)
        {
            // Host setup
            _networkManagerInstance.ConnectionApprovalCallback = ConnectionApproval;
            (byte[] allocationId, byte[] key, byte[] connectionData, string ip, int port) = S_RelayManager.instance.GetHostConnectionInfo();
            transport.SetHostRelayData(ip, (ushort)port, allocationId, key, connectionData, true);
            _networkManagerInstance.StartHost();
        }

        else
        {
            // Client setup
            (byte[] allocationId, byte[] key, byte[] connectionData, byte[] hostConnectionData, string ip, int port) = S_RelayManager.instance.GetClientConnectionInfo();
            transport.SetClientRelayData(ip, (ushort)port, allocationId, key, connectionData, hostConnectionData, true);
            _networkManagerInstance.StartClient();
        }
    }

    private void Update()
    {
        if (_networkManagerInstance.ShutdownInProgress)
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