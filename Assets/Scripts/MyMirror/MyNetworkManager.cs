using System;
using System.Collections.Generic;
using Extensions;
using Mirror;
using Steam;
using Steam.Lobby;
using Steam.Player;
using Steam.UI;
using Steamworks;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyMirror
{
    public class MyNetworkManager : NetworkManager
    {
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private LobbyPlayerController _intermediatePlayerPrefab;
        [SerializeField] private GamePlayerController _gamePlayerPrefab;
        [SerializeField] private UILobbyController _lobbyController;
        [SerializeField] [Scene] private string _menuScene = null;
        [SerializeField] private IntVariable _playersNum;
        [SerializeField] private bool _withoutSteam;
        public event Action<NetworkConnectionToClient> OnSceneChange; 
        public List<LobbyPlayerController> LobbyPlayers { get; } = new();
        public List<GamePlayerController> GamePLayers { get; } = new();
        public UILobbyController LobbyController => _lobbyController;

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            if(SceneManager.GetActiveScene().name != _menuScene.SceneName()) return;
            var gamePlayer = Instantiate(_intermediatePlayerPrefab);
            gamePlayer.Init(conn.connectionId, numPlayers, _withoutSteam?0:SteamMatchmaking.GetLobbyMemberByIndex(SteamLobby.LobbyId, numPlayers).m_SteamID);
            gamePlayer.gameObject.transform.position = _spawnPoints[numPlayers].position;
            gamePlayer.gameObject.transform.rotation = _spawnPoints[numPlayers].rotation;
            NetworkServer.AddPlayerForConnection(conn, gamePlayer.gameObject);
        }

        public override void ServerChangeScene(string newSceneName)
        {
            if (SceneManager.GetActiveScene().name == _menuScene.SceneName() && newSceneName.StartsWith("Scene_Map"))
            {
                _playersNum.Value = LobbyPlayers.Count;
                for (int i = LobbyPlayers.Count - 1; i >= 0; i--)
                {
                    var conn = LobbyPlayers[i].connectionToClient;
                    var gamePlayerInstance = Instantiate(_gamePlayerPrefab);
                    gamePlayerInstance.gameObject.SetActive(false);
                    gamePlayerInstance.Init(LobbyPlayers[i].PlayerIdNumber, LobbyPlayers[i].PlayerName);
                    NetworkServer.Destroy(conn.identity.gameObject);
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

        public void Leave()
        {
            if(NetworkServer.activeHost)
                StopHost();
            if (NetworkServer.active)
                StopServer();
            else if (NetworkClient.active)
                StopClient();
        }

        public override void OnStopServer()
        {
            foreach (var player in GamePLayers)
            {
                Destroy(player.gameObject);
            }
            base.OnStopServer();
        }
    }
}