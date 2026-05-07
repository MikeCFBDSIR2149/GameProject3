using UnityEngine;
using System.Collections.Generic;

namespace CharacterUniversal
{
    public class Attack : MonoBehaviour
    {
        public float damage = 10f;
        // 允许攻击的目标Tag列表（如 Enemy, Player 等）
        [Tooltip("只有目标Tag在此列表中时才会造成伤害")]
        public List<string> targetTags = new List<string>();

        // 假设通过触发器造成伤害
        private void OnTriggerEnter(Collider other)
        {
            // 判断目标Tag是否在允许列表中
            if (targetTags.Count > 0 && !targetTags.Contains(other.tag))
                return;
            // 只找接口
            IDamageable target = other.GetComponent<IDamageable>();
            target?.TakeDamage(damage);
        }
    }
}
