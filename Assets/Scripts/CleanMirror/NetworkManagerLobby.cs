using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CleanMirror
{
    public class NetworkManagerLobby : NetworkManager
    {
        [Scene] [SerializeField] private string _menuScene ;
        
        [Header("Room")]
        [SerializeField] private NetworkRoomPlayerController _roomPlayerPrefab = null;

        public List<NetworkRoomPlayerController> RoomPlayers { get; } = new List<NetworkRoomPlayerController>();
        public List<NetworkGamePlayerController> GamePlayers { get; } = new List<NetworkGamePlayerController>();
        
        public static event Action OnClientConnected;
        public static event Action OnClientDisconnected;

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
        
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            if (SceneManager.GetActiveScene().name != _menuScene.SceneName()) return;
            var roomPlayerInstance = Instantiate(_roomPlayerPrefab);

            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
        }

    }
}