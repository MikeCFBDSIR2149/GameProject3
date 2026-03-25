using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class CrosshairUI : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private GraphicRaycaster _raycaster;
        private EventSystem _eventSystem;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _raycaster = GetComponentInParent<Canvas>().GetComponent<GraphicRaycaster>();
            _eventSystem = EventSystem.current;
        }

        private void Update()
        {
            // 使用GraphicRaycaster检测十字准星下的UI
            PointerEventData pointerData = new PointerEventData(_eventSystem);
            // 获取十字准星中心屏幕坐标
            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            pointerData.position = screenCenter;
            List<RaycastResult> results = new List<RaycastResult>();
            _raycaster.Raycast(pointerData, results);
            foreach (var result in results)
            {
                Debug.Log("aaa {result.gameObject.name}");
                HighlightRingUI ringUI = result.gameObject.GetComponent<HighlightRingUI>();
                if (ringUI == null) continue;
                Debug.Log($"[CrosshairUI] 命中高亮UI: {ringUI.gameObject.name}");
                // 找到对应BulletHighlight
                var highlightOwner = HighlightManager.Instance.GetHighlightOwner(ringUI);
                if (highlightOwner is Player.BulletHighlight bulletHighlight)
                {
                    var (bullet, poolKey) = bulletHighlight.GetBulletAndPoolKey();
                    if (bullet != null && !string.IsNullOrEmpty(poolKey))
                    {
                        ObjectPoolManager.Instance.Dispose(poolKey, bullet);
                    }
                }
            }
        }
    }
}
