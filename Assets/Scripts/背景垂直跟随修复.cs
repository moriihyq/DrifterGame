using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 背景垂直跟随修复 - 禁止背景跟随角色上下跳动
/// </summary>
public class 背景垂直跟随修复 : MonoBehaviour
{
    /// <summary>
    /// 一键禁用背景垂直跟随
    /// </summary>
    [ContextMenu("🚫 禁用背景垂直跟随")]
    public void DisableVerticalFollow()
    {
        Debug.Log("<color=cyan>开始禁用背景垂直跟随...</color>");
        
        GameObject backgroundParent = GameObject.Find("BackgroundParallax");
        if (backgroundParent == null)
        {
            Debug.LogError("<color=red>❌ 找不到BackgroundParallax对象！</color>");
            return;
        }
        
        // 检查并修复背景跟随修复器
        背景跟随修复器 follower = backgroundParent.GetComponent<背景跟随修复器>();
        if (follower != null)
        {
            follower.followXOnly = true; // 只跟随X轴
            follower.followYOnly = false;
            Debug.Log("<color=green>✅ 已设置背景跟随修复器只跟随水平移动</color>");
        }
        
        // 检查并修复增强视差控制器
        增强视差控制器 enhanced = backgroundParent.GetComponent<增强视差控制器>();
        if (enhanced != null)
        {
            // 修改增强视差控制器的跟随方式
            FixEnhancedController(enhanced);
            Debug.Log("<color=green>✅ 已修复增强视差控制器的垂直跟随</color>");
        }
        
        // 检查并修复原始ParallaxController
        ParallaxController original = backgroundParent.GetComponent<ParallaxController>();
        if (original != null)
        {
            FixOriginalController(original);
            Debug.Log("<color=green>✅ 已修复原始ParallaxController的垂直跟随</color>");
        }
        
        if (follower == null && enhanced == null && original == null)
        {
            Debug.LogWarning("<color=orange>⚠️ 没有发现任何背景控制器组件</color>");
        }
        
        Debug.Log("<color=green>✅ 背景垂直跟随修复完成！</color>");
        Debug.Log("<color=yellow>现在背景只会跟随水平移动，不会跟随跳跃</color>");
    }
    
    /// <summary>
    /// 修复增强视差控制器
    /// </summary>
    private void FixEnhancedController(增强视差控制器 controller)
    {
        // 为增强视差控制器添加水平跟随功能
        GameObject obj = controller.gameObject;
        HorizontalOnlyFollower horizontalFollower = obj.GetComponent<HorizontalOnlyFollower>();
        if (horizontalFollower == null)
        {
            horizontalFollower = obj.AddComponent<HorizontalOnlyFollower>();
        }
        
        horizontalFollower.targetCamera = controller.referenceCamera;
        horizontalFollower.followOffset = controller.followOffset;
        
        // 禁用增强控制器的跟随功能，让HorizontalOnlyFollower处理
        controller.followCamera = false;
        
        Debug.Log("<color=cyan>已为增强视差控制器添加水平跟随组件</color>");
    }
    
    /// <summary>
    /// 修复原始ParallaxController
    /// </summary>
    private void FixOriginalController(ParallaxController controller)
    {
        // 为原始ParallaxController添加水平跟随功能
        GameObject obj = controller.gameObject;
        HorizontalOnlyFollower horizontalFollower = obj.GetComponent<HorizontalOnlyFollower>();
        if (horizontalFollower == null)
        {
            horizontalFollower = obj.AddComponent<HorizontalOnlyFollower>();
        }
        
        horizontalFollower.targetCamera = controller.referenceCamera;
        horizontalFollower.followOffset = new Vector3(0, 0, 10);
        
        Debug.Log("<color=cyan>已为原始ParallaxController添加水平跟随组件</color>");
    }
    
    /// <summary>
    /// 设置背景固定高度
    /// </summary>
    [ContextMenu("📏 设置背景固定高度")]
    public void SetFixedBackgroundHeight()
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
        
        // 设置背景高度为摄像机当前高度
        Vector3 currentPos = backgroundParent.transform.position;
        float targetHeight = mainCamera.transform.position.y;
        
        backgroundParent.transform.position = new Vector3(currentPos.x, targetHeight, currentPos.z);
        
        // 添加固定高度组件
        FixedHeightBackground fixedHeightComponent = backgroundParent.GetComponent<FixedHeightBackground>();
        if (fixedHeightComponent == null)
        {
            fixedHeightComponent = backgroundParent.AddComponent<FixedHeightBackground>();
        }
        
        fixedHeightComponent.fixedY = targetHeight;
        
