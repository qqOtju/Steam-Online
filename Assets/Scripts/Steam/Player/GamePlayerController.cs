using InputSystem;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Steam.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class GamePlayerController : NetworkBehaviour
    {
        [SerializeField] private float _movementSpeed = 4f;
        [SerializeField] private float _jumpHeight = 2f;
        [SerializeField] private Transform _cameraTarget;
        [SerializeField] private GameObject _meshContainer;
        [SerializeField] private float _rotationSmoothTime = 0.1f;
        [SerializeField] private AnimationCurve _movementCurve;

        private PlayerMovement _movement;
        private Controls _controls;
        private Rigidbody _rb;
        private Vector2 _input;
        
        public override void OnStartAuthority()
        {
            enabled = true;
            _controls.Player.Move.performed += SetMovement;
            _controls.Player.Move.canceled += ResetMovement;
            _controls.Player.Jump.performed += Jump;
        }

        public override void OnStopAuthority()
        {
            base.OnStopAuthority();
            _controls.Player.Move.performed -= SetMovement;
            _controls.Player.Move.canceled -= ResetMovement;
        }

        [ClientCallback]
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            _rb = GetComponent<Rigidbody>();
            _controls = new();
            _movement = new(_rb, _cameraTarget.gameObject, 
                _meshContainer, _rotationSmoothTime, _movementCurve);
        }

        [ClientCallback]
        private void OnEnable() => _controls.Enable();

        [ClientCallback]
        private void OnDisable() => _controls.Disable();

        [ClientCallback]
        private void Update() => Move();

        [Client]
        private void SetMovement(InputAction.CallbackContext context) => 
            _input = context.ReadValue<Vector2>();

        [Client]
        private void ResetMovement(InputAction.CallbackContext context) =>
            _input = Vector2.zero;

        [Client]
        private void Move() =>
            _movement.MoveUpdate(_controls, true, _movementSpeed,
                _movementSpeed*2, _movementSpeed, _movementSpeed * 2);

        [Client]
        private void Jump(InputAction.CallbackContext obj) => _movement.Jump(_jumpHeight, true);


    }
}