using Mirror;
using MyMirror;
using Steam.Level;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Steam
{
    public class SteamLobby : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject _menuPanel;
        [SerializeField] private GameObject _lobbyPanel;
        [Header("Other")]
        [SerializeField] private TextMeshProUGUI _lobbyNameText;
        [Scene] [SerializeField] private string _menuScene = null;
        [SerializeField] private MyNetworkManager _networkManager;
        
        private const string HostAddressKey = "HostAddress";
        
        protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
        protected Callback<LobbyCreated_t> lobbyCreated;
        protected Callback<LobbyEnter_t> lobbyEntered;
        
        private SOLevelInfo _currentLevel;
        public static ulong CurrentLobbyId { get; private set; }
        public static CSteamID LobbyId { get; private set; }

        private void Awake()
        {
            if(!SteamManager.Initialized)
            {
                _lobbyNameText.gameObject.SetActive(true);
                _lobbyNameText.text = "Steam is not initialized";
                return;
            }
            gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
            lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
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
            /*_menuPanel.gameObject.SetActive(true);
            _lobbyPanel.gameObject.SetActive(false);
            if (isServer) _networkManager.StopHost();
            else _networkManager.StopClient();*/
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