using Unity.Services.Lobbies.Models;

public static class S_LobbyEvents
{
    public delegate void LobbyUpdated(Lobby p_lobby);

    public static LobbyUpdated onLobbyUpdated;
}