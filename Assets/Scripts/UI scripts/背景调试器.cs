using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 背景调试器 - 专门诊断背景显示问题
/// </summary>
public class 背景调试器 : MonoBehaviour
{
    [Header("调试选项")]
    [Tooltip("显示详细调试信息")]
    public bool showDetailedDebug = true;
    
    [Tooltip("背景图路径")]
    public string backgroundPath = "Assets/Map/2 Background/Day/";
    
    /// <summary>
    /// 完整的背景诊断
    /// </summary>
    [ContextMenu("🔍 完整背景诊断")]
    public void FullBackgroundDiagnosis()
    {
        Debug.Log("<color=cyan>======= 开始完整背景诊断 =======</color>");
        
        // 1. 检查背景图片文件
        CheckBackgroundFiles();
        
        // 2. 检查场景中的背景对象
        CheckSceneObjects();
        
        // 3. 检查摄像机设置
        CheckCameraSettings();
        
        // 4. 检查ParallaxController
        CheckParallaxController();
        
        Debug.Log("<color=cyan>======= 背景诊断完成 =======</color>");
    }
    
    /// <summary>
    /// 检查背景图片文件
    /// </summary>
    private void CheckBackgroundFiles()
    {
        Debug.Log("<color=yellow>--- 检查背景图片文件 ---</color>");
        
        for (int i = 1; i <= 5; i++)
        {
            string filePath = backgroundPath + i + ".png";
            
            #if UNITY_EDITOR
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(filePath);
            if (sprite != null)
            {
                Debug.Log($"<color=green>✅ 找到背景图 {i}: {filePath}</color>");
                Debug.Log($"<color=white>   尺寸: {sprite.texture.width}x{sprite.texture.height}</color>");
            }
            else
            {
                Debug.LogError($"<color=red>❌ 找不到背景图 {i}: {filePath}</color>");
            }
            #endif
        }
    }
    
    /// <summary>
    /// 检查场景中的背景对象
    /// </summary>
    private void CheckSceneObjects()
    {
        Debug.Log("<color=yellow>--- 检查场景背景对象 ---</color>");
        
        GameObject backgroundParent = GameObject.Find("BackgroundParallax");
        if (backgroundParent == null)
        {
            Debug.LogError("<color=red>❌ 场景中没有找到BackgroundParallax对象</color>");
            Debug.Log("<color=orange>建议: 运行自动背景设置器创建背景对象</color>");
            return;
        }
        
        Debug.Log($"<color=green>✅ 找到BackgroundParallax对象</color>");
        Debug.Log($"<color=white>   位置: {backgroundParent.transform.position}</color>");
        Debug.Log($"<color=white>   子对象数量: {backgroundParent.transform.childCount}</color>");
        
        // 检查每个子层
        for (int i = 0; i < backgroundParent.transform.childCount; i++)
        {
            Transform child = backgroundParent.transform.GetChild(i);
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            
            if (sr == null)
            {
                Debug.LogWarning($"<color=orange>⚠️ {child.name} 缺少SpriteRenderer组件</color>");
                continue;
            }
            
            if (sr.sprite == null)
            {
                Debug.LogWarning($"<color=orange>⚠️ {child.name} 的Sprite为空</color>");
            }
            else
            {
                Debug.Log($"<color=green>✅ {child.name}: Sprite={sr.sprite.name}, Order={sr.sortingOrder}</color>");
                Debug.Log($"<color=white>   位置: {child.position}, 激活: {child.gameObject.activeInHierarchy}</color>");
            }
        }
    }
    
    /// <summary>
    /// 检查摄像机设置
    /// </summary>
    private void CheckCameraSettings()
    {
        Debug.Log("<color=yellow>--- 检查摄像机设置 ---</color>");
        
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("<color=red>❌ 找不到Main Camera</color>");
            return;
        }
        
