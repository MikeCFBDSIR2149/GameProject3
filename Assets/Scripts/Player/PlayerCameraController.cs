using UnityEngine;

namespace Player
{
    public class PlayerCameraController : MonoBehaviour
    {
        public InputController inputController;
        public float verticalLookSensitivity = 1f;
        public float minPitch = -80f;
        public float maxPitch = 80f;
        private float _pitch;

        private void OnEnable()
        {
            MousePointerManager.Instance.LockCursor();
            if (inputController != null)
            {
                inputController.OnLookInputChanged += SetLookInput;
            }
        }
        private void OnDisable()
        {
            MousePointerManager.Instance?.UnlockCursor();
            if (inputController != null)
            {
                inputController.OnLookInputChanged -= SetLookInput;
            }
        }
        private void SetLookInput(Vector2 lookDelta)
        {
            float deltaY = lookDelta.y * verticalLookSensitivity;
            if (Mathf.Abs(deltaY) > 0.0001f)
            {
                _pitch -= deltaY;
                _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
            }
        }
        private void LateUpdate()
        {
            transform.localEulerAngles = new Vector3(_pitch, 0, 0);
        }
    }
}
