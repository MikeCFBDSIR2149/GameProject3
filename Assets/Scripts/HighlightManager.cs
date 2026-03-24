using System.Collections.Generic;
using UnityEngine;
using Player;

public class HighlightManager : MonoSingleton<HighlightManager>
{
    private readonly Dictionary<IHighlightInViewport, IHighlightUI> highlightUIDict = new Dictionary<IHighlightInViewport, IHighlightUI>();

    private void OnEnable()
    {
        GameplayManager.Instance.OnStatusChanged += OnGameplayStatusChanged;
    }
    
    private void OnDisable()
    {
        if (GameplayManager.Instance != null)
            GameplayManager.Instance.OnStatusChanged -= OnGameplayStatusChanged;
    }
    
    private void OnGameplayStatusChanged(GameplayStatus status)
    {
        if (status == GameplayStatus.Default)
        {
            ClearAllHighlights();
        }
    }
    
    // 更新和创建高亮 UI
    public void UpdateHighlight(IHighlightInViewport highlightObj, Vector3 screenPos)
    {
        if (!highlightUIDict.ContainsKey(highlightObj))
        {
            // TODO: 创建UI实例，替换null为实际IHighlightUI实现
            IHighlightUI ui = null;
            highlightUIDict[highlightObj] = ui;
            Debug.Log($"[HighlightManager] 创建高亮UI: {highlightObj} at {screenPos}");
        }
        else
        {
            highlightUIDict[highlightObj]?.SetPosition(screenPos);
            Debug.Log($"[HighlightManager] 更新高亮UI: {highlightObj} at {screenPos}");
        }
    }

    // 关闭高亮 UI
    public void CloseHighlight(IHighlightInViewport highlightObj)
    {
        if (highlightUIDict.TryGetValue(highlightObj, out var ui))
        {
            ui?.Dispose();
            highlightUIDict.Remove(highlightObj);
            Debug.Log($"[HighlightManager] 移除高亮UI: {highlightObj}");
        }
    }
    
    // 清除所有高亮 UI
    private void ClearAllHighlights()
    {
        foreach (var kv in highlightUIDict)
        {
            kv.Value?.Dispose();
            Debug.Log($"[HighlightManager] 清除高亮UI: {kv.Key}");
        }
        highlightUIDict.Clear();
    }
}
