using InputSystem;
using Mirror;
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
        [Header("Atom constants")]
        [SerializeField] private FloatConstant _maxHealth;
        [SerializeField] private FloatConstant _speed;
        [SerializeField] private FloatConstant _sprintSpeed;
        [SerializeField] private FloatConstant _airSpeed;
        [SerializeField] private FloatConstant _airSprintSpeed;
        [SerializeField] private FloatConstant _jumpHeight;
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

        private readonly int _animGroundedParam = Animator.StringToHash("Grounded");
        private readonly int _animSpeedParam = Animator.StringToHash("Speed");

        private PlayerMovementController _movement;
        private PlayerHealthController _health;
        private Transform _transform;
        private Animator _animator;
        private Controls _controls;
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
            _rb = GetComponent<Rigidbody>();
            _controls = new();
        }

        [ClientCallback]
        private void Start()
        {
            _transform = transform;
            _animator = GetComponent<Animator>();
            _movement = new(_rb, _cameraTarget.gameObject, 
                _meshContainer, _rotationSmoothTime, _movementCurve);
            _health = new(_maxHealth.Value);
            _currentHealth.Value = _maxHealth.Value;
            _health.OnDeath += () => _controls.Disable();
        }

        [ClientCallback]
        private void OnEnable() => _controls.Enable();

        [ClientCallback]
        private void OnDisable() => _controls.Disable();

        [ClientCallback]
        private void Update()
        {
            if(GroundedCheck())
                _animator.SetBool(_animGroundedParam, _isGrounded.Value);
        }

        [ClientCallback]
        private void FixedUpdate()
        {
            if(_input != Vector2.zero) 
                Move();
            _animator.SetFloat(_animSpeedParam, _rb.velocity.magnitude);
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
                _sprintSpeed.Value * 2, _airSpeed.Value, _airSprintSpeed.Value * 2, _isGrounded.Value,  _isSprinting.Value);
        
        #endregion
        
        #region GroundCheck

        [Client]
        private bool GroundedCheck()
        {
            var position = _transform.position;
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

        public void GetDamage(float dmg) => _currentHealth.Value = _health.GetDamage(dmg);

        public void GetHeal(float heal) => _currentHealth.Value = _health.GetHeal(heal);

        #endregion
    }
}