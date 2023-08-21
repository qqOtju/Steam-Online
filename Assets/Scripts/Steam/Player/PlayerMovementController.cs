using UnityEngine;

namespace Steam.Player
{
    public class PlayerMovementController
    {
        private readonly AnimationCurve _movementCurve;
        private readonly Transform _targetTransform;
        private readonly float _rotationSmoothTime;
        private readonly Transform _meshTransform;
        private readonly Rigidbody _rb;
        
        private float _rotationVelocity;
        private float _movementValue;
        
        public PlayerMovementController(Rigidbody rb ,GameObject cameraTarget, 
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

        public void Move(Vector2 inputValue, float speed)
        {
            var forceDir = inputValue.y * GetCameraForward(_targetTransform);
            forceDir += inputValue.x * GetCameraRight(_targetTransform);
            if (inputValue == Vector2.zero) _movementValue = 0;
            else _movementValue += Time.deltaTime;
            var velocity = forceDir * (speed * _movementCurve.Evaluate(_movementValue));
            velocity.y = _rb.velocity.y;
            _rb.velocity = velocity;
        }

        #region Camera

        /// <summary>
        /// Returns the forward direction of the camera
        /// </summary>
        private Vector3 GetCameraForward(Transform target)
        {
            var forward = target.forward;
            forward.y = 0;
            return forward.normalized;
        }
        
        /// <summary>
        /// Returns the right direction of the camera
        /// </summary>
        private Vector3 GetCameraRight(Transform target)
        {
            var right = target.right;
            right.y = 0;
            return right.normalized;
        }        

        #endregion
        
        /// <summary>
        /// Smoothly rotates the model to face the direction of movement
        /// </summary>
        public void RotateModel(Vector2 inputValue)
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