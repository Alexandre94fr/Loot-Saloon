#region
using Unity.Services.Lobbies.Models;
#endregion

public static class S_LobbyEvents
{
    public delegate void LobbyReady();

    public delegate void LobbyUnready();

    public delegate void LobbyUpdated();

    public delegate void LobbyUpdatedWithParam(Lobby p_lobby);

    public static LobbyUpdatedWithParam OnLobbyUpdatedWithParam;

    public static LobbyUpdated OnLobbyUpdated;

    public static LobbyReady OnLobbyReady;
    
    public static LobbyUnready OnLobbyUnready;
}