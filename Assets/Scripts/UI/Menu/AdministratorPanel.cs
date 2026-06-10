using DG.Tweening;
using GameProgress;
using TMPro;
using UnityEngine;
using UnityEngine.Android;

namespace UI.Menu
{
    public class AdministratorPanel : UIBase
    {
        [Header("动画设置")]
        [SerializeField] private RectTransform animatedPanel;
        [SerializeField] private float openDuration = 0.45f;
        [SerializeField] private float closeDuration = 0.25f;
        [SerializeField, Min(0f)] private float backEaseOvershoot = 0.8f;

        [Header("IsNewPlayer")] 
        [SerializeField] private TextMeshProUGUI isNewPlayerStatus;

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
            GetData();

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

        public void CloseSelf()
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
            GameProgressManager.Instance.SaveGameProgress();
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

        private void GetData()
        {
            isNewPlayerStatus.text = GameProgressManager.Instance.GetGameProgress().isNewPlayer.ToString();
        }

        public void SetDataIsNewPlayer()
        {
            GameProgressManager.Instance.SetIsNewPlayer(true);
        }
        
        public void SetDataToDefault()
        {
            GameProgressManager.Instance.ResetToDefault();
        }
    }
}
