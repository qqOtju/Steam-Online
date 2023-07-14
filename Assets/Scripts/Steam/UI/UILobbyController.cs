using System.Collections.Generic;
using System.Linq;
using GridLayout;
using Mirror;
using MyMirror;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Steam.UI
{
    public class UILobbyController : NetworkBehaviour
    {
        [SerializeField] private TextMeshProUGUI _lobbyNameText;
        [SerializeField] private UIPlayerInfo _playerInfoPrefab;
        [SerializeField] private AbstractGridLayout _layout;
        [SerializeField] private MyNetworkManager _networkManager;
        [SerializeField] private Button _startGameBtn;
        [SerializeField] private TextMeshProUGUI _readyBtnText;
        
        private readonly List<UIPlayerInfo> _playerInfos = new();
        private MyNetworkManager _room;

        public static UILobbyController Instance;
        public IntermediatePlayer _localIntermediatePlayer;
        public bool playerIfnoCreated = false;
        public ulong currentLobbyId;

        private MyNetworkManager Room
        {
            get
            {
                if (_room != null) return _room;
                return _room = NetworkManager.singleton as MyNetworkManager;
            }
        } 
        
        private void Awake()
        {
            _startGameBtn.interactable = false;
            if(Instance == null) Instance = this;
        }

        public void ReadyPlayer() => _localIntermediatePlayer.ChangeReady();

        public void LeaveLobby()
        {
            SteamMatchmaking.LeaveLobby(new CSteamID(currentLobbyId));
            if(isServer) _networkManager.StopHost();
            else _networkManager.StopClient();
            SteamLobby.Instance.LeaveLobby();
        }
        
        public void UpdateLobbyName()
        {
            currentLobbyId = SteamLobby.CurrentLobbyId;
            _lobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(currentLobbyId), "name");
        }
        
        public void FindLocalPlayer() =>
            _localIntermediatePlayer = GameObject.Find("LocalPlayer").GetComponent<IntermediatePlayer>();
        
        public void UpdatePlayerList()
        {
            if(!playerIfnoCreated) CreateHostPlayerItem();
            if(_playerInfos.Count < _networkManager.RoomPlayers.Count) CreateClientPlayerItem();
            if(_playerInfos.Count > _networkManager.RoomPlayers.Count) RemovePlayerItem();
            if(_playerInfos.Count > 0 && _playerInfos.Count == _networkManager.RoomPlayers.Count) UpdatePlayerItem();
         }
        
        private void CreateHostPlayerItem()
        {
            foreach (var player in _networkManager.RoomPlayers)
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
            playerIfnoCreated = true;
        }

        private void CreateClientPlayerItem()
        {
            foreach (var player in _networkManager.RoomPlayers)
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
            foreach (var player in _networkManager.RoomPlayers)
            foreach (var info in _playerInfos)
            {
                if (info.connectionId != player.connectionId) continue;
                info.playerName = player.playerName;
                info.ready = player.readyStatus;
                info.Init();
                if (player == _localIntermediatePlayer)
                    UpdateBtn();
            }
            CheckLobbyReadyStatus();
        }

        private void RemovePlayerItem()
        {
            var playerInfosToRemove = _playerInfos.Where(info =>
                    _networkManager.RoomPlayers.All(player => player.connectionId != info.connectionId)).ToList();
            if (playerInfosToRemove.Count <= 0) return;
            foreach (var info in playerInfosToRemove)
            {
                var objToRemove = info.gameObject;
                _playerInfos.Remove(info);
                Destroy(objToRemove);
            }
        }

        private void UpdateBtn() =>
            _readyBtnText.text = _localIntermediatePlayer.readyStatus ? "<color=red>Not ready</color>" : "<color=green>Ready</color>";

        private void CheckLobbyReadyStatus()
        {
            var ready = true;
            foreach (var player in _networkManager.RoomPlayers.Where(player => !player.readyStatus))
                ready = false;


            if (ready)
                _startGameBtn.interactable = (_localIntermediatePlayer.playerIdNumber == 0);
            else
                _startGameBtn.interactable = false;
        }
    }
}