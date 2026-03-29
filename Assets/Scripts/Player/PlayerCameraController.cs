using UnityEngine;
using UserOptions;

namespace Player
{
    public class PlayerCameraController : MonoBehaviour, ISyncFromOptions
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
            // 注册OptionsManager事件
            if (OptionsManager.Instance != null)
            {
                OptionsManager.Instance.OnOptionsChanged += SyncFromOptions;
                SyncFromOptions(); // 主动同步一次
            }
        }
        private void OnDisable()
        {
            MousePointerManager.Instance?.UnlockCursor();
            if (inputController != null)
            {
                inputController.OnLookInputChanged -= SetLookInput;
            }
            // 注销OptionsManager事件
            if (OptionsManager.Instance != null)
            {
                OptionsManager.Instance.OnOptionsChanged -= SyncFromOptions;
            }
        }
        private void SetLookInput(Vector2 lookDelta)
        {
            float deltaY = lookDelta.y * verticalLookSensitivity * Time.unscaledDeltaTime;
            _pitch -= deltaY;
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

        }
        private void LateUpdate()
        {
            transform.localEulerAngles = new Vector3(_pitch, 0, 0);
        }

        // 实现ISyncFromOptions接口
        public void SyncFromOptions()
        {
            OptionsManager optionsMgr = OptionsManager.Instance;
            if (!optionsMgr) return;
            OptionsData options = optionsMgr.GetOptions();
            if (options != null)
            {
                verticalLookSensitivity = options.verticalSensitivity;
            }
        }
    }
}
