using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 快速背景跟随修复 - 一键解决背景不跟随摄像机的问题
/// </summary>
public class 快速背景跟随修复 : MonoBehaviour
{
    /// <summary>
    /// 一键修复背景跟随问题
    /// </summary>
    [ContextMenu("🚀 一键修复背景跟随")]
    public void FixBackgroundFollow()
    {
        Debug.Log("<color=cyan>开始修复背景跟随问题...</color>");
        
        // 1. 查找BackgroundParallax对象
        GameObject backgroundParent = GameObject.Find("BackgroundParallax");
        if (backgroundParent == null)
        {
            Debug.LogError("<color=red>❌ 找不到BackgroundParallax对象！请先运行自动背景设置器</color>");
            return;
        }
        
        // 2. 查找摄像机
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        if (mainCamera == null)
        {
            Debug.LogError("<color=red>❌ 找不到摄像机！</color>");
            return;
        }
        
        // 3. 添加背景跟随修复器
        背景跟随修复器 follower = backgroundParent.GetComponent<背景跟随修复器>();
        if (follower == null)
        {
            follower = backgroundParent.AddComponent<背景跟随修复器>();
            Debug.Log("<color=green>✅ 已添加背景跟随修复器组件</color>");
        }
        
        // 4. 配置跟随参数
        follower.targetCamera = mainCamera;
        follower.followOffset = new Vector3(0, 0, 10); // 背景在摄像机后面10单位
        follower.smoothFollow = false; // 直接跟随，不平滑
        
        // 5. 立即同步位置
        follower.SyncToCamera();
        
        // 6. 检查是否有旧的ParallaxController需要禁用
        ParallaxController oldController = backgroundParent.GetComponent<ParallaxController>();
        if (oldController != null)
        {
            Debug.Log("<color=yellow>⚠️ 发现旧的ParallaxController，建议替换为增强版</color>");
            Debug.Log("<color=cyan>提示：可以运行'替换为增强视差控制器'来获得更好的效果</color>");
        }
        
        Debug.Log("<color=green>✅ 背景跟随修复完成！</color>");
        Debug.Log("<color=yellow>现在背景应该会跟随摄像机移动了</color>");
    }
    
    /// <summary>
    /// 替换为增强视差控制器
    /// </summary>
    [ContextMenu("🔄 替换为增强视差控制器")]
    public void ReplaceWithEnhancedController()
    {
        Debug.Log("<color=cyan>开始替换为增强视差控制器...</color>");
        
        GameObject backgroundParent = GameObject.Find("BackgroundParallax");
        if (backgroundParent == null)
        {
            Debug.LogError("<color=red>❌ 找不到BackgroundParallax对象！</color>");
            return;
        }
        
        // 移除旧组件
        ParallaxController oldController = backgroundParent.GetComponent<ParallaxController>();
        背景跟随修复器 oldFollower = backgroundParent.GetComponent<背景跟随修复器>();
        
        // 保存旧的配置
        Camera targetCamera = null;
        if (oldController != null && oldController.referenceCamera != null)
        {
            targetCamera = oldController.referenceCamera;
        }
        else if (oldFollower != null && oldFollower.targetCamera != null)
        {
            targetCamera = oldFollower.targetCamera;
        }
        else
        {
            targetCamera = Camera.main ?? FindObjectOfType<Camera>();
        }
        
        // 移除旧组件
        if (oldController != null)
        {
            #if UNITY_EDITOR
            DestroyImmediate(oldController);
            #else
            Destroy(oldController);
            #endif
            Debug.Log("<color=yellow>已移除旧的ParallaxController</color>");
        }
        
        if (oldFollower != null)
        {
            #if UNITY_EDITOR
            DestroyImmediate(oldFollower);
            #else
            Destroy(oldFollower);
            #endif
            Debug.Log("<color=yellow>已移除旧的背景跟随修复器</color>");
        }
        
        // 添加增强版控制器
        增强视差控制器 enhancedController = backgroundParent.AddComponent<增强视差控制器>();
        enhancedController.referenceCamera = targetCamera;
        enhancedController.followCamera = true;
        enhancedController.followOffset = new Vector3(0, 0, 10);
        enhancedController.enableParallax = true;
        
        // 自动配置图层
        Transform[] childLayers = new Transform[backgroundParent.transform.childCount];
        for (int i = 0; i < backgroundParent.transform.childCount; i++)
        {
            childLayers[i] = backgroundParent.transform.GetChild(i);
        }
        
        // 创建视差图层配置
        enhancedController.parallaxLayers = new 增强视差控制器.ParallaxLayer[childLayers.Length];
        float[] factors = { 0.1f, 0.3f, 0.5f, 0.7f, 0.9f };
        string[] names = { "远景天空", "远山", "中景", "近景树木", "前景装饰" };
        
        for (int i = 0; i < childLayers.Length && i < 5; i++)
        {
            enhancedController.parallaxLayers[i] = new 增强视差控制器.ParallaxLayer
            {
                layerTransform = childLayers[i],
                parallaxFactor = factors[i],
                layerName = i < names.Length ? names[i] : $"图层{i + 1}",
                enabled = true,
                verticalParallaxFactor = factors[i] * 0.5f,
                initialLocalPosition = childLayers[i].localPosition
            };
        }
        
        // 立即同步位置
        enhancedController.SyncToCamera();
        
        Debug.Log("<color=green>✅ 已替换为增强视差控制器！</color>");
        Debug.Log("<color=yellow>新控制器具有更好的跟随和视差效果</color>");
    }
    
