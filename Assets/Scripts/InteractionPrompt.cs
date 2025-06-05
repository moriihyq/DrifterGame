using UnityEngine;
using TMPro;

/// <summary>
/// 交互提示UI脚本 - 显示玩家可以交互的提示
/// 用于复活点等交互对象的提示显示
/// </summary>
public class InteractionPrompt : MonoBehaviour
{
    [Header("提示设置")]
    [Tooltip("提示文本组件")]
    public TextMeshPro promptText;
    
    [Tooltip("背景精灵渲染器")]
    public SpriteRenderer backgroundSprite;
    
    [Tooltip("提示显示的时间（0为永久显示）")]
    public float displayDuration = 0f;
    
    [Tooltip("是否面向摄像机")]
    public bool faceCamera = true;
    
    [Tooltip("上下浮动动画")]
    public bool enableFloatAnimation = true;
    
    [Tooltip("浮动幅度")]
    public float floatAmount = 0.2f;
    
    [Tooltip("浮动速度")]
    public float floatSpeed = 2f;
    
    private Vector3 originalPosition;
    private float floatTimer = 0f;
    private Camera mainCamera;
    
    void Awake()
    {
        // 获取主摄像机
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        // 如果没有找到TextMeshPro组件，尝试获取
        if (promptText == null)
        {
            promptText = GetComponentInChildren<TextMeshPro>();
        }
        
        // 如果没有找到SpriteRenderer组件，尝试获取
        if (backgroundSprite == null)
        {
            backgroundSprite = GetComponent<SpriteRenderer>();
        }
        
        // 记录原始位置
        originalPosition = transform.position;
    }
    
    void Start()
    {
        // 如果设置了显示时间，自动销毁
        if (displayDuration > 0)
        {
            Destroy(gameObject, displayDuration);
        }
    }
    
    void Update()
    {
        // 面向摄像机
        if (faceCamera && mainCamera != null)
        {
            FaceCamera();
        }
        
        // 浮动动画
        if (enableFloatAnimation)
        {
            FloatAnimation();
        }
    }
    
    /// <summary>
    /// 让提示面向摄像机
    /// </summary>
    void FaceCamera()
    {
        Vector3 directionToCamera = mainCamera.transform.position - transform.position;
        directionToCamera.z = 0; // 保持在2D平面
        
        if (directionToCamera != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        }
    }
    
    /// <summary>
    /// 浮动动画
    /// </summary>
    void FloatAnimation()
    {
        floatTimer += Time.deltaTime * floatSpeed;
        float yOffset = Mathf.Sin(floatTimer) * floatAmount;
        transform.position = originalPosition + Vector3.up * yOffset;
    }
    
    /// <summary>
    /// 设置提示文本
    /// </summary>
    public void SetPromptText(string text)
    {
        if (promptText != null)
        {
            promptText.text = text;
        }
    }
    
    /// <summary>
    /// 设置提示颜色
    /// </summary>
    public void SetPromptColor(Color color)
    {
        if (promptText != null)
        {
            promptText.color = color;
        }
    }
    
    /// <summary>
    /// 设置背景颜色
    /// </summary>
    public void SetBackgroundColor(Color color)
    {
        if (backgroundSprite != null)
        {
            backgroundSprite.color = color;
        }
    }
    
    /// <summary>
    /// 显示提示
    /// </summary>
    public void ShowPrompt()
    {
        gameObject.SetActive(true);
    }
    
    /// <summary>
    /// 隐藏提示
    /// </summary>
    public void HidePrompt()
    {
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 销毁提示
    /// </summary>
    public void DestroyPrompt()
    {
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 设置显示时长
    /// </summary>
    public void SetDisplayDuration(float duration)
    {
        displayDuration = duration;
        
        if (duration > 0)
        {
            CancelInvoke("DestroyPrompt");
            Invoke("DestroyPrompt", duration);
        }
    }
} 