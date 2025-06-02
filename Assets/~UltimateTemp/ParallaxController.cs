using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 视差滚动控制器 - 让多层背景图以不同速度跟随摄像机移动
/// 配合CinemachineCamera实现视差效果
/// </summary>
public class ParallaxController : MonoBehaviour
{
    [Header("视差设置")]
    [Tooltip("参考摄像机（留空自动查找Main Camera）")]
    public Camera referenceCamera;
    
    [Tooltip("是否启用视差效果")]
    public bool enableParallax = true;
    
    [Tooltip("是否显示调试信息")]
    public bool showDebugInfo = true;
    
    [Header("背景图层")]
    [Tooltip("所有背景图层的配置")]
    public List<ParallaxLayer> parallaxLayers = new List<ParallaxLayer>();
    
    private Vector3 previousCameraPosition;
    private bool isInitialized = false;
    
    [System.Serializable]
    public class ParallaxLayer
    {
        [Tooltip("背景图层的Transform")]
        public Transform layerTransform;
        
        [Tooltip("视差系数 (0=不动, 1=跟摄像机同速, >1=比摄像机快)")]
        [Range(0f, 2f)]
        public float parallaxFactor = 0.5f;
        
        [Tooltip("图层名称（用于调试）")]
        public string layerName = "背景层";
        
        [Tooltip("是否启用此图层")]
        public bool enabled = true;
        
        [Tooltip("是否启用垂直视差")]
        public bool enableVerticalParallax = true;
        
        [Tooltip("垂直视差系数")]
        [Range(0f, 2f)]
        public float verticalParallaxFactor = 0.5f;
        
        [Tooltip("图层的初始位置（自动记录）")]
        [HideInInspector]
        public Vector3 initialPosition;
    }
    
    void Start()
    {
        InitializeParallax();
    }
    
    void LateUpdate()
    {
        if (enableParallax && isInitialized)
        {
            UpdateParallax();
        }
    }
    
    /// <summary>
    /// 初始化视差系统
    /// </summary>
    private void InitializeParallax()
    {
        // 自动查找摄像机
        if (referenceCamera == null)
        {
            referenceCamera = Camera.main;
            if (referenceCamera == null)
            {
                referenceCamera = FindObjectOfType<Camera>();
            }
        }
        
        if (referenceCamera == null)
        {
            Debug.LogError($"<color=red>[ParallaxController] 找不到参考摄像机！</color>");
            return;
        }
        
        // 记录摄像机初始位置
        previousCameraPosition = referenceCamera.transform.position;
        
        // 初始化所有图层
        foreach (var layer in parallaxLayers)
        {
            if (layer.layerTransform != null)
            {
                layer.initialPosition = layer.layerTransform.position;
                
                if (showDebugInfo)
                {
                    Debug.Log($"<color=cyan>[ParallaxController] 初始化图层: {layer.layerName}</color>");
                    Debug.Log($"<color=yellow>初始位置: {layer.initialPosition}, 视差系数: {layer.parallaxFactor}</color>");
                }
            }
        }
        
        isInitialized = true;
        
        if (showDebugInfo)
        {
            Debug.Log($"<color=green>[ParallaxController] 视差系统初始化完成！共{parallaxLayers.Count}个图层</color>");
        }
    }
    
    /// <summary>
    /// 更新视差效果
    /// </summary>
    private void UpdateParallax()
    {
        // 计算摄像机移动量
        Vector3 deltaMovement = referenceCamera.transform.position - previousCameraPosition;
        
        // 更新每个图层
        foreach (var layer in parallaxLayers)
        {
            if (layer.enabled && layer.layerTransform != null)
            {
                // 计算水平视差移动
                float horizontalMovement = deltaMovement.x * layer.parallaxFactor;
                
                // 计算垂直视差移动
                float verticalMovement = 0f;
                if (layer.enableVerticalParallax)
                {
                    verticalMovement = deltaMovement.y * layer.verticalParallaxFactor;
                }
                
                // 应用移动
                Vector3 newPosition = layer.layerTransform.position;
                newPosition.x += horizontalMovement;
                newPosition.y += verticalMovement;
                
                layer.layerTransform.position = newPosition;
            }
        }
        
        // 更新摄像机位置记录
        previousCameraPosition = referenceCamera.transform.position;
    }
    
    /// <summary>
    /// 添加新的视差图层
    /// </summary>
    public void AddParallaxLayer(Transform layerTransform, float parallaxFactor, string layerName = "新图层")
    {
        ParallaxLayer newLayer = new ParallaxLayer
        {
            layerTransform = layerTransform,
            parallaxFactor = parallaxFactor,
            layerName = layerName,
            enabled = true,
            enableVerticalParallax = true,
            verticalParallaxFactor = parallaxFactor,
            initialPosition = layerTransform.position
        };
        
        parallaxLayers.Add(newLayer);
        
        if (showDebugInfo)
        {
            Debug.Log($"<color=green>[ParallaxController] 添加新图层: {layerName}</color>");
        }
    }
    
    /// <summary>
    /// 重置所有图层到初始位置
    /// </summary>
    [ContextMenu("重置所有图层位置")]
    public void ResetAllLayers()
    {
        foreach (var layer in parallaxLayers)
        {
            if (layer.layerTransform != null)
            {
                layer.layerTransform.position = layer.initialPosition;
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"<color=yellow>[ParallaxController] 所有图层已重置到初始位置</color>");
        }
    }
    
