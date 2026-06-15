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

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private bool autoFindAnimator = true;

        [Tooltip("Animator 里的移动参数名")]
        [SerializeField] private string walkBoolName = "Walk";

        [Tooltip("Animator 里的跳跃参数名")]
        [SerializeField] private string jumpTriggerName = "Jump";

        [Tooltip("判定为走路的最小输入阈值")]
        [SerializeField] private float moveAnimThreshold = 0.01f;

        private int _walkHash;
        private int _jumpHash;
        private bool _animatorReady;

        private Vector2 _moveInput;
        private float _lookDeltaX;
        private float _pendingLookDeltaX;
        private Rigidbody _rigidbody;
        [SerializeField] private bool _isGrounded;
        private bool isPaused;

        protected virtual void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            SyncFromOptions();
            CacheAnimator();
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

            if (animator != null && _animatorReady)
            {
                animator.SetBool(_walkHash, false);
            }
        }

        private void CacheAnimator()
        {
            if (autoFindAnimator && animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }

            if (animator != null)
            {
                _walkHash = Animator.StringToHash(walkBoolName);
                _jumpHash = Animator.StringToHash(jumpTriggerName);
                _animatorReady = true;
            }
            else
            {
                _animatorReady = false;
            }
        }

        protected virtual void SetMoveInput(Vector2 input)
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

        protected virtual void FixedUpdate()
        {
            if (isPaused)
            {
                _pendingLookDeltaX = 0f;
                ForceUprightRotation();
                UpdateWalkAnimation(false);
                return;
            }

            if (Mathf.Abs(_pendingLookDeltaX) > 0.0001f && _rigidbody)
            {
                float yRotation = _pendingLookDeltaX * horizontalLookSensitivity * Time.fixedDeltaTime * 0.5f;
                Quaternion deltaRotation = Quaternion.Euler(0, yRotation, 0);
                _rigidbody.MoveRotation(_rigidbody.rotation * deltaRotation);
                _pendingLookDeltaX = 0f;
                ForceUprightRotation();
            }

            _isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundMask);

            if (_moveInput != Vector2.zero && _rigidbody)
            {
                Vector3 move = new Vector3(_moveInput.x, 0, _moveInput.y);
                move = transform.TransformDirection(move) * (moveSpeed * Time.fixedDeltaTime);
                Vector3 targetPosition = _rigidbody.position + move;
                _rigidbody.MovePosition(targetPosition);
            }

            bool isWalking = _isGrounded && _moveInput.sqrMagnitude > moveAnimThreshold;
            UpdateWalkAnimation(isWalking);
        }

        private void UpdateWalkAnimation(bool isWalking)
        {
            if (animator == null || !_animatorReady) return;
            animator.SetBool(_walkHash, isWalking);
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

                if (animator != null && _animatorReady)
                {
                    animator.ResetTrigger(_jumpHash);
                    animator.SetTrigger(_jumpHash);
                }

                UpdateWalkAnimation(false);
                _isGrounded = false;
            }
        }

        private void OnGameplayStatusChanged(EGameplayStatus status)
        {
            isPaused = (status == EGameplayStatus.Paused);
        }
    }
}