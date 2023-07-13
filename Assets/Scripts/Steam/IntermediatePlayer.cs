using Mirror;
using MyMirror;
using Steam.UI;
using Steamworks;

namespace Steam
{
    public class IntermediatePlayer : NetworkBehaviour
    {
        [SyncVar] public int connectionId;
        [SyncVar] public int playerIdNumber;
        [SyncVar] public ulong playerSteamId;
        [SyncVar(hook = nameof(PlayerNameUpdate))] public string playerName;
        [SyncVar(hook = nameof(PlayerReadyUpdate))] public bool readyStatus;
        
        private MyNetworkManager _room;
        private MyNetworkManager Room
        {
            get
            {
                if (_room != null) return _room;
                return _room = NetworkManager.singleton as MyNetworkManager;
            }
        } 
        
        public override void OnStartAuthority()
        {
            CmdSetPlayerName(SteamFriends.GetPersonaName());
            gameObject.name = "LocalPlayer";
            UILobbyController.Instance.FindLocalPlayer();
            UILobbyController.Instance.UpdateLobbyName();
        }
        public override void OnStartClient()
        {
            DontDestroyOnLoad(gameObject);
            Room.GamePlayers.Add(this);
            UILobbyController.Instance.UpdateLobbyName();
            UILobbyController.Instance.UpdatePlayerList();
        }


        public override void OnStopClient()
        {
            Room.GamePlayers.Remove(this);
            UILobbyController.Instance.UpdatePlayerList();
        }

        private void PlayerNameUpdate(string oldValue, string newValue)
        {
            if (isServer) this.playerName = newValue;
            if (isClient) UILobbyController.Instance.UpdatePlayerList();
        }

        private void PlayerReadyUpdate(bool oldValue, bool newValue)
        {
            if (isServer) readyStatus = newValue;
            if (isClient) UILobbyController.Instance.UpdatePlayerList();
        }

        public void ChangeReady()
        {
            if(isOwned) CmdSetPlayerReady();
        }
        
        [Command]
        private void CmdSetPlayerName(string playerName) 
            => this.PlayerNameUpdate(this.playerName, playerName);

        [Command]
        private void CmdSetPlayerReady() 
            => this.PlayerReadyUpdate(this.readyStatus, !this.readyStatus);
        
    }
}