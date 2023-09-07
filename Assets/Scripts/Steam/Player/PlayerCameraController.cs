using Cinemachine;
using InputSystem;
using Mirror;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Steam.Player
{
    
    public class PlayerCameraController : NetworkBehaviour
    {
        [Header("Events")]
        [SerializeField] private BoolEvent _onMenuToggle;
        [Header("Camera values")] 
        [SerializeField] private Vector2 _clamp;
        [SerializeField] private FloatVariable _mouseSensitivity;
        [Header("References")]
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;
        [SerializeField] private Transform _cameraTarget;

        private float _cameraTargetPitch, _cameraTargetYaw;
        private Controls _controls;
        private Vector2 _input;
        
        public override void OnStartAuthority()
        {
            enabled = true;
            _virtualCamera.gameObject.SetActive(true);
            Subscribe();
            _onMenuToggle.Register(OnEscapePerformed);
        }
        
        public override void OnStopAuthority()
        {
            Unsubscribe();
            _onMenuToggle.Unregister(OnEscapePerformed);
        }

        [ClientCallback]
        private void Awake() => _controls = new();
        
        [ClientCallback]
        private void OnEnable() => _controls.Enable();

        [ClientCallback]
        private void OnDisable() => _controls.Disable();

        [ClientCallback]
        private void LateUpdate() => Rotate(_input);

        private void Subscribe()
        {
            _controls.Player.Look.performed += OnLookPerformed;
            _controls.Player.Look.canceled += OnLookPerformed;
        }

        private void Unsubscribe()
        {
            _controls.Player.Look.performed -= OnLookPerformed;
            _controls.Player.Look.canceled -= OnLookPerformed;
        }
        
        private void OnLookPerformed(InputAction.CallbackContext obj) => _input = obj.ReadValue<Vector2>();

        private void OnEscapePerformed(bool status)
        {
            if(status)
                Subscribe();
            else
            {
                _input = Vector2.zero;
                Unsubscribe();
            }
        }
        private void Rotate(Vector2 lookAxis)
        {
            var deltaTimeMultiplier = 1.0f;
            _cameraTargetYaw += lookAxis.x * deltaTimeMultiplier * _mouseSensitivity.Value;
            _cameraTargetPitch += lookAxis.y * deltaTimeMultiplier * _mouseSensitivity.Value;
            _cameraTargetYaw = ClampAngle(_cameraTargetYaw, float.MinValue, float.MaxValue);
            _cameraTargetPitch = ClampAngle(_cameraTargetPitch, _clamp.x, _clamp.y);
            _cameraTarget.rotation = Quaternion.Euler(_cameraTargetPitch,
                _cameraTargetYaw, 0);
        }        
        
        private float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }
    } 
}