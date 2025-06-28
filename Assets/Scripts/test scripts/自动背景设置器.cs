using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;

/// <summary>
/// 自动背景设置器 - 一键创建和配置5层视差背景
/// </summary>
public class 自动背景设置器 : MonoBehaviour
{
    [Header("背景图路径设置")]
    [Tooltip("背景图所在文件夹路径")]
    public string backgroundPath = "Assets/Map/2 Background/Day/";
    
    [Header("自动设置选项")]
    [Tooltip("是否自动创建背景对象")]
    public bool autoCreateObjects = true;
    
    [Tooltip("是否自动设置视差控制器")]
    public bool autoSetupParallax = true;
    
    [Tooltip("背景图的Z坐标位置")]
    public float backgroundZPosition = 10f;
    
    /// <summary>
    /// 一键自动设置5层背景
    /// </summary>
    [ContextMenu("🚀 一键设置5层背景")]
    public void AutoSetup5LayerBackground()
    {
        Debug.Log("<color=cyan>开始自动设置5层视差背景...</color>");
        
        try
        {
            // 1. 创建主背景容器
            GameObject backgroundParent = CreateBackgroundParent();
            
            // 2. 创建5个背景图层
            GameObject[] backgroundLayers = CreateBackgroundLayers(backgroundParent);
            
            // 3. 设置Sprite Renderer和图片
            SetupSpriteRenderers(backgroundLayers);
            
            // 4. 添加并配置ParallaxController
            if (autoSetupParallax)
            {
                SetupParallaxController(backgroundParent, backgroundLayers);
            }
            
            Debug.Log("<color=green>✅ 5层视差背景设置完成！</color>");
            Debug.Log("<color=yellow>请在Game视图中测试摄像机移动效果。</color>");
            
        }
        catch (System.Exception e)
        {
            Debug.LogError($"<color=red>设置背景时出错: {e.Message}</color>");
        }
    }
    
    /// <summary>
    /// 创建背景容器对象
    /// </summary>
    private GameObject CreateBackgroundParent()
    {
        // 检查是否已存在
        GameObject existing = GameObject.Find("BackgroundParallax");
        if (existing != null)
        {
            Debug.Log("<color=yellow>发现已存在的BackgroundParallax，将在其基础上设置</color>");
            return existing;
        }
        
        GameObject parent = new GameObject("BackgroundParallax");
        parent.transform.position = Vector3.zero;
        
        Debug.Log("<color=green>创建BackgroundParallax容器</color>");
        return parent;
    }
    
    /// <summary>
    /// 创建5个背景图层对象
    /// </summary>
    private GameObject[] CreateBackgroundLayers(GameObject parent)
    {
        string[] layerNames = {
            "Background_Layer1_远景天空",
            "Background_Layer2_远山", 
            "Background_Layer3_中景",
            "Background_Layer4_近景树木",
            "Background_Layer5_前景装饰"
        };
        
        GameObject[] layers = new GameObject[5];
        
        for (int i = 0; i < 5; i++)
        {
            // 检查是否已存在
            Transform existing = parent.transform.Find(layerNames[i]);
            if (existing != null)
            {
                layers[i] = existing.gameObject;
                Debug.Log($"<color=yellow>发现已存在的{layerNames[i]}</color>");
            }
            else
            {
                layers[i] = new GameObject(layerNames[i]);
                layers[i].transform.SetParent(parent.transform);
                Debug.Log($"<color=green>创建{layerNames[i]}</color>");
            }
            
            // 设置位置
            layers[i].transform.position = new Vector3(0, 0, backgroundZPosition);
        }
        
        return layers;
    }
    
