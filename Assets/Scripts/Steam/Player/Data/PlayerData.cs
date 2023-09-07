using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Steam.Player.Data
{
    [CreateAssetMenu(menuName = "Player/Data", fileName = "Player Data")]
    public class PlayerData : ScriptableObject
    {
        [Header("Atom constants")] 
        [SerializeField] private FloatConstant _maxHealth;
        [Header("Atom variables")] 
        [SerializeField] private FloatVariable _currentH;
        [SerializeField] private BoolVariable _isGrounded;
        [SerializeField] private BoolVariable _isSprinting;
        [SerializeField] private IntVariable _playerIdNumber;
        [SerializeField] private StringVariable _playerName;
        [Header("Events")]
        [SerializeField] private BoolEvent _onMenuToggle;
        [Header("Variables")] 
        [SerializeField] private float _speed = 2;
        [SerializeField] private float _sprintSpeed = 5;
        [SerializeField] private float _airSpeed = 2;
        [SerializeField] private float _airSprintSpeed = 4;
        [SerializeField] private float _jumpHeight = 1.5f;
        [SerializeField] private float _rotationSmoothTime = 0.1f;
        [Header("Ground Check")]
        [SerializeField] private float _groundedOffset = 0.14f;
        [SerializeField] private float _groundedRadius = 0.28f;
        [SerializeField] private LayerMask _groundLayers;

        public FloatConstant MaxHealth => _maxHealth;
        public float Speed => _speed;
        public float SprintSpeed => _sprintSpeed;
        public float AirSpeed => _airSpeed;
        public float AirSprintSpeed => _airSprintSpeed;
        public float JumpHeight => _jumpHeight;
        public FloatVariable CurrentH => _currentH;
        public BoolVariable IsGrounded => _isGrounded;
        public BoolVariable IsSprinting => _isSprinting;
        public IntVariable PlayerIdNumber => _playerIdNumber;
        public StringVariable PlayerName => _playerName;
        public float RotationSmoothTime => _rotationSmoothTime;
        public float GroundedOffset => _groundedOffset;
        public float GroundedRadius => _groundedRadius;
        public LayerMask GroundLayers => _groundLayers;
        public BoolEvent OnMenuToggle => _onMenuToggle;
    }
}