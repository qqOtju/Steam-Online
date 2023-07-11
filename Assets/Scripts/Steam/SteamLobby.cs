using Mirror;
using Steamworks;
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
        [Scene] [SerializeField] private string _menuScene = null;
        [SerializeField] private NetworkManager _networkManager;

        public static CSteamID LobbyId { get; private set; }

        private const string HostAddressKey = "HostAddress";

        private void Start()
        {
            if(!SteamManager.Initialized) return;
            _menuScene = _menuScene.SceneName();
            Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
            Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        }

        public void HostLobby()
        {
            _menuPanel.SetActive(false);
            _lobbyPanel.SetActive(true);
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, _networkManager.maxConnections);
        }
        
        public void StartGame()
        {
            Debug.Log($"1: {SceneManager.GetActiveScene().name}| 2: {_menuScene}");
            if (SceneManager.GetActiveScene().name == _menuScene)
            {
                _networkManager.ServerChangeScene("Scene_Map_01");
            }
        }

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

        private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback) =>
            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);

        private void OnLobbyEntered(LobbyEnter_t callback)
        {
            if(NetworkServer.active) return;
            var hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);
            _networkManager.networkAddress = hostAddress;
            _networkManager.StartClient();
            _menuPanel.SetActive(false);
            _lobbyPanel.SetActive(true);
            //_startGameBtn.interactable = NetworkServer.activeHost;
        }


    }
}