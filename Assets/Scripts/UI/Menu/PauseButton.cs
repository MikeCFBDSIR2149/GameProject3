using UnityEngine;

namespace UI.Menu
{
    public class PauseButton : MonoBehaviour
    {
        private static bool _isOpen;

        public static void ResetPauseState()
        {
            _isOpen = false;
        }
        
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

            _isOpen = false;
        }

        public void OpenPausePanel()
        {
            if (_isOpen) return;
            if (GameplayManager.Instance != null && GameplayManager.Instance.Status == EGameplayStatus.GameOver)
                return;
            UIManager.Instance.ShowUI("PauseMenu", asRootCanvas: true);
            GameplayManager.Instance.SetGameplayStatus(EGameplayStatus.Paused);
            MousePointerManager.Instance.UnlockCursor();
            _isOpen = true;
        }

        public void ClosePausePanel()
        {
            if (!_isOpen) return;
            UIManager.Instance.HideUI("PauseMenu");
            GameplayManager.Instance.SetToPreviousStatus();
            MousePointerManager.Instance.LockCursor();
            _isOpen = false;
        }

        private void OnCancelInputChanged()
        {
            OpenPausePanel();
        }
    }
}
