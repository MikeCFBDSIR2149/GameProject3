using Unity.Cinemachine;
using UnityEngine;

namespace Player
{
    public class PlayerCameraDetector : MonoBehaviour
    {
        private readonly RaycastHit[] raycastHits = new RaycastHit[32];

        public bool UseCinemachine = true;
        public CinemachineThirdPersonAim cameraAim;
        public Camera Camera;
        public float minDistance = 0.5f;
        public float maxDistance = 100f;
        public LayerMask LayerFilter = Physics.DefaultRaycastLayers;
        public string IgnoreTag = string.Empty;

        public Vector3 DetectAimPosition()
        {
            if (Camera == null)
            {
                Camera = Camera.main;
            }
            if (UseCinemachine && cameraAim != null)
            {
                return cameraAim.AimTarget;
            }

            return DetectByRaycast();
        }

        private Vector3 DetectByRaycast()
        {
            if (Camera == null)
            {
                return transform.position + transform.forward * Mathf.Max(maxDistance, 0f);
            }

            float clampedMinDistance = Mathf.Max(0f, minDistance);
            float clampedMaxDistance = Mathf.Max(clampedMinDistance, maxDistance);

            Vector3 origin = Camera.transform.position;
            Vector3 direction = Camera.transform.forward;
            Vector3 fallbackPoint = origin + direction * clampedMaxDistance;

            RaycastHit bestHit = default;
            bool hasHit = false;
            float bestDistance = float.MaxValue;
            int hitCount = Physics.RaycastNonAlloc(origin, direction, raycastHits, clampedMaxDistance, LayerFilter, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = raycastHits[i];

                if (hit.distance < clampedMinDistance)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(IgnoreTag) && hit.collider != null && hit.collider.CompareTag(IgnoreTag))
                {
                    continue;
                }

                if (hit.distance < bestDistance)
                {
                    bestDistance = hit.distance;
                    bestHit = hit;
                    hasHit = true;
                }
            }

            return hasHit ? bestHit.point : fallbackPoint;
        }
    }
}
