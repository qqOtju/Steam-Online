using InputSystem;
using UnityEngine;

namespace Steam.Player
{
    public class PlayerMovement
    {
        private readonly AnimationCurve _movementCurve;
        private readonly Transform _targetTransform;
        private readonly float _rotationSmoothTime;
        private readonly Transform _meshTransform;
        private readonly Rigidbody _rb;
        
        private float _rotationVelocity;
        private float _movementValue;
        
        public PlayerMovement(Rigidbody rb ,GameObject cameraTarget, 
            GameObject meshContainer, float rotationSmoothTime, AnimationCurve movementCurve)
        {
            _rb = rb;
            _movementCurve = movementCurve;
            _targetTransform = cameraTarget.transform;
            _meshTransform = meshContainer.transform;
            _rotationSmoothTime = rotationSmoothTime;
        }

        public void Jump(float jumpHeight, bool isGrounded)
        {
            if(!isGrounded) return;
            var velocity = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            var vel = _rb.velocity;
            _rb.velocity = new Vector3(vel.x, velocity, vel.z);
        }

        /// <summary>
        /// Function that is called in the Update method of the PlayerController
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="isGrounded"></param>
        /// <param name="speed"></param>
        /// <param name="sprintSpeed"></param>
        /// <param name="airSpeed"></param>
        /// <param name="airSprintSpeed"></param>
        public void MoveUpdate(Controls controls, bool isGrounded, float speed, float sprintSpeed, float airSpeed, float airSprintSpeed)
        {
            var cntrls = controls.Player;
            var inputValue = cntrls.Move.ReadValue<Vector2>();
            Move(inputValue, isGrounded, false, speed, sprintSpeed, airSpeed, airSprintSpeed);
            RotateModel(inputValue);
        }

        /// <summary>
        /// Moves the player based on the input value
        /// </summary>
        /// <param name="inputValue"></param>
        /// <param name="isGrounded"></param>
        /// <param name="isSprinting"></param>
        /// <param name="speed"></param>
        /// <param name="sprintSpeed"></param>
        /// <param name="airSpeed"></param>
        /// <param name="airSprintSpeed"></param>
        private void Move(Vector2 inputValue, bool isGrounded, bool isSprinting, float speed, float sprintSpeed,
            float airSpeed, float airSprintSpeed)
        {
            if (inputValue == Vector2.zero)
            {
                _movementValue = 0;
                return;
            }

            var forceDir = inputValue.y * GetCameraForward(_targetTransform);
            forceDir += inputValue.x * GetCameraRight(_targetTransform);
            _movementValue += Time.deltaTime;
            Vector3 velocity;
            if (isGrounded)
                velocity = forceDir * ((isSprinting ? sprintSpeed : speed) * _movementCurve.Evaluate(_movementValue));
            else
                velocity = forceDir * ((isSprinting ? airSprintSpeed : airSpeed) * _movementCurve.Evaluate(_movementValue));

            velocity.y = _rb.velocity.y;
            
            _rb.velocity = velocity;
        }

        /// <summary>
        /// Returns the forward direction of the camera
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private Vector3 GetCameraForward(Transform target)
        {
            var forward = target.forward;
            forward.y = 0;
            return forward.normalized;
        }
        
        /// <summary>
        /// Returns the right direction of the camera
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private Vector3 GetCameraRight(Transform target)
        {
            var right = target.right;
            right.y = 0;
            return right.normalized;
        }
        
        /// <summary>
        /// Smoothly rotates the model to face the direction of movement
        /// </summary>
        /// <param name="inputValue"></param>
        private void RotateModel(Vector2 inputValue)
        {
            if(inputValue == Vector2.zero) return;
            var inputDirection = new Vector3(inputValue.x, 0, inputValue.y).normalized;
            var targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _targetTransform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(_meshTransform.eulerAngles.y, targetRotation, ref _rotationVelocity,
                _rotationSmoothTime);
            _meshTransform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }
    }
}