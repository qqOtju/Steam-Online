using System.Collections.Generic;
using Mirror;
using MyMirror;
using UnityEngine;

namespace Steam.Scene
{
    public class PlayerSpawnSystem : NetworkBehaviour
    {
        [SerializeField] private List<Transform> _spawnPoint;
        
        private MyNetworkManager _networkManager;
        
        private MyNetworkManager Manager
        {
            get
            {
                if (_networkManager != null) return _networkManager;
                return _networkManager = NetworkManager.singleton as MyNetworkManager;
            }
        }

        public override void OnStartServer() => Manager.OnSceneChange += SpawnPlayers;

        [ServerCallback]
        private void OnDestroy() => Manager.OnSceneChange -= SpawnPlayers;

        [Server]
        private void SpawnPlayers(NetworkConnection conn)
        {
            var players = Manager.GamePlayers;
            for (int i = 0; i < players.Count; i++)
            {
                players[i].transform.position = _spawnPoint[i].position;
                players[i].gameObject.SetActive(true);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0, 1, 0, 0.35f);
            foreach (var point in _spawnPoint)
                Gizmos.DrawSphere(point.position, 0.3f);
        }
    }
}