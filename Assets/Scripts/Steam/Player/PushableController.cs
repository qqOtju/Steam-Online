using System;
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
        [SerializeField] private FloatConstant _punchPower;

        private const int YPunchPower = 3;
        
        private Controls _controls;
        private Rigidbody _rb;
        
        [ClientCallback]
        private void Awake()
        {
            Debug.Log("AWAKE");
            _controls = new();
            _rb = GetComponent<Rigidbody>();
        }
        
        [ClientCallback]
        private void OnEnable()
        {
            _controls.Enable();
        }

        [ClientCallback]
        private void OnDisable()
        {
            _controls.Disable();
        }

        public override void OnStartAuthority()
        {
            Debug.Log("OnStartAuthority");
            _controls.Player.Punch.performed += Punch;
        }

        public override void OnStopAuthority()
        {
            _controls.Player.Punch.performed -= Punch;
        }

        #region Punch

        private void Punch(InputAction.CallbackContext obj)
        {
            Debug.Log("Punch");
            if (!isOwned || !isLocalPlayer) return;
            Debug.Log("Punch After");
            if (Physics.Raycast(transform.position, _meshContainer.transform.forward, out var ray, 2f))
                if (ray.collider.gameObject.TryGetComponent<PushableController>(out var player))
                {
                    _punchParticle.Play();
                    CmdPushPlayer(player, _meshContainer.transform.forward, _punchPower.Value);
                    //Push(player, _meshContainer.transform.forward,  _punchPower.Value);
                }
        }

        [Command]
        private void CmdPushPlayer(NetworkBehaviour player, Vector3 dir, float power)
        {
            if(isServer) RpcGetPunched(player, dir, power);
            if(isClient) player.GetComponent<PushableController>().GetPushed(dir, power);
        }

        [ClientRpc]
        private void RpcGetPunched(NetworkBehaviour player, Vector3 dir, float power) =>
            player.GetComponent<PushableController>().GetPushed(dir, power);

        private void Push(NetworkBehaviour player, Vector3 dir, float power) =>
            player.GetComponent<Rigidbody>().AddForce(dir * power + Vector3.up * YPunchPower, ForceMode.Impulse);
        
        public void GetPushed(Vector3 dir, float power)
        {
            _rb.AddForce(dir * power + Vector3.up * power, ForceMode.Impulse);
        }

        #endregion
    }
}