    /// <summary>
    /// 设置Sprite Renderer组件和背景图
    /// </summary>
    private void SetupSpriteRenderers(GameObject[] layers)
    {
        for (int i = 0; i < layers.Length; i++)
        {
            // 添加或获取Sprite Renderer组件
            SpriteRenderer spriteRenderer = layers[i].GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = layers[i].AddComponent<SpriteRenderer>();
                Debug.Log($"<color=green>为{layers[i].name}添加SpriteRenderer</color>");
            }
            
            // 设置排序层级
            spriteRenderer.sortingOrder = -5 + i; // -5, -4, -3, -2, -1
            
            // 尝试加载对应的背景图
            string spritePath = backgroundPath + (i + 1) + ".png";
            Sprite backgroundSprite = LoadSpriteFromPath(spritePath);
            
            if (backgroundSprite != null)
            {
                spriteRenderer.sprite = backgroundSprite;
                Debug.Log($"<color=green>为{layers[i].name}设置背景图: {spritePath}</color>");
            }
            else
            {
                Debug.LogWarning($"<color=orange>找不到背景图: {spritePath}</color>");
                Debug.LogWarning($"<color=orange>请手动将背景图拖入{layers[i].name}的Sprite字段</color>");
            }
        }
    }
    
    /// <summary>
    /// 从路径加载Sprite
    /// </summary>
    private Sprite LoadSpriteFromPath(string path)
    {
        #if UNITY_EDITOR
        // 在编辑器中使用AssetDatabase
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        #else
        // 运行时使用Resources（需要将图片放在Resources文件夹）
        string resourcePath = path.Replace("Assets/Resources/", "").Replace(".png", "");
        return Resources.Load<Sprite>(resourcePath);
        #endif
    }
    
    /// <summary>
    /// 设置ParallaxController组件
    /// </summary>
    private void SetupParallaxController(GameObject parent, GameObject[] layers)
    {
        // 添加或获取ParallaxController组件
        ParallaxController parallaxController = parent.GetComponent<ParallaxController>();
        if (parallaxController == null)
        {
            parallaxController = parent.AddComponent<ParallaxController>();
            Debug.Log("<color=green>添加ParallaxController组件</color>");
        }
        
        // 设置参考摄像机
        if (parallaxController.referenceCamera == null)
        {
            parallaxController.referenceCamera = Camera.main;
            if (parallaxController.referenceCamera == null)
            {
                parallaxController.referenceCamera = FindObjectOfType<Camera>();
            }
        }
        
        // 配置视差图层
        parallaxController.parallaxLayers.Clear();
        
        float[] parallaxFactors = { 0.1f, 0.3f, 0.5f, 0.7f, 0.9f };
        string[] layerNames = { "远景天空", "远山", "中景", "近景树木", "前景装饰" };
        
        for (int i = 0; i < layers.Length; i++)
        {
            var layer = new ParallaxController.ParallaxLayer
            {
                layerTransform = layers[i].transform,
                parallaxFactor = parallaxFactors[i],
                layerName = layerNames[i],
                enabled = true,
                enableVerticalParallax = true,
                verticalParallaxFactor = parallaxFactors[i] * 0.5f,
                initialPosition = layers[i].transform.position
            };
            
            parallaxController.parallaxLayers.Add(layer);
            Debug.Log($"<color=green>配置视差图层: {layerNames[i]} (系数: {parallaxFactors[i]})</color>");
        }
        
        // 启用设置
        parallaxController.enableParallax = true;
        parallaxController.showDebugInfo = true;
        
        Debug.Log("<color=green>ParallaxController配置完成</color>");
    }
    
    /// <summary>
    /// 清理所有背景对象
    /// </summary>
    [ContextMenu("🗑️ 清理背景对象")]
    public void CleanupBackground()
    {
        GameObject existing = GameObject.Find("BackgroundParallax");
        if (existing != null)
        {
            #if UNITY_EDITOR
            DestroyImmediate(existing);
            #else
            Destroy(existing);
            #endif
            Debug.Log("<color=yellow>已清理背景对象</color>");
        }
        else
        {
            Debug.Log("<color=yellow>没有找到需要清理的背景对象</color>");
        }
    }
    
    /// <summary>
    /// 验证背景设置
    /// </summary>
    [ContextMenu("🔍 检查背景设置")]
    public void ValidateBackgroundSetup()
    {
        Debug.Log("<color=cyan>=== 背景设置检查报告 ===</color>");
        
        GameObject backgroundParent = GameObject.Find("BackgroundParallax");
        if (backgroundParent == null)
        {
            Debug.LogError("<color=red>❌ 找不到BackgroundParallax对象</color>");
            return;
        }
        
        ParallaxController controller = backgroundParent.GetComponent<ParallaxController>();
        if (controller == null)
        {
            Debug.LogError("<color=red>❌ BackgroundParallax缺少ParallaxController组件</color>");
            return;
        }
        
        Debug.Log($"<color=green>✅ 找到ParallaxController，共{controller.parallaxLayers.Count}个图层</color>");
        
        if (controller.referenceCamera == null)
        {
            Debug.LogWarning("<color=orange>⚠️ 参考摄像机未设置</color>");
        }
        else
        {
            Debug.Log($"<color=green>✅ 参考摄像机: {controller.referenceCamera.name}</color>");
        }
        
        // 检查每个图层
        for (int i = 0; i < controller.parallaxLayers.Count; i++)
        {
            var layer = controller.parallaxLayers[i];
            if (layer.layerTransform == null)
            {
                Debug.LogWarning($"<color=orange>⚠️ 图层{i}的Transform未设置</color>");
                continue;
            }
            
            SpriteRenderer sr = layer.layerTransform.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                Debug.LogWarning($"<color=orange>⚠️ {layer.layerName}缺少SpriteRenderer</color>");
            }
            else if (sr.sprite == null)
            {
                Debug.LogWarning($"<color=orange>⚠️ {layer.layerName}的Sprite未设置</color>");
            }
            else
            {
                Debug.Log($"<color=green>✅ {layer.layerName}: 视差系数{layer.parallaxFactor}, Order{sr.sortingOrder}</color>");
            }
        }
        
        Debug.Log("<color=cyan>=== 检查完成 ===</color>");
    }
}

