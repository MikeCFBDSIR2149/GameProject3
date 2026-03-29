using UnityEngine;
using UserOptions;

namespace UI.UserOptions
{
    public class UserOptionsButton : MonoBehaviour
    {
        private static bool isOpen;
        
        private void OnEnable()
        {
            if (GlobalInputController.Instance != null)
            {
                GlobalInputController.Instance.OnCancelInputChanged += OnCancelInputChanged;
            }
        }

        private void OnDisable()
        {
            if (GlobalInputController.Instance != null)
            {
                GlobalInputController.Instance.OnCancelInputChanged -= OnCancelInputChanged;
            }
        }

        public void OpenUserOptionsPanel()
        {
            if (isOpen) return;
            UIManager.Instance.ShowUI("UserOptionsMain", asRootCanvas: true);
            MousePointerManager.Instance.UnlockCursor();
            isOpen = true;
        }

        public void CloseUserOptionsPanel()
        {
            if (!isOpen) return;
            OptionsManager.Instance.SaveOptions();
            UIManager.Instance.HideUI("UserOptionsMain");
            MousePointerManager.Instance.LockCursor();
            isOpen = false;
        }

        private void OnCancelInputChanged()
        {
            OpenUserOptionsPanel();
        }
    }
}
