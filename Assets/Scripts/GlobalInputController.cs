using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UserOptions;

public class GlobalInputController : MonoSingleton<GlobalInputController>
{
    private PlayerInputActions _inputActions;
    public event System.Action OnCancelInputChanged;
    public event System.Action OnSubmitInputChanged;
    public event System.Action OnReservedKeyInputChanged;

    private readonly List<InputController> _registeredInputControllers = new List<InputController>();
    private bool _inputControllersDisabled = false;

    protected override void Awake()
    {
        base.Awake();
        _inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        _inputActions.Enable();
        _inputActions.UI.Cancel.performed += OnCancel;
        _inputActions.UI.Submit.performed += OnSubmit;
        _inputActions.UI.ReservedKey.performed += OnReservedKey;
    }

    private void OnDisable()
    {
        _inputActions.UI.Cancel.performed -= OnCancel;
        _inputActions.UI.Submit.performed -= OnSubmit;
        _inputActions.UI.ReservedKey.performed -= OnReservedKey;
        _inputActions.Disable();
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnCancelInputChanged?.Invoke();
        }
    }

    private void OnSubmit(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnSubmitInputChanged?.Invoke();
        }
    }

    private void OnReservedKey(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnReservedKeyInputChanged?.Invoke();
        }
    }

    /// <summary>
    /// Register an InputController to be managed by this GlobalInputController.
    /// </summary>
    public void RegisterInputController(InputController inputController)
    {
        if (inputController != null && !_registeredInputControllers.Contains(inputController))
        {
            _registeredInputControllers.Add(inputController);
            if (_inputControllersDisabled)
            {
                inputController.SetEnabled(false);
            }
        }
    }

    /// <summary>
    /// Unregister an InputController from being managed by this GlobalInputController.
    /// </summary>
    public void UnregisterInputController(InputController inputController)
    {
        if (inputController != null)
        {
            _registeredInputControllers.Remove(inputController);
        }
    }

    /// <summary>
    /// Disable all registered InputControllers.
    /// </summary>
    public void DisableInputControllers()
    {
        _inputControllersDisabled = true;
        foreach (var controller in _registeredInputControllers)
        {
            if (controller != null)
            {
                controller.SetEnabled(false);
            }
        }
    }

    /// <summary>
    /// Enable all registered InputControllers.
    /// </summary>
    public void EnableInputControllers()
    {
        _inputControllersDisabled = false;
        foreach (var controller in _registeredInputControllers)
        {
            if (controller != null)
            {
                controller.SetEnabled(true);
            }
        }
    }
}

