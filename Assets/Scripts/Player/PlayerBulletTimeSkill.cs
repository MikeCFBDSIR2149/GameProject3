using UnityEngine;
using System.Collections;

namespace Player
{
    public class PlayerBulletTimeSkill : MonoBehaviour
    {
        [Header("Bullet Time Settings")]
        public float bulletTimeDuration = 2f;
        public InputController inputController;
        private bool isBulletTimeActive = false;
        private Coroutine _bulletTimeCoroutine;

        private void OnEnable()
        {
            if (inputController != null)
                inputController.OnBulletTimeSkillInputChanged += TryActivateBulletTime;
        }

        private void OnDisable()
        {
            if (inputController != null)
                inputController.OnBulletTimeSkillInputChanged -= TryActivateBulletTime;
        }

        private void TryActivateBulletTime()
        {
            if (!isBulletTimeActive)
            {
                _bulletTimeCoroutine = StartCoroutine(BulletTimeRoutine());
            }
        }

        private IEnumerator BulletTimeRoutine()
        {
            isBulletTimeActive = true;
            GameplayManager.Instance.SetGameplayStatus(GameplayStatus.BulletTime);
            yield return new WaitForSecondsRealtime(bulletTimeDuration);
            GameplayManager.Instance.SetGameplayStatus(GameplayStatus.Default);
            isBulletTimeActive = false;
        }
    }
}
