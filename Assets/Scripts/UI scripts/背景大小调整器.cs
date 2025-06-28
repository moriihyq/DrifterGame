using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 背景大小调整器 - 简单调整背景图层大小
/// </summary>
public class 背景大小调整器 : MonoBehaviour
{
    [Header("背景大小设置")]
    [Tooltip("背景缩放倍数 (推荐: 1.0-2.0)")]
    [Range(0.5f, 3.0f)]
    public float backgroundScale = 1.2f;
    
    [Tooltip("是否保持宽高比")]
    public bool maintainAspectRatio = true;
    
    [Header("微调设置")]
    [Tooltip("水平缩放微调")]
    [Range(0.5f, 2.0f)]
    public float horizontalScale = 1.0f;
    
    [Tooltip("垂直缩放微调")]
    [Range(0.5f, 2.0f)]
    public float verticalScale = 1.0f;
    
    /// <summary>
    /// 一键调整背景大小为适中
    /// </summary>
    [ContextMenu("🎯 调整为适中大小")]
    public void SetModerateSizing()
    {
        backgroundScale = 1.2f;
        ApplyBackgroundScale();
        Debug.Log("<color=green>✅ 背景大小已调整为适中 (1.2倍)</color>");
    }
    
    /// <summary>
    /// 一键调整背景大小为紧凑
    /// </summary>
    [ContextMenu("📐 调整为紧凑大小")]
    public void SetCompactSizing()
    {
        backgroundScale = 1.0f;
        ApplyBackgroundScale();
        Debug.Log("<color=green>✅ 背景大小已调整为紧凑 (1.0倍)</color>");
    }
    
    /// <summary>
    /// 一键调整背景大小为最小
    /// </summary>
    [ContextMenu("🔍 调整为最小大小")]
    public void SetMinimalSizing()
    {
        backgroundScale = 0.8f;
        ApplyBackgroundScale();
        Debug.Log("<color=green>✅ 背景大小已调整为最小 (0.8倍)</color>");
    }
    
    /// <summary>
    /// 应用背景缩放
    /// </summary>
    [ContextMenu("✨ 应用当前设置")]
    public void ApplyBackgroundScale()
    {
        Debug.Log("<color=cyan>开始调整背景大小...</color>");
        
        GameObject backgroundParent = GameObject.Find("BackgroundParallax");
        if (backgroundParent == null)
        {
            Debug.LogError("<color=red>❌ 找不到BackgroundParallax对象！</color>");
            return;
        }
        
        Camera mainCamera = Camera.main ?? FindObjectOfType<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("<color=red>❌ 找不到摄像机！</color>");
            return;
        }
        
        // 计算基础所需尺寸（不使用过大的扩展倍数）
        CalculateBasicRequiredSize(mainCamera, out float baseWidth, out float baseHeight);
        
        // 应用用户设置的缩放
        float targetWidth = baseWidth * backgroundScale;
        float targetHeight = baseHeight * backgroundScale;
        
        // 调整每个背景层
        int layerCount = 0;
        for (int i = 0; i < backgroundParent.transform.childCount; i++)
        {
            Transform child = backgroundParent.transform.GetChild(i);
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            
            if (sr != null && sr.sprite != null)
            {
                ScaleBackgroundLayer(sr, targetWidth, targetHeight);
                layerCount++;
            }
        }
        
        Debug.Log($"<color=green>✅ 已调整 {layerCount} 个背景层的大小！</color>");
        Debug.Log($"<color=yellow>背景缩放倍数: {backgroundScale:F1}x</color>");
    }
    
    /// <summary>
    /// 计算基础所需尺寸（合理的大小，不过度扩展）
    /// </summary>
    private void CalculateBasicRequiredSize(Camera camera, out float requiredWidth, out float requiredHeight)
    {
        if (camera.orthographic)
        {
            // 正交摄像机
            requiredHeight = camera.orthographicSize * 2f;
            requiredWidth = requiredHeight * camera.aspect;
        }
        else
        {
            // 透视摄像机
            float distance = Mathf.Abs(camera.transform.position.z - transform.position.z);
            requiredHeight = 2f * distance * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            requiredWidth = requiredHeight * camera.aspect;
        }
        
        Debug.Log($"<color=cyan>基础所需尺寸: {requiredWidth:F1} x {requiredHeight:F1}</color>");
    }
    
    /// <summary>
    /// 缩放背景图层
    /// </summary>
    private void ScaleBackgroundLayer(SpriteRenderer spriteRenderer, float targetWidth, float targetHeight)
    {
        Sprite sprite = spriteRenderer.sprite;
        if (sprite == null) return;
        
        // 计算精灵的实际尺寸
        float spriteWidth = sprite.bounds.size.x;
        float spriteHeight = sprite.bounds.size.y;
        
        float scaleX, scaleY;
        
        if (maintainAspectRatio)
        {
            // 保持宽高比，使用较大的缩放比例确保覆盖
            scaleX = targetWidth / spriteWidth;
            scaleY = targetHeight / spriteHeight;
            float uniformScale = Mathf.Max(scaleX, scaleY);
            
            // 应用微调
            scaleX = uniformScale * horizontalScale;
            scaleY = uniformScale * verticalScale;
        }
        else
        {
            // 分别缩放宽高
            scaleX = (targetWidth / spriteWidth) * horizontalScale;
            scaleY = (targetHeight / spriteHeight) * verticalScale;
        }
        
        // 应用缩放
        Vector3 newScale = new Vector3(scaleX, scaleY, 1f);
        spriteRenderer.transform.localScale = newScale;
        
        Debug.Log($"<color=green>✅ {spriteRenderer.name}</color>");
        Debug.Log($"<color=white>   缩放: {scaleX:F2} x {scaleY:F2}</color>");
        Debug.Log($"<color=white>   最终尺寸: {spriteWidth * scaleX:F1} x {spriteHeight * scaleY:F1}</color>");
    }
    
    /// <summary>
    /// 重置所有背景到原始大小
    /// </summary>
    [ContextMenu("🔄 重置为原始大小")]
    public void ResetToOriginalSize()
    {
        Debug.Log("<color=cyan>重置背景到原始大小...</color>");
        
        GameObject backgroundParent = GameObject.Find("BackgroundParallax");
        if (backgroundParent == null)
        {
            Debug.LogError("<color=red>❌ 找不到BackgroundParallax对象！</color>");
            return;
        }
        
        int layerCount = 0;
        for (int i = 0; i < backgroundParent.transform.childCount; i++)
        {
            Transform child = backgroundParent.transform.GetChild(i);
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            
            if (sr != null)
            {
                sr.transform.localScale = Vector3.one;
                layerCount++;
            }
        }
        
        Debug.Log($"<color=green>✅ 已重置 {layerCount} 个背景层到原始大小</color>");
    }
    
    /// <summary>
    /// 显示当前背景信息
    /// </summary>
    [ContextMenu("ℹ️ 显示背景信息")]
    public void ShowBackgroundInfo()
    {
        Debug.Log("<color=cyan>=== 背景大小信息 ===</color>");
        
        GameObject backgroundParent = GameObject.Find("BackgroundParallax");
        if (backgroundParent == null)
        {
            Debug.LogError("<color=red>❌ 找不到BackgroundParallax对象！</color>");
            return;
        }
        
        for (int i = 0; i < backgroundParent.transform.childCount; i++)
        {
            Transform child = backgroundParent.transform.GetChild(i);
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            
            if (sr != null && sr.sprite != null)
            {
                Vector3 scale = sr.transform.localScale;
                Bounds bounds = sr.bounds;
                
                Debug.Log($"<color=white>{child.name}:</color>");
                Debug.Log($"<color=white>   缩放: {scale.x:F2} x {scale.y:F2}</color>");
                Debug.Log($"<color=white>   实际尺寸: {bounds.size.x:F1} x {bounds.size.y:F1}</color>");
            }
        }
        
        Debug.Log("<color=cyan>=== 信息显示完成 ===</color>");
    }
}

#if UNITY_EDITOR
/// <summary>
/// 编辑器菜单
/// </summary>
public static class BackgroundSizeMenu
{
    [MenuItem("Tools/背景设置/🎯 调整为适中大小")]
    public static void SetModerateSizing()
    {
        var adjuster = FindOrCreateAdjuster();
        adjuster.SetModerateSizing();
        CleanupAdjuster(adjuster);
    }
    
    [MenuItem("Tools/背景设置/📐 调整为紧凑大小")]
    public static void SetCompactSizing()
    {
        var adjuster = FindOrCreateAdjuster();
        adjuster.SetCompactSizing();
        CleanupAdjuster(adjuster);
    }
    
    [MenuItem("Tools/背景设置/🔍 调整为最小大小")]
    public static void SetMinimalSizing()
    {
        var adjuster = FindOrCreateAdjuster();
        adjuster.SetMinimalSizing();
        CleanupAdjuster(adjuster);
    }
    
    [MenuItem("Tools/背景设置/🔄 重置背景大小")]
    public static void ResetBackgroundSize()
    {
        var adjuster = FindOrCreateAdjuster();
        adjuster.ResetToOriginalSize();
        CleanupAdjuster(adjuster);
    }
    
    [MenuItem("Tools/背景设置/ℹ️ 显示背景信息")]
    public static void ShowBackgroundInfo()
    {
        var adjuster = FindOrCreateAdjuster();
        adjuster.ShowBackgroundInfo();
        CleanupAdjuster(adjuster);
    }
    
    private static 背景大小调整器 FindOrCreateAdjuster()
    {
        var adjuster = Object.FindObjectOfType<背景大小调整器>();
        if (adjuster == null)
        {
            GameObject tempObj = new GameObject("TempBackgroundSizeAdjuster");
            adjuster = tempObj.AddComponent<背景大小调整器>();
        }
        return adjuster;
    }
    
    private static void CleanupAdjuster(背景大小调整器 adjuster)
    {
        if (adjuster.gameObject.name == "TempBackgroundSizeAdjuster")
        {
            Object.DestroyImmediate(adjuster.gameObject);
        }
    }
}
#endif 