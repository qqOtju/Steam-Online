using System;
using System.Collections;
using InputSystem;
using Mirror;
using Mirror.Experimental;
using Steam.Interfaces;
using Steam.Player.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Steam.Player
{
    [SelectionBase]
    [RequireComponent(typeof(Rigidbody), typeof(Animator))]
    public class GamePlayerController : NetworkBehaviour, IHealth
    {
        [Header("Data")]
        [SerializeField] private PlayerData _data;
        [Header("References")] 
        [SerializeField] private Transform _cameraTarget;
        [SerializeField] private GameObject _meshContainer;
        [SerializeField] private AnimationCurve _movementCurve;
        [SerializeField] private ParticleSystem _landParticle;

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
            _data.PlayerIdNumber.Value = idNumber;
            _data.PlayerName.Value = nickname;
            InputSubscribe();
            _data.OnMenuToggle.Register(OnEscapePerformed);
        }

        public override void OnStartClient()
        {
            _health = new(_data.MaxHealth.Value);
            _health.OnDeath += CmdHandleDeath;
        }

        public override void OnStopAuthority()
        { 
            InputUnsubscribe();
            _data.OnMenuToggle.Unregister(OnEscapePerformed);
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
                _meshContainer, _data.RotationSmoothTime, _movementCurve);
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
            _animator.SetBool(_animGrounded, _data.IsGrounded.Value);
            if(_data.IsGrounded.Value) _landParticle.Play();
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
        
        private void InputUnsubscribe()
        {
            _controls.Player.Move.performed -= SetMovement;
            _controls.Player.Move.canceled -= ResetMovement;
            _controls.Player.Jump.performed -= Jump;
            _controls.Player.Sprint.performed -= SprintChange;
            _controls.Player.Sprint.canceled -= SprintChange;
        }
        
                
        private void InputSubscribe()
        {
            _controls.Player.Move.performed += SetMovement;
            _controls.Player.Move.canceled += ResetMovement;
            _controls.Player.Jump.performed += Jump;
            _controls.Player.Sprint.performed += SprintChange;
            _controls.Player.Sprint.canceled += SprintChange;
        }
        
        #region Client

        [Client]
        private void SetMovement(InputAction.CallbackContext context) =>
            _input = context.ReadValue<Vector2>();

        [Client]
        private void ResetMovement(InputAction.CallbackContext context) =>
            _input = Vector2.zero;

        [Client]
        private void Jump(InputAction.CallbackContext obj) =>
            _movement.Jump(_data.JumpHeight, _data.IsGrounded.Value);

        [Client]
        private void SprintChange(InputAction.CallbackContext obj) =>
            _data.IsSprinting.Value = _controls.Player.Sprint.inProgress;


        [Client]
        private void Move()
        {
            if(_data.IsGrounded.Value)
                _movement.Move(_input, _data.IsSprinting.Value ? _data.SprintSpeed : _data.Speed);
            else
                _movement.Move(_input, _data.IsSprinting.Value ? _data.AirSprintSpeed : _data.AirSpeed);
            _movement.RotateModel(_input);
        }

        [Client]
        private void OnEscapePerformed(bool status)
        {
            if (status)
                InputSubscribe();
            else
                InputUnsubscribe();
        }
        
        #endregion

        #region GroundCheck

        [Client]
        private bool GroundedCheck()
        {
            var position = transform.position;
            var spherePosition = new Vector3(position.x, position.y - _data.GroundedOffset,
                position.z);
            var foo = Physics.CheckSphere(spherePosition, _data.GroundedRadius, _data.GroundLayers,
                QueryTriggerInteraction.Ignore);
            if (foo == _data.IsGrounded.Value) return false;
            _data.IsGrounded.Value = foo;
            return true;
        }

        private void OnDrawGizmos()
        {
            var transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            var transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);
            Gizmos.color = _data.IsGrounded.Value ? transparentGreen : transparentRed;
            var position = transform.position;
            Gizmos.DrawSphere(
                new Vector3(position.x, position.y - _data.GroundedOffset, position.z),
                _data.GroundedRadius);
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
            if(isOwned) _data.CurrentH.Value = newValue;
        }

        #endregion
    }
}