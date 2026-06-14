using DG.Tweening;
using UnityEngine;

namespace UI.Menu
{
    public class PauseMenu : UIBase
    {
        [Header("动画设置")]
        [SerializeField] private RectTransform animatedPanel;
        [SerializeField] private float openDuration = 0.35f;
        [SerializeField] private float closeDuration = 0.22f;
        [SerializeField, Range(0.1f, 1f)] private float minScale = 0.85f;

        private Vector2 _shownAnchoredPosition;
        private Vector2 _hiddenAnchoredPosition;
        private bool _hasCachedPositions;
        private Tween _panelTween;

        public override void UpdateUI(object data)
        {
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
            // For pause menu, start from above the screen and from a smaller scale
            animatedPanel.anchoredPosition = _hiddenAnchoredPosition;
            animatedPanel.localScale = Vector3.one * minScale;
            animatedPanel.gameObject.SetActive(true);

            base.OnShow(data);

            // Play position + scale together (smooth non-bouncy easing)
            Sequence seq = DOTween.Sequence();
            seq.Append(animatedPanel.DOAnchorPos(_shownAnchoredPosition, openDuration).SetEase(Ease.OutCubic));
            seq.Join(animatedPanel.DOScale(Vector3.one, openDuration).SetEase(Ease.OutCubic));
            seq.SetUpdate(true);
            _panelTween = seq;
        }

        public void ClosePausePanel()
        {
            if (animatedPanel == null || !gameObject.activeInHierarchy)
            {
                UIManager.Instance.HideUI(UIName);
                return;
            }

            CachePanelPositions();
            KillCurrentTween();

            // create a sequence to move up and scale down together (smooth non-bouncy easing)
            Sequence closeSeq = DOTween.Sequence();
            closeSeq.Append(animatedPanel.DOAnchorPos(_hiddenAnchoredPosition, closeDuration).SetEase(Ease.InCubic));
            closeSeq.Join(animatedPanel.DOScale(Vector3.one * minScale, closeDuration).SetEase(Ease.InCubic));
            closeSeq.SetUpdate(true).OnComplete(() =>
            {
                UIManager.Instance.HideUI(UIName);
            });
            _panelTween = closeSeq;
        }

        public override void OnHide()
        {
            if (GameplayManager.Instance.doNotTriggerListener) return;
            KillCurrentTween();
            // Ensure the PauseButton static state is reset and gameplay/cursor restored
            PauseButton.ResetPauseState();
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.SetToPreviousStatus();
            }
            if (MousePointerManager.Instance != null)
            {
                MousePointerManager.Instance.LockCursor();
            }
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
            // For pause menu, hidden position is above the visible area (move down when showing)
            _hiddenAnchoredPosition = _shownAnchoredPosition + Vector2.up * (canvasHeight + panelHeight) / 2;
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
            PauseButton.ResetPauseState();
        }
    }
}
