using Cinemachine;
using InputSystem;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Steam.Player
{
    public class PlayerCameraController : NetworkBehaviour
    {
        [Header("Camera values")] 
        [SerializeField] private Vector2 _clamp;
        [SerializeField] private float _mouseSensitivity;
        [Header("Ref")]
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;
        [SerializeField] private Transform _cameraTarget;

        private float _cameraTargetPitch, _cameraTargetYaw;
        private CinemachineTransposer _transposer;
        private Controls _controls;
        
        public override void OnStartAuthority()
        {
            _transposer = _virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
            _virtualCamera.gameObject.SetActive(true);
            enabled = true;
            _controls.Player.Look.performed += OnLookPerformed;
        }

        [ClientCallback]
        private void Awake() => _controls = new();

        /// <summary>
        /// ClientCallback because server dont have input
        /// </summary>
        [ClientCallback]
        private void OnEnable() => _controls.Enable();

        [ClientCallback]
        private void OnDisable() => _controls.Disable();

        private void OnLookPerformed(InputAction.CallbackContext obj) => Rotate(obj.ReadValue<Vector2>());

        private void Rotate(Vector2 lookAxis)
        {
            
            var deltaTimeMultiplier = 1.0f;
            _cameraTargetYaw += lookAxis.x * deltaTimeMultiplier * _mouseSensitivity;
            _cameraTargetPitch += lookAxis.y * deltaTimeMultiplier * _mouseSensitivity;
            _cameraTargetYaw = ClampAngle(_cameraTargetYaw, float.MinValue, float.MaxValue);
            _cameraTargetPitch = ClampAngle(_cameraTargetPitch, _clamp.x, _clamp.y);
            _cameraTarget.rotation = Quaternion.Euler(_cameraTargetPitch,
                _cameraTargetYaw, 0);
            
            /*var deltaTime = Time.deltaTime;
            _transposer.m_FollowOffset.y = Mathf.Clamp(_transposer.m_FollowOffset.y - (lookAxis.y * deltaTime),
                _maxFollowOffset.x, _maxFollowOffset.y);
            _cameraTargetTransform.Rotate(0,lookAxis.x * deltaTime, 0);*/
        }        
        
        private float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }
    } 
}