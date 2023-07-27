using Mirror;
using MyMirror;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Steam.Level
{
    [SelectionBase]
    [RequireComponent(typeof(Collider))]
    public class LevelTrigger : MonoBehaviour
    {
        [SerializeField] [Scene] private string _nextScene;

        private MyNetworkManager _networkManager;
        private bool _sceneChange;

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
            if (!_sceneChange && other.CompareTag("Player"))
            {
                _sceneChange = true;
                Manager.ServerChangeScene(_nextScene.SceneName());    
            }
        }
    }
}