using Mirror;
using MyMirror;
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
        [SerializeField] [Scene] private string _menuScene = null;
        [SerializeField] private MyNetworkManager _networkManager;
        public static CSteamID LobbyId { get; private set; }
        public static ulong CurrentLobbyId { get; private set; }

        private const string HostAddressKey = "HostAddress";

        private void Awake()
        {
            if (!SteamManager.Initialized)
            {
                _lobbyNameText.gameObject.SetActive(true);
                _lobbyNameText.text = "Steam is not initialized";
                return;
            }
            
            Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
            Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            Callback<LobbyKicked_t>.Create(OnLobbyLeave);
            Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdated);
        }

        #region ButtonActions

        public void HostLobby()
        {
            _menuPanel.SetActive(false);
            _lobbyPanel.SetActive(true);
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, _networkManager.maxConnections);
        }

        public void StartGame()
        {
            if (SceneManager.GetActiveScene().name == _menuScene.SceneName())
                _networkManager.ServerChangeScene("Scene_Map_01");
        }

        public void LeaveLobby()
        {
            SteamMatchmaking.LeaveLobby(LobbyId);
            _networkManager.StopHost();
            /*if (isServer) _networkManager.StopHost();
            else _networkManager.StopClient();*/
            _menuPanel.gameObject.SetActive(true);
            _lobbyPanel.gameObject.SetActive(false);
            gameObject.SetActive(true);
        }

        #endregion

        #region Callbacks

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
            if (NetworkServer.active) return;
            _networkManager.networkAddress =
                SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);
            _networkManager.StartClient();
            _menuPanel.SetActive(false);
            _lobbyPanel.SetActive(true);
        }

        private void OnLobbyLeave(LobbyKicked_t callback) => LeaveLobby();

        private void OnLobbyChatUpdated(LobbyChatUpdate_t callback) { }

        #endregion
    }
}