        Debug.Log($"<color=green>✅ 已设置背景固定高度: {targetHeight}</color>");
    }
    
    /// <summary>
    /// 测试垂直跟随修复效果
    /// </summary>
    [ContextMenu("🧪 测试垂直跟随修复")]
    public void TestVerticalFollowFix()
    {
        Debug.Log("<color=cyan>测试垂直跟随修复效果...</color>");
        
        GameObject backgroundParent = GameObject.Find("BackgroundParallax");
        if (backgroundParent == null)
        {
            Debug.LogError("<color=red>❌ 找不到BackgroundParallax对象！</color>");
            return;
        }
        
        // 检查各种跟随组件的设置
        背景跟随修复器 follower = backgroundParent.GetComponent<背景跟随修复器>();
        if (follower != null)
        {
            Debug.Log($"<color=white>背景跟随修复器 - 只跟随X轴: {follower.followXOnly}</color>");
        }
        
        HorizontalOnlyFollower horizontal = backgroundParent.GetComponent<HorizontalOnlyFollower>();
        if (horizontal != null)
        {
            Debug.Log($"<color=white>水平跟随器 - 启用: {horizontal.enabled}</color>");
        }
        
        增强视差控制器 enhanced = backgroundParent.GetComponent<增强视差控制器>();
        if (enhanced != null)
        {
            Debug.Log($"<color=white>增强视差控制器 - 跟随摄像机: {enhanced.followCamera}</color>");
        }
        
        FixedHeightBackground fixedHeight = backgroundParent.GetComponent<FixedHeightBackground>();
        if (fixedHeight != null)
        {
            Debug.Log($"<color=white>固定高度组件 - 固定Y: {fixedHeight.fixedY}</color>");
        }
        
        Debug.Log("<color=cyan>=== 测试完成 ===</color>");
    }
}

/// <summary>
/// 水平跟随器 - 只跟随水平移动的组件
/// </summary>
public class HorizontalOnlyFollower : MonoBehaviour
{
    [Header("水平跟随设置")]
    [Tooltip("目标摄像机")]
    public Camera targetCamera;
    
    [Tooltip("跟随偏移")]
    public Vector3 followOffset = new Vector3(0, 0, 10);
    
    [Tooltip("是否启用平滑跟随")]
    public bool smoothFollow = false;
    
    [Tooltip("平滑跟随速度")]
    [Range(1f, 20f)]
    public float followSpeed = 10f;
    
    private float fixedY; // 固定的Y坐标
    
    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main ?? FindObjectOfType<Camera>();
        }
        
        // 记录初始Y坐标
        fixedY = transform.position.y;
        
        Debug.Log($"<color=green>[水平跟随器] 初始化完成，固定Y坐标: {fixedY}</color>");
    }
    
    void LateUpdate()
    {
        if (targetCamera != null)
        {
            UpdateHorizontalPosition();
        }
    }
    
    /// <summary>
    /// 更新水平位置
    /// </summary>
    private void UpdateHorizontalPosition()
    {
        Vector3 cameraPos = targetCamera.transform.position;
        
        // 只更新X和Z坐标，保持Y坐标固定
        Vector3 targetPos = new Vector3(
            cameraPos.x + followOffset.x,
            fixedY, // 使用固定的Y坐标
            cameraPos.z + followOffset.z
        );
        
        if (smoothFollow)
        {
            Vector3 currentPos = transform.position;
            currentPos.y = fixedY; // 确保Y坐标始终固定
            transform.position = Vector3.Lerp(currentPos, targetPos, followSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = targetPos;
        }
    }
    
    /// <summary>
    /// 设置固定高度
    /// </summary>
    public void SetFixedHeight(float height)
    {
        fixedY = height;
        Vector3 pos = transform.position;
        pos.y = fixedY;
        transform.position = pos;
        
        Debug.Log($"<color=cyan>[水平跟随器] 设置固定高度: {height}</color>");
    }
}

/// <summary>
/// 固定高度背景组件
/// </summary>
public class FixedHeightBackground : MonoBehaviour
{
    [Header("固定高度设置")]
    [Tooltip("固定的Y坐标")]
    public float fixedY = 0f;
    
    void LateUpdate()
    {
        Vector3 pos = transform.position;
        if (Mathf.Abs(pos.y - fixedY) > 0.001f)
        {
            pos.y = fixedY;
            transform.position = pos;
        }
    }
    
    /// <summary>
    /// 设置固定高度
    /// </summary>
    public void SetFixedHeight(float height)
    {
        fixedY = height;
        Vector3 pos = transform.position;
        pos.y = fixedY;
        transform.position = pos;
    }
}

#if UNITY_EDITOR
/// <summary>
/// 编辑器菜单
/// </summary>
public static class VerticalFollowFixMenu
{
    [MenuItem("Tools/背景设置/🚫 禁用背景垂直跟随")]
    public static void DisableVerticalFollow()
    {
        var fixer = FindOrCreateFixer();
        fixer.DisableVerticalFollow();
        CleanupFixer(fixer);
    }
    
    [MenuItem("Tools/背景设置/📏 设置背景固定高度")]
    public static void SetFixedHeight()
    {
        var fixer = FindOrCreateFixer();
        fixer.SetFixedBackgroundHeight();
        CleanupFixer(fixer);
    }
    
    [MenuItem("Tools/背景设置/🧪 测试垂直跟随修复")]
    public static void TestFix()
    {
        var fixer = FindOrCreateFixer();
        fixer.TestVerticalFollowFix();
        CleanupFixer(fixer);
    }
    
    private static 背景垂直跟随修复 FindOrCreateFixer()
    {
        var fixer = Object.FindObjectOfType<背景垂直跟随修复>();
        if (fixer == null)
        {
            GameObject tempObj = new GameObject("TempVerticalFollowFixer");
            fixer = tempObj.AddComponent<背景垂直跟随修复>();
        }
        return fixer;
    }
    
    private static void CleanupFixer(背景垂直跟随修复 fixer)
    {
        if (fixer.gameObject.name == "TempVerticalFollowFixer")
        {
            Object.DestroyImmediate(fixer.gameObject);
        }
    }
}
#endif 