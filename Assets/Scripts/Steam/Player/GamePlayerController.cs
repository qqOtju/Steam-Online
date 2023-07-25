using System.Collections;
using InputSystem;
using Mirror;
using Mirror.Experimental;
using Steam.Interface;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Steam.Player
{
    [SelectionBase]
    [RequireComponent(typeof(Rigidbody), typeof(Animator))]
    public class GamePlayerController : NetworkBehaviour, IHealth
    {
        #region Serializefield
        
        [Header("Atom constants")] 
        [SerializeField] private FloatConstant _maxHealth;
        [SerializeField] private FloatConstant _speed;
        [SerializeField] private FloatConstant _sprintSpeed;
        [SerializeField] private FloatConstant _airSpeed;
        [SerializeField] private FloatConstant _airSprintSpeed;
        [SerializeField] private FloatConstant _jumpHeight;
        [SerializeField] private FloatConstant _punchPower;
        [Header("Atom variables")] 
        [SerializeField] private FloatVariable _currentHealth;
        [SerializeField] private BoolVariable _isGrounded;
        [SerializeField] private BoolVariable _isSprinting;
        [Header("Variables")] 
        [SerializeField] private float _rotationSmoothTime = 0.1f;
        [Header("Ground Check")]
        [SerializeField] private float _groundedOffset = 0.14f;
        [SerializeField] private float _groundedRadius = 0.28f;
        [SerializeField] private LayerMask _groundLayers;
        [Header("Other")] 
        [SerializeField] private Transform _cameraTarget;
        [SerializeField] private GameObject _meshContainer;
        [SerializeField] private AnimationCurve _movementCurve;
        
        #endregion

        private readonly int _animGrounded = Animator.StringToHash("Grounded");
        private readonly int _animPunch = Animator.StringToHash("Punch");
        private readonly int _animSpeed = Animator.StringToHash("Speed");

        private readonly WaitForSeconds _wfs = new(0.5f);
        private PlayerMovementController _movement;
        private NetworkAnimator _networkAnimator;
        private PlayerHealthController _health;
        private Controls _controls;
        private Animator _animator;
        private Rigidbody _rb;
        private Vector2 _input;

        public override void OnStartAuthority()
        {
            enabled = true;
            _controls.Player.Move.performed += SetMovement;
            _controls.Player.Move.canceled += ResetMovement;
            _controls.Player.Jump.performed += Jump;
            _controls.Player.Sprint.performed += SprintChange;
            _controls.Player.Sprint.canceled += SprintChange;
            _controls.Player.Punch.performed += Punch;
        }

        public override void OnStopAuthority()
        {
            _controls.Player.Move.performed -= SetMovement;
            _controls.Player.Move.canceled -= ResetMovement;
            _controls.Player.Jump.performed -= Jump;
            _controls.Player.Sprint.performed -= SprintChange;
            _controls.Player.Sprint.canceled -= SprintChange;
        }

        #region ClientCallback

        [ClientCallback]
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            _controls = new();
        }

        [ClientCallback]
        private void Start()
        {
            if (!isOwned) return;
            _rb = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();
            _networkAnimator = GetComponent<NetworkAnimator>();
            _health = new(_maxHealth.Value);
            _currentHealth.Value = _maxHealth.Value;
            _movement = new(_rb, _cameraTarget.gameObject,
                _meshContainer, _rotationSmoothTime, _movementCurve);
            StartCoroutine(WaitReady());
        }

        [ClientCallback]
        private IEnumerator WaitReady()
        {
            while (true)
            {
                if (!NetworkClient.ready) yield return _wfs;
                else
                {
                    GetComponent<NetworkRigidbody>().enabled = true;
                    yield break;
                }
            }
        }

        [ClientCallback]
        private void OnEnable() => _controls.Enable();

        [ClientCallback]
        private void OnDisable() => _controls.Disable();

        [ClientCallback]
        private void Update()
        {
            if (!isOwned || !NetworkClient.ready || !GroundedCheck()) return;
            _animator.SetBool(_animGrounded, _isGrounded.Value);
        }

        [ClientCallback]
        private void FixedUpdate()
        {
            if (!isOwned && !NetworkClient.ready) return;
            if (_input != Vector2.zero)
                Move();
            _animator.SetFloat(_animSpeed, _rb.velocity.magnitude);
        }

        #endregion

        #region Client

        [Client]
        private void SetMovement(InputAction.CallbackContext context) =>
            _input = context.ReadValue<Vector2>();

        [Client]
        private void ResetMovement(InputAction.CallbackContext context) =>
            _input = Vector2.zero;

        [Client]
        private void Jump(InputAction.CallbackContext obj) =>
            _movement.Jump(_jumpHeight.Value, _isGrounded.Value);

        [Client]
        private void SprintChange(InputAction.CallbackContext obj) =>
            _isSprinting.Value = _controls.Player.Sprint.inProgress;


        [Client]
        private void Move() =>
            _movement.MoveUpdate(_input, _speed.Value,
                _sprintSpeed.Value * 2, _airSpeed.Value, _airSprintSpeed.Value * 2, _isGrounded.Value,
                _isSprinting.Value);

        #endregion

        #region GroundCheck

        [Client]
        private bool GroundedCheck()
        {
            var position = transform.position;
            var spherePosition = new Vector3(position.x, position.y - _groundedOffset,
                position.z);
            var foo = Physics.CheckSphere(spherePosition, _groundedRadius, _groundLayers,
                QueryTriggerInteraction.Ignore);
            if (foo == _isGrounded.Value) return false;
            _isGrounded.Value = foo;
            return true;
        }

        private void OnDrawGizmos()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            Gizmos.color = _isGrounded.Value ? transparentGreen : transparentRed;

            var position = transform.position;
            Gizmos.DrawSphere(
                new Vector3(position.x, position.y - _groundedOffset, position.z),
                _groundedRadius);
        }

        #endregion

        #region Punch

        private void Punch(InputAction.CallbackContext obj)
        {
            if (!isOwned || !isLocalPlayer) return;
            _networkAnimator.SetTrigger(_animPunch);
            CmdPushPlayer();
        }

        [Command]
        private void CmdPushPlayer()
        {
            if (Physics.Raycast(transform.position, _meshContainer.transform.forward, out var ray, 2f))
                if (ray.collider.gameObject.TryGetComponent<GamePlayerController>(out var player))
                {
                    if (isServer) RpcGetPunched(player, _meshContainer.transform.forward);
                    if (isClient) Push(player.GetComponent<Rigidbody>(), _meshContainer.transform.forward * 2);
                }
        }

        [ClientRpc]
        private void RpcGetPunched(NetworkBehaviour player, Vector3 dir) =>
            Push(player.GetComponent<Rigidbody>(), dir);

        private void Push(Rigidbody rb, Vector3 dir) =>
            rb.AddForce(dir * _punchPower.Value + Vector3.up * 3, ForceMode.Impulse);

        #endregion

        #region Health

        public void GetDamage(float dmg) =>
            _currentHealth.Value = _health.GetDamage(dmg);

        public void GetHeal(float heal) =>
            _currentHealth.Value = _health.GetHeal(heal);

        [ClientRpc]
        private void HandleDeath() =>
            _controls.Disable();

        #endregion
    }
}