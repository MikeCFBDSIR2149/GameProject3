using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    private PlayerInputActions _inputActions;
    private Vector2 _moveInput;

    public event System.Action<Vector2> OnMoveInputChanged;

    private void Awake()
    {
        _inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        _inputActions.Enable();
        _inputActions.Player.Move.performed += OnMove;
        _inputActions.Player.Move.canceled += OnMove;
    }

    private void OnDisable()
    {
        _inputActions.Player.Move.performed -= OnMove;
        _inputActions.Player.Move.canceled -= OnMove;
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
}
