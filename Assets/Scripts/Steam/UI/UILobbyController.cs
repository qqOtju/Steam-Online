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
        [Header("Lobby main")] [SerializeField]
        private TextMeshProUGUI _lobbyNameText;

        [SerializeField] private UIPlayerInfo _playerInfoPrefab;

        [Tooltip("Players info container")] [SerializeField]
        private AbstractGridLayout _layout;

        [SerializeField] private MyNetworkManager _networkManager;
        [Header("Buttons")] [SerializeField] private Button _startGameBtn;
        [SerializeField] private TextMeshProUGUI _startButtonText;
        [SerializeField] private TextMeshProUGUI _readyBtnText;

        private Dictionary<LobbyPlayerController, UIPlayerInfo> _players = new();
        private readonly Color _multiplyColor = new(0.5f, 0.5f, 0.5f, 1f);
        private LobbyPlayerController _localLobbyPlayerController;
        private Color _startBtnBaseColor;
        private bool _playerInfoCreated;

        private void Awake()
        {
            _startBtnBaseColor = _startButtonText.color;
            _startButtonText.color = _multiplyColor;
            _startGameBtn.interactable = false;
        }

        public void ReadyPlayer() =>
            _localLobbyPlayerController.ChangeReady();

        public void SetLocalPlayer(LobbyPlayerController localPlayerController) =>
            _localLobbyPlayerController = localPlayerController;
        
        public void UpdateLobbyName() =>
            _lobbyNameText.text = $"{SteamMatchmaking.GetLobbyData(SteamLobby.LobbyId, "name")}`s lobby";


        #region UpdateUI

        public void UpdatePlayerList()
        {
            if (!_playerInfoCreated) CreateHostPlayerItem();
            if (_players.Count < _networkManager.LobbyPlayers.Count) CreateClientPlayerItem();
            if (_players.Count > _networkManager.LobbyPlayers.Count) RemovePlayerItem();
            if (_players.Count > 0 && _players.Count == _networkManager.LobbyPlayers.Count) UpdatePlayerItem();
        }

        private void CreateHostPlayerItem()
        {
            foreach (var player in _networkManager.LobbyPlayers)
            {
                var newPlayerItem = Instantiate(_playerInfoPrefab, _layout.gameObject.transform);
                newPlayerItem.Init(player.PlayerName, player.ConnectionId,
                    player.PlayerSteamId, player.ReadyStatus);
                newPlayerItem.UpdateInfo();
                _layout.Align(true);
                _players.Add(player, newPlayerItem);
            }

            _playerInfoCreated = true;
        }

        private void CreateClientPlayerItem()
        {
            foreach (var player in _networkManager.LobbyPlayers)
            {
                if(_players.ContainsKey(player)) continue;
                var newPlayerItem = Instantiate(_playerInfoPrefab, _layout.gameObject.transform);
                newPlayerItem.Init(player.PlayerName, player.ConnectionId,
                    player.PlayerSteamId, player.ReadyStatus);
                newPlayerItem.UpdateInfo();
                _layout.Align(true);
                _players.Add(player, newPlayerItem);
            }
        }

        private void RemovePlayerItem()
        {
            /*var playerInfosToRemove = _playerInfos.Where(info =>
                _networkManager.LobbyPlayers.All(player => player.ConnectionId != info.ConnectionId)).ToList();
            if (playerInfosToRemove.Count <= 0) return;
            foreach (var info in playerInfosToRemove)
            {
                var objToRemove = info.gameObject;
                _playerInfos.Remove(info);
                Destroy(objToRemove);
            }*/
        }

        private void UpdatePlayerItem()
        {
            foreach (var player in _networkManager.LobbyPlayers)
            {
                _players.TryGetValue(player, out var info);
                if (info == null)
                {
                    Debug.Log("Info = null");
                    continue;
                }
                info.Init(player.PlayerName, player.ReadyStatus);
                info.UpdateInfo();
                if (player == _localLobbyPlayerController)
                    UpdateBtn();
            }
            CheckLobbyReadyStatus();
        }

        #endregion

        private void UpdateBtn() =>
            _readyBtnText.text = _localLobbyPlayerController.ReadyStatus
                ? "<color=red>Not ready</color>"
                : "<color=green>Ready</color>";

        private void CheckLobbyReadyStatus()
        {
            var ready = true;
            foreach (var player in _networkManager.LobbyPlayers.Where(player => !player.ReadyStatus))
                ready = false;


            if (ready)
            {
                _startButtonText.color = _startBtnBaseColor;
                _startGameBtn.interactable = _localLobbyPlayerController.PlayerIdNumber == 0;
            }
            else
            {
                _startButtonText.color = _multiplyColor;
                _startGameBtn.interactable = false;
            }
        }
    }
}