#if UNITY_EDITOR
/// <summary>
/// 编辑器扩展 - 在菜单栏添加快捷工具
/// </summary>
public static class BackgroundSetupMenu
{
    [MenuItem("Tools/背景设置/🚀 自动设置5层背景")]
    public static void AutoSetupBackground()
    {
        // 在场景中查找或创建设置器
        var setupper = Object.FindObjectOfType<自动背景设置器>();
        if (setupper == null)
        {
            GameObject tempObj = new GameObject("TempBackgroundSetupper");
            setupper = tempObj.AddComponent<自动背景设置器>();
        }
        
        setupper.AutoSetup5LayerBackground();
        
        // 如果是临时创建的，设置完后删除
        if (setupper.gameObject.name == "TempBackgroundSetupper")
        {
            Object.DestroyImmediate(setupper.gameObject);
        }
    }
    
    [MenuItem("Tools/背景设置/🔍 检查背景设置")]
    public static void ValidateBackground()
    {
        var setupper = Object.FindObjectOfType<自动背景设置器>();
        if (setupper == null)
        {
            GameObject tempObj = new GameObject("TempBackgroundSetupper");
            setupper = tempObj.AddComponent<自动背景设置器>();
        }
        
        setupper.ValidateBackgroundSetup();
        
        if (setupper.gameObject.name == "TempBackgroundSetupper")
        {
            Object.DestroyImmediate(setupper.gameObject);
        }
    }
    
    [MenuItem("Tools/背景设置/🗑️ 清理背景对象")]
    public static void CleanBackground()
    {
        var setupper = Object.FindObjectOfType<自动背景设置器>();
        if (setupper == null)
        {
            GameObject tempObj = new GameObject("TempBackgroundSetupper");
            setupper = tempObj.AddComponent<自动背景设置器>();
        }
        
        setupper.CleanupBackground();
        
        if (setupper.gameObject.name == "TempBackgroundSetupper")
        {
            Object.DestroyImmediate(setupper.gameObject);
        }
    }
}
#endif 