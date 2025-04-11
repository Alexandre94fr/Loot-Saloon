#region
using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
#endregion

public class S_RelayManager : MonoBehaviour
{
    public static S_RelayManager instance;
    private string _joinCode;
    private string _ip;
    private int _port;
    private byte[] _key;
    private byte[] _connectionData;
    private byte[] _hostConnectionData;
    private byte[] _allocationIdBytes;
    private Guid _allocationId;
    private bool _isHost = false;

    public bool IsHost
    {
        get { return _isHost; }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async Task<string> CreateRelay(int p_maxConnection)
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(p_maxConnection);
        _joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        RelayServerEndpoint dtlsEndpoint = allocation.ServerEndpoints.First(p_connection => p_connection.ConnectionType == "dtls");
        _ip = dtlsEndpoint.Host;
        _port = dtlsEndpoint.Port;

        _connectionData = allocation.ConnectionData;
        _allocationId = allocation.AllocationId;
        _allocationIdBytes = allocation.AllocationIdBytes;
        _key = allocation.Key;
        _isHost = true;
        Debug.Log($"<color=green>Relay created with join code: {_joinCode}</color>");
        return _joinCode;
    }

    public async Task<bool> JoinRelay(string p_joinCode)
    {
        _joinCode = p_joinCode;
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(p_joinCode);
        _ip = joinAllocation.ServerEndpoints.First(p_connection => p_connection.ConnectionType == "dtls").Host;
        _port = joinAllocation.ServerEndpoints.First(p_connection => p_connection.ConnectionType == "dtls").Port;

        _connectionData = joinAllocation.ConnectionData;
        _allocationId = joinAllocation.AllocationId;
        _allocationIdBytes = joinAllocation.AllocationIdBytes;
        _key = joinAllocation.Key;

        // Assign HostConnectionData
        _hostConnectionData = joinAllocation.HostConnectionData;

        return true;
    }

    public string GetAllocationId()
    {
        return _allocationId.ToString();
    }

    public string GetConnectionData()
    {
        return Convert.ToBase64String(_connectionData);
    }

    public (byte[] AllocationId, byte[] Key, byte[] ConnectionData, string DtlsAddress, int DtlsPort) GetHostConnectionInfo()
    {
        return (_allocationIdBytes, _key, _connectionData, _ip, _port);
    }

    public (byte[] AllocationId, byte[] Key, byte[] ConnectionData, byte[] HostConnectionData, string DtlsAddress, int DtlsPort) GetClientConnectionInfo()
    {
        return (_allocationIdBytes, _key, _connectionData, _hostConnectionData, _ip, _port);
    }
}