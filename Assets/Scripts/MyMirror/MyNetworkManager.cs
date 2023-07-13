using System;
using System.Collections.Generic;
using Mirror;
using Steam;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyMirror
{
    public class MyNetworkManager : NetworkManager
    {
        [SerializeField] private IntermediatePlayer _intermediatePlayerPrefab;
        [Scene] [SerializeField] private string _menuScene = null;
        public event Action<CSteamID> OnPlayerConnect;
        public List<IntermediatePlayer> GamePlayers { get; } = new();

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            var gamePlayer = Instantiate(_intermediatePlayerPrefab);
            gamePlayer.connectionId = conn.connectionId;
            gamePlayer.playerIdNumber = numPlayers;
            var steamId = SteamMatchmaking.GetLobbyMemberByIndex(SteamLobby.LobbyId, numPlayers);
            gamePlayer.playerSteamId = steamId.m_SteamID;
            NetworkServer.AddPlayerForConnection(conn, gamePlayer.gameObject);
            OnPlayerConnect?.Invoke(steamId);
        }

        public override void ServerChangeScene(string newSceneName)
        {
            if (SceneManager.GetActiveScene().name == _menuScene.SceneName() && newSceneName.StartsWith("Scene_Map"))
            {
                for (int i = GamePlayers.Count - 1; i >= 0; i--)
                {
                    var conn = GamePlayers[i].connectionToClient;
                    var gamePlayerInstance = Instantiate(_intermediatePlayerPrefab);
                    
                    NetworkServer.Destroy(conn.identity.gameObject);

                    NetworkServer.ReplacePlayerForConnection(conn, gamePlayerInstance.gameObject);
                }
            }
            base.ServerChangeScene(newSceneName);
        }
    }
}