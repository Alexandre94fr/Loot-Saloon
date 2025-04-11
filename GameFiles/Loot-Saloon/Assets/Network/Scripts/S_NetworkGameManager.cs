#region
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
#endregion

public class S_NetworkGameManager : MonoBehaviour
{
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

    private void ConnectionApproval(NetworkManager.ConnectionApprovalRequest p_request, NetworkManager.ConnectionApprovalResponse p_response)
    {
        p_response.Approved = true;
        p_response.CreatePlayerObject = true;
        p_response.Pending = false;
    }
}