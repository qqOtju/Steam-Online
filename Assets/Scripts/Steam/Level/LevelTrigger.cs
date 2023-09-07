using Extensions;
using Mirror;
using MyMirror;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Steam.Level
{
    [SelectionBase]
    [RequireComponent(typeof(Collider))]
    public class LevelTrigger : MonoBehaviour
    {
        [SerializeField] [Scene] private string _nextScene;
        [SerializeField] private IntVariable _playersNum;
        [SerializeField] private bool _leaveLobby;
        [SerializeField] private VoidEvent _onLobbyLeave;
        
        private MyNetworkManager _networkManager;
        private bool _sceneChange;
        private int _stayingPlayers;

        private MyNetworkManager Manager
        {
            get
            {
                if (_networkManager != null) return _networkManager;
                return _networkManager = NetworkManager.singleton as MyNetworkManager;
            }
        }

        [Server]
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            Debug.Log("OnTriggerEnter");
            _stayingPlayers++;
            if (_sceneChange || _stayingPlayers != _playersNum.Value) return;
            _sceneChange = true;
            if(_leaveLobby)
                _onLobbyLeave.Raise();
            else
                Manager.ServerChangeScene(_nextScene.SceneName());
        }

        [Server]
        private void OnTriggerExit(Collider other)
        {
            if(other.CompareTag("Player"))
                _stayingPlayers--;
        }
    }
}