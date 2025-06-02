using UnityEngine;

/// <summary>
/// 背景跟随修复器 - 确保背景始终跟随摄像机移动
/// </summary>
public class 背景跟随修复器 : MonoBehaviour
{
    [Header("跟随设置")]
    [Tooltip("参考摄像机")]
    public Camera targetCamera;
    
    [Tooltip("背景跟随偏移")]
    public Vector3 followOffset = new Vector3(0, 0, 10);
    
    [Tooltip("是否启用平滑跟随")]
    public bool smoothFollow = false;
    
    [Tooltip("平滑跟随速度")]
    [Range(1f, 20f)]
    public float followSpeed = 10f;
    
    [Tooltip("是否只跟随X轴")]
    public bool followXOnly = false;
    
    [Tooltip("是否只跟随Y轴")]
    public bool followYOnly = false;
    
    private Vector3 lastCameraPosition;
    
    void Start()
    {
        // 自动查找摄像机
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                targetCamera = FindObjectOfType<Camera>();
            }
        }
        
        if (targetCamera != null)
        {
            lastCameraPosition = targetCamera.transform.position;
            // 初始化背景位置
            UpdateBackgroundPosition();
            Debug.Log($"<color=green>[背景跟随] 初始化完成，跟随摄像机: {targetCamera.name}</color>");
        }
        else
        {
            Debug.LogError("<color=red>[背景跟随] 找不到目标摄像机！</color>");
        }
    }
    
    void LateUpdate()
    {
        if (targetCamera != null)
        {
            UpdateBackgroundPosition();
        }
    }
    
    /// <summary>
    /// 更新背景位置
    /// </summary>
    private void UpdateBackgroundPosition()
    {
        Vector3 cameraPos = targetCamera.transform.position;
        
        // 计算目标位置
        Vector3 targetPos = cameraPos + followOffset;
        
        // 根据设置限制跟随轴
        if (followXOnly)
        {
            targetPos.y = transform.position.y;
        }
        else if (followYOnly)
        {
            targetPos.x = transform.position.x;
        }
        
        // 应用位置
        if (smoothFollow)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = targetPos;
        }
        
        lastCameraPosition = cameraPos;
    }
    
    /// <summary>
    /// 立即同步到摄像机位置
    /// </summary>
    [ContextMenu("🎯 立即同步位置")]
    public void SyncToCamera()
    {
        if (targetCamera != null)
        {
            Vector3 targetPos = targetCamera.transform.position + followOffset;
            transform.position = targetPos;
            Debug.Log($"<color=green>[背景跟随] 已同步到摄像机位置: {targetPos}</color>");
        }
    }
    
    /// <summary>
    /// 设置跟随偏移
    /// </summary>
    public void SetFollowOffset(Vector3 offset)
    {
        followOffset = offset;
        Debug.Log($"<color=cyan>[背景跟随] 跟随偏移设置为: {offset}</color>");
    }
}

/// <summary>
/// 增强版ParallaxController - 结合背景跟随功能
/// </summary>
public class 增强视差控制器 : MonoBehaviour
{
    [Header("基础跟随设置")]
    [Tooltip("参考摄像机")]
    public Camera referenceCamera;
    
    [Tooltip("背景容器是否跟随摄像机")]
    public bool followCamera = true;
    
    [Tooltip("跟随偏移")]
    public Vector3 followOffset = new Vector3(0, 0, 10);
    
    [Header("视差设置")]
    [Tooltip("是否启用视差效果")]
    public bool enableParallax = true;
    
    [Tooltip("视差图层列表")]
    public ParallaxLayer[] parallaxLayers;
    
    private Vector3 previousCameraPosition;
    private Vector3 basePosition; // 背景容器的基础位置
    
    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform layerTransform;
        [Range(0f, 2f)]
        public float parallaxFactor = 0.5f;
        public string layerName = "图层";
        public bool enabled = true;
        [Range(0f, 2f)]
        public float verticalParallaxFactor = 0.5f;
        public Vector3 initialLocalPosition; // 相对于容器的初始位置
    }
    
    void Start()
    {
        InitializeSystem();
    }
    
    void LateUpdate()
    {
        if (referenceCamera != null)
        {
            // 1. 更新背景容器的基础位置（跟随摄像机）
            if (followCamera)
            {
                UpdateBasePosition();
            }
            
            // 2. 更新视差效果
            if (enableParallax)
            {
                UpdateParallaxEffect();
            }
        }
    }
    
    /// <summary>
    /// 初始化系统
    /// </summary>
    private void InitializeSystem()
    {
        // 查找摄像机
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
            Debug.LogError("<color=red>[增强视差控制器] 找不到参考摄像机！</color>");
            return;
        }
        
        // 记录摄像机位置
        previousCameraPosition = referenceCamera.transform.position;
        
        // 设置背景容器初始位置
        basePosition = referenceCamera.transform.position + followOffset;
        transform.position = basePosition;
        
        // 初始化各图层的相对位置
        foreach (var layer in parallaxLayers)
        {
            if (layer.layerTransform != null)
            {
                layer.initialLocalPosition = layer.layerTransform.localPosition;
            }
        }
        
        Debug.Log($"<color=green>[增强视差控制器] 初始化完成！跟随摄像机: {referenceCamera.name}</color>");
    }
    
    /// <summary>
    /// 更新背景容器基础位置
    /// </summary>
    private void UpdateBasePosition()
    {
        Vector3 cameraPos = referenceCamera.transform.position;
        basePosition = cameraPos + followOffset;
        transform.position = basePosition;
    }
    
    /// <summary>
    /// 更新视差效果
    /// </summary>
    private void UpdateParallaxEffect()
    {
        Vector3 currentCameraPos = referenceCamera.transform.position;
        Vector3 deltaMovement = currentCameraPos - previousCameraPosition;
        
        foreach (var layer in parallaxLayers)
        {
            if (layer.enabled && layer.layerTransform != null)
            {
                // 计算视差移动量
                float horizontalParallax = deltaMovement.x * (1f - layer.parallaxFactor);
                float verticalParallax = deltaMovement.y * (1f - layer.verticalParallaxFactor);
                
                // 应用视差偏移（相对于初始位置）
                Vector3 parallaxOffset = new Vector3(-horizontalParallax, -verticalParallax, 0);
                layer.layerTransform.localPosition = layer.initialLocalPosition + parallaxOffset;
            }
        }
        
        previousCameraPosition = currentCameraPos;
    }
    
    /// <summary>
    /// 重置所有图层位置
    /// </summary>
    [ContextMenu("🔄 重置视差图层")]
    public void ResetParallaxLayers()
    {
        foreach (var layer in parallaxLayers)
        {
            if (layer.layerTransform != null)
            {
                layer.layerTransform.localPosition = layer.initialLocalPosition;
            }
        }
        Debug.Log("<color=yellow>[增强视差控制器] 所有图层已重置</color>");
    }
    
    /// <summary>
    /// 立即同步到摄像机
    /// </summary>
    [ContextMenu("🎯 同步到摄像机")]
    public void SyncToCamera()
    {
        if (referenceCamera != null)
        {
            basePosition = referenceCamera.transform.position + followOffset;
            transform.position = basePosition;
            previousCameraPosition = referenceCamera.transform.position;
            Debug.Log("<color=green>[增强视差控制器] 已同步到摄像机位置</color>");
        }
    }
} 