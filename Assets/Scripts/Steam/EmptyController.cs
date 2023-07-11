using Mirror;
using MyMirror;

namespace Steam
{
    public class EmptyController : NetworkBehaviour
    {
        private MyNetworkManager _networkManager;

        private MyNetworkManager Manager
        {
            get
            {
                if (_networkManager != null) return _networkManager;
                return _networkManager = (MyNetworkManager)NetworkManager.singleton;
            }
        }
        public override void OnStartClient()
        {
            base.OnStartClient();
            Manager.GamePlayers.Add(this);
        }
    }
}