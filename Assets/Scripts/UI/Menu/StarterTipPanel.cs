using DG.Tweening;
using GameProgress;
using UnityEngine;

namespace UI.Menu
{
    public class StarterTipPanel : UIBase
    {
        [Header("动画设置")]
        [SerializeField] private RectTransform animatedPanel;
        [SerializeField] private float openDuration = 0.45f;
        [SerializeField] private float closeDuration = 0.25f;
        [SerializeField, Min(0f)] private float backEaseOvershoot = 0.8f;

        private Vector2 _shownAnchoredPosition;
        private Vector2 _hiddenAnchoredPosition;
        private bool _hasCachedPositions;
        private Tween _panelTween;

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

        public void CloseStarterTipPanel()
        {
            if (animatedPanel == null || !gameObject.activeInHierarchy)
            {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.HideUI(UIName);
                }
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
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.HideUI(UIName);
                    }
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
            {
                return;
            }

            _shownAnchoredPosition = animatedPanel.anchoredPosition;

            Canvas canvas = animatedPanel.GetComponentInParent<Canvas>();
            float canvasHeight = 0f;
            if (canvas != null)
            {
                RectTransform canvasRect = canvas.transform as RectTransform;
                if (canvasRect != null)
                {
                    canvasHeight = canvasRect.rect.height;
                }
            }

            if (canvasHeight <= 0f)
            {
                canvasHeight = Screen.height;
            }

            float panelHeight = animatedPanel.rect.height;
            _hiddenAnchoredPosition = _shownAnchoredPosition + Vector2.down * (canvasHeight + panelHeight);
            _hasCachedPositions = true;
        }

        private void KillCurrentTween()
        {
            if (_panelTween != null && _panelTween.IsActive())
            {
                _panelTween.Kill();
            }

            _panelTween = null;
        }

        private void OnDestroy()
        {
            KillCurrentTween();
        }

        public void ClosePermanently()
        {
            GameProgressManager.Instance.SetIsNewPlayer(false);
            GameProgressManager.Instance.SaveGameProgress();
            CloseStarterTipPanel();
        }
        
        public void ProceedToStart()
        {
            LevelManager.Instance.LoadNextScene();
        }
    }
}
