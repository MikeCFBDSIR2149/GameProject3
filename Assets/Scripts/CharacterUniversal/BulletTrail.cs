using UnityEngine;

namespace CharacterUniversal
{
    [RequireComponent(typeof(TrailRenderer))]
    public class BulletTrail : MonoBehaviour
    {
        public enum TrailControlMode
        {
            Auto,
            Manual
        }

        [Header("Trail Control")]
        [SerializeField] private TrailControlMode controlMode = TrailControlMode.Auto;

        private TrailRenderer _trailRenderer;

        private void Awake()
        {
            _trailRenderer = GetComponent<TrailRenderer>();
            if (_trailRenderer != null)
            {
                // Mirror the serialized preference if desired
                _trailRenderer.enabled = true;
                _trailRenderer.emitting = false;
            }
        }

        private void OnEnable()
        {
            // Ensure trail is clean when object is activated (pool take)
            ResetTrail();

            if (controlMode == TrailControlMode.Auto)
                StartTrail();
            else
                StopTrail();
        }

        private void OnDisable()
        {
            // Ensure trail cleared when object is disabled/returned to pool
            if (_trailRenderer != null)
            {
                _trailRenderer.emitting = false;
                ResetTrail();
            }
        }

        /// <summary>
        /// Clear the trail. Public so object pool or external code can call it explicitly.
        /// </summary>
        public void ResetTrail()
        {
            if (_trailRenderer == null)
                _trailRenderer = GetComponent<TrailRenderer>();

            if (_trailRenderer != null)
            {
                // TrailRenderer.Clear() exists in supported Unity versions
                _trailRenderer.Clear();
            }
        }

        public void StartTrail()
        {
            if (!_trailRenderer)
                _trailRenderer = GetComponent<TrailRenderer>();

            if (_trailRenderer)
                _trailRenderer.emitting = true;
        }

        public void StopTrail()
        {
            if (!_trailRenderer)
                _trailRenderer = GetComponent<TrailRenderer>();

            if (_trailRenderer)
                _trailRenderer.emitting = false;
        }

        public bool IsManualControl => controlMode == TrailControlMode.Manual;
    }
}
