using System;
using UnityEngine;

namespace Player
{
    public class PlayerMotor : MonoBehaviour
    {
        public InputController inputController;
        public float moveSpeed = 5f;
        public float horizontalLookSensitivity;
        public float jumpForce = 5f;
        public LayerMask groundMask;
        public float groundCheckDistance;

        private Vector2 _moveInput;
        private float _lookDeltaX;
        private Rigidbody _rigidbody;
        [SerializeField] private bool _isGrounded;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            if (inputController != null)
            {
                inputController.OnMoveInputChanged += SetMoveInput;
                inputController.OnLookInputChanged += SetLookInput;
                inputController.OnJumpInputChanged += OnJumpInput;
            }
        }

        private void OnDisable()
        {
            if (inputController != null)
            {
                inputController.OnMoveInputChanged -= SetMoveInput;
                inputController.OnLookInputChanged -= SetLookInput;
                inputController.OnJumpInputChanged -= OnJumpInput;
            }
        }

        private void SetMoveInput(Vector2 input)
        {
            _moveInput = input;
        }

        private void SetLookInput(Vector2 lookDelta)
        {
            _lookDeltaX = lookDelta.x;
        }

        private void Update()
        {
            // 水平旋转
            if (Mathf.Abs(_lookDeltaX) > 0.0001f)
            {
                transform.Rotate(0, _lookDeltaX * horizontalLookSensitivity * Time.unscaledDeltaTime, 0, Space.World);
            }
            // 移动（用 unscaledDeltaTime，保证子弹时间下流畅）
            if (_moveInput != Vector2.zero && _rigidbody)
            {
                Vector3 move = new Vector3(_moveInput.x, 0, _moveInput.y);
                move = transform.TransformDirection(move) * (moveSpeed * Time.deltaTime);
                Vector3 targetPosition = _rigidbody.position + move;
                _rigidbody.MovePosition(targetPosition);
            }
        }

        private void FixedUpdate()
        {
            // 检查是否在地面
            _isGrounded = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundCheckDistance, groundMask);
            // 移动逻辑已移至 Update
        }

        private void OnJumpInput()
        {
            if (_isGrounded && _rigidbody)
            {
                _rigidbody.linearVelocity = new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z);
                _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            }
        }
    }
}
