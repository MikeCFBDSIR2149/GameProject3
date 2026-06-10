using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameProgress
{
    public class GameProgressUpdater : MonoBehaviour
    {
        private void OnEnable()
        {
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnStatusChanged += HandleGameplayStatusChanged;
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
                UpdateGameProgress();
            }
        }

        public void UpdateGameProgress()
        {
            int currentLevelIndex = LevelManager.Instance.GetCurrentLevelIndex();

            GameProgressManager progressManager = GameProgressManager.Instance;
            if (progressManager == null)
            {
                return;
            }

            progressManager.SetLevelCleared(currentLevelIndex);
        }
    }
}
