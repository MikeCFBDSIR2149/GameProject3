using CharacterUniversal;
using UnityEngine;

namespace Enemy.Melee
{
    /// <summary>
    /// 近战攻击可高亮对象：攻击窗口内，且在 BulletTime 时显示高亮圈
    /// </summary>
    public class MeleeAttackHighlight : MonoBehaviour, IHighlightInViewport
    {
        public float highlightMinDistance;
        public float highlightMaxDistance = 3f;
        public string highlightUIPrefabName = "HighlightRing";
        public string HighlightUIPrefabName => highlightUIPrefabName;
        public float HighlightMinDistance => Mathf.Max(0f, highlightMinDistance);
        public float HighlightMaxDistance => Mathf.Max(HighlightMinDistance, highlightMaxDistance);

        [Tooltip("关联的近战攻击判定盒（用于判断是否处于攻击窗口）")]
        public MeleeAttackHitbox hitbox;

        private bool _isHighlighted;

        private void OnDisable()
        {
            // 确保对象被关闭时，高亮 UI 一定被清掉（避免残留）
            if (_isHighlighted)
            {
                _isHighlighted = false;
                OnHighlightStateChanged(false);
                HighlightManager.Instance?.CloseHighlight(this);
            }
        }

        private void Update()
        {
            // 仅在子弹时间中显示可弹反提示（沿用你子弹高亮逻辑）
            if (GameplayManager.Instance == null || GameplayManager.Instance.Status != EGameplayStatus.BulletTime)
                return;

            var player = GameplayManager.Instance.Player;
            if (!player) return;

            Camera cam = player.GetPlayerCamera();
            if (!cam) return;

            bool shouldHighlight = CheckHighlightCondition();
            Vector3 screenPos = GetScreenPosition(cam);

            // 必须在相机前方
            if (shouldHighlight && screenPos.z > 0)
            {
                if (!_isHighlighted)
                {
                    _isHighlighted = true;
                    OnHighlightStateChanged(true);
                }
                HighlightManager.Instance.UpdateHighlight(this, screenPos);
            }
            else
            {
                if (_isHighlighted)
                {
                    _isHighlighted = false;
                    OnHighlightStateChanged(false);
                    HighlightManager.Instance.CloseHighlight(this);
                }
            }
        }

        public bool CheckHighlightCondition()
        {
            if (hitbox == null) return false;
            if (!hitbox.gameObject.activeInHierarchy) return false; // 攻击窗口外不显示

            var player = GameplayManager.Instance?.Player;
            if (!player) return false;

            float minDistance = HighlightMinDistance;
            float maxDistance = HighlightMaxDistance;
            float dist = Vector3.Distance(transform.position, player.GetWorldPosition());
            return dist >= minDistance && dist <= maxDistance;
        }

        public Vector3 GetScreenPosition(Camera cam)
        {
            return cam.WorldToScreenPoint(transform.position);
        }

        public void OnHighlightStateChanged(bool isHighlighted)
        {
            // 预留：音效/特效
        }
    }
}