using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Cinemachine摄像机抖动修复工具
/// 解决摄像机跟随时的抖动问题
/// </summary>
public class CinemachineCameraFix : MonoBehaviour
{
    [Header("抖动诊断与修复")]
    [SerializeField] private bool autoFix = true;
    [SerializeField] private bool showDebugInfo = true;
    
    [Header("推荐的Damping设置")]
    [SerializeField] private Vector3 recommendedPositionDamping = new Vector3(1.5f, 1.5f, 1f);
    [SerializeField] private float recommendedRotationDamping = 1f;
    
    [Header("玩家移动优化")]
    [SerializeField] private bool optimizePlayerMovement = true;
    [SerializeField] private float smoothMovementMultiplier = 0.8f;
    
    void Start()
    {
        if (autoFix)
        {
            FixCinemachineCameraShaking();
        }
        
        if (optimizePlayerMovement)
        {
            OptimizePlayerMovement();
        }
    }
    
    [ContextMenu("🔧 修复摄像机抖动")]
    public void FixCinemachineCameraShaking()
    {
        Debug.Log("<color=#00FFFF>🔧 开始修复Cinemachine摄像机抖动问题...</color>");
        
        // 查找所有Cinemachine虚拟摄像机
        var cinemachineCameras = FindObjectsOfType<MonoBehaviour>().
            Where(obj => obj.GetType().Name.Contains("CinemachineCamera"));
        
        if (cinemachineCameras.Count() == 0)
        {
            Debug.LogWarning("未找到CinemachineCamera组件！");
            return;
        }
        
        foreach (var camera in cinemachineCameras)
        {
            FixSingleCamera(camera);
        }
        
        // 修复玩家移动
        if (optimizePlayerMovement)
        {
            OptimizePlayerMovement();
        }
        
        Debug.Log("<color=#00FF00>✅ 摄像机抖动修复完成！</color>");
    }
    
