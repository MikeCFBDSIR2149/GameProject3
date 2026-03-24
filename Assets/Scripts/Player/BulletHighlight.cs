using UnityEngine;

namespace Player
{
    public class BulletHighlight : MonoBehaviour, IHighlightInViewport
    {
        public float highlightDistance = 10f;
        private bool _isHighlighted = false;

        private void Update()
        {
            if (GameplayManager.Instance.Status != GameplayStatus.BulletTime)
                return;
            Player player = GameplayManager.Instance?.Player;
            if (!player) return;
            Camera cam = player.GetPlayerCamera();
            if (!cam) return;

            bool shouldHighlight = CheckHighlightCondition();
            Vector3 screenPos = GetScreenPosition(cam);

            if (shouldHighlight)
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
            var player = GameplayManager.Instance?.Player;
            if (player == null) return false;
            float dist = Vector3.Distance(transform.position, player.GetWorldPosition());
            return dist <= highlightDistance;
        }

        public Vector3 GetScreenPosition(Camera cam)
        {
            return cam.WorldToScreenPoint(transform.position);
        }

        public void OnHighlightStateChanged(bool isHighlighted)
        {
            // 可在此处处理高亮状态变化时的本地逻辑（如特效、声音等）
            if (isHighlighted)
            {
                Debug.Log($"[BulletHighlight] 高亮开启: {gameObject.name}");
            }
            else
            {
                Debug.Log($"[BulletHighlight] 高亮关闭: {gameObject.name}");
            }
        }
    }
}
