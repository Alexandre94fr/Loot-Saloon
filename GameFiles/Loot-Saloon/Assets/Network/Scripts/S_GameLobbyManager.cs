using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class S_GameLobbyManager : MonoBehaviour
{
    public static S_GameLobbyManager instance;

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

    public async Task<bool> CreateLobby()
    {
        Debug.Log("Creating lobby...");

        Dictionary<string, string> playerData = new Dictionary<string, string>()
        {
            { "GamerTag", "HostPlayer" }
        };
        bool succeeded = await S_LobbyManager.instance.CreateLobbyAsync(2, false, playerData);
        return succeeded;
    }

    public async Task<bool> JoinLobby(string p_code)
    {
        Debug.Log("Joining lobby...");
        Dictionary<string,string> playerData = new Dictionary<string, string>()
        {
            { "GamerTag", "JoinPlayer" }
        };

        bool succeeded = await S_LobbyManager.instance.JoinLobby(p_code, playerData);
        return succeeded;
    }

    public string GetLobbyCode()
    {
        return S_LobbyManager.instance.GetLobbyCode();
    }
}
