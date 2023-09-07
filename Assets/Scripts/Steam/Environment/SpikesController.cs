using Mirror;
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
        private void SetSpikes()
        {
            for (int i = 0; i < transform.childCount; i++)
                DestroyImmediate(transform.GetChild(i).gameObject, true);
            var size = _collider.bounds.size;
            var y = transform.position.y + 0.5f;
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.z; j++)
                {
                    var go = Instantiate(_prefab, new Vector3(i - size.x / 2, y, j - size.z / 2),
                        new Quaternion(0, 0, 0, 0),
                        transform.parent);
                    go.transform.parent = transform;
                }
            }
        }
    }
}