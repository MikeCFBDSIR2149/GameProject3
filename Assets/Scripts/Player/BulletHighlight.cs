using CharacterUniversal;
using UnityEngine;

namespace Player
{
    public class BulletHighlight : MonoBehaviour, IHighlightInViewport
    {
        public float highlightDistance = 10f;
        public string referencePoolKey;
        // 是否使用遮挡检测（射线）
        public bool useOcclusionCheck = true;
        // 用于遮挡检测的图层掩码，只检测这些图层上的碰撞体作为遮挡物
        public LayerMask occlusionMask = ~0;
        // 遮挡检测的间隔（秒），避免每帧对每个子弹都做射线检测带来过大开销
        public float occlusionCheckInterval = 0.1f;
        private float _lastOcclusionCheckTime = 0f;
        // 缓存上一次的遮挡检测结果，减少频繁射线开销
        private bool _cachedOccluded = false;
        private bool _isHighlighted = false;
        public string highlightUIPrefabName = "HighlightRing";
        public string HighlightUIPrefabName => highlightUIPrefabName;

        private void Update()
        {
            if (GameplayManager.Instance.Status != EGameplayStatus.BulletTime)
                return;
            Player player = GameplayManager.Instance?.Player;
            if (!player) return;
            Camera cam = player.GetPlayerCamera();
            if (!cam) return;

            bool shouldHighlight = CheckHighlightCondition(cam);
            Vector3 screenPos = GetScreenPosition(cam);

            // 只有目标在摄像机前方(z>0)时才高亮
            if (shouldHighlight && screenPos.z > 0)
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
        
        private void OnDisable()
        {
            if (!_isHighlighted)
                return;
            _isHighlighted = false;
            OnHighlightStateChanged(false);
            HighlightManager.Instance?.CloseHighlight(this);
        }

        // 检查是否满足高亮条件：距离、以及可视（非被遮挡）
        public bool CheckHighlightCondition(Camera cam)
        {
            Player player = GameplayManager.Instance?.Player;
            if (!player) return false;
            float dist = Vector3.Distance(transform.position, player.GetWorldPosition());
            if (dist > highlightDistance)
                return false;

            // 如果不开启遮挡检测，则仅基于距离判断
            if (!useOcclusionCheck)
                return true;

            // 只在一定间隔内做一次射线检测，结果缓存用于其余帧
            if (Time.time - _lastOcclusionCheckTime > occlusionCheckInterval)
            {
                _lastOcclusionCheckTime = Time.time;

                // 从摄像机（优先）或玩家视点向子弹做射线，判断中间是否有遮挡物
                Vector3 origin;
                if (cam != null)
                    origin = cam.transform.position;
                else
                {
                    Camera pCam = player.GetPlayerCamera();
                    origin = pCam != null ? pCam.transform.position : player.GetWorldPosition();
                }

                Vector3 dir = transform.position - origin;
                float maxDist = dir.magnitude;
                if (maxDist <= 0f)
                {
                    _cachedOccluded = false;
                    return true;
                }

                // 只检测到子弹位置之前的第一个碰撞体
                if (Physics.Raycast(origin, dir, out RaycastHit hit, maxDist, occlusionMask, QueryTriggerInteraction.Ignore))
                {
                    // 如果射线首先击中了不是自己（或自己子对象）的物体，则认为被遮挡
                    if (hit.collider != null)
                    {
                        GameObject hitGo = hit.collider.gameObject;
                        if (hitGo != gameObject && !hitGo.transform.IsChildOf(transform))
                            _cachedOccluded = true;
                        else
                            _cachedOccluded = false;
                    }
                    else
                    {
                        _cachedOccluded = false;
                    }
                }
                else
                {
                    // 没有击中任何物体，则说明可见
                    _cachedOccluded = false;
                }
            }

            return !_cachedOccluded;
        }

        // 实现接口所需的无参方法，向后兼容：使用玩家摄像机作为射线起点
        public bool CheckHighlightCondition()
        {
            Camera cam = GameplayManager.Instance?.Player?.GetPlayerCamera();
            return CheckHighlightCondition(cam);
        }

        public Vector3 GetScreenPosition(Camera cam)
        {
            return cam.WorldToScreenPoint(transform.position);
        }

        public void OnHighlightStateChanged(bool isHighlighted)
        {
            // 可在此处处理高亮状态变化时的本地逻辑（如特效、声音等）
            /*if (isHighlighted)
            {
                Debug.Log($"[BulletHighlight] 高亮开启: {gameObject.name}");
            }
            else
            {
                Debug.Log($"[BulletHighlight] 高亮关闭: {gameObject.name}");
            }*/
        }

        // 返回自身和对象池key
        public (GameObject bullet, string poolKey) GetBulletAndPoolKey()
        {
            return (gameObject, referencePoolKey);
        }
    }
}
