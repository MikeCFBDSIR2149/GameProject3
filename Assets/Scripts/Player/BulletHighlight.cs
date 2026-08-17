using CharacterUniversal;
using UnityEngine;

namespace Player
{
    public class BulletHighlight : MonoBehaviour, IHighlightInViewport, IHighlightOcclusionSource
    {
        public float highlightMinDistance;
        public float highlightMaxDistance = 5f;
        public string referencePoolKey;

        [Header("Occlusion")]
        public bool useOcclusionCheck = true;
        public LayerMask occlusionMask = ~0;
        public float occlusionCheckInterval = 0.1f;

        public string highlightUIPrefabName = "HighlightRing";

        private IContainSender _containSender;

        public Transform HighlightTransform => transform;
        public string HighlightUIPrefabName => highlightUIPrefabName;
        public float HighlightMinDistance => Mathf.Max(0f, highlightMinDistance);
        public float HighlightMaxDistance => Mathf.Max(HighlightMinDistance, highlightMaxDistance);
        public bool IsHighlightEligible => true;
        public int InteractionPriority => 0;

        public bool UseHighlightOcclusion => useOcclusionCheck;
        public LayerMask HighlightOcclusionMask => occlusionMask;
        public float HighlightOcclusionInterval => Mathf.Max(0.01f, occlusionCheckInterval);
        public Transform HighlightOcclusionRoot => transform;

        public ISender Sender => _containSender?.Sender;

        private void Awake()
        {
            _containSender = GetComponent<IContainSender>();
        }

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

        public (GameObject bullet, string poolKey) GetBulletAndPoolKey()
        {
            return (gameObject, referencePoolKey);
        }
    }
}
