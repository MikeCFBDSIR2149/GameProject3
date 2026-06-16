using Render;
using UnityEngine;

namespace UI.Starter
{
    public class GameOverUIController : MonoBehaviour
    {
        [SerializeField] private string gameOverUIName = "GameOverMenu";
        [SerializeField] private GlobalVolumeController globalVolumeController;
        [SerializeField] private GameObject gameOverUIAdditional;

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
            if (gameOverUIAdditional) gameOverUIAdditional.SetActive(true);
            _isVisible = true;
            if (globalVolumeController) globalVolumeController.GameOverEffect(true);
        }

        private void HideGameOverUI()
        {
            if (!_isVisible || UIManager.Instance == null)
                return;

            UIManager.Instance.HideUI(gameOverUIName);
            if (gameOverUIAdditional) gameOverUIAdditional.SetActive(false);
            _isVisible = false;
            if (globalVolumeController) globalVolumeController.GameOverEffect(false);
        }
    }
}


