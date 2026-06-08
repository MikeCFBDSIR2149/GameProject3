using DG.Tweening;
using UnityEngine;

namespace UI.Menu
{
    public class LevelSelectionPanel : UIBase
    {
        [Header("关卡格子预制体")]
        [SerializeField] private GameObject levelSegmentPrefab;

        [Header("关卡格子容器")]
        [SerializeField] private Transform segmentContainer;

        [Header("动画设置")]
        [SerializeField] private RectTransform animatedPanel;
        [SerializeField] private float openDuration = 0.45f;
        [SerializeField] private float closeDuration = 0.25f;
        [SerializeField, Min(0f)] private float backEaseOvershoot = 1.7f;

        private Vector2 _shownAnchoredPosition;
        private Vector2 _hiddenAnchoredPosition;
        private bool _hasCachedPositions;
        private Tween _panelTween;

        public override void UpdateUI(object data)
        {
            RefreshSegments();
        }

        public override void OnInit()
        {
            CachePanelPositions();
        }

        public override void OnShow(object data = null)
        {
            CachePanelPositions();

            if (animatedPanel == null)
            {
                base.OnShow(data);
                return;
            }

            KillCurrentTween();
            animatedPanel.anchoredPosition = _hiddenAnchoredPosition;
            animatedPanel.gameObject.SetActive(true);

            base.OnShow(data);

            _panelTween = animatedPanel
                .DOAnchorPos(_shownAnchoredPosition, openDuration)
                .SetEase(Ease.OutBack, backEaseOvershoot)
                .SetUpdate(true);
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
            // Play close animation first, then hide
            if (animatedPanel == null || !gameObject.activeInHierarchy)
            {
                UIManager.Instance.HideUI(UIName);
                return;
            }

            CachePanelPositions();
            KillCurrentTween();

            _panelTween = animatedPanel
                .DOAnchorPos(_hiddenAnchoredPosition, closeDuration)
                .SetEase(Ease.InBack, backEaseOvershoot)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    UIManager.Instance.HideUI(UIName);
                });
        }

        public override void OnHide()
        {
            KillCurrentTween();
            base.OnHide();
        }

        private void CachePanelPositions()
        {
            if (animatedPanel == null || _hasCachedPositions)
                return;

            _shownAnchoredPosition = animatedPanel.anchoredPosition;

            Canvas canvas = animatedPanel.GetComponentInParent<Canvas>();
            float canvasHeight = 0f;
            if (canvas != null)
            {
                RectTransform canvasRect = canvas.transform as RectTransform;
                if (canvasRect != null)
                    canvasHeight = canvasRect.rect.height;
            }

            if (canvasHeight <= 0f)
                canvasHeight = Screen.height;

            float panelHeight = animatedPanel.rect.height;
            _hiddenAnchoredPosition = _shownAnchoredPosition + Vector2.down * (canvasHeight + panelHeight);
            _hasCachedPositions = true;
        }

        private void KillCurrentTween()
        {
            if (_panelTween != null && _panelTween.IsActive())
                _panelTween.Kill();
            _panelTween = null;
        }

        private void OnDestroy()
        {
            KillCurrentTween();
        }
    }
}

