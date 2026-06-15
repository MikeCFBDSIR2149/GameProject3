using Render;
using UnityEngine;

namespace UI.Starter
{
    public class GameOverUIController : MonoBehaviour
    {
        [SerializeField] private string gameOverUIName = "GameOverMenu";
        [SerializeField] private GlobalVolumeController globalVolumeController;

        private bool _isVisible;

        private void OnEnable()
        {
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnStatusChanged += HandleGameplayStatusChanged;
                HandleGameplayStatusChanged(GameplayManager.Instance.Status);
            }
        }

        private void OnDisable()
        {
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnStatusChanged -= HandleGameplayStatusChanged;
            }
        }

        private void HandleGameplayStatusChanged(EGameplayStatus status)
        {
            if (GameplayManager.Instance.doNotTriggerListener) return;
            if (status == EGameplayStatus.GameOver)
            {
                ShowGameOverUI();
                MousePointerManager.Instance?.UnlockCursor();
                return;
            }

            HideGameOverUI();
        }

        private void ShowGameOverUI()
        {
            if (_isVisible || UIManager.Instance == null)
                return;

            UIManager.Instance.ShowUI(gameOverUIName, asRootCanvas: true);
            _isVisible = true;
            if (globalVolumeController != null) globalVolumeController.GameOverEffect(true);
        }

        private void HideGameOverUI()
        {
            if (!_isVisible || UIManager.Instance == null)
                return;

            UIManager.Instance.HideUI(gameOverUIName);
            _isVisible = false;
            if (globalVolumeController != null) globalVolumeController.GameOverEffect(false);
        }
    }
}


