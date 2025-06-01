using UnityEngine;

/// <summary>
/// 临时使用的Boss动画管理器，在正确设置Animator Controller前使用
/// </summary>
[RequireComponent(typeof(Animator))]
public class BossAnimationManager : MonoBehaviour
{
    private Animator anim;
    private BossController bossController;
    
    // 临时使用的动画状态
    public enum BossAnimationState
    {
        Idle,
        Walking,
        MeleeAttack,
        RangedAttack,
        Hurt,
        Dead
    }
    
    private BossAnimationState currentState = BossAnimationState.Idle;
    
    void Start()
    {
        anim = GetComponent<Animator>();
        bossController = GetComponent<BossController>();
        
        // 检查Animator参数是否设置正确
        CheckAnimatorParameters();
    }
    
    /// <summary>
    /// 检查Animator参数是否设置正确
    /// </summary>
    private void CheckAnimatorParameters()
    {
        // 检查是否存在必要的参数
        bool hasIsMovingParam = false;
        bool hasIsActiveParam = false;
        bool hasAttackParam = false;
        bool hasShootParam = false;
        bool hasHurtParam = false;
        bool hasIsDeadParam = false;
        
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == "isMoving") hasIsMovingParam = true;
            else if (param.name == "isActive") hasIsActiveParam = true;
            else if (param.name == "Attack") hasAttackParam = true;
            else if (param.name == "Shoot") hasShootParam = true;
            else if (param.name == "Hurt") hasHurtParam = true;
            else if (param.name == "IsDead") hasIsDeadParam = true;
        }
        
        // 如果缺少任何参数，输出警告
        if (!hasIsMovingParam) Debug.LogWarning("Animator缺少'isMoving'参数");
        if (!hasIsActiveParam) Debug.LogWarning("Animator缺少'isActive'参数");
        if (!hasAttackParam) Debug.LogWarning("Animator缺少'Attack'参数");
        if (!hasShootParam) Debug.LogWarning("Animator缺少'Shoot'参数");
        if (!hasHurtParam) Debug.LogWarning("Animator缺少'Hurt'参数");
        if (!hasIsDeadParam) Debug.LogWarning("Animator缺少'IsDead'参数");
        
        // 如果缺少动画状态，输出警告
        if (!HasAnimation("Idle")) Debug.LogWarning("缺少'Idle'动画状态");
        if (!HasAnimation("Walk")) Debug.LogWarning("缺少'Walk'动画状态");
        if (!HasAnimation("Attack")) Debug.LogWarning("缺少'Attack'动画状态");
        if (!HasAnimation("Shoot")) Debug.LogWarning("缺少'Shoot'动画状态");
        if (!HasAnimation("Hurt")) Debug.LogWarning("缺少'Hurt'动画状态");
        if (!HasAnimation("Death")) Debug.LogWarning("缺少'Death'动画状态");
    }
    
    /// <summary>
    /// 检查指定名称的动画状态是否存在
    /// </summary>
    private bool HasAnimation(string stateName)
    {
        // 这不是100%准确的检查方法，但可以作为基本检查
        RuntimeAnimatorController controller = anim.runtimeAnimatorController;
        if (controller == null) return false;
        
        foreach (var clip in controller.animationClips)
        {
            if (clip.name == stateName) return true;
        }
        return false;
    }
    
    /// <summary>
    /// 播放走路动画
    /// </summary>
    public void PlayWalkAnimation(bool isWalking)
    {
        if (anim == null) return;
        
        try
        {
            anim.SetBool("isMoving", isWalking);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"设置isMoving参数时发生错误: {e.Message}");
        }
    }
    
    /// <summary>
    /// 播放近战攻击动画
    /// </summary>
    public void PlayAttackAnimation()
    {
        if (anim == null) return;
        
        try
        {
            anim.SetTrigger("Attack");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"设置Attack触发器时发生错误: {e.Message}");
        }
    }
    
    /// <summary>
    /// 播放远程攻击动画
    /// </summary>
    public void PlayShootAnimation()
    {
        if (anim == null) return;
        
        try
        {
            anim.SetTrigger("Shoot");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"设置Shoot触发器时发生错误: {e.Message}");
        }
    }
    
    /// <summary>
    /// 播放受伤动画
    /// </summary>
    public void PlayHurtAnimation()
    {
        if (anim == null) return;
        
        try
        {
            anim.SetTrigger("Hurt");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"设置Hurt触发器时发生错误: {e.Message}");
        }
    }
    
    /// <summary>
    /// 设置死亡状态
    /// </summary>
    public void SetDeathState(bool isDead)
    {
        if (anim == null) return;
        
        try
        {
            anim.SetBool("IsDead", isDead);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"设置IsDead参数时发生错误: {e.Message}");
        }
    }
    
    /// <summary>
    /// 设置激活状态
    /// </summary>
    public void SetActiveState(bool isActive)
    {
        if (anim == null) return;
        
        try
        {
            anim.SetBool("isActive", isActive);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"设置isActive参数时发生错误: {e.Message}");
        }
    }
}
