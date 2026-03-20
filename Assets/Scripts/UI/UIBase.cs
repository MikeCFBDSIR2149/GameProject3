using UnityEngine;

namespace UI
{
    public abstract class UIBase : MonoBehaviour
    {
        [Header("UI基本信息")]
        [SerializeField] protected string uiName;
        [SerializeField] protected bool isPersistent = false; // 是否持久化
    
        public string UIName => uiName;
    
        // UI生命周期方法
        public virtual void OnInit() { }
        public virtual void OnShow(object data = null) 
        { 
            gameObject.SetActive(true);
            UpdateUI(data);
        }
        public virtual void OnHide() 
        { 
            gameObject.SetActive(false);
        }
        public virtual void OnPause() { }
        public virtual void OnResume() { }
        public virtual void OnUpdate() { }
    
        // 更新UI数据
        public abstract void UpdateUI(object data);
    
        // 查找UI组件
        protected T FindComponent<T>(string path) where T : Component
        {
            Transform target = transform.Find(path);
            return target != null ? target.GetComponent<T>() : null;
        }
    }
}