    /// <summary>
    /// 测试背景跟随效果
    /// </summary>
    [ContextMenu("🧪 测试背景跟随")]
    public void TestBackgroundFollow()
    {
        Debug.Log("<color=cyan>测试背景跟随效果...</color>");
        
        Camera mainCamera = Camera.main ?? FindObjectOfType<Camera>();
        GameObject backgroundParent = GameObject.Find("BackgroundParallax");
        
        if (mainCamera == null || backgroundParent == null)
        {
            Debug.LogError("<color=red>❌ 缺少必要组件无法测试</color>");
            return;
        }
        
        Vector3 cameraPos = mainCamera.transform.position;
        Vector3 backgroundPos = backgroundParent.transform.position;
        float distance = Vector3.Distance(cameraPos, backgroundPos);
        
        Debug.Log($"<color=white>摄像机位置: {cameraPos}</color>");
        Debug.Log($"<color=white>背景位置: {backgroundPos}</color>");
        Debug.Log($"<color=white>距离: {distance:F2}</color>");
        
        if (distance < 50f)
        {
            Debug.Log("<color=green>✅ 背景距离摄像机较近，应该可见</color>");
        }
        else
        {
            Debug.LogWarning("<color=orange>⚠️ 背景距离摄像机较远，可能不可见</color>");
        }
        
        // 检查跟随组件
        背景跟随修复器 follower = backgroundParent.GetComponent<背景跟随修复器>();
        增强视差控制器 enhanced = backgroundParent.GetComponent<增强视差控制器>();
        
        if (follower != null)
        {
            Debug.Log("<color=green>✅ 发现背景跟随修复器组件</color>");
        }
        else if (enhanced != null)
        {
            Debug.Log("<color=green>✅ 发现增强视差控制器组件</color>");
        }
        else
        {
            Debug.LogWarning("<color=orange>⚠️ 没有发现跟随组件，背景可能不会跟随摄像机</color>");
        }
    }
}

#if UNITY_EDITOR
/// <summary>
/// 编辑器菜单
/// </summary>
public static class BackgroundFollowFixMenu
{
    [MenuItem("Tools/背景设置/🚀 一键修复背景跟随")]
    public static void FixBackgroundFollow()
    {
        var fixer = FindOrCreateFixer();
        fixer.FixBackgroundFollow();
        CleanupFixer(fixer);
    }
    
    [MenuItem("Tools/背景设置/🔄 替换为增强视差控制器")]
    public static void ReplaceController()
    {
        var fixer = FindOrCreateFixer();
        fixer.ReplaceWithEnhancedController();
        CleanupFixer(fixer);
    }
    
    [MenuItem("Tools/背景设置/🧪 测试背景跟随")]
    public static void TestFollow()
    {
        var fixer = FindOrCreateFixer();
        fixer.TestBackgroundFollow();
        CleanupFixer(fixer);
    }
    
    private static 快速背景跟随修复 FindOrCreateFixer()
    {
        var fixer = Object.FindObjectOfType<快速背景跟随修复>();
        if (fixer == null)
        {
            GameObject tempObj = new GameObject("TempBackgroundFollowFixer");
            fixer = tempObj.AddComponent<快速背景跟随修复>();
        }
        return fixer;
    }
    
    private static void CleanupFixer(快速背景跟随修复 fixer)
    {
        if (fixer.gameObject.name == "TempBackgroundFollowFixer")
        {
            Object.DestroyImmediate(fixer.gameObject);
        }
    }
}
#endif 