    void FixSingleCamera(MonoBehaviour camera)
    {
        Debug.Log($"<color=#FFFF00>正在修复摄像机: {camera.name}</color>");
        
        // 使用反射来设置Cinemachine属性
        try
        {
            // 获取TrackerSettings
            var trackerSettings = camera.GetType().GetProperty("TrackerSettings");
            if (trackerSettings != null)
            {
                var settings = trackerSettings.GetValue(camera);
                if (settings != null)
                {
                    // 设置PositionDamping
                    var positionDampingProperty = settings.GetType().GetProperty("PositionDamping");
                    if (positionDampingProperty != null)
                    {
                        positionDampingProperty.SetValue(settings, recommendedPositionDamping);
                        Debug.Log($"<color=#00FF00>✓ 已设置PositionDamping为: {recommendedPositionDamping}</color>");
                    }
                    
                    // 设置QuaternionDamping
                    var quaternionDampingProperty = settings.GetType().GetProperty("QuaternionDamping");
                    if (quaternionDampingProperty != null)
                    {
                        quaternionDampingProperty.SetValue(settings, recommendedRotationDamping);
                        Debug.Log($"<color=#00FF00>✓ 已设置QuaternionDamping为: {recommendedRotationDamping}</color>");
                    }
                }
            }
            
            // 检查跟随目标
            var followProperty = camera.GetType().GetProperty("Follow");
            if (followProperty != null)
            {
                var followTarget = followProperty.GetValue(camera) as Transform;
                if (followTarget != null)
                {
                    Debug.Log($"<color=#00FF00>✓ 跟随目标: {followTarget.name}</color>");
                    
                    // 确保跟随目标有Rigidbody2D
                    var rb = followTarget.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        // 优化Rigidbody设置以减少抖动
                        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
                        Debug.Log("<color=#00FF00>✓ 已为玩家启用Rigidbody2D插值</color>");
                    }
                }
                else
                {
                    Debug.LogWarning("⚠️ 摄像机没有设置跟随目标！");
                }
            }
            
        }
        catch (System.Exception e)
        {
            Debug.LogError($"修复摄像机时出错: {e.Message}");
        }
    }
    
    void OptimizePlayerMovement()
    {
        Debug.Log("<color=#FFFF00>优化玩家移动以减少摄像机抖动...</color>");
        
        // 查找所有玩家控制脚本
        var playerControllers = FindObjectsOfType<PlayerController>();
        var playerRuns = FindObjectsOfType<PlayerRun>();
        
        foreach (var player in playerControllers)
        {
            OptimizePlayerController(player);
        }
        
        foreach (var player in playerRuns)
        {
            OptimizePlayerRun(player);
        }
    }
    
    void OptimizePlayerController(PlayerController player)
    {
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // 启用插值以平滑移动
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            
            // 调整线性阻力以减少滑动
            if (rb.linearDamping < 2f)
            {
                rb.linearDamping = 2f;
            }
            
            Debug.Log($"<color=#00FF00>✓ 已优化 {player.name} 的Rigidbody2D设置</color>");
        }
    }
    
    void OptimizePlayerRun(PlayerRun player)
    {
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // 启用插值以平滑移动
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            
            // 调整线性阻力
            if (rb.linearDamping < 2f)
            {
                rb.linearDamping = 2f;
            }
            
            Debug.Log($"<color=#00FF00>✓ 已优化 {player.name} 的PlayerRun Rigidbody2D设置</color>");
        }
    }
    
    [ContextMenu("📊 诊断摄像机抖动问题")]
    public void DiagnoseCameraShaking()
    {
        Debug.Log("<color=#00FFFF>📊 开始诊断摄像机抖动问题...</color>");
        
        // 检查帧率
        Debug.Log($"<color=#FFFF00>当前帧率: {1f / Time.deltaTime:F1} FPS</color>");
        if (1f / Time.deltaTime < 30f)
        {
            Debug.LogWarning("⚠️ 帧率过低可能导致摄像机抖动！");
        }
        
        // 检查垂直同步
        Debug.Log($"<color=#FFFF00>垂直同步设置: {QualitySettings.vSyncCount}</color>");
        if (QualitySettings.vSyncCount == 0)
        {
            Debug.LogWarning("⚠️ 建议启用垂直同步以减少抖动");
        }
        
        // 检查玩家Rigidbody设置
        CheckPlayerRigidbodySettings();
        
        // 检查Cinemachine设置
        CheckCinemachineSettings();
        
        Debug.Log("<color=#00FFFF>📊 诊断完成！请查看上方建议</color>");
    }
    
    void CheckPlayerRigidbodySettings()
    {
        Debug.Log("<color=#FFFF00>检查玩家Rigidbody设置...</color>");
        
        var players = FindObjectsOfType<MonoBehaviour>().Where(obj => 
            obj.name.ToLower().Contains("player") && obj.GetComponent<Rigidbody2D>() != null);
        
        foreach (var player in players)
        {
            var rb = player.GetComponent<Rigidbody2D>();
            Debug.Log($"<color=#CCCCCC>玩家 {player.name}:</color>");
            Debug.Log($"  - 插值模式: {rb.interpolation}");
            Debug.Log($"  - 线性阻力: {rb.linearDamping}");
            Debug.Log($"  - 角阻力: {rb.angularDamping}");
            
            if (rb.interpolation == RigidbodyInterpolation2D.None)
            {
                Debug.LogWarning($"⚠️ {player.name} 未启用插值，可能导致抖动！");
            }
        }
    }
    
    void CheckCinemachineSettings()
    {
        Debug.Log("<color=#FFFF00>检查Cinemachine设置...</color>");
        
        var cinemachineCameras = FindObjectsOfType<MonoBehaviour>().
            Where(obj => obj.GetType().Name.Contains("CinemachineCamera"));
        
        foreach (var camera in cinemachineCameras)
        {
            Debug.Log($"<color=#CCCCCC>摄像机 {camera.name}:</color>");
            
            try
            {
                var trackerSettings = camera.GetType().GetProperty("TrackerSettings");
                if (trackerSettings != null)
                {
                    var settings = trackerSettings.GetValue(camera);
                    if (settings != null)
                    {
                        var positionDamping = settings.GetType().GetProperty("PositionDamping")?.GetValue(settings);
                        Debug.Log($"  - PositionDamping: {positionDamping}");
                        
                        if (positionDamping != null && positionDamping.ToString().Contains("(1, 1, 1)"))
                        {
                            Debug.LogWarning("⚠️ PositionDamping设置过低，建议增加到(1.5, 1.5, 1)");
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"无法检查 {camera.name} 的设置: {e.Message}");
            }
        }
    }
    
    #if UNITY_EDITOR
    [ContextMenu("⚙️ 应用推荐的项目设置")]
    public void ApplyRecommendedProjectSettings()
    {
        Debug.Log("<color=#00FFFF>⚙️ 应用推荐的项目设置...</color>");
        
        // 设置垂直同步
        QualitySettings.vSyncCount = 1;
        Debug.Log("<color=#00FF00>✓ 已启用垂直同步</color>");
        
        // 设置目标帧率
        Application.targetFrameRate = 60;
        Debug.Log("<color=#00FF00>✓ 已设置目标帧率为60FPS</color>");
        
        // 设置固定时间步长
        Time.fixedDeltaTime = 1f / 50f; // 50Hz物理更新
        Debug.Log("<color=#00FF00>✓ 已设置固定时间步长为50Hz</color>");
        
        Debug.Log("<color=#00FF00>✅ 项目设置优化完成！</color>");
    }
    #endif
    
    void Update()
    {
        if (showDebugInfo && Input.GetKeyDown(KeyCode.F11))
        {
            DiagnoseCameraShaking();
        }
    }
    
    void OnGUI()
    {
        if (showDebugInfo)
        {
            GUI.Label(new Rect(10, 10, 300, 20), $"FPS: {1f / Time.deltaTime:F1}");
            GUI.Label(new Rect(10, 30, 300, 20), $"VSync: {QualitySettings.vSyncCount}");
            GUI.Label(new Rect(10, 50, 300, 20), "按F11键诊断摄像机抖动");
        }
    }
}

// 扩展方法
public static class LinqExtensions
{
    public static System.Collections.Generic.IEnumerable<T> Where<T>(this T[] array, System.Func<T, bool> predicate)
    {
        foreach (T item in array)
        {
            if (predicate(item))
                yield return item;
        }
    }
    
    public static int Count<T>(this System.Collections.Generic.IEnumerable<T> enumerable)
    {
        int count = 0;
        foreach (T item in enumerable)
        {
            count++;
        }
        return count;
    }
} 