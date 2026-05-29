using UnityEngine;

namespace UI.Menu
{
    public class LevelSelectionPanel : UIBase
    {
        [Header("关卡格子预制体")]
        [SerializeField] private GameObject levelSegmentPrefab;

        [Header("关卡格子容器")]
        [SerializeField] private Transform segmentContainer;

        public override void UpdateUI(object data)
        {
            RefreshSegments();
        }

        private void RefreshSegments()
        {
            if (levelSegmentPrefab == null)
            {
                Debug.LogWarning($"[LevelSelectionPanel] Missing LevelSegment prefab reference on {name}.");
                return;
            }

            if (segmentContainer == null)
            {
                Debug.LogWarning($"[LevelSelectionPanel] Missing segment container reference on {name}.");
                return;
            }

            ClearSegments();

            if (LevelManager.Instance == null)
            {
                Debug.LogWarning("[LevelSelectionPanel] LevelManager.Instance is null.");
                return;
            }

            int levelCount = LevelManager.Instance.GetSelectableLevelCount();
            for (int i = 1; i <= levelCount; i++)
            {
                GameObject segmentObj = Instantiate(levelSegmentPrefab, segmentContainer, false);
                LevelSegment segment = segmentObj.GetComponent<LevelSegment>();
                
                if (segment != null)
                {
                    // 设置场景索引（对应 Build Settings 中的索引）
                    segment.SetLevelIndex(i);
                    
                    string displayName = LevelManager.Instance.GetLevelDisplayName(i);
                    segment.SetDisplayName(displayName);
                }
            }
        }

        private void ClearSegments()
        {
            for (int i = segmentContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(segmentContainer.GetChild(i).gameObject);
            }
        }

        public void CloseSelf()
        {
            UIManager.Instance.HideUI(UIName);
        }
    }
}

