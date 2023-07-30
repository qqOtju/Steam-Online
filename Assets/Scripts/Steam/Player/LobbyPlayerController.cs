using Mirror;
using MyMirror;
using Steam.UI;
using Steamworks;
using UnityEngine;

namespace Steam.Player
{
    public class LobbyPlayerController : NetworkBehaviour
    {
        [SyncVar(hook = nameof(PlayerNameUpdate))]
        private string _playerName;
        [SyncVar(hook = nameof(PlayerReadyUpdate))]
        private bool _readyStatus;
        [SyncVar] private ulong _playerSteamId;
        [SyncVar] private int _playerIdNumber;
        [SyncVar] private int _connectionId;
        
        private UILobbyController _lobbyController;
        private UILobbyController LobbyController
        {
            get
            {
                if (_lobbyController != null) return _lobbyController;
                return _lobbyController = Room.LobbyController;
            }
        }
        
        private MyNetworkManager _room;
        private MyNetworkManager Room
        {
            get
            {
                if (_room != null) return _room;
                return _room = NetworkManager.singleton as MyNetworkManager;
            }
        }

        public ulong PlayerSteamId => _playerSteamId;
        public int PlayerIdNumber => _playerIdNumber;
        public int ConnectionId => _connectionId;
        public bool ReadyStatus => _readyStatus;
        public string PlayerName => _playerName;

        #region NetworkCallbacks

        /*
        * Когда я создаю лобби создается новый экзепляр этого класса (объекта) и посколько я имею над этим объектом власть (authority),
        * то у меня вызывается метод OnStartAuthority() и OnStartClient(), но когда ко мне подключается игрок, например Данил, то создается
        * экзепляр класса (объекта) над которым я не имею власти (authority) и у него вызывается только OnStartClient().
        *
        * Когда создается класс (объект) над которым у меня есть власть он отправляет команду CmdSetPlayerName() на сервер, эта команда вызывает
        * метод PlayerNameUpdate() в котором есть проверка на то этот метод происходит на сервере или на клиенте, если на сервере то переменной
        * _playerName устанавливается новое значение, а если на клиенте то обновляется интерфейс лобби. Не стоит забывать, что при смене значения
        * переменной _playerName срабатывает хук с тем же методом PlayerNameUpdate(), что означает, что при отправке команды CmdSetPlayerName() метод
        * PlayerNameUpdate() сработает 2 раза 1 раз на сервера другой раз на клиенте. Важно понимать, чтобы сработал хук на SyncVar переменная должна поменять
        * ИМЕННО НА СЕРВЕРЕ.
        */

        /// <summary>
        /// Like Start(), but only called for objects the client has authority over.
        /// Means called only on the local player.
        /// </summary>
        public override void OnStartAuthority()
        {
            CmdSetPlayerName(_playerSteamId == 0 ? "Oleg" : SteamFriends.GetPersonaName());
            gameObject.name = "LocalPlayer";
            LobbyController.SetLocalPlayer(this);
        }

        /// <summary>
        /// Like Start(), but only called on client and host.
        /// Means called for all objects.
        /// Separated from the OnStartAuthority(), because needs to be called on each lobby player. 
        /// </summary>
        public override void OnStartClient()
        {
            if (Room.LobbyPlayers.Contains(this)) return;
            Debug.Log("OnStartClient");
            DontDestroyOnLoad(gameObject);
            Room.LobbyPlayers.Add(this);
            LobbyController.UpdatePlayerList();
        }


        public override void OnStopClient()
        {
            Room.LobbyPlayers.Remove(this);
            LobbyController.UpdatePlayerList();
        }

        #endregion
        
        #region Commands

        [Command]
        private void CmdSetPlayerName(string playerName)
            => PlayerNameUpdate(_playerName, playerName);

        [Command]
        private void CmdSetPlayerReady()
            => PlayerReadyUpdate(_readyStatus, !_readyStatus);

        #endregion

        public void Init(int connId, int idNum, ulong steamId)
        {
            _connectionId = connId;
            _playerIdNumber = idNum;
            _playerSteamId = steamId;
        }

        public void ChangeReady()
        {
            if (isOwned) CmdSetPlayerReady();
        }

        private void PlayerNameUpdate(string oldValue, string newValue)
        {
            if (isServer) _playerName = newValue;
            if (isClient) LobbyController.UpdatePlayerList();
        }

        private void PlayerReadyUpdate(bool oldValue, bool newValue)
        {
            if (isServer) _readyStatus = newValue;
            if (isClient) LobbyController.UpdatePlayerList();
        }
    }
}