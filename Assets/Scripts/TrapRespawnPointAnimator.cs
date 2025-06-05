using UnityEngine;

/// <summary>
/// Trap复活点动画控制器 - 专门处理使用Trap作为复活点的动画逻辑
/// 控制从静止状态到激活播放动画的转换
/// </summary>
public class TrapRespawnPointAnimator : MonoBehaviour
{
    [Header("Trap动画设置")]
    [Tooltip("Trap的动画控制器")]
    public Animator trapAnimator;
    
    [Tooltip("静止状态的动画名称")]
    public string idleAnimationName = "Trap_Idle";
    
    [Tooltip("激活状态的动画名称")]
    public string activeAnimationName = "SpikeExtendRetract";
    
    [Tooltip("激活触发器名称")]
    public string activationTrigger = "Activate";
    
    [Tooltip("是否循环播放激活动画")]
    public bool loopActiveAnimation = true;
    
    [Tooltip("激活后动画播放速度")]
    public float activeAnimationSpeed = 1f;
    
    private bool isActivated = false;
    private RespawnPoint respawnPoint;
    
    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private string currentAnimationState = "未初始化";
    
    void Awake()
    {
        // 获取组件
        if (trapAnimator == null)
            trapAnimator = GetComponent<Animator>();
            
        respawnPoint = GetComponent<RespawnPoint>();
        
        if (trapAnimator == null)
        {
            Debug.LogError("[TrapRespawnPointAnimator] 找不到Animator组件！", this);
            enabled = false;
            return;
        }
    }
    
    void Start()
    {
        // 设置初始状态为静止
        SetIdleState();
        
        if (showDebugInfo)
        {
            Debug.Log($"[TrapRespawnPointAnimator] Trap复活点动画控制器已初始化：{gameObject.name}");
        }
    }
    
    void Update()
    {
        // 更新调试信息
        if (showDebugInfo)
        {
            UpdateDebugInfo();
        }
        
        // 检查复活点状态变化
        CheckRespawnPointState();
    }
    
    /// <summary>
    /// 检查复活点状态变化
    /// </summary>
    void CheckRespawnPointState()
    {
        if (respawnPoint != null)
        {
            bool shouldBeActivated = respawnPoint.IsActivated;
            
            if (shouldBeActivated && !isActivated)
            {
                ActivateTrapAnimation();
            }
            else if (!shouldBeActivated && isActivated)
            {
                DeactivateTrapAnimation();
            }
        }
    }
    
    /// <summary>
    /// 设置静止状态
    /// </summary>
    public void SetIdleState()
    {
        if (trapAnimator == null) return;
        
        // 停止所有动画
        trapAnimator.speed = 0f;
        
        // 如果有特定的静止动画，播放它
        if (!string.IsNullOrEmpty(idleAnimationName))
        {
            trapAnimator.Play(idleAnimationName);
        }
        
        isActivated = false;
        currentAnimationState = "静止状态";
        
        if (showDebugInfo)
        {
            Debug.Log($"[TrapRespawnPointAnimator] 设置为静止状态：{gameObject.name}");
        }
    }
    
    /// <summary>
    /// 激活Trap动画
    /// </summary>
    public void ActivateTrapAnimation()
    {
        if (trapAnimator == null || isActivated) return;
        
        isActivated = true;
        
        // 恢复动画播放速度
        trapAnimator.speed = activeAnimationSpeed;
        
        // 触发激活动画
        if (!string.IsNullOrEmpty(activationTrigger))
        {
            trapAnimator.SetTrigger(activationTrigger);
        }
        
        // 如果指定了激活动画名称，直接播放
        if (!string.IsNullOrEmpty(activeAnimationName))
        {
            trapAnimator.Play(activeAnimationName);
        }
        
        // 设置循环模式
        if (loopActiveAnimation)
        {
            trapAnimator.SetBool("IsActive", true);
        }
        
        currentAnimationState = "激活状态";
        
        if (showDebugInfo)
        {
            Debug.Log($"[TrapRespawnPointAnimator] 激活Trap动画：{gameObject.name}");
        }
    }
    
    /// <summary>
    /// 停用Trap动画
    /// </summary>
    public void DeactivateTrapAnimation()
    {
        if (trapAnimator == null || !isActivated) return;
        
        isActivated = false;
        
        // 停止循环
        trapAnimator.SetBool("IsActive", false);
        
        // 回到静止状态
        SetIdleState();
        
        if (showDebugInfo)
        {
            Debug.Log($"[TrapRespawnPointAnimator] 停用Trap动画：{gameObject.name}");
        }
    }
    
    /// <summary>
    /// 手动切换动画状态
    /// </summary>
    public void ToggleAnimation()
    {
        if (isActivated)
        {
            DeactivateTrapAnimation();
        }
        else
        {
            ActivateTrapAnimation();
        }
    }
    
    /// <summary>
    /// 设置动画播放速度
    /// </summary>
    public void SetAnimationSpeed(float speed)
    {
        activeAnimationSpeed = speed;
        
        if (trapAnimator != null && isActivated)
        {
            trapAnimator.speed = speed;
        }
    }
    
    /// <summary>
    /// 更新调试信息
    /// </summary>
    void UpdateDebugInfo()
    {
        if (trapAnimator != null)
        {
            AnimatorStateInfo stateInfo = trapAnimator.GetCurrentAnimatorStateInfo(0);
            currentAnimationState = $"{(isActivated ? "激活" : "静止")} - {stateInfo.shortNameHash}";
        }
    }
    
    /// <summary>
    /// 获取当前动画状态信息
    /// </summary>
    public string GetAnimationStateInfo()
    {
        if (trapAnimator == null) return "无动画控制器";
        
        AnimatorStateInfo stateInfo = trapAnimator.GetCurrentAnimatorStateInfo(0);
        return $"状态：{(isActivated ? "激活" : "静止")}，动画：{stateInfo.shortNameHash}，进度：{stateInfo.normalizedTime:F2}";
    }
    
    /// <summary>
    /// 检查动画是否正在播放
    /// </summary>
    public bool IsAnimationPlaying()
    {
        if (trapAnimator == null) return false;
        
        AnimatorStateInfo stateInfo = trapAnimator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.normalizedTime < 1.0f || loopActiveAnimation;
    }
    
    /// <summary>
    /// 强制重置动画状态
    /// </summary>
    public void ResetAnimation()
    {
        if (trapAnimator == null) return;
        
        trapAnimator.Rebind();
        trapAnimator.Update(0f);
        
        SetIdleState();
        
        if (showDebugInfo)
        {
            Debug.Log($"[TrapRespawnPointAnimator] 重置动画状态：{gameObject.name}");
        }
    }
    
    /// <summary>
    /// 在Inspector中显示的调试按钮方法
    /// </summary>
    [ContextMenu("测试激活动画")]
    void TestActivateAnimation()
    {
        ActivateTrapAnimation();
    }
    
    [ContextMenu("测试停用动画")]
    void TestDeactivateAnimation()
    {
        DeactivateTrapAnimation();
    }
    
    [ContextMenu("重置动画")]
    void TestResetAnimation()
    {
        ResetAnimation();
    }
} 