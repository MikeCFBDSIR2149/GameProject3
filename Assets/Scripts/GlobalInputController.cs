using UnityEngine;
using UnityEngine.InputSystem;
using UserOptions;

public class GlobalInputController : MonoSingleton<GlobalInputController>
{
    private PlayerInputActions _inputActions;
    public event System.Action OnCancelInputChanged;

    protected override void Awake()
    {
        base.Awake();
        _inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        _inputActions.Enable();
        _inputActions.UI.Cancel.performed += OnCancel;
    }

    private void OnDisable()
    {
        _inputActions.UI.Cancel.performed -= OnCancel;
        _inputActions.Disable();
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnCancelInputChanged?.Invoke();
        }
    }
}

