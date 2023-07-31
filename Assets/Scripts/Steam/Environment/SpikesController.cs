using Mirror;
using Steam.Interface;
using Steam.Player;
using UnityEngine;

namespace Steam.Environment
{
    [SelectionBase]
    [ExecuteInEditMode]
    [RequireComponent(typeof(Collider))]
    public class SpikesController : NetworkBehaviour
    {
        [SerializeField] private float _damage = 50f;
        [SerializeField] private GameObject _prefab;
        [SerializeField] private Collider _collider;
        [SerializeField] private bool _upd = false;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.TryGetComponent<GamePlayerController>(out var player))
                CmdDealDamage(player);
        }

        [Command(requiresAuthority = false)]
        private void CmdDealDamage(GamePlayerController player) =>
            DealDamage(player);
        
        [ServerCallback]
        private void DealDamage(GamePlayerController player) =>
            player.GetDamage(_damage);

        [ContextMenu("Set Spikes")]
        private void Upd()
        {
            for (int i = 0; i < transform.childCount; i++)
                DestroyImmediate(transform.GetChild(i).gameObject);
            var size = _collider.bounds.size;
            Debug.Log(size);
            var y = transform.position.y + 0.5f;
            for (int i = -2; i < size.x - 2; i++)
            {
                for (int j = 0; j < size.z; j++)
                {
                    var go = Instantiate(_prefab, new Vector3(i - size.z / 2, y, j - size.x / 2),
                        new Quaternion(0, 0, 0, 0),
                        transform.parent);
                    go.transform.parent = transform;
                }
            }
        }
        
        private void Update()
        {
            if(_upd && !Application.isPlaying) Upd();
        }
    }
}