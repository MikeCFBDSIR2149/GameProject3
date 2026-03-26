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
    
    private void OnGameplayStatusChanged(EGameplayStatus status)
    {
        if (status == EGameplayStatus.Default)
        {
            ClearAllHighlights();
        }
    }
    
    // 更新和创建高亮 UI
    public void UpdateHighlight(IHighlightInViewport highlightObj, Vector3 screenPos)
    {
        if (!highlightUIDict.TryGetValue(highlightObj, out IHighlightUI value))
        {
            // 用多实例接口，而不是 ShowUI（单例）
            string prefabName = highlightObj.HighlightUIPrefabName;
            if (UIManager.Instance.CreateUIInstance(prefabName) is not IHighlightUI ui)
            {
                Debug.LogWarning($"[HighlightManager] 无法创建 {prefabName} UI，请检查预制体是否挂了 IHighlightUI / UIBase。");
                return;
            }

            highlightUIDict[highlightObj] = ui;
            ui.SetPosition(screenPos);
        }
        else
        {
            value.SetPosition(screenPos);
        }
    }

    // 关闭高亮 UI
    public void CloseHighlight(IHighlightInViewport highlightObj)
    {
        if (highlightUIDict.TryGetValue(highlightObj, out IHighlightUI ui))
        {
            // 因为是多实例，所以这里不能用 HideUI("HighlightRing")
            // 需要直接销毁这个具体实例
            UIManager.Instance.DestroyUIInstance(ui as UIBase);

            highlightUIDict.Remove(highlightObj);
            Debug.Log($"[HighlightManager] 移除高亮UI: {highlightObj}");
        }
    }
    
    // 清除所有高亮 UI
    private void ClearAllHighlights()
    {
        foreach (KeyValuePair<IHighlightInViewport, IHighlightUI> kv in highlightUIDict)
        {
            // 这里要销毁每一个具体实例，而不是按名字 Hide
            UIBase uiBase = kv.Value as UIBase;
            if (uiBase != null)
            {
                UIManager.Instance.DestroyUIInstance(uiBase);
            }

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
