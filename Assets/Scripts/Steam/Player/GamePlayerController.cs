using System;
using System.Collections;
using InputSystem;
using Mirror;
using Mirror.Experimental;
using Steam.Interfaces;
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
        [Header("Atom variables")] 
        [SerializeField] private FloatVariable _currentH;
        [SerializeField] private BoolVariable _isGrounded;
        [SerializeField] private BoolVariable _isSprinting;
        [SerializeField] private IntVariable _playerIdNumber;
        [SerializeField] private StringVariable _playerName;
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
        [SerializeField] private ParticleSystem _landParticle;

        #endregion

        [HideInInspector] [SyncVar] public int idNumber;
        [HideInInspector] [SyncVar] public string nickname;
        [SyncVar(hook = nameof(UpdateHealthValue))] 
        private float _syncHealth;

        private readonly int _animGrounded = Animator.StringToHash("Grounded");
        private readonly int _animSpeed = Animator.StringToHash("Speed");

        private readonly WaitForSeconds _wfs = new(0.5f);
        private PlayerMovementController _movement;
        private PlayerHealthController _health;
        private Controls _controls;
        private Animator _animator;
        private Rigidbody _rb;
        private Vector2 _input;
        
        public event Action<NetworkBehaviour> OnPlayerDeath;
        [HideInInspector] public int index;

        public void Init(int id, string nickname)
        {
            idNumber = id;
            this.nickname = nickname;
        }

        #region Network
        
        public override void OnStartAuthority()
        {
            enabled = true;
            _playerIdNumber.Value = idNumber;
            _playerName.Value = nickname;
            _controls.Player.Move.performed += SetMovement;
            _controls.Player.Move.canceled += ResetMovement;
            _controls.Player.Jump.performed += Jump;
            _controls.Player.Sprint.performed += SprintChange;
            _controls.Player.Sprint.canceled += SprintChange;
        }

        public override void OnStartClient()
        {
            _health = new(_maxHealth.Value);
            _health.OnDeath += CmdHandleDeath;
        }

        public override void OnStopAuthority()
        {
            _controls.Player.Move.performed -= SetMovement;
            _controls.Player.Move.canceled -= ResetMovement;
            _controls.Player.Jump.performed -= Jump;
            _controls.Player.Sprint.performed -= SprintChange;
            _controls.Player.Sprint.canceled -= SprintChange;
        }
        
        #endregion

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
            _rb = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();
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
            if(_isGrounded.Value) _landParticle.Play();
        }

        [ClientCallback]
        private void FixedUpdate()
        {
            if (!isOwned && !NetworkClient.ready) return;
            Move();
            if (_input == Vector2.zero)
                _animator.SetFloat(_animSpeed, 0);
            else
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
        private void Move()
        {
            if(_isGrounded.Value)
                _movement.Move(_input, _isSprinting.Value ? _sprintSpeed.Value : _speed.Value);
            else
                _movement.Move(_input, _isSprinting.Value ? _airSprintSpeed.Value : _airSpeed.Value);
            _movement.RotateModel(_input);
        }
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

        #region Health

        public void GetDamage(float dmg) =>
            _syncHealth = _health.GetDamage(dmg);

        public void GetHeal(float heal) =>
           _syncHealth = _health.GetHeal(heal);

        [Command(requiresAuthority = false)]
        private void CmdHandleDeath()
        {
            OnPlayerDeath?.Invoke(this);
            if (isServer)
                _health.GetHeal(50);
        }

        private void UpdateHealthValue(float oldValue, float newValue) 
        { 
            if(isOwned) _currentH.Value = newValue;
        }

        #endregion
    }
}