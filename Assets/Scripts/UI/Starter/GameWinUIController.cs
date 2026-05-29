using UnityEngine;

namespace UI.Starter
{
    public class GameWinUIController : MonoBehaviour
    {
        [SerializeField] private string gameWinUIName = "GameWinMenu";

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
            if (status == EGameplayStatus.GameWin)
            {
                ShowGameWinUI();
                MousePointerManager.Instance?.UnlockCursor();
                return;
            }

            HideGameWinUI();
        }

        private void ShowGameWinUI()
        {
            if (_isVisible || UIManager.Instance == null)
                return;

            UIManager.Instance.ShowUI(gameWinUIName, asRootCanvas: true);
            _isVisible = true;
        }

        private void HideGameWinUI()
        {
            if (!_isVisible || UIManager.Instance == null)
                return;

            UIManager.Instance.HideUI(gameWinUIName);
            _isVisible = false;
        }
    }
}


