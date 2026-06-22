using System.Collections;
using UnityEngine;

namespace CharacterUniversal
{
    public class PlayerBulletCollisionEffect : MonoBehaviour
    {
        [SerializeField] private ParticleSystem particle;
        private Coroutine _effectCoroutine;

        private void OnEnable()
        {
            particle.Play();
            _effectCoroutine = StartCoroutine(EffectDurationCoroutine());
        }

        private void OnDisable()
        {
            if (_effectCoroutine != null) StopCoroutine(_effectCoroutine);
            _effectCoroutine = null;
        }

        private IEnumerator EffectDurationCoroutine()
        {
            yield return new WaitForSeconds(particle.main.duration);
            Destroy(gameObject);
        }
    }
}
