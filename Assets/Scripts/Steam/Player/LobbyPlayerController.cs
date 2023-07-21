using Mirror;
using MyMirror;
using Steam.UI;
using Steamworks;
using UnityEngine;

namespace Steam.Player
{
    public class LobbyPlayerController : NetworkBehaviour
    {
        [HideInInspector] 
        [SyncVar(hook = nameof(PlayerNameUpdate))] public string playerName;
        [HideInInspector]
        [SyncVar(hook = nameof(PlayerReadyUpdate))] public bool readyStatus;
        [HideInInspector] 
        [SyncVar] public ulong playerSteamId;
        [HideInInspector] 
        [SyncVar] public int playerIdNumber;
        [HideInInspector] 
        [SyncVar] public int connectionId;
        
        private MyNetworkManager _lobby;
        private MyNetworkManager Lobby
        {
            get
            {
                if (_lobby != null) return _lobby;
                return _lobby = NetworkManager.singleton as MyNetworkManager;
            }
        }

        /// <summary>
        /// Like Start(), but only called for objects the client has authority over.
        /// Means called only on the local player.
        /// </summary>
        public override void OnStartAuthority()
        {
            CmdSetPlayerName(SteamFriends.GetPersonaName());
            UILobbyController.Instance.SetLocalPlayer(this);
            UILobbyController.Instance.UpdateLobbyName();
        }
        
        
        public override void OnStartClient()
        {
            if(Lobby.LobbyPlayers.Contains(this)) return;
            DontDestroyOnLoad(gameObject);
            Lobby.LobbyPlayers.Add(this);
            UILobbyController.Instance.UpdateLobbyName();
            UILobbyController.Instance.UpdatePlayerList();
        }

        public override void OnStopClient()
        {
            Lobby.LobbyPlayers.Remove(this);
            UILobbyController.Instance.UpdatePlayerList();
        }

        private void PlayerNameUpdate(string oldValue, string newValue)
        {
            if (isServer) playerName = newValue;
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
        private void CmdSetPlayerName(string newName) 
            => PlayerNameUpdate(playerName, newName);

        [Command]
        private void CmdSetPlayerReady() 
            => PlayerReadyUpdate(readyStatus, !readyStatus);
        
    }
}