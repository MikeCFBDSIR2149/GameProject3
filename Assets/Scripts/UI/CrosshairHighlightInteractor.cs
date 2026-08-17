using CharacterUniversal;
using Enemy.Melee;
using UnityEngine;

namespace UI
{
    [DefaultExecutionOrder(600)]
    public class CrosshairHighlightInteractor : MonoBehaviour
    {
        public Player.PlayerAttackBack attackBack;

        private void LateUpdate()
        {
            GameplayManager gameplayManager = GameplayManager.Instance;
            if (gameplayManager == null || gameplayManager.Status != EGameplayStatus.BulletTime)
                return;

            HighlightManager highlightManager = HighlightManager.Instance;
            if (highlightManager == null || !highlightManager.TryGetBestTarget(out IHighlightInViewport target))
                return;

            if (target is Player.BulletHighlight bulletHighlight)
            {
                ReturnBullet(bulletHighlight);
                return;
            }

            if (target is MeleeAttackHighlight meleeHighlight && meleeHighlight.hitbox != null)
            {
                Player.Player player = gameplayManager.Player;
                if (player != null)
                    meleeHighlight.hitbox.Parry(player.transform);
            }
        }

        private void ReturnBullet(Player.BulletHighlight bulletHighlight)
        {
            if (attackBack == null || bulletHighlight == null)
                return;

            (GameObject bullet, string poolKey) = bulletHighlight.GetBulletAndPoolKey();
            ISender sender = bulletHighlight.Sender;
            if (!bullet || string.IsNullOrEmpty(poolKey) || sender == null)
                return;

            ObjectPoolManager poolManager = ObjectPoolManager.Instance;
            if (poolManager == null)
                return;

            Vector3 spawnPosition = bullet.transform.position;
            poolManager.Dispose(poolKey, bullet);
            attackBack.RegisterBulletReturn(sender, spawnPosition);
        }
    }
}
