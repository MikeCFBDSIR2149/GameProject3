using UnityEngine;

namespace UI.Menu.MenuSet
{
    public class PauseButton : MonoBehaviour
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

        public void OpenPausePanel()
        {
            if (isOpen) return;
            UIManager.Instance.ShowUI("PauseMenu", asRootCanvas: true);
            GameplayManager.Instance.SetGameplayStatus(EGameplayStatus.Paused);
            MousePointerManager.Instance.UnlockCursor();
            isOpen = true;
        }

        public void ClosePausePanel()
        {
            if (!isOpen) return;
            UIManager.Instance.HideUI("PauseMenu");
            GameplayManager.Instance.SetToPreviousStatus();
            MousePointerManager.Instance.LockCursor();
            isOpen = false;
        }

        private void OnCancelInputChanged()
        {
            OpenPausePanel();
        }
    }
}
