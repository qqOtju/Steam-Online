using System.Collections.Generic;
using Mirror;
using MyMirror;
using Steam.Player;
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
            var players = Manager.GamePLayers;
            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                player.transform.position = _spawnPoint[i].position;
                player.gameObject.SetActive(true);
                player.index = i;
                player.OnPlayerDeath += OnOnPlayerDeath;
            }
        }

        [ClientRpc]
        private void OnOnPlayerDeath(NetworkBehaviour obj)
        {
            var index = obj.gameObject.GetComponent<GamePlayerController>().index;
            obj.gameObject.transform.position = _spawnPoint[index].position;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0, 1, 0, 0.35f);
            foreach (var point in _spawnPoint)
                Gizmos.DrawSphere(point.position, 0.3f);
        }
    }
}