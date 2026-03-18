using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    public InputController _inputController;
    
    public float _moveSpeed = 5f;
    private Vector2 _moveInput;

    private void OnEnable()
    {
        if (_inputController != null)
        {
            _inputController.OnMoveInputChanged += SetMoveInput;
        }
    }

    private void OnDisable()
    {
        if (_inputController != null)
        {
            _inputController.OnMoveInputChanged -= SetMoveInput;
        }
    }

    private void SetMoveInput(Vector2 input)
    {
        _moveInput = input;
    }

    private void Update()
    {
        Vector3 move = new Vector3(_moveInput.x, 0, _moveInput.y) * (_moveSpeed * Time.deltaTime);
        transform.Translate(move, Space.World);
    }
}
