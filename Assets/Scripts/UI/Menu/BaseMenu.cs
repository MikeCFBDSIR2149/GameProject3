// BaseMenu.cs
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public abstract class BaseMenu : UIBase
    {
        [Header("菜单设置")]
        [SerializeField] protected Button backButton;
        [SerializeField] protected Animator animator;
        [SerializeField] protected CanvasGroup canvasGroup;
        
        protected MenuData menuData;
        
        public override void OnInit()
        {
            base.OnInit();
            
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackButtonClicked);
            }
            
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
            
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }
        
        public override void OnShow(object data)
        {
            base.OnShow(data);
            
            menuData = data as MenuData;
            
            if (animator != null)
            {
                animator.SetTrigger("Show");
            }
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }
        
        public override void OnHide()
        {
            base.OnHide();
            
            if (animator != null)
            {
                animator.SetTrigger("Hide");
            }
            else
            {
                gameObject.SetActive(false);
            }
            
            if (canvasGroup != null)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }
        
        // 返回按钮按下
        public virtual void OnBackPressed()
        {
            if (menuData != null )
            {
                UIManager.Instance.HideUI(gameObject.name);
            }
        }
        
        protected virtual void OnBackButtonClicked()
        {
            OnBackPressed();
        }
        
        protected void OnDestroy()
        {
            //base.OnDestroy();
            
            if (backButton != null)
            {
                backButton.onClick.RemoveListener(OnBackButtonClicked);
            }
        }
    }
}