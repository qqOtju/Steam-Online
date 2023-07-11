using GridLayout;
using Mirror;
using MyMirror;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace Steam.UI
{
    public class UILobbyController : MonoBehaviour
    {
        [SerializeField] private UIPlayerInfo _playerInfoPrefab;
        [SerializeField] private AbstractGridLayout _layout;
        [SerializeField] private MyNetworkManager _networkManager;
        [SerializeField] private Button _startGameBtn;
        private Transform _playerListContainer;
        
        private void Awake()
        {
            _networkManager.OnPlayerConnect += NetworkManagerOnOnPlayerConnect;
        }

        private void Start()
        {
            _playerListContainer = _layout.gameObject.transform;
        }

        private void NetworkManagerOnOnPlayerConnect(CSteamID obj)
        {
            var playerInfo = Instantiate(_playerInfoPrefab, _playerListContainer);
            playerInfo.SetSteamId(obj.m_SteamID);
            _layout.Align();
            _startGameBtn.interactable = NetworkServer.activeHost;
        }
    }
}