using System.Collections.Generic;
using UI;
using UnityEngine;

namespace CharacterUniversal
{
    public class HighlightManager : MonoSingleton<HighlightManager>
    {
        private readonly Dictionary<IHighlightInViewport, IHighlightUI> _highlightUIDict = new Dictionary<IHighlightInViewport, IHighlightUI>();

        private void OnEnable()
        {
            if (GameplayManager.Instance != null)
                GameplayManager.Instance.OnStatusChanged += OnGameplayStatusChanged;
        }
    
        private void OnDisable()
        {
            if (GameplayManager.Instance != null)
                GameplayManager.Instance.OnStatusChanged -= OnGameplayStatusChanged;
        }
    
        private void OnGameplayStatusChanged(EGameplayStatus status)
        {
            if (status == EGameplayStatus.Default || (GameplayManager.Instance != null && GameplayManager.Instance.IsTerminalState))
            {
                ClearAllHighlights();
            }
        }
    
        // 更新和创建高亮 UI
        public void UpdateHighlight(IHighlightInViewport highlightObj, Vector3 screenPos)
        {
            float distanceRatio = CalculateDistanceRatio(highlightObj);

            if (!_highlightUIDict.TryGetValue(highlightObj, out IHighlightUI value))
            {
                // 用多实例接口，而不是 ShowUI（单例）
                string prefabName = highlightObj.HighlightUIPrefabName;
                if (UIManager.Instance.CreateUIInstance(prefabName) is not IHighlightUI ui)
                {
                    // Debug.LogWarning($"[HighlightManager] 无法创建 {prefabName} UI，请检查预制体是否挂了 IHighlightUI / UIBase。");
                    return;
                }

                _highlightUIDict[highlightObj] = ui;
                ui.SetPosition(screenPos);
                ui.SetDistanceRatio(distanceRatio);
            }
            else
            {
                value.SetPosition(screenPos);
                value.SetDistanceRatio(distanceRatio);
            }
        }

        private static float CalculateDistanceRatio(IHighlightInViewport highlightObj)
        {
            if (GameplayManager.Instance == null)
                return 0f;

            var player = GameplayManager.Instance.Player;
            if (!player)
                return 0f;

            if (highlightObj is not Component highlightComponent)
                return 0f;

            float minDistance = Mathf.Max(0f, highlightObj.HighlightMinDistance);
            float maxDistance = Mathf.Max(minDistance, highlightObj.HighlightMaxDistance);
            float distance = Vector3.Distance(highlightComponent.transform.position, player.GetWorldPosition());

            if (Mathf.Approximately(minDistance, maxDistance))
                return 0f;

            return Mathf.Clamp01(Mathf.InverseLerp(minDistance, maxDistance, distance));
        }

        // 关闭高亮 UI
        public void CloseHighlight(IHighlightInViewport highlightObj)
        {
            if (_highlightUIDict.TryGetValue(highlightObj, out IHighlightUI ui))
            {
                // 因为是多实例，所以这里不能用 HideUI("HighlightRing")
                // 需要直接销毁这个具体实例
                UIManager.Instance.DestroyUIInstance(ui as UIBase);

                _highlightUIDict.Remove(highlightObj);
                // Debug.Log($"[HighlightManager] 移除高亮UI: {highlightObj}");
            }
        }
    
        // 清除所有高亮 UI
        private void ClearAllHighlights()
        {
            foreach (KeyValuePair<IHighlightInViewport, IHighlightUI> kv in _highlightUIDict)
            {
                // 这里要销毁每一个具体实例，而不是按名字 Hide
                UIBase uiBase = kv.Value as UIBase;
                if (uiBase != null)
                {
                    UIManager.Instance.DestroyUIInstance(uiBase);
                }

                // Debug.Log($"[HighlightManager] 清除高亮UI: {kv.Key}");
            }

            _highlightUIDict.Clear();
        }

        // 通过UI查找对应的高亮对象
        public IHighlightInViewport GetHighlightOwner(IHighlightUI ui)
        {
            foreach (KeyValuePair<IHighlightInViewport, IHighlightUI> kv in _highlightUIDict)
            {
                if (kv.Value == ui)
                    return kv.Key;
            }
            return null;
        }
    }
}
