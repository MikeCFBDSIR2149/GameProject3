using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    private PlayerInputActions _inputActions;
    private Vector2 _moveInput;
    private Vector2 _lookInput;

    public event System.Action<Vector2> OnMoveInputChanged;
    public event System.Action<Vector2> OnLookInputChanged;
    public event System.Action OnJumpInputChanged;

    private void Awake()
    {
        _inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        _inputActions.Enable();
        _inputActions.Player.Move.performed += OnMove;
        _inputActions.Player.Move.canceled += OnMove;
        _inputActions.Player.Look.performed += OnLook;
        _inputActions.Player.Look.canceled += OnLook;
        _inputActions.Player.Jump.performed += OnJump;
    }

    private void OnDisable()
    {
        _inputActions.Player.Move.performed -= OnMove;
        _inputActions.Player.Move.canceled -= OnMove;
        _inputActions.Player.Look.performed -= OnLook;
        _inputActions.Player.Look.canceled -= OnLook;
        _inputActions.Player.Jump.performed -= OnJump;
        _inputActions.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                _moveInput = context.ReadValue<Vector2>();
                break;
            case InputActionPhase.Disabled:
            case InputActionPhase.Waiting:
            case InputActionPhase.Started:
            case InputActionPhase.Canceled:
            default:
                _moveInput = Vector2.zero;
                break;
        }
        OnMoveInputChanged?.Invoke(_moveInput);
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                _lookInput = context.ReadValue<Vector2>();
                break;
            case InputActionPhase.Disabled:
            case InputActionPhase.Waiting:
            case InputActionPhase.Started:
            case InputActionPhase.Canceled:
            default:
                _lookInput = Vector2.zero;
                break;
        }
        OnLookInputChanged?.Invoke(_lookInput);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnJumpInputChanged?.Invoke();
        }
    }
}