    /// <summary>
    /// 启用/禁用视差效果
    /// </summary>
    public void SetParallaxEnabled(bool enabled)
    {
        enableParallax = enabled;
        
        if (showDebugInfo)
        {
            Debug.Log($"<color=cyan>[ParallaxController] 视差效果: {(enabled ? "启用" : "禁用")}</color>");
        }
    }
    
    /// <summary>
    /// 设置指定图层的视差系数
    /// </summary>
    public void SetLayerParallaxFactor(int layerIndex, float parallaxFactor)
    {
        if (layerIndex >= 0 && layerIndex < parallaxLayers.Count)
        {
            parallaxLayers[layerIndex].parallaxFactor = parallaxFactor;
            
            if (showDebugInfo)
            {
                Debug.Log($"<color=cyan>[ParallaxController] 设置图层{layerIndex}视差系数为: {parallaxFactor}</color>");
            }
        }
    }
    
    /// <summary>
    /// 自动设置5层背景的推荐视差系数
    /// </summary>
    [ContextMenu("应用5层背景推荐设置")]
    public void ApplyRecommended5LayerSettings()
    {
        if (parallaxLayers.Count >= 5)
        {
            // 推荐的5层背景视差系数（从远到近）
            float[] recommendedFactors = { 0.1f, 0.3f, 0.5f, 0.7f, 0.9f };
            string[] layerNames = { "远景天空", "远山", "中景", "近景树木", "前景装饰" };
            
            for (int i = 0; i < 5; i++)
            {
                if (i < parallaxLayers.Count)
                {
                    parallaxLayers[i].parallaxFactor = recommendedFactors[i];
                    parallaxLayers[i].verticalParallaxFactor = recommendedFactors[i] * 0.5f; // 垂直移动减半
                    parallaxLayers[i].layerName = layerNames[i];
                    parallaxLayers[i].enabled = true;
                }
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"<color=green>[ParallaxController] 已应用5层背景推荐设置</color>");
            }
        }
        else
        {
            Debug.LogWarning($"<color=yellow>[ParallaxController] 需要至少5个图层才能应用推荐设置</color>");
        }
    }
    
    /// <summary>
    /// 显示所有图层信息
    /// </summary>
    [ContextMenu("显示图层信息")]
    public void LogLayerInfo()
    {
        Debug.Log($"<color=cyan>=== 视差图层信息 ===</color>");
        Debug.Log($"摄像机: {(referenceCamera != null ? referenceCamera.name : "未找到")}");
        Debug.Log($"视差启用: {enableParallax}");
        Debug.Log($"图层数量: {parallaxLayers.Count}");
        
        for (int i = 0; i < parallaxLayers.Count; i++)
        {
            var layer = parallaxLayers[i];
            Debug.Log($"<color=white>图层 {i}: {layer.layerName}</color>");
            Debug.Log($"  启用: {layer.enabled}");
            Debug.Log($"  视差系数: {layer.parallaxFactor}");
            Debug.Log($"  垂直视差: {layer.enableVerticalParallax} (系数: {layer.verticalParallaxFactor})");
            Debug.Log($"  Transform: {(layer.layerTransform != null ? layer.layerTransform.name : "未设置")}");
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ParallaxController))]
public class ParallaxControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        ParallaxController controller = (ParallaxController)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("快速操作", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("应用5层推荐设置"))
        {
            controller.ApplyRecommended5LayerSettings();
        }
        if (GUILayout.Button("重置图层位置"))
        {
            controller.ResetAllLayers();
        }
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("显示图层信息"))
        {
            controller.LogLayerInfo();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("图层管理", EditorStyles.boldLabel);
        
        if (GUILayout.Button("自动查找背景图层"))
        {
            AutoFindBackgroundLayers(controller);
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "视差滚动使用说明:\n" +
            "• 将背景图的Transform拖到对应图层槽\n" +
            "• 调整视差系数 (0=不动, 1=跟摄像机同速)\n" +
            "• 远景用小数值(0.1-0.3)，近景用大数值(0.7-0.9)\n" +
            "• 点击'应用5层推荐设置'快速配置", 
            MessageType.Info);
    }
    
    private void AutoFindBackgroundLayers(ParallaxController controller)
    {
        // 尝试自动查找名称包含"背景"、"Background"等的GameObject
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        List<Transform> foundLayers = new List<Transform>();
        
        foreach (GameObject obj in allObjects)
        {
            string name = obj.name.ToLower();
            if (name.Contains("背景") || name.Contains("background") || 
                name.Contains("layer") || name.Contains("parallax") ||
                name.Contains("bg") || name.Contains("远景") || 
                name.Contains("近景") || name.Contains("中景"))
            {
                foundLayers.Add(obj.transform);
            }
        }
        
        if (foundLayers.Count > 0)
        {
            Debug.Log($"找到 {foundLayers.Count} 个可能的背景图层");
            foreach (Transform layer in foundLayers)
            {
                Debug.Log($"- {layer.name}");
            }
        }
        else
        {
            Debug.Log("未找到符合命名规则的背景图层");
        }
    }
}
#endif 