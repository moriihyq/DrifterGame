using UnityEngine;

/// <summary>
/// 平滑玩家移动脚本 - 专门针对摄像机跟随优化
/// 替代PlayerRun，减少摄像机抖动
/// </summary>
public class SmoothPlayerMovement : MonoBehaviour
{
    [Header("移动设置")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float acceleration = 10f; // 加速度
    public float deceleration = 15f; // 减速度
    
    [Header("平滑设置")]
    [Range(0.1f, 1f)]
    public float smoothing = 0.8f; // 移动平滑程度
    public bool useSmoothing = true; // 是否启用平滑移动
    
    [Header("Rigidbody优化")]
    public bool autoOptimizeRigidbody = true;
    public RigidbodyInterpolation2D interpolationMode = RigidbodyInterpolation2D.Interpolate;
    public float linearDrag = 2f;
    
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 targetVelocity;
    private Vector2 smoothedVelocity;
    private float currentHorizontalSpeed;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        
        if (autoOptimizeRigidbody && rb != null)
        {
            OptimizeRigidbodySettings();
        }
    }
    
    void Start()
    {
        // 初始化
        targetVelocity = Vector2.zero;
        smoothedVelocity = Vector2.zero;
        currentHorizontalSpeed = 0f;
    }
    
    void Update()
    {
        HandleInput();
        
        if (useSmoothing)
        {
            ApplySmoothMovement();
        }
        else
        {
            ApplyDirectMovement();
        }
        
        HandleSpriteFlip();
    }
    
    void HandleInput()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        
        // 计算目标速度
        targetVelocity.x = moveInput * targetSpeed;
        targetVelocity.y = rb.linearVelocity.y; // 保持垂直速度不变
    }
    
    void ApplySmoothMovement()
    {
        // 平滑插值到目标速度
        float targetHorizontalSpeed = targetVelocity.x;
        
        // 使用不同的加速度和减速度
        float speedDifference = targetHorizontalSpeed - currentHorizontalSpeed;
        float accelerationToUse = (Mathf.Sign(speedDifference) == Mathf.Sign(currentHorizontalSpeed)) ? acceleration : deceleration;
        
        // 平滑过渡到目标速度
        currentHorizontalSpeed = Mathf.MoveTowards(currentHorizontalSpeed, targetHorizontalSpeed, accelerationToUse * Time.deltaTime);
        
        // 应用额外的平滑
        smoothedVelocity.x = Mathf.Lerp(smoothedVelocity.x, currentHorizontalSpeed, smoothing);
        smoothedVelocity.y = rb.linearVelocity.y;
        
        // 设置刚体速度
        rb.linearVelocity = smoothedVelocity;
    }
    
    void ApplyDirectMovement()
    {
        // 直接设置速度（原有方式）
        rb.linearVelocity = new Vector2(targetVelocity.x, rb.linearVelocity.y);
    }
    
    void HandleSpriteFlip()
    {
        if (sr != null && targetVelocity.x != 0)
        {
            sr.flipX = targetVelocity.x < 0;
        }
    }
    
    void OptimizeRigidbodySettings()
    {
        if (rb == null) return;
        
        // 启用插值以减少抖动
        rb.interpolation = interpolationMode;
        
        // 设置线性阻力
        rb.linearDamping = linearDrag;
        
        // 冻结Z轴旋转（2D游戏）
        rb.freezeRotation = true;
        
        Debug.Log($"<color=#00FF00>[SmoothPlayerMovement] 已优化 {gameObject.name} 的Rigidbody2D设置</color>");
        Debug.Log($"<color=#FFFF00>  - 插值模式: {rb.interpolation}</color>");
        Debug.Log($"<color=#FFFF00>  - 线性阻力: {rb.linearDamping}</color>");
    }
    
    /// <summary>
    /// 运行时调整平滑程度
    /// </summary>
    /// <param name="newSmoothing">新的平滑值 (0.1-1.0)</param>
    public void SetSmoothing(float newSmoothing)
    {
        smoothing = Mathf.Clamp(newSmoothing, 0.1f, 1f);
        Debug.Log($"<color=#FFFF00>[SmoothPlayerMovement] 平滑程度设置为: {smoothing}</color>");
    }
    
    /// <summary>
    /// 切换平滑移动模式
    /// </summary>
    public void ToggleSmoothing()
    {
        useSmoothing = !useSmoothing;
        Debug.Log($"<color=#FFFF00>[SmoothPlayerMovement] 平滑移动: {(useSmoothing ? "启用" : "禁用")}</color>");
    }
    
    /// <summary>
    /// 获取当前移动速度（用于动画系统）
    /// </summary>
    public float GetCurrentSpeed()
    {
        return Mathf.Abs(currentHorizontalSpeed);
    }
    
    /// <summary>
    /// 检查是否在奔跑
    /// </summary>
    public bool IsRunning()
    {
        return Input.GetKey(KeyCode.LeftShift) && Mathf.Abs(targetVelocity.x) > 0.1f;
    }
    
    /// <summary>
    /// 获取移动方向 (-1左, 0停止, 1右)
    /// </summary>
    public int GetMoveDirection()
    {
        if (targetVelocity.x > 0.1f) return 1;
        if (targetVelocity.x < -0.1f) return -1;
        return 0;
    }
    
    void OnValidate()
    {
        // 确保参数在合理范围内
        walkSpeed = Mathf.Max(0f, walkSpeed);
        runSpeed = Mathf.Max(walkSpeed, runSpeed);
        acceleration = Mathf.Max(1f, acceleration);
        deceleration = Mathf.Max(1f, deceleration);
        smoothing = Mathf.Clamp(smoothing, 0.1f, 1f);
        linearDrag = Mathf.Max(0f, linearDrag);
    }
    
    // 调试信息
    void OnDrawGizmos()
    {
        if (Application.isPlaying && rb != null)
        {
            // 绘制速度向量
            Vector3 velocityDirection = transform.position + (Vector3)rb.linearVelocity * 0.1f;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, velocityDirection);
            Gizmos.DrawSphere(velocityDirection, 0.1f);
            
            // 绘制目标速度
            Vector3 targetDirection = transform.position + (Vector3)targetVelocity * 0.1f;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetDirection);
            Gizmos.DrawWireSphere(targetDirection, 0.1f);
        }
    }
}

/// <summary>
/// 扩展的玩家移动脚本，包含更多功能
/// 可以替代PlayerController中的移动部分
/// </summary>
[System.Serializable]
public class AdvancedMovementSettings
{
    [Header("基础移动")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    
    [Header("平滑设置")]
    public bool enableSmoothing = true;
    [Range(0.1f, 1f)]
    public float smoothingFactor = 0.8f;
    public float acceleration = 10f;
    public float deceleration = 15f;
    
    [Header("摄像机优化")]
    public bool optimizeForCamera = true;
    public RigidbodyInterpolation2D interpolationMode = RigidbodyInterpolation2D.Interpolate;
    public float optimalLinearDrag = 2f;
    
    [Header("调试")]
    public bool showDebugInfo = false;
    public bool showGizmos = false;
} 