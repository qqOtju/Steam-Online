using System;
using System.Collections.Generic;
using Mirror;
using Steam;
using Steam.Player;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyMirror
{
    public class MyNetworkManager : NetworkManager
    {
        [SerializeField] private IntermediatePlayer _intermediatePlayerPrefab;
        [SerializeField] private GamePlayerController _gamePlayerPrefab;
        [Scene] [SerializeField] private string _menuScene = null;
        public event Action<CSteamID> OnPlayerConnect;
        public event Action<NetworkConnectionToClient> OnSceneChange; 
        public List<IntermediatePlayer> RoomPlayers { get; } = new();
        public List<GamePlayerController> GamePLayers { get; } = new();

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            Debug.Log("OnServerAddPlayer");
            if(SceneManager.GetActiveScene().name != _menuScene.SceneName()) return;
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
                for (int i = RoomPlayers.Count - 1; i >= 0; i--)
                {
                    var conn = RoomPlayers[i].connectionToClient;
                    var gamePlayerInstance = Instantiate(_gamePlayerPrefab);
                    NetworkServer.Destroy(conn.identity.gameObject);
                    gamePlayerInstance.gameObject.SetActive(false);
                    NetworkServer.ReplacePlayerForConnection(conn, gamePlayerInstance.gameObject);
                    GamePLayers.Add(gamePlayerInstance);
                }
            }
            base.ServerChangeScene(newSceneName);
        }

        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            base.OnServerReady(conn);
            OnSceneChange?.Invoke(conn);
        }
    }
}