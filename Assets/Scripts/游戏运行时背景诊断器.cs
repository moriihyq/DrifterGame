using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 游戏运行时背景诊断器 - 专门解决游戏中背景不显示的问题
/// </summary>
public class 游戏运行时背景诊断器 : MonoBehaviour
{
    [Header("实时监控")]
    [Tooltip("是否在游戏运行时实时显示信息")]
    public bool showRuntimeInfo = true;
    
    [Tooltip("监控更新频率（秒）")]
    public float updateInterval = 2f;
    
    private float lastUpdateTime;
    
    void Start()
    {
        if (showRuntimeInfo)
        {
            Debug.Log("<color=cyan>🎮 游戏运行时背景诊断器已启动</color>");
            DiagnoseRuntimeBackground();
        }
    }
    
    void Update()
    {
        if (showRuntimeInfo && Time.time - lastUpdateTime > updateInterval)
        {
            lastUpdateTime = Time.time;
            QuickRuntimeCheck();
        }
    }
    
    /// <summary>
    /// 游戏运行时背景诊断
    /// </summary>
    [ContextMenu("🎮 诊断游戏运行时背景")]
    public void DiagnoseRuntimeBackground()
    {
        Debug.Log("<color=cyan>===== 🎮 游戏运行时背景诊断 =====</color>");
        
        // 1. 检查背景对象
        GameObject backgroundParent = GameObject.Find("BackgroundParallax");
        if (backgroundParent == null)
        {
            Debug.LogError("<color=red>❌ 找不到BackgroundParallax对象！</color>");
            return;
        }
        
        // 2. 检查摄像机信息
        Camera mainCamera = Camera.main ?? FindObjectOfType<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("<color=red>❌ 找不到摄像机！</color>");
            return;
        }
        
        Debug.Log($"<color=green>✅ 摄像机: {mainCamera.name}</color>");
        Debug.Log($"<color=white>  游戏摄像机位置: {mainCamera.transform.position}</color>");
        Debug.Log($"<color=white>  背景位置: {backgroundParent.transform.position}</color>");
        
        // 3. 计算摄像机视野范围
        Bounds cameraViewBounds = GetCameraViewBounds(mainCamera);
        Debug.Log($"<color=white>  摄像机视野范围: {cameraViewBounds}</color>");
        
        // 4. 检查背景是否在摄像机视野内
        bool anyBackgroundVisible = false;
        for (int i = 0; i < backgroundParent.transform.childCount; i++)
        {
            Transform child = backgroundParent.transform.GetChild(i);
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            
            if (sr != null && sr.sprite != null)
            {
                Bounds backgroundBounds = sr.bounds;
                bool isVisible = cameraViewBounds.Intersects(backgroundBounds);
                
                Debug.Log($"<color=white>{child.name}:</color>");
                Debug.Log($"<color=white>  背景范围: {backgroundBounds}</color>");
                Debug.Log($"<color=white>  在视野内: {(isVisible ? "✅ 是" : "❌ 否")}</color>");
                Debug.Log($"<color=white>  排序层级: {sr.sortingLayerName} ({sr.sortingOrder})</color>");
                
                if (isVisible) anyBackgroundVisible = true;
            }
        }
        
        // 5. 给出诊断结果和建议
        Debug.Log("<color=cyan>===== 📋 诊断结果 =====</color>");
        if (!anyBackgroundVisible)
        {
            Debug.LogWarning("<color=yellow>⚠️ 背景不在摄像机视野内！</color>");
            Debug.Log("<color=cyan>💡 建议解决方案：</color>");
            Debug.Log("<color=white>1. 调整背景位置到摄像机视野内</color>");
            Debug.Log("<color=white>2. 增大背景尺寸</color>");
            Debug.Log("<color=white>3. 调整摄像机位置</color>");
        }
        else
        {
            Debug.Log("<color=green>✅ 背景在摄像机视野内</color>");
            Debug.Log("<color=yellow>如果还是看不到，可能是渲染层级问题</color>");
        }
        
        // 6. 检查Cinemachine（安全版本）
        CheckCinemachineIssuesSafe();
        
