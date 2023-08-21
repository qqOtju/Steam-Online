using InputSystem;
using Mirror;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Steam.Player
{
    public class PushableController : NetworkBehaviour
    {
        [SerializeField] private ParticleSystem _punchParticle;
        [SerializeField] private GameObject _meshContainer;
        [SerializeField] private float _punchPowerX;
        [SerializeField] private float _punchPowerY;
        
        private Controls _controls;
        private Rigidbody _rb;
        
        [ClientCallback]
        private void Awake()
        {
            _controls = new();
            _rb = GetComponent<Rigidbody>();
        }
        
        [ClientCallback]
        private void OnEnable() =>
            _controls.Enable();

        [ClientCallback]
        private void OnDisable() =>
            _controls.Disable();

        public override void OnStartAuthority()
        {
            _controls.Player.Punch.performed += Push;
        }

        public override void OnStopAuthority()
        {
            _controls.Player.Punch.performed -= Push;
        }

        #region Push

        private void Push(InputAction.CallbackContext obj)
        {
            if (!isOwned || !isLocalPlayer) return;
            if (!Physics.Raycast(transform.position, _meshContainer.transform.forward, out var ray, 2f)) return;
            if (!ray.collider.gameObject.TryGetComponent<PushableController>(out var player)) return;
            _punchParticle.Play();
            CmdPushPlayer(player, _meshContainer.transform.forward, _punchPowerX);
        }

        [Command]
        private void CmdPushPlayer(NetworkBehaviour player, Vector3 dir, float power)
        {
            if(isServer) RpcGetPushed(player, dir, power);
            if(isClient) player.GetComponent<PushableController>().GetPushed(dir, power);
        }

        [ClientRpc]
        private void RpcGetPushed(NetworkBehaviour player, Vector3 dir, float power) =>
            player.GetComponent<PushableController>().GetPushed(dir, power);

        private void GetPushed(Vector3 dir, float power) =>
            _rb.AddForce(dir * power + Vector3.up * _punchPowerY, ForceMode.Impulse);

        #endregion
    }
}