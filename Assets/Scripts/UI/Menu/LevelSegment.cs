using GameProgress;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu
{
    public class LevelSegment : MonoBehaviour, ISyncFromGameProgress
    {
        [SerializeField] private TextMeshProUGUI levelNameText;
        [SerializeField] private Image snapshotImage;
        [SerializeField] private GameObject levelClearObject;

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
            snapshotImage.sprite = Resources.Load<Sprite>($"Snapshots/Level{sceneIndex - 1}");
        }

        public void SetClear()
        {
            SyncFromGameProgress();
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

        public void SyncFromGameProgress()
        {
            levelClearObject.SetActive(GameProgressManager.Instance.GetClearedLevels().Contains(_sceneIndex));
        }
    }
}