        Debug.Log($"<color=green>✅ 找到Main Camera: {mainCamera.name}</color>");
        Debug.Log($"<color=white>   位置: {mainCamera.transform.position}</color>");
        Debug.Log($"<color=white>   Clear Flags: {mainCamera.clearFlags}</color>");
        Debug.Log($"<color=white>   Culling Mask: {mainCamera.cullingMask}</color>");
        Debug.Log($"<color=white>   Orthographic: {mainCamera.orthographic}</color>");
        
        if (mainCamera.orthographic)
        {
            Debug.Log($"<color=white>   Orthographic Size: {mainCamera.orthographicSize}</color>");
        }
    }
    
    /// <summary>
    /// 检查ParallaxController
    /// </summary>
    private void CheckParallaxController()
    {
        Debug.Log("<color=yellow>--- 检查ParallaxController ---</color>");
        
        GameObject backgroundParent = GameObject.Find("BackgroundParallax");
        if (backgroundParent == null) return;
        
        ParallaxController controller = backgroundParent.GetComponent<ParallaxController>();
        if (controller == null)
        {
            Debug.LogWarning("<color=orange>⚠️ BackgroundParallax缺少ParallaxController组件</color>");
            return;
        }
        
        Debug.Log($"<color=green>✅ 找到ParallaxController</color>");
        Debug.Log($"<color=white>   启用视差: {controller.enableParallax}</color>");
        Debug.Log($"<color=white>   参考摄像机: {(controller.referenceCamera ? controller.referenceCamera.name : "未设置")}</color>");
        Debug.Log($"<color=white>   图层数量: {controller.parallaxLayers.Count}</color>");
        
        for (int i = 0; i < controller.parallaxLayers.Count; i++)
        {
            var layer = controller.parallaxLayers[i];
            Debug.Log($"<color=white>   图层{i}: {layer.layerName}, 系数={layer.parallaxFactor}, Transform={(layer.layerTransform ? layer.layerTransform.name : "未设置")}</color>");
        }
    }
    
    /// <summary>
    /// 强制手动创建简单背景
    /// </summary>
    [ContextMenu("🔧 强制创建简单背景")]
    public void ForceCreateSimpleBackground()
    {
        Debug.Log("<color=cyan>开始强制创建简单背景...</color>");
        
        // 清理现有背景
        GameObject existing = GameObject.Find("SimpleBackground");
        if (existing != null)
        {
            #if UNITY_EDITOR
            DestroyImmediate(existing);
            #else
            Destroy(existing);
            #endif
        }
        
        // 创建简单背景容器
        GameObject simpleBackground = new GameObject("SimpleBackground");
        simpleBackground.transform.position = Vector3.zero;
        
        // 只创建一个测试背景层
        GameObject testLayer = new GameObject("TestBackgroundLayer");
        testLayer.transform.SetParent(simpleBackground.transform);
        testLayer.transform.position = new Vector3(0, 0, 10);
        
        // 添加SpriteRenderer
        SpriteRenderer sr = testLayer.AddComponent<SpriteRenderer>();
        sr.sortingOrder = -10; // 确保在最底层
        
        // 尝试加载第一张背景图
        #if UNITY_EDITOR
        string testImagePath = backgroundPath + "1.png";
        Sprite testSprite = AssetDatabase.LoadAssetAtPath<Sprite>(testImagePath);
        
        if (testSprite != null)
        {
            sr.sprite = testSprite;
            Debug.Log($"<color=green>✅ 成功加载测试背景图: {testImagePath}</color>");
        }
        else
        {
            Debug.LogError($"<color=red>❌ 无法加载测试背景图: {testImagePath}</color>");
            Debug.Log("<color=orange>请手动将背景图拖入TestBackgroundLayer的Sprite字段</color>");
        }
        #endif
        
        Debug.Log("<color=green>✅ 简单背景创建完成！请查看Scene视图中的SimpleBackground对象</color>");
        Debug.Log("<color=yellow>如果仍然看不到，请检查摄像机位置和图层设置</color>");
    }
    
    /// <summary>
    /// 测试背景可见性
    /// </summary>
    [ContextMenu("👁️ 测试背景可见性")]
    public void TestBackgroundVisibility()
    {
        Debug.Log("<color=cyan>测试背景可见性...</color>");
        
        // 查找所有SpriteRenderer
        SpriteRenderer[] allRenderers = FindObjectsOfType<SpriteRenderer>();
        Debug.Log($"<color=white>场景中共找到 {allRenderers.Length} 个SpriteRenderer</color>");
        
        foreach (SpriteRenderer sr in allRenderers)
        {
            if (sr.sprite != null)
            {
                bool isVisible = sr.gameObject.activeInHierarchy && sr.enabled;
                Debug.Log($"<color={(isVisible ? "green" : "red")}>{(isVisible ? "✅" : "❌")} {sr.name}: Sprite={sr.sprite.name}, Order={sr.sortingOrder}, 可见={isVisible}</color>");
                
                if (showDetailedDebug)
                {
                    Debug.Log($"<color=white>     位置: {sr.transform.position}, 缩放: {sr.transform.localScale}</color>");
                    Debug.Log($"<color=white>     颜色: {sr.color}, Material: {(sr.material ? sr.material.name : "None")}</color>");
                }
            }
        }
    }
    
    /// <summary>
    /// 调整摄像机到合适位置
    /// </summary>
    [ContextMenu("📷 调整摄像机位置")]
    public void AdjustCameraPosition()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("<color=red>❌ 找不到Main Camera</color>");
            return;
        }
        
        // 调整摄像机位置以便看到背景
        mainCamera.transform.position = new Vector3(0, 0, -10);
        
        // 如果是正交摄像机，确保合适的尺寸
        if (mainCamera.orthographic)
        {
            mainCamera.orthographicSize = 5;
        }
        
        Debug.Log("<color=green>✅ 摄像机位置已调整到 (0, 0, -10)</color>");
        Debug.Log("<color=yellow>现在应该能看到背景了！</color>");
    }
}

#if UNITY_EDITOR
/// <summary>
/// 编辑器菜单扩展
/// </summary>
public static class BackgroundDebugMenu
{
    [MenuItem("Tools/背景设置/🔍 完整背景诊断")]
    public static void RunFullDiagnosis()
    {
        var debugger = Object.FindObjectOfType<背景调试器>();
        if (debugger == null)
        {
            GameObject tempObj = new GameObject("TempBackgroundDebugger");
            debugger = tempObj.AddComponent<背景调试器>();
        }
        
        debugger.FullBackgroundDiagnosis();
        
        if (debugger.gameObject.name == "TempBackgroundDebugger")
        {
            Object.DestroyImmediate(debugger.gameObject);
        }
    }
    
    [MenuItem("Tools/背景设置/🔧 强制创建简单背景")]
    public static void ForceCreateBackground()
    {
        var debugger = Object.FindObjectOfType<背景调试器>();
        if (debugger == null)
        {
            GameObject tempObj = new GameObject("TempBackgroundDebugger");
            debugger = tempObj.AddComponent<背景调试器>();
        }
        
        debugger.ForceCreateSimpleBackground();
        
        if (debugger.gameObject.name == "TempBackgroundDebugger")
        {
            Object.DestroyImmediate(debugger.gameObject);
        }
    }
    
    [MenuItem("Tools/背景设置/📷 调整摄像机位置")]
    public static void AdjustCamera()
    {
        var debugger = Object.FindObjectOfType<背景调试器>();
        if (debugger == null)
        {
            GameObject tempObj = new GameObject("TempBackgroundDebugger");
            debugger = tempObj.AddComponent<背景调试器>();
        }
        
        debugger.AdjustCameraPosition();
        
        if (debugger.gameObject.name == "TempBackgroundDebugger")
        {
            Object.DestroyImmediate(debugger.gameObject);
        }
    }
}
#endif 