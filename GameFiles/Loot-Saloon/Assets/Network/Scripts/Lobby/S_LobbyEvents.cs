#region
using System;
using Unity.Services.Lobbies.Models;
#endregion

public static class S_LobbyEvents
{
    public static Action<Lobby> OnLobbyUpdatedWithParam;

    public static Action OnLobbyUpdated;

    public static Action OnLobbyReady;
    
    public static Action OnLobbyUnready;

    public static void ClearEvents()
    {
        OnLobbyUpdatedWithParam = null;
        OnLobbyUpdated          = null;
        OnLobbyReady            = null;
        OnLobbyUnready          = null;
    }
}