        Debug.Log("<color=cyan>===== 🎮 运行时诊断完成 =====</color>");
    }
    
    /// <summary>
    /// 快速运行时检查
    /// </summary>
    private void QuickRuntimeCheck()
    {
        GameObject backgroundParent = GameObject.Find("BackgroundParallax");
        Camera mainCamera = Camera.main ?? FindObjectOfType<Camera>();
        
        if (backgroundParent == null || mainCamera == null) return;
        
        float distance = Vector3.Distance(mainCamera.transform.position, backgroundParent.transform.position);
        if (distance > 100f)
        {
            Debug.LogWarning($"<color=yellow>⚠️ 运行时警告：摄像机距离背景过远 ({distance:F1})</color>");
        }
    }
    
    /// <summary>
    /// 获取摄像机视野范围
    /// </summary>
    private Bounds GetCameraViewBounds(Camera camera)
    {
        Vector3 center = camera.transform.position;
        Vector3 size;
        
        if (camera.orthographic)
        {
            float height = camera.orthographicSize * 2f;
            float width = height * camera.aspect;
            size = new Vector3(width, height, 1000f); // Z轴给一个大范围
        }
        else
        {
            // 透视摄像机，在一定距离处计算视野大小
            float distance = 20f; // 假设背景在摄像机前方20单位
            float height = 2f * distance * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float width = height * camera.aspect;
            size = new Vector3(width, height, 1000f);
        }
        
        return new Bounds(center, size);
    }
    
    /// <summary>
    /// 安全地检查Cinemachine相关问题（不会导致编译错误）
    /// </summary>
    private void CheckCinemachineIssuesSafe()
    {
        try
        {
            // 使用反射安全地检查Cinemachine
            var cinemachineType = System.Type.GetType("Cinemachine.CinemachineVirtualCamera, Cinemachine");
            if (cinemachineType != null)
            {
                var cinemachineCamera = FindObjectOfType(cinemachineType) as Component;
                if (cinemachineCamera != null)
                {
                    Debug.Log("<color=white>检测到Cinemachine虚拟摄像机...</color>");
                    Debug.Log($"<color=white>  虚拟摄像机位置: {cinemachineCamera.transform.position}</color>");
                    
                    // 尝试获取Follow属性
                    var followProperty = cinemachineType.GetProperty("Follow");
                    if (followProperty != null)
                    {
                        var followTarget = followProperty.GetValue(cinemachineCamera) as Transform;
                        Debug.Log($"<color=white>  跟随目标: {(followTarget != null ? followTarget.name : "无")}</color>");
                        
                        if (followTarget != null)
                        {
                            Debug.Log($"<color=white>  跟随目标位置: {followTarget.position}</color>");
                            
                            // 检查跟随目标是否距离背景很远
                            GameObject backgroundParent = GameObject.Find("BackgroundParallax");
                            if (backgroundParent != null)
                            {
                                float distance = Vector3.Distance(followTarget.position, backgroundParent.transform.position);
                                if (distance > 50f)
                                {
                                    Debug.LogWarning($"<color=yellow>⚠️ 角色距离背景很远 ({distance:F1})，可能需要调整背景跟随</color>");
                                }
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log("<color=white>未检测到Cinemachine虚拟摄像机</color>");
                }
            }
            else
            {
                Debug.Log("<color=white>项目中未安装Cinemachine包</color>");
            }
        }
        catch (System.Exception e)
        {
            Debug.Log($"<color=white>Cinemachine检查跳过: {e.Message}</color>");
        }
    }
    
    /// <summary>
    /// 一键修复游戏运行时背景问题
    /// </summary>
    [ContextMenu("🛠️ 修复游戏运行时背景")]
    public void FixRuntimeBackground()
    {
        Debug.Log("<color=cyan>🛠️ 开始修复游戏运行时背景问题...</color>");
        
        GameObject backgroundParent = GameObject.Find("BackgroundParallax");
        Camera mainCamera = Camera.main ?? FindObjectOfType<Camera>();
        
        if (backgroundParent == null || mainCamera == null)
        {
            Debug.LogError("<color=red>❌ 找不到背景或摄像机</color>");
            return;
        }
        
        int fixedCount = 0;
        
        // 1. 调整背景位置到摄像机视野内
        Vector3 cameraPos = mainCamera.transform.position;
        Vector3 newBackgroundPos = new Vector3(cameraPos.x, cameraPos.y, cameraPos.z + 5f);
        backgroundParent.transform.position = newBackgroundPos;
        
        Debug.Log($"<color=green>✅ 调整背景位置到: {newBackgroundPos}</color>");
        fixedCount++;
        
        // 2. 确保背景排序层级正确
        for (int i = 0; i < backgroundParent.transform.childCount; i++)
        {
            Transform child = backgroundParent.transform.GetChild(i);
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            
            if (sr != null)
            {
                if (sr.sortingOrder >= 0)
                {
                    sr.sortingOrder = -10 - i; // 确保背景在最底层
                    Debug.Log($"<color=green>✅ 调整 {child.name} 排序层级为: {sr.sortingOrder}</color>");
                    fixedCount++;
                }
                
                // 确保背景完全不透明
                if (sr.color.a < 1f)
                {
                    Color color = sr.color;
                    color.a = 1f;
                    sr.color = color;
                    Debug.Log($"<color=green>✅ 修复 {child.name} 透明度</color>");
                    fixedCount++;
                }
            }
        }
        
        // 3. 检查并启用视差控制器
        ParallaxController parallaxController = FindObjectOfType<ParallaxController>();
        if (parallaxController != null)
        {
            parallaxController.enabled = true;
            Debug.Log("<color=green>✅ 启用视差控制器</color>");
            fixedCount++;
        }
        
        Debug.Log($"<color=green>🎉 运行时修复完成！共修复了 {fixedCount} 个问题</color>");
        Debug.Log("<color=yellow>背景现在应该在游戏中正常显示了！</color>");
    }
    
    /// <summary>
    /// 强制背景跟随摄像机
    /// </summary>
    [ContextMenu("📌 强制背景跟随摄像机")]
    public void ForceBackgroundFollowCamera()
    {
        GameObject backgroundParent = GameObject.Find("BackgroundParallax");
        Camera mainCamera = Camera.main ?? FindObjectOfType<Camera>();
        
        if (backgroundParent == null || mainCamera == null)
        {
            Debug.LogError("<color=red>❌ 找不到背景或摄像机</color>");
            return;
        }
        
        // 添加一个简单的跟随脚本
        var follower = backgroundParent.GetComponent<简单背景跟随>();
        if (follower == null)
        {
            follower = backgroundParent.AddComponent<简单背景跟随>();
        }
        
        follower.targetCamera = mainCamera;
        follower.followDistance = 10f;
        
        Debug.Log("<color=green>✅ 已添加背景跟随摄像机功能</color>");
        Debug.Log("<color=yellow>背景现在会始终跟随摄像机移动</color>");
    }
}

/// <summary>
/// 简单的背景跟随组件
/// </summary>
public class 简单背景跟随 : MonoBehaviour
{
    [Header("跟随设置")]
    public Camera targetCamera;
    public float followDistance = 10f;
    public bool followX = true;
    public bool followY = true;
    
    void LateUpdate()
    {
        if (targetCamera == null) return;
        
        Vector3 cameraPos = targetCamera.transform.position;
        Vector3 newPos = transform.position;
        
        if (followX) newPos.x = cameraPos.x;
        if (followY) newPos.y = cameraPos.y;
        newPos.z = cameraPos.z + followDistance;
        
        transform.position = newPos;
    }
}

#if UNITY_EDITOR
/// <summary>
/// 编辑器菜单扩展
/// </summary>
public static class RuntimeBackgroundMenu
{
    [MenuItem("Tools/背景设置/🎮 诊断游戏运行时背景")]
    public static void DiagnoseRuntime()
    {
        var diagnoser = FindOrCreateDiagnoser();
        diagnoser.DiagnoseRuntimeBackground();
        CleanupDiagnoser(diagnoser);
    }
    
    [MenuItem("Tools/背景设置/🛠️ 修复游戏运行时背景")]
    public static void FixRuntime()
    {
        var diagnoser = FindOrCreateDiagnoser();
        diagnoser.FixRuntimeBackground();
        CleanupDiagnoser(diagnoser);
    }
    
    [MenuItem("Tools/背景设置/📌 强制背景跟随摄像机")]
    public static void ForceFollow()
    {
        var diagnoser = FindOrCreateDiagnoser();
        diagnoser.ForceBackgroundFollowCamera();
        CleanupDiagnoser(diagnoser);
    }
    
    private static 游戏运行时背景诊断器 FindOrCreateDiagnoser()
    {
        var diagnoser = Object.FindObjectOfType<游戏运行时背景诊断器>();
        if (diagnoser == null)
        {
            GameObject tempObj = new GameObject("TempRuntimeBackgroundDiagnoser");
            diagnoser = tempObj.AddComponent<游戏运行时背景诊断器>();
        }
        return diagnoser;
    }
    
    private static void CleanupDiagnoser(游戏运行时背景诊断器 diagnoser)
    {
        if (diagnoser.gameObject.name == "TempRuntimeBackgroundDiagnoser")
        {
            Object.DestroyImmediate(diagnoser.gameObject);
        }
    }
}
#endif 