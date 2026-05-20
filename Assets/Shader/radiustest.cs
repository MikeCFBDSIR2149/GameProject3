using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Reflection;

public class AORadiusController : MonoBehaviour
{
    [Header("核心引用")]
    public ScriptableRendererData rendererData;

    [Header("半径数值")]
    public float radiusMin = 0.22f;
    public float radiusMax = 1.5f;
    
    [Header("平滑速度")]
    public float changeSpeed = 3f;

    [Header("按键设置")]
    public KeyCode toggleKey = KeyCode.Q;

    // 内部状态
    private float currentRadius;
    private float targetRadius;
    private bool isAtMax = false;

    // 反射缓存
    private ScriptableRendererFeature ssaoFeature;
    private FieldInfo settingsField;
    private FieldInfo radiusField;

    void Awake()
    {
        currentRadius = radiusMin;
        targetRadius = radiusMin;
        InitReflection();
    }

    // 按键
    void OnGUI()
    {
        Event e = Event.current;
        if (e.isKey && e.type == EventType.KeyDown && e.keyCode == toggleKey)
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        isAtMax = !isAtMax;
        targetRadius = isAtMax ? radiusMax : radiusMin;
        Debug.Log($"<color=cyan>【AO Controller】状态切换至: {(isAtMax ? "最大" : "最小")}</color>");
    }

    void Update()
    {
        if (ssaoFeature == null || radiusField == null) return;

        // 平滑数值
        if (!Mathf.Approximately(currentRadius, targetRadius))
        {
            currentRadius = Mathf.MoveTowards(currentRadius, targetRadius, Time.deltaTime * changeSpeed);
            ApplyValue(currentRadius);
        }
    }

    // 退出时还原radiusa到默认是

    // 1. 当游戏退出或停止播放时触发
    void OnApplicationQuit()
    {
        ResetToDefault();
    }

    // 2. 当脚本物体被禁用或销毁时触发（双重保险）
    void OnDisable()
    {
        ResetToDefault();
    }

    // 直接将数值设为最小值并写入 URP
    private void ResetToDefault()
    {
        if (ssaoFeature != null && radiusField != null)
        {
            ApplyValue(radiusMin);
            // Debug.Log("<color=orange>【AO Controller】已自动还原为默认半径: " + radiusMin + "</color>");
        }
    }

    private void ApplyValue(float val)
    {
        if (ssaoFeature == null) return;
        
        // 反射修改：取出副本 -> 修改副本 -> 覆盖原位
        object settingsCopy = settingsField.GetValue(ssaoFeature);
        radiusField.SetValue(settingsCopy, val);
        settingsField.SetValue(ssaoFeature, settingsCopy);

        // 如果在编辑器中，强制刷新一下资源，虽然不是必须但更稳定
#if UNITY_EDITOR
        if (!Application.isPlaying) 
        {
            UnityEditor.EditorUtility.SetDirty(rendererData);
        }
#endif
    }

    private bool InitReflection()
    {
        if (rendererData == null) return false;

        foreach (var feature in rendererData.rendererFeatures)
        {
            if (feature != null && feature.GetType().Name.Contains("ScreenSpaceAmbientOcclusion"))
            {
                ssaoFeature = feature;
                settingsField = feature.GetType().GetField("m_Settings", BindingFlags.Instance | BindingFlags.NonPublic);
                if (settingsField != null)
                {
                    var settingsType = settingsField.FieldType;
                    radiusField = settingsType.GetField("Radius", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                ?? settingsType.GetField("radius", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                }
                break;
            }
        }

        if (ssaoFeature != null && radiusField != null)
        {
            Debug.Log("<color=green>【AO】初始化成功！</color>");
            return true;
        }
        return false;
    }
}