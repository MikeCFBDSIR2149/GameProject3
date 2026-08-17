using CharacterUniversal;
using UnityEngine;

namespace Enemy.Melee
{
    /// <summary>
    /// 近战攻击可高亮对象：攻击窗口内，且在子弹时间中显示高亮圈。
    /// </summary>
    public class MeleeAttackHighlight : MonoBehaviour, IHighlightInViewport
    {
        public float highlightMinDistance;
        public float highlightMaxDistance = 3f;
        public string highlightUIPrefabName = "HighlightRing";

        [Tooltip("关联的近战攻击判定盒（用于判断是否处于攻击窗口）")]
        public MeleeAttackHitbox hitbox;

        public Transform HighlightTransform => transform;
        public string HighlightUIPrefabName => highlightUIPrefabName;
        public float HighlightMinDistance => Mathf.Max(0f, highlightMinDistance);
        public float HighlightMaxDistance => Mathf.Max(HighlightMinDistance, highlightMaxDistance);
        public bool IsHighlightEligible => hitbox != null && hitbox.gameObject.activeInHierarchy;
        public int InteractionPriority => 1;

        private void OnEnable()
        {
            HighlightManager.Instance?.Register(this);
        }

        private void OnDisable()
        {
            HighlightManager.Instance?.Unregister(this);
        }

        public void OnHighlightStateChanged(bool isHighlighted)
        {
            // 预留：高亮状态变化时的本地音效或特效。
        }
    }
}
