using TMPro;
using UnityEngine;

namespace UI.Menu
{
    public class LevelSegment : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI levelNameText;

        private int _sceneIndex = -1;

        private void Awake()
        {
            CacheText();
        }

        public void SetDisplayName(string displayName)
        {
            CacheText();

            if (levelNameText == null)
            {
                Debug.LogWarning($"[LevelSegment] Missing TextMeshProUGUI reference on {name}.");
                return;
            }

            levelNameText.text = displayName ?? string.Empty;
        }

        public void SetLevelIndex(int sceneIndex)
        {
            _sceneIndex = sceneIndex;
        }

        public void LoadLevel()
        {
            if (_sceneIndex < 0)
            {
                Debug.LogWarning($"[LevelSegment] Invalid scene index: {_sceneIndex}");
                return;
            }

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadScene(_sceneIndex);
            }
            else
            {
                Debug.LogError("[LevelSegment] LevelManager.Instance is null.");
            }
        }

        private void CacheText()
        {
            if (levelNameText == null)
            {
                levelNameText = GetComponentInChildren<TextMeshProUGUI>(true);
            }
        }
    }
}

