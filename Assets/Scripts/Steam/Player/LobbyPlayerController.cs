using Mirror;
using MyMirror;
using Steam.UI;
using Steamworks;

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
        private MyNetworkManager _lobby;
        private MyNetworkManager Lobby
        {
            get
            {
                if (_lobby != null) return _lobby;
                return _lobby = NetworkManager.singleton as MyNetworkManager;
            }
        }

        public ulong PlayerSteamId => _playerSteamId;
        public int PlayerIdNumber => _playerIdNumber;
        public int ConnectionId => _connectionId;
        public bool ReadyStatus => _readyStatus;
        public string PlayerName => _playerName;
        
        public void Init(int connId, int idNum, ulong steamId, UILobbyController lobbyController)
        {
            _connectionId = connId;
            _playerIdNumber = idNum;
            _playerSteamId = steamId;
            _lobbyController = lobbyController;
        }
        
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
            CmdSetPlayerName(SteamFriends.GetPersonaName());
            _lobbyController.SetLocalPlayer(this);
            _lobbyController.UpdateLobbyName();
        }

        /// <summary>
        /// Like Start(), but only called on client and host.
        /// Means called for all objects.
        /// Separated from the OnStartAuthority(), because needs to be called on each lobby player. 
        /// </summary>
        public override void OnStartClient()
        {
            if (Lobby.LobbyPlayers.Contains(this)) return;
            //if the new client
            DontDestroyOnLoad(gameObject);
            Lobby.LobbyPlayers.Add(this);
            // UILobbyController.Instance.UpdateLobbyName();
            _lobbyController.UpdatePlayerList();
        }

        public override void OnStopClient()
        {
            Lobby.LobbyPlayers.Remove(this);
            _lobbyController.UpdatePlayerList();
        }

        #endregion

        #region Commands

        [Command]
        private void CmdSetPlayerName(string newName)
            => PlayerNameUpdate(_playerName, newName);

        [Command]
        private void CmdSetPlayerReady()
            => PlayerReadyUpdate(_readyStatus, !_readyStatus);

        #endregion
        
        public void ChangeReady() => 
            CmdSetPlayerReady();

        private void PlayerNameUpdate(string oldValue, string newValue)
        {
            if (isServer) _playerName = newValue;
            if (isClient) _lobbyController.UpdatePlayerList();
        }

        private void PlayerReadyUpdate(bool oldValue, bool newValue)
        {
            if (isServer) _readyStatus = newValue;
            if (isClient) _lobbyController.UpdatePlayerList();
        }

    }
}