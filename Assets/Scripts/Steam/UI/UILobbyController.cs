using System.Collections.Generic;
using System.Linq;
using GridLayout;
using Mirror;
using MyMirror;
using Steam.Player;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Steam.UI
{
    public class UILobbyController : NetworkBehaviour
    {
        [Header("Lobby main")]
        [SerializeField] private TextMeshProUGUI _lobbyNameText;
        [SerializeField] private UIPlayerInfo _playerInfoPrefab;
        [Tooltip("Players info container")]
        [SerializeField] private AbstractGridLayout _layout;
        [SerializeField] private MyNetworkManager _networkManager;
        [Header("Buttons")]
        [SerializeField] private Button _startGameBtn;
        [SerializeField] private TextMeshProUGUI _startButtonText;
        [SerializeField] private TextMeshProUGUI _readyBtnText;
        
        public static UILobbyController Instance;

        private readonly Color _multiplyColor = new(0.5f, 0.5f, 0.5f, 1f);
        private LobbyPlayerController _localLobbyPlayerController;
        private readonly List<UIPlayerInfo> _playerInfos = new();
        private Color _startBtnBaseColor;
        private bool _playerInfoCreated;
        
        private void Awake()
        {
            _startBtnBaseColor = _startButtonText.color;
            _startButtonText.color = _multiplyColor;
            _startGameBtn.interactable = false;
            if(Instance == null) Instance = this;
        }

        public void ReadyPlayer() => _localLobbyPlayerController.ChangeReady();

        public void UpdateLobbyName()
        {
            _lobbyNameText.text = $"{SteamMatchmaking.GetLobbyData(SteamLobby.LobbyId, "name")}`s lobby";
        }
        
        public void SetLocalPlayer(LobbyPlayerController localPlayerController) =>
            _localLobbyPlayerController = localPlayerController;
        
        public void UpdatePlayerList()
        {
            if(!_playerInfoCreated) CreateHostPlayerItem();
            if(_playerInfos.Count < _networkManager.LobbyPlayers.Count) CreateClientPlayerItem();
            if(_playerInfos.Count > _networkManager.LobbyPlayers.Count) RemovePlayerItem();
            if(_playerInfos.Count > 0 && _playerInfos.Count == _networkManager.LobbyPlayers.Count) UpdatePlayerItem();
         }
        
        private void CreateHostPlayerItem()
        {
            foreach (var player in _networkManager.LobbyPlayers)
            {
                var newPlayerItem = Instantiate(_playerInfoPrefab,_layout.gameObject.transform);
                newPlayerItem.playerName = player.playerName;
                newPlayerItem.connectionId = player.connectionId;
                newPlayerItem.playerSteamId = player.playerSteamId;
                newPlayerItem.ready = player.readyStatus;
                newPlayerItem.Init(); 
                _layout.Align(true);
                _playerInfos.Add(newPlayerItem);
            }
            _playerInfoCreated = true;
        }

        private void CreateClientPlayerItem()
        {
            foreach (var player in _networkManager.LobbyPlayers)
            {
                if (_playerInfos.Any(newPlayer => newPlayer.connectionId == player.connectionId)) continue;
                var newPlayerItem = Instantiate(_playerInfoPrefab, _layout.gameObject.transform);
                newPlayerItem.playerName = player.playerName;
                newPlayerItem.connectionId = player.connectionId;
                newPlayerItem.playerSteamId = player.playerSteamId;
                newPlayerItem.ready = player.readyStatus;
                newPlayerItem.Init();
                _layout.Align(true);
                _playerInfos.Add(newPlayerItem);
            }
        }

        private void UpdatePlayerItem()
        {
            foreach (var player in _networkManager.LobbyPlayers)
            foreach (var info in _playerInfos)
            {
                if (info.connectionId != player.connectionId) continue;
                info.playerName = player.playerName;
                info.ready = player.readyStatus;
                info.Init();
                if (player == _localLobbyPlayerController)
                    UpdateBtn();
            }
            CheckLobbyReadyStatus();
        }

        private void RemovePlayerItem()
        {
            var playerInfosToRemove = _playerInfos.Where(info =>
                    _networkManager.LobbyPlayers.All(player => player.connectionId != info.connectionId)).ToList();
            if (playerInfosToRemove.Count <= 0) return;
            foreach (var info in playerInfosToRemove)
            {
                var objToRemove = info.gameObject;
                _playerInfos.Remove(info);
                Destroy(objToRemove);
            }
        }

        private void UpdateBtn() =>
            _readyBtnText.text = _localLobbyPlayerController.readyStatus ? "<color=red>Not ready</color>" : "<color=green>Ready</color>";

        private void CheckLobbyReadyStatus()
        {
            var ready = true;
            foreach (var player in _networkManager.LobbyPlayers.Where(player => !player.readyStatus))
                ready = false;


            if (ready)
            {
                _startButtonText.color = _startBtnBaseColor;
                _startGameBtn.interactable = _localLobbyPlayerController.playerIdNumber == 0;
            }
            else
            {
                _startButtonText.color = _multiplyColor;
                _startGameBtn.interactable = false;
            }
        }
    }
}