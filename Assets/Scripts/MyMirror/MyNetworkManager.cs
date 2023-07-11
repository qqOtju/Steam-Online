using System;
using System.Collections.Generic;
using Mirror;
using Steam;
using Steam.UI;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyMirror
{
    public class MyNetworkManager : NetworkManager
    {
        [SerializeField] private GameObject _playerPrefab;
        [Scene] [SerializeField] private string _menuScene = null;
        public event Action<CSteamID> OnPlayerConnect;

        public List<NetworkBehaviour> GamePlayers { get; } = new();

        /*public override void OnClientConnect()
        {
            base.OnClientConnect();
            var steamId = SteamMatchmaking.GetLobbyMemberByIndex(SteamLobby.LobbyId, numPlayers - 1);
            OnPlayerConnect?.Invoke(steamId);
        }*/

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            base.OnServerAddPlayer(conn);
            _menuScene = _menuScene.SceneName();
            //var playerInfoDisplay = conn.identity.GetComponent<PlayerInfoDisplay>();
            //playerInfoDisplay.SetSteamId(steamId.m_SteamID);
            var steamId = SteamMatchmaking.GetLobbyMemberByIndex(SteamLobby.LobbyId, numPlayers - 1);
            OnPlayerConnect?.Invoke(steamId);
        }

        public override void OnClientSceneChanged()
        {
            base.OnClientSceneChanged();
        }

        public override void ServerChangeScene(string newSceneName)
        {
            if (SceneManager.GetActiveScene().name == _menuScene && newSceneName.StartsWith("Scene_Map"))
            {
                for (int i = GamePlayers.Count - 1; i >= 0; i--)
                {
                    var conn = GamePlayers[i].connectionToClient;
                    var gamePlayerInstance = Instantiate(_playerPrefab);
                    
                    NetworkServer.Destroy(conn.identity.gameObject);

                    NetworkServer.ReplacePlayerForConnection(conn, gamePlayerInstance.gameObject);
                }
            }
            base.ServerChangeScene(newSceneName);
        }
    }
}