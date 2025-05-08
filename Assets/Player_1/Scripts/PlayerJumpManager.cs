using UnityEngine;

/// <summary>
/// 管理角色跳跃动画状态的组件
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerJumpManager : MonoBehaviour
{
    private Animator animator;
    private bool isInJumpAnimation = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// 触发跳跃起始动画，由PlayerController在按下space键且在地面时调用
    /// </summary>
    public void TriggerJumpStart()
    {
        if (!isInJumpAnimation)
        {
            Debug.Log("开始跳跃动画序列");
            animator.ResetTrigger("jump_off");
            animator.SetBool("jump_continue", false);
            animator.SetTrigger("jump_off");
            isInJumpAnimation = true;
        }
    }

    /// <summary>
    /// 在jump_off动画最后一帧通过Animation Event调用
    /// </summary>
    public void JumpOffEnd()
    {
        Debug.Log("起跳动画完成，切换到持续跳跃动画");
        animator.SetBool("jump_continue", true);
    }

    /// <summary>
    /// 落地时重置所有跳跃动画状态
    /// </summary>
    public void ResetJumpState()
    {
        Debug.Log("重置跳跃动画状态");
        isInJumpAnimation = false;
        animator.SetBool("jump_continue", false);
        animator.ResetTrigger("jump_off");
    }
}
