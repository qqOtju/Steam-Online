using Mirror;
using Steamworks;
using UnityEngine;

namespace Steam
{
    public class SteamLobby : MonoBehaviour
    {
        [SerializeField] private NetworkManager _networkManager;
        [SerializeField] private GameObject _buttons = null;

        public static CSteamID LobbyId { get; private set; }

        private const string HostAddressKey = "HostAddress";
        
        protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
        protected Callback<LobbyCreated_t> lobbyCreated;
        protected Callback<LobbyEnter_t> lobbyEntered;
        

        private void Start()
        {
            if(!SteamManager.Initialized) return;

            gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
            lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        }

        public void HostLobby()
        {
            _buttons.SetActive(false);
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, _networkManager.maxConnections);
        }

        private void OnLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult != EResult.k_EResultOK)
            {
                _buttons.SetActive(true);
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
            _buttons.SetActive(false);
        }
        
    }
}