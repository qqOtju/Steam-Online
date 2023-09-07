using Extensions;
using Mirror;
using MyMirror;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Steam.Level
{
    [SelectionBase]
    public class LevelTrigger : MonoBehaviour
    {
        [SerializeField] [Scene] private string _nextScene;
        [SerializeField] private IntVariable _playersNum;

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
            if(other.CompareTag("Player"))
            {
                _stayingPlayers++;
                if (!_sceneChange && _stayingPlayers == _playersNum.Value)
                {
                    _sceneChange = true;
                    Manager.ServerChangeScene(_nextScene.SceneName());
                }
            }
        }

        [Server]
        private void OnTriggerExit(Collider other)
        {
            if(other.CompareTag("Player"))
                _stayingPlayers--;
        }
    }
}