using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 背景填充修复器 - 确保背景始终填满镜头
/// </summary>
public class 背景填充修复器 : MonoBehaviour
{
    [Header("填充设置")]
    [Tooltip("额外扩展倍数")]
    [Range(1.5f, 5f)]
    public float expansionMultiplier = 1.5f;
    
    [Tooltip("是否启用无缝循环")]
    public bool enableSeamlessLoop = true;
    
    [Tooltip("循环边界缓冲")]
    public float loopBuffer = 10f;
    
    /// <summary>
    /// 一键修复背景填充问题
    /// </summary>
    [ContextMenu("🖼️ 一键修复背景填充")]
    public void FixBackgroundFilling()
    {
        Debug.Log("<color=cyan>开始修复背景填充问题...</color>");
        
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
        
        // 计算所需的背景尺寸
        CalculateRequiredSize(mainCamera, out float requiredWidth, out float requiredHeight);
        
        // 修复每个背景层
        int layerCount = 0;
        for (int i = 0; i < backgroundParent.transform.childCount; i++)
        {
            Transform child = backgroundParent.transform.GetChild(i);
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            
            if (sr != null && sr.sprite != null)
            {
                ScaleBackgroundLayer(sr, requiredWidth, requiredHeight);
                
                // 如果启用无缝循环，添加循环组件
                if (enableSeamlessLoop)
                {
                    AddSeamlessLoop(child.gameObject, sr);
                }
                
                layerCount++;
            }
        }
        
        Debug.Log($"<color=green>✅ 已修复 {layerCount} 个背景层的填充问题！</color>");
        Debug.Log("<color=yellow>背景现在应该能完全填满镜头了</color>");
    }
    
    /// <summary>
    /// 计算所需的背景尺寸
    /// </summary>
    private void CalculateRequiredSize(Camera camera, out float requiredWidth, out float requiredHeight)
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
        
        // 应用扩展倍数确保完全覆盖
        requiredWidth *= expansionMultiplier;
        requiredHeight *= expansionMultiplier;
        
        Debug.Log($"<color=cyan>计算所需尺寸: {requiredWidth:F1} x {requiredHeight:F1}</color>");
    }
    
    /// <summary>
    /// 缩放背景图层
    /// </summary>
    private void ScaleBackgroundLayer(SpriteRenderer spriteRenderer, float targetWidth, float targetHeight)
    {
        Sprite sprite = spriteRenderer.sprite;
        if (sprite == null) return;
        
        // 计算精灵的实际尺寸（考虑PPU）
        float spriteWidth = sprite.bounds.size.x;
        float spriteHeight = sprite.bounds.size.y;
        
        // 计算所需的缩放比例
        float scaleX = targetWidth / spriteWidth;
        float scaleY = targetHeight / spriteHeight;
        
        // 使用较大的缩放比例确保完全覆盖
        float uniformScale = Mathf.Max(scaleX, scaleY);
        
        // 应用缩放
        Vector3 newScale = Vector3.one * uniformScale;
        spriteRenderer.transform.localScale = newScale;
        
        Debug.Log($"<color=green>✅ {spriteRenderer.name} 缩放为: {newScale.x:F2}</color>");
        Debug.Log($"<color=white>   原始尺寸: {spriteWidth:F1}x{spriteHeight:F1}</color>");
        Debug.Log($"<color=white>   缩放后尺寸: {spriteWidth * uniformScale:F1}x{spriteHeight * uniformScale:F1}</color>");
    }
    
    /// <summary>
    /// 添加无缝循环功能
    /// </summary>
    private void AddSeamlessLoop(GameObject layerObject, SpriteRenderer spriteRenderer)
    {
        BackgroundSeamlessLoop loopComponent = layerObject.GetComponent<BackgroundSeamlessLoop>();
        if (loopComponent == null)
        {
            loopComponent = layerObject.AddComponent<BackgroundSeamlessLoop>();
        }
        
        loopComponent.spriteRenderer = spriteRenderer;
        loopComponent.loopBuffer = loopBuffer;
        
        Debug.Log($"<color=cyan>为 {layerObject.name} 添加无缝循环功能</color>");
    }
    
    /// <summary>
    /// 自动调整所有背景层尺寸
    /// </summary>
    [ContextMenu("📐 自动调整背景尺寸")]
    public void AutoAdjustBackgroundSizes()
    {
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
        
        CalculateRequiredSize(mainCamera, out float requiredWidth, out float requiredHeight);
        
        for (int i = 0; i < backgroundParent.transform.childCount; i++)
        {
            Transform child = backgroundParent.transform.GetChild(i);
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            
            if (sr != null && sr.sprite != null)
            {
                ScaleBackgroundLayer(sr, requiredWidth, requiredHeight);
            }
        }
        
        Debug.Log("<color=green>✅ 所有背景层尺寸调整完成！</color>");
    }
    
    /// <summary>
    /// 测试背景覆盖范围
    /// </summary>
    [ContextMenu("🧪 测试背景覆盖")]
    public void TestBackgroundCoverage()
    {
        Debug.Log("<color=cyan>测试背景覆盖范围...</color>");
        
        Camera mainCamera = Camera.main ?? FindObjectOfType<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("<color=red>❌ 找不到摄像机！</color>");
            return;
        }
        
        CalculateRequiredSize(mainCamera, out float requiredWidth, out float requiredHeight);
        
        GameObject backgroundParent = GameObject.Find("BackgroundParallax");
        if (backgroundParent == null) return;
        
        for (int i = 0; i < backgroundParent.transform.childCount; i++)
        {
            Transform child = backgroundParent.transform.GetChild(i);
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            
            if (sr != null && sr.sprite != null)
            {
                float actualWidth = sr.bounds.size.x;
                float actualHeight = sr.bounds.size.y;
                
                bool widthOK = actualWidth >= requiredWidth * 0.9f;
                bool heightOK = actualHeight >= requiredHeight * 0.9f;
                
                string status = (widthOK && heightOK) ? "✅ 充足" : "❌ 不足";
                Debug.Log($"<color=white>{child.name}: {status}</color>");
                Debug.Log($"<color=white>   需要: {requiredWidth:F1}x{requiredHeight:F1}</color>");
                Debug.Log($"<color=white>   实际: {actualWidth:F1}x{actualHeight:F1}</color>");
            }
        }
        
        Debug.Log("<color=cyan>=== 覆盖测试完成 ===</color>");
    }
}

/// <summary>
/// 背景无缝循环组件
/// </summary>
public class BackgroundSeamlessLoop : MonoBehaviour
{
    [Header("循环设置")]
    [Tooltip("精灵渲染器")]
    public SpriteRenderer spriteRenderer;
    
    [Tooltip("循环边界缓冲")]
    public float loopBuffer = 10f;
    
    [Tooltip("是否启用水平循环")]
    public bool enableHorizontalLoop = true;
    
    [Tooltip("是否启用垂直循环")]
    public bool enableVerticalLoop = false;
    
    private Camera targetCamera;
    private float spriteWidth;
    private float spriteHeight;
    private Vector3 startPosition;
    
    void Start()
    {
        targetCamera = Camera.main ?? FindObjectOfType<Camera>();
        
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            spriteWidth = spriteRenderer.bounds.size.x;
            spriteHeight = spriteRenderer.bounds.size.y;
            startPosition = transform.position;
            
            Debug.Log($"<color=green>[无缝循环] {name} 初始化完成</color>");
        }
    }
    
    void LateUpdate()
    {
        if (targetCamera == null || spriteRenderer == null) return;
        
        Vector3 cameraPos = targetCamera.transform.position;
        Vector3 currentPos = transform.position;
        
        // 水平循环
        if (enableHorizontalLoop)
        {
            float distanceX = cameraPos.x - currentPos.x;
            if (distanceX > spriteWidth / 2f + loopBuffer)
            {
                currentPos.x += spriteWidth;
            }
            else if (distanceX < -spriteWidth / 2f - loopBuffer)
            {
                currentPos.x -= spriteWidth;
            }
        }
        
        // 垂直循环
        if (enableVerticalLoop)
        {
            float distanceY = cameraPos.y - currentPos.y;
            if (distanceY > spriteHeight / 2f + loopBuffer)
            {
                currentPos.y += spriteHeight;
            }
            else if (distanceY < -spriteHeight / 2f - loopBuffer)
            {
                currentPos.y -= spriteHeight;
            }
        }
        
        transform.position = currentPos;
    }
}

#if UNITY_EDITOR
/// <summary>
/// 编辑器菜单
/// </summary>
public static class BackgroundFillingMenu
{
    [MenuItem("Tools/背景设置/🖼️ 一键修复背景填充")]
    public static void FixBackgroundFilling()
    {
        var fixer = FindOrCreateFixer();
        fixer.FixBackgroundFilling();
        CleanupFixer(fixer);
    }
    
    [MenuItem("Tools/背景设置/📐 自动调整背景尺寸")]
    public static void AutoAdjustSizes()
    {
        var fixer = FindOrCreateFixer();
        fixer.AutoAdjustBackgroundSizes();
        CleanupFixer(fixer);
    }
    
    [MenuItem("Tools/背景设置/🧪 测试背景覆盖")]
    public static void TestCoverage()
    {
        var fixer = FindOrCreateFixer();
        fixer.TestBackgroundCoverage();
        CleanupFixer(fixer);
    }
    
    private static 背景填充修复器 FindOrCreateFixer()
    {
        var fixer = Object.FindObjectOfType<背景填充修复器>();
        if (fixer == null)
        {
            GameObject tempObj = new GameObject("TempBackgroundFillingFixer");
            fixer = tempObj.AddComponent<背景填充修复器>();
        }
        return fixer;
    }
    
    private static void CleanupFixer(背景填充修复器 fixer)
    {
        if (fixer.gameObject.name == "TempBackgroundFillingFixer")
        {
            Object.DestroyImmediate(fixer.gameObject);
        }
    }
}
#endif 