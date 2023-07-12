using System.Linq;
using CleanMirror.UI;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CleanMirror
{
    public class NetworkRoomPlayerController : NetworkBehaviour
    {
        [Header("UI")] 
        [SerializeField] private GameObject _lobbyUI;
        [SerializeField] private TMP_Text[] _playerNameTexts;
        [SerializeField] private TMP_Text[] _playerReadyTexts;
        [SerializeField] private Button _startGameButton;
        
        [SyncVar(hook = nameof(HandleDisplayNameChanged))]
        public string _displayName = "Loading...";

        [SyncVar(hook = nameof(HandleReadyStatusChanged))]
        public bool _isReady;

        private bool _isLeader;
        public bool IsLeader
        {
            set
            {
                _isLeader = value;
                _startGameButton.gameObject.SetActive(value);
            }
        }

        private NetworkManagerLobby _room;
        private NetworkManagerLobby Room
        {
            get
            {
                if (_room != null) return _room;
                return _room = NetworkManager.singleton as NetworkManagerLobby;
            }
        }

        #region NetworkCallbacks

        public override void OnStartAuthority()
        {
            CmdSetDisplayName(PlayerNameInputController.DisplayName);
            _lobbyUI.SetActive(true);
        }

        public override void OnStartClient()
        {
            Room.RoomPlayers.Add(this);
            Debug.Log($"{Room.RoomPlayers.Count}");
            var log = "";
            foreach (var player  in Room.RoomPlayers)
            {
                log += player._displayName + "|" + player._isReady + "\n";
            }
            Debug.Log(log);
            UpdateDisplay();
        }

        public override void OnStopClient()
        {
            Room.RoomPlayers.Remove(this);
            UpdateDisplay();
        }

        #endregion

        #region Hooks

        public void HandleReadyStatusChanged(bool oldValue, bool newValue) => UpdateDisplay();
        public void HandleDisplayNameChanged(string oldValue, string newValue) => UpdateDisplay();

        #endregion

        private void UpdateDisplay()
        {
            if (!isOwned)
            {
                foreach (var player in Room.RoomPlayers.Where(player => player.isOwned))
                {
                    player.UpdateDisplay();
                    break;
                }

                return;
            }

            for (int i = 0; i < _playerNameTexts.Length; i++)
            {
                _playerNameTexts[i].text = "Waiting For Player...";
                _playerReadyTexts[i].text = string.Empty;
            }

            for (int i = 0; i < Room.RoomPlayers.Count; i++)
            {
                _playerNameTexts[i].text = Room.RoomPlayers[i]._displayName;
                _playerReadyTexts[i].text = Room.RoomPlayers[i]._isReady
                    ? "<color=green>Ready</color>"
                    : "<color=red>Not Ready</color>";
            }
        }

        public void HandleReadyToStart(bool readyToStart)
        {
            if (!_isLeader)
                return;
            _startGameButton.interactable = readyToStart;
        }

        #region Commands

        [Command]
        private void CmdSetDisplayName(string displayName) => _displayName = displayName;

        [Command]
        public void CmdReadyUp()
        {
            _isReady = !_isReady;
            Room.NotifyPlayersOfReadyState();
        }
       
        [Command]
        public void CmdStartGame()
        {
            if (Room.RoomPlayers[0].connectionToClient != connectionToClient)
                return;
            Room.StartGame();
        }

        #endregion


    }
}