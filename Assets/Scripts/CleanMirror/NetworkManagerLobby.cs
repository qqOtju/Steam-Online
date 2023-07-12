using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CleanMirror
{
    public class NetworkManagerLobby : NetworkManager
    {
        [SerializeField] private int _minPlayers = 2;
        [Scene] [SerializeField] private string _menuScene;

        [Header("Room")] 
        [SerializeField] private NetworkRoomPlayerController _roomPlayerPrefab;
        [Header("Game")]
        [SerializeField] private NetworkGamePlayerController _gamePlayerPrefab;
        
        public List<NetworkRoomPlayerController> RoomPlayers { get; } = new();
        public List<NetworkGamePlayerController> GamePlayers { get; } = new();
        public static event Action OnClientConnected;
        public static event Action OnClientDisconnected;

        #region ClientCallbacks

        public override void OnClientConnect()
        {
            base.OnClientConnect();
            OnClientConnected?.Invoke();
        }

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            OnClientDisconnected?.Invoke();
        }

        #endregion

        #region ServerCallbacks

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            if (SceneManager.GetActiveScene().name != _menuScene.SceneName()) return;
            var isLeader = RoomPlayers.Count == 0;
            var roomPlayerInstance = Instantiate(_roomPlayerPrefab);
            roomPlayerInstance.IsLeader = isLeader;
            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            if(conn.identity != null)
            {
                var player = conn.identity.GetComponent<NetworkRoomPlayerController>();
                RoomPlayers.Remove(player);
                NotifyPlayersOfReadyState();
            }
            base.OnServerDisconnect(conn);
        }

        public override void OnStopServer() => RoomPlayers.Clear();

        public override void ServerChangeScene(string newSceneName)
        {
            //From menu to game
            if(SceneManager.GetActiveScene().name == _menuScene.SceneName() && newSceneName.StartsWith("SceneMap"))
                for (int i = RoomPlayers.Count - 1; i >= 0; i--)
                {
                    var conn = RoomPlayers[i].connectionToClient;
                    var gamePlayerInstance = Instantiate(_gamePlayerPrefab);
                    gamePlayerInstance.SetDisplayName(RoomPlayers[i]._displayName);
                    NetworkServer.Destroy(conn.identity.gameObject);
                    NetworkServer.ReplacePlayerForConnection(conn, gamePlayerInstance.gameObject);
                }
            base.ServerChangeScene(newSceneName);
        }

        
        
        #endregion

        public void NotifyPlayersOfReadyState()
        {
            foreach (var player in RoomPlayers)
                player.HandleReadyToStart(IsReadyToStart());
        }

        private bool IsReadyToStart() => numPlayers >= _minPlayers && RoomPlayers.All(player => player._isReady);

        public void StartGame()
        {
            if (SceneManager.GetActiveScene().name != _menuScene.SceneName() || !IsReadyToStart()) return;
            ServerChangeScene("SceneMap_01");
        }
        
    }
}