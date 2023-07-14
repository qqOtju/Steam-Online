using InputSystem;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Steam
{
    [RequireComponent(typeof(Rigidbody))]
    public class GamePlayerController : NetworkBehaviour
    {
        [SerializeField] private float _movementSpeed = 4f;

        private Rigidbody _rb;
        private Controls _controls;
        private Vector2 _input;
        private bool _enabled;
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            _controls = new();
        }

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public override void OnStartAuthority()
        {
            _enabled = true;
            _controls.Player.Move.performed += SetMovement;
            _controls.Player.Move.canceled += ResetMovement;
        }

 
        public override void OnStopAuthority()
        {
            base.OnStopAuthority();
            _controls.Player.Move.performed -= SetMovement;
            _controls.Player.Move.canceled -= ResetMovement;
        }

        [ClientCallback]
        private void OnEnable() => _controls.Enable();

        [ClientCallback]
        private void OnDisable() => _controls.Disable();

        [ClientCallback]
        private void Update() => Move();

        [Client]
        private void SetMovement(InputAction.CallbackContext context) => _input = context.ReadValue<Vector2>();

        [Client]
        private void ResetMovement(InputAction.CallbackContext context) => _input = Vector2.zero;

        [Client]
        private void Move()
        {
            _rb.velocity = _input * _movementSpeed;
        }
    }
}