using System;
using System.Collections.Generic;
using Mirror;
using Steam;
using Steam.Player;
using Steam.UI;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyMirror
{
    public class MyNetworkManager : NetworkManager
    {
        [SerializeField] private LobbyPlayerController _lobbyPlayerPrefab;
        [SerializeField] private GamePlayerController _gamePlayerPrefab;
        [SerializeField] private UILobbyController _lobbyController;
        [SerializeField] [Scene] private string _menuScene = null;
        public List<LobbyPlayerController> LobbyPlayers { get; } = new();
        public List<GamePlayerController> GamePlayers { get; } = new();
        public event Action<NetworkConnectionToClient> OnSceneChange;

        /// <summary>
        /// Called on server when a client requests to add the player.
        /// Adds the lobby player.
        /// </summary>
        /// <param name="conn">connection of the client</param>
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            if(SceneManager.GetActiveScene().name != _menuScene.SceneName()) return;
            var gamePlayer = Instantiate(_lobbyPlayerPrefab);
            gamePlayer.Init(conn.connectionId, numPlayers, 
                SteamMatchmaking.GetLobbyMemberByIndex(SteamLobby.LobbyId, numPlayers).m_SteamID, _lobbyController);
            NetworkServer.AddPlayerForConnection(conn, gamePlayer.gameObject);
        }

        /// <summary>
        /// Adds the game player.
        /// Swaps the connection between the lobby player and the game player.
        /// </summary>
        public override void ServerChangeScene(string newSceneName)
        {
            if (SceneManager.GetActiveScene().name != _menuScene.SceneName() &&
                !newSceneName.StartsWith("Scene_Map")) return;
            for (int i = LobbyPlayers.Count - 1; i >= 0; i--)
            {
                var conn = LobbyPlayers[i].connectionToClient;
                var gamePlayerInstance = Instantiate(_gamePlayerPrefab);
                NetworkServer.Destroy(conn.identity.gameObject);
                gamePlayerInstance.gameObject.SetActive(false);
                NetworkServer.ReplacePlayerForConnection(conn, gamePlayerInstance.gameObject);
                GamePlayers.Add(gamePlayerInstance);
            }

            base.ServerChangeScene(newSceneName);
        }

        /// <summary>Called on the server when a client is ready (= loaded the scene)</summary>
        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            base.OnServerReady(conn);
            OnSceneChange?.Invoke(conn);
        }
    }
}