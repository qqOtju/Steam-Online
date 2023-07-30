using Mirror;
using MyMirror;
using Steam.Level;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Steam
{
    public class SteamLobby : NetworkBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject _menuPanel;
        [SerializeField] private GameObject _lobbyPanel;
        [Header("Other")]
        [Scene] [SerializeField] private string _menuScene = null;
        [SerializeField] private MyNetworkManager _networkManager;
        
        private const string HostAddressKey = "HostAddress";
        
        private SOLevelInfo _currentLevel;
        public static ulong CurrentLobbyId { get; private set; }
        public static CSteamID LobbyId { get; private set; }

        private void Awake()
        {
            Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
            Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            Callback<LobbyKicked_t>.Create(OnLobbyLeave);
        }     
        
        public void HostLobby()
        {
            _menuPanel.SetActive(false);
            _lobbyPanel.SetActive(true);
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, _networkManager.maxConnections);
        }
        
        public void SetLevel(SOLevelInfo levelInfo) => _currentLevel = levelInfo;
        
        public void StartGame()
        {
            if (SceneManager.GetActiveScene().name == _menuScene.SceneName())
                _networkManager.ServerChangeScene(_currentLevel.LevelScene.SceneName());
        }

        public void LeaveLobby()
        {
            SteamMatchmaking.LeaveLobby(LobbyId);
            _menuPanel.gameObject.SetActive(true);
            _lobbyPanel.gameObject.SetActive(false);
            if (isServer) _networkManager.StopHost();
            else _networkManager.StopClient();
        }
        
        public void ExitGame() => Application.Quit();

        private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback) =>
            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        
        private void OnLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult != EResult.k_EResultOK)
            {
                _menuPanel.SetActive(true);
                _lobbyPanel.SetActive(false);
                return;
            }
            LobbyId = new CSteamID(callback.m_ulSteamIDLobby);
            _networkManager.StartHost();
            SteamMatchmaking.SetLobbyData(LobbyId, HostAddressKey,
                SteamUser.GetSteamID().ToString());
            SteamMatchmaking.SetLobbyData(LobbyId, "name", SteamFriends.GetPersonaName());
        }
        
        private void OnLobbyEntered(LobbyEnter_t callback)
        {
            CurrentLobbyId = callback.m_ulSteamIDLobby;
            if(NetworkServer.active) return;
            _networkManager.networkAddress = 
                SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);
            _networkManager.StartClient();
            _menuPanel.SetActive(false);
            _lobbyPanel.SetActive(true);
        }
        
        private void OnLobbyLeave(LobbyKicked_t callback) => LeaveLobby();
        
    }
}