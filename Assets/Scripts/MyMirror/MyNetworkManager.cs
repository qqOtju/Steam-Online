using Mirror;
using Steam;
using Steamworks;

namespace MyMirror
{
    public class MyNetworkManager : NetworkManager 
    {
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            base.OnServerAddPlayer(conn);
            var steamId = SteamMatchmaking.GetLobbyMemberByIndex(SteamLobby.LobbyId, numPlayers - 1);
            var playerInfoDisplay = conn.identity.GetComponent<PlayerInfoDisplay>();
            playerInfoDisplay.SetSteamId(steamId.m_SteamID);
        }
    }
}