using System.Collections.Generic;
using UnityEngine;
using Player;
using UI;

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
            IHighlightUI ui = UIManager.Instance.ShowUI("HighlightRing") as IHighlightUI;
            highlightUIDict[highlightObj] = ui;
            ui?.SetPosition(screenPos);
        }
        else
        {
            highlightUIDict[highlightObj]?.SetPosition(screenPos);
        }
    }

    // 关闭高亮 UI
    public void CloseHighlight(IHighlightInViewport highlightObj)
    {
        if (highlightUIDict.TryGetValue(highlightObj, out IHighlightUI ui))
        {
            UIManager.Instance.HideUI("HighlightRing");
            highlightUIDict.Remove(highlightObj);
            Debug.Log($"[HighlightManager] 移除高亮UI: {highlightObj}");
        }
    }
    
    // 清除所有高亮 UI
    private void ClearAllHighlights()
    {
        foreach (KeyValuePair<IHighlightInViewport, IHighlightUI> kv in highlightUIDict)
        {
            UIManager.Instance.HideUI("HighlightRing");
            Debug.Log($"[HighlightManager] 清除高亮UI: {kv.Key}");
        }
        highlightUIDict.Clear();
    }

    // 通过UI查找对应的高亮对象
    public IHighlightInViewport GetHighlightOwner(IHighlightUI ui)
    {
        foreach (KeyValuePair<IHighlightInViewport, IHighlightUI> kv in highlightUIDict)
        {
            if (kv.Value == ui)
                return kv.Key;
        }
        return null;
    }
}
