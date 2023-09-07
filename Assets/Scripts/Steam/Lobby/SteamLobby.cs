using Extensions;
using Mirror;
using MyMirror;
using Steam.Level;
using Steamworks;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Steam.Lobby
{
    public class SteamLobby : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject _menuPanel;
        [SerializeField] private GameObject _lobbyPanel;
        [Header("Events")]
        [SerializeField] private VoidEvent _onLobbyLeave;
        [Header("References")]
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
            gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
            lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            _onLobbyLeave.Register(LeaveLobby);
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            gameLobbyJoinRequested.Dispose();
            lobbyCreated.Dispose();
            lobbyEntered.Dispose();
            _onLobbyLeave.Unregister(LeaveLobby);
        }

        public void HostLobby()
        {
            _menuPanel.SetActive(false);
            _lobbyPanel.SetActive(true);
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, _networkManager.maxConnections);
        }
        
        //ToDo переробити цей жах
        private void LeaveLobby()
        {
            if(!LobbyId.IsLobby()) return;
            Cursor.lockState = CursorLockMode.None;
            SteamMatchmaking.LeaveLobby(LobbyId);
            _networkManager.Leave();
            Destroy(_networkManager.gameObject);
            Destroy(gameObject);
            SceneManager.LoadScene(_menuScene.SceneName(), LoadSceneMode.Single);
        }
        
        public void SetLevel(SOLevelInfo levelInfo) => _currentLevel = levelInfo;
        
        public void StartGame()
        {
            if (SceneManager.GetActiveScene().name == _menuScene.SceneName())
                _networkManager.ServerChangeScene(_currentLevel.LevelScene.SceneName());
        }
        
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
    }
}