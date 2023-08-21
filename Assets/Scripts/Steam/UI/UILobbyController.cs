using System.Collections.Generic;
using System.Linq;
using GridLayout;
using Mirror;
using MyMirror;
using Steam.Level;
using Steam.Lobby;
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
        [SerializeField] private UIPlayerInfo _playerInfoPrefab;
        [Tooltip("Players info container")]
        [SerializeField] private AbstractGridLayout _layout;
        [SerializeField] private MyNetworkManager _networkManager;
        [SerializeField] private SteamLobby _steamLobby;
        [SerializeField] private LocalLobby _localLobby;
        [Header("Start Button")]
        [SerializeField] private Image _startBtnIcon;
        [SerializeField] private TextMeshProUGUI _startBtnText;
        [SerializeField] private UIMyButton _startBtn;
        [Header("Map Button")]
        [SerializeField] private Image _mapBtnIcon;
        [SerializeField] private TextMeshProUGUI _mapBtnText;
        [SerializeField] private UIMyButton _mapBtn;
        [Header("Level")]
        [SerializeField] private SOLevelInfo[] _levels;
        [SerializeField] private Image _levelImage;
        [SerializeField] private TextMeshProUGUI _levelNameText;
        [Header("Other")]
        [SerializeField] private bool _withoutSteam;

        private readonly Dictionary<LobbyPlayerController, UIPlayerInfo> _players = new();
        private readonly Color _multiplyColor = new(0.5f, 0.5f, 0.5f, 1f);
        private LobbyPlayerController _localLobbyPlayer;
        private SOLevelInfo _currentLevel;
        private Color _startBtnBaseColor;
        private bool _playerInfoCreated;
        private int _currentLevelIndex;

        private void Awake()
        {
            SetLevel(0);
            _startBtnBaseColor = _startBtnText.color;
            _startBtnText.color = _multiplyColor;
            _startBtn.Interactable = false;
            _mapBtnIcon.color = _multiplyColor;
            _mapBtnText.color = _multiplyColor;
            _mapBtn.Interactable = false;
        }

        public void ReadyPlayer() => _localLobbyPlayer.ChangeReady();

        public void SetLocalPlayer(LobbyPlayerController playerController)
        {
            _localLobbyPlayer = playerController;
            if (_localLobbyPlayer.PlayerIdNumber != 0) return;
            _mapBtn.Interactable = true;
            _mapBtnIcon.color = _startBtnBaseColor;
            _mapBtnText.color = _startBtnBaseColor;
        }

        public void ChangeLevel(int dir)
        {
            if (dir > 0)
                SetLevel(_currentLevelIndex + 1);
            else if(dir < 0)
                SetLevel(_currentLevelIndex - 1);
        }

        private void SetLevel(int index)
        {
            if (index < 0)
                index = _levels.Length - 1;
            if (index == _levels.Length)
                index = 0;
            _currentLevelIndex = index;
            _currentLevel = _levels[_currentLevelIndex];
            _levelImage.sprite = _currentLevel.LevelImage;
            _levelNameText.text = _currentLevel.LevelName;
            if (_withoutSteam) _localLobby.SetLevel(_currentLevel);
            else _steamLobby.SetLevel(_currentLevel);
        }

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
                newPlayerItem.UpdateUI();
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
                newPlayerItem.UpdateUI();
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
                info.UpdateUI();
            }
            CheckLobbyReadyStatus();
        }

        private void CheckLobbyReadyStatus()
        {
            var ready = true;
            foreach (var player in _networkManager.LobbyPlayers.Where(player => !player.ReadyStatus))
                ready = false;


            if (ready && _localLobbyPlayer.PlayerIdNumber == 0)
            {
                _startBtnText.color = _startBtnBaseColor;
                _startBtnIcon.color = _startBtnBaseColor;
                _startBtn.Interactable = -_localLobbyPlayer.PlayerIdNumber == 0;
            }
            else
            {
                _startBtnText.color = _multiplyColor;
                _startBtnIcon.color = _multiplyColor;
                _startBtn.Interactable = false;
            }
        }

        #endregion
    }
}