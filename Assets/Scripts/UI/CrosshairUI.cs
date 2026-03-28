using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class CrosshairUI : MonoBehaviour
    {
        private GraphicRaycaster _raycaster;
        private EventSystem _eventSystem;

        public Player.PlayerAttackBack attackBack;

        private void Awake()
        {
            _raycaster = GetComponentInParent<Canvas>().GetComponent<GraphicRaycaster>();
            _eventSystem = EventSystem.current;
        }

        private void Update()
        {
            if (GameplayManager.Instance.Status == EGameplayStatus.Default)
                return;
            // 使用GraphicRaycaster检测十字准星下的UI
            PointerEventData pointerData = new PointerEventData(_eventSystem);
            // 获取十字准星中心屏幕坐标
            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            pointerData.position = screenCenter;
            List<RaycastResult> results = new List<RaycastResult>();
            _raycaster.Raycast(pointerData, results);
            foreach (RaycastResult result in results)
            {
                HighlightRingUI ringUI = result.gameObject.GetComponent<HighlightRingUI>();
                if (!ringUI) 
                    continue;
                IHighlightInViewport highlightOwner = HighlightManager.Instance.GetHighlightOwner(ringUI);
                if (highlightOwner is not Player.BulletHighlight bulletHighlight) 
                    continue;
                (GameObject bullet, var poolKey) = bulletHighlight.GetBulletAndPoolKey();
                if (!bullet || string.IsNullOrEmpty(poolKey)) 
                    continue;
                IContainSender containSender = bullet.GetComponent<IContainSender>();
                if (containSender is { Sender: not null } && attackBack)
                {
                    // 先回收敌人子弹
                    ObjectPoolManager.Instance.Dispose(poolKey, bullet);
                    // 再登记反弹目标
                    attackBack.RegisterBulletReturn(containSender.Sender);
                }
            }
        }
    }
}
