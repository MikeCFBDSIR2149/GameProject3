using UnityEngine;
using UserOptions;

namespace Player
{
    public class PlayerMotor : MonoBehaviour, ISyncFromOptions
    {
        public InputController inputController;
        public float moveSpeed = 5f;
        public float horizontalLookSensitivity;
        public float jumpForce = 5f;
        public LayerMask groundMask;
        public float groundCheckDistance;

        private Vector2 _moveInput;
        private float _lookDeltaX;
        private float _pendingLookDeltaX;
        private Rigidbody _rigidbody;
        [SerializeField] private bool _isGrounded;
        private bool isPaused;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            SyncFromOptions();
        }

        private void OnEnable()
        {
            if (inputController != null)
            {
                inputController.OnMoveInputChanged += SetMoveInput;
                inputController.OnLookInputChanged += SetLookInput;
                inputController.OnJumpInputChanged += OnJumpInput;
            }
            if (OptionsManager.Instance != null)
            {
                OptionsManager.Instance.OnOptionsChanged += SyncFromOptions;
            }
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnStatusChanged += OnGameplayStatusChanged;
                // 初始化 isPaused 状态
                isPaused = (GameplayManager.Instance.Status == EGameplayStatus.Paused);
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
            if (OptionsManager.Instance != null)
            {
                OptionsManager.Instance.OnOptionsChanged -= SyncFromOptions;
            }
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnStatusChanged -= OnGameplayStatusChanged;
            }
        }

        private void SetMoveInput(Vector2 input)
        {
            _moveInput = input;
        }

        private void SetLookInput(Vector2 lookDelta)
        {
            _pendingLookDeltaX += lookDelta.x;
        }

        public void SyncFromOptions()
        {
            OptionsManager optionsMgr = OptionsManager.Instance;
            if (!optionsMgr) return;
            OptionsData options = optionsMgr.GetOptions();
            if (options != null)
            {
                horizontalLookSensitivity = options.horizontalSensitivity;
            }
        }

        private void FixedUpdate()
        {
            // 暂停时不旋转且清空 lookDelta，防止累积
            if (isPaused)
            {
                _pendingLookDeltaX = 0f;
                ForceUprightRotation();
                return;
            }

            // 旋转（刚体模式，消耗_pendingLookDeltaX）
            if (Mathf.Abs(_pendingLookDeltaX) > 0.0001f && _rigidbody)
            {
                float yRotation = _pendingLookDeltaX * horizontalLookSensitivity * Time.fixedDeltaTime * 0.5f;
                Quaternion deltaRotation = Quaternion.Euler(0, yRotation, 0);
                _rigidbody.MoveRotation(_rigidbody.rotation * deltaRotation);
                _pendingLookDeltaX = 0f;
                ForceUprightRotation();
            }

            // 检查是否在地面
            _isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundMask);
            
            // 移动逻辑
            if (_moveInput != Vector2.zero && _rigidbody)
            {
                Vector3 move = new Vector3(_moveInput.x, 0, _moveInput.y);
                move = transform.TransformDirection(move) * (moveSpeed * Time.fixedDeltaTime);
                Vector3 targetPosition = _rigidbody.position + move;
                _rigidbody.MovePosition(targetPosition);
            }
        }

        private void ForceUprightRotation()
        {
            if (!_rigidbody) return;

            Vector3 euler = _rigidbody.rotation.eulerAngles;
            _rigidbody.MoveRotation(Quaternion.Euler(0f, euler.y, 0f));
        }


        private void OnJumpInput()
        {
            if (_isGrounded && _rigidbody)
            {
                _rigidbody.linearVelocity = new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z);
                _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            }
        }

        private void OnGameplayStatusChanged(EGameplayStatus status)
        {
            isPaused = (status == EGameplayStatus.Paused);
        }
    }
}
