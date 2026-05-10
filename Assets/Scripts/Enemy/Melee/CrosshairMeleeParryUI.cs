using System.Collections.Generic;
using CharacterUniversal;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Enemy.Melee
{
    /// <summary>
    /// 近战弹反：在 BulletTime 状态下，用 UI Raycaster 检测准星下的 HighlightRingUI，
    /// 若其 owner 是 MeleeAttackHighlight，则执行 Parry。
    /// </summary>
    public class CrosshairMeleeParryUI : MonoBehaviour
    {
        private GraphicRaycaster _raycaster;
        private EventSystem _eventSystem;

        [Tooltip("玩家Transform，用于计算击退方向等")]
        public Transform player;

        private void Awake()
        {
            _raycaster = GetComponentInParent<Canvas>()?.GetComponent<GraphicRaycaster>();
            _eventSystem = EventSystem.current;
        }

        private void Update()
        {
            if (GameplayManager.Instance == null) return;

            // 仅在子弹时间允许弹反（与 BulletHighlight 显示一致）
            if (GameplayManager.Instance.Status != EGameplayStatus.BulletTime)
                return;

            if (_raycaster == null || _eventSystem == null) return;
            if (player == null && GameplayManager.Instance.Player != null)
                player = GameplayManager.Instance.Player.transform;

            PointerEventData pointerData = new PointerEventData(_eventSystem);
            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            pointerData.position = screenCenter;

            List<RaycastResult> results = new List<RaycastResult>();
            _raycaster.Raycast(pointerData, results);

            foreach (RaycastResult result in results)
            {
                HighlightRingUI ringUI = result.gameObject.GetComponent<HighlightRingUI>();
                if (!ringUI) continue;

                IHighlightInViewport owner = HighlightManager.Instance.GetHighlightOwner(ringUI);
                if (owner is not MeleeAttackHighlight meleeHighlight) continue;

                if (meleeHighlight == null || meleeHighlight.hitbox == null) continue;

                meleeHighlight.hitbox.Parry(player);

                // Parry 后 hitbox 会 Disarm -> highlight 下帧会 Close
                // 这里可以直接 break，避免一帧内多次触发
                break;
            }
        }
    }
}