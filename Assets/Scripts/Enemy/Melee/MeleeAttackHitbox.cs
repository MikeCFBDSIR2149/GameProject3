using CharacterUniversal;
using UnityEngine;

namespace Enemy.Melee
{
    /// <summary>
    /// 近战攻击判定盒：
    /// - 攻击时 Arm() 启用（可造成伤害、可被弹反）
    /// - 结束/被弹反 Disarm() 禁用
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class MeleeAttackHitbox : MonoBehaviour
    {
        [Header("Damage")]
        public float damage = 10f;

        [Header("Owner")]
        public HybridEnemy owner;

        private bool _armed;

        private void Awake()
        {
            // 必须是 Trigger
            Collider col = GetComponent<Collider>();
            col.isTrigger = true;

            // 默认关闭
            Disarm();
        }

        public void Arm()
        {
            _armed = true;
            gameObject.SetActive(true);
        }

        public void Disarm()
        {
            _armed = false;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 被玩家弹反调用
        /// </summary>
        public void Parry(Transform player)
        {
            if (!_armed) return;

            // 先关掉判定，保证不会再命中玩家
            Disarm();

            if (owner != null)
            {
                owner.OnMeleeParried(player);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_armed) return;
            if (!other.CompareTag("Player")) return;

            // 只找 IDamageable，不改 Health.cs
            other.GetComponent<IDamageable>()?.TakeDamage(damage);
        }
    }
}