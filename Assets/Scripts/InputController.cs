using System;
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
    public event System.Action OnAttackInputChanged;
    public event System.Action OnBulletTimeSkillInputChanged;

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
        _inputActions.Player.Attack.performed += OnAttack;
        // 绑定 BulletTimeSkill
        _inputActions.Player.BulletTimeSkill.performed += OnBulletTimeSkill;
    }

    private void OnDisable()
    {
        _inputActions.Player.Move.performed -= OnMove;
        _inputActions.Player.Move.canceled -= OnMove;
        _inputActions.Player.Look.performed -= OnLook;
        _inputActions.Player.Look.canceled -= OnLook;
        _inputActions.Player.Jump.performed -= OnJump;
        _inputActions.Player.Attack.performed -= OnAttack;
        // 解绑 BulletTimeSkill
        _inputActions.Player.BulletTimeSkill.performed -= OnBulletTimeSkill;
        _inputActions.Disable();
    }
    private void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            try
            {
                OnAttackInputChanged?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Exception when invoking OnAttackInputChanged: {e}");
            }
        }
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
        try
        {
            OnMoveInputChanged?.Invoke(_moveInput);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Exception when invoking OnMoveInputChanged: {e}");
        }
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
        try
        {
            OnLookInputChanged?.Invoke(_lookInput);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Exception when invoking OnLookInputChanged: {e}");
        }
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            try
            {
                OnJumpInputChanged?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Exception when invoking OnJumpInputChanged: {e}");
            }
        }
    }

    private void OnBulletTimeSkill(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            try
            {
                OnBulletTimeSkillInputChanged?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Exception when invoking OnBulletTimeSkillInputChanged: {e}");
            }
        }
    }
}
