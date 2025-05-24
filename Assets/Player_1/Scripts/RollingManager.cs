using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class RollingManager : MonoBehaviour
{
    [Header("翻滚设置")]
    public float rollSpeed = 10f;        // 固定翻滚速度
    public float rollDuration = 0.5f;    // 翻滚持续时间
    public float rollCooldown = 2f;      // 翻滚冷却时间

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerController playerController;
    
    private bool isRolling = false;
    private bool canRoll = true;
    private float rollCooldownTimer = 0f;
    private float currentRollTime = 0f;
    private float rollDirection = 0f;
    private bool wasMoving = false;      // 记录翻滚前是否在移动

    // 动画参数名称常量
    private readonly int SpeedHash = Animator.StringToHash("Speed");
    private readonly int RollTriggerHash = Animator.StringToHash("Roll");     // 修改为Roll
    private readonly int IsRollingHash = Animator.StringToHash("IsRolling"); // 添加IsRolling参数

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        HandleRollCooldown();
        HandleRollInput();
        
        if (isRolling)
        {
            UpdateRoll();
        }

        // 调试信息
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log($"右键按下 - canRoll: {canRoll}, isRolling: {isRolling}, isGrounded: {playerController.isGrounded}");
        }
    }

    void HandleRollCooldown()
    {
        if (!canRoll)
        {
            rollCooldownTimer += Time.deltaTime;
            if (rollCooldownTimer >= rollCooldown)
            {
                canRoll = true;
                rollCooldownTimer = 0f;
            }
        }
    }

    void HandleRollInput()
    {
        if (Input.GetMouseButtonDown(1) && canRoll && !isRolling && playerController.isGrounded)
        {
            StartRoll();
        }
    }

    void StartRoll()
    {
        if (!canRoll || isRolling) return;

        // 记录当前移动状态
        wasMoving = animator.GetFloat(SpeedHash) > 0.1f;
        
        isRolling = true;
        canRoll = false;
        currentRollTime = 0f;
        
        // 设置翻滚方向
        rollDirection = spriteRenderer.flipX ? -1f : 1f;
        
        // 设置动画状态
        animator.SetBool(IsRollingHash, true);
        animator.SetTrigger(RollTriggerHash);
        
        // 设置翻滚速度
        rb.linearVelocity = new Vector2(rollDirection * rollSpeed, rb.linearVelocity.y);
        
        // 调试信息
        Debug.Log("开始翻滚");
    }

    void UpdateRoll()
    {
        currentRollTime += Time.deltaTime;
        
        // 保持翻滚速度
        rb.linearVelocity = new Vector2(rollDirection * rollSpeed, rb.linearVelocity.y);

        // 获取当前动画状态信息
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        // 检查是否在Roll状态且动画播放完成
        if (currentRollTime >= rollDuration || 
            (stateInfo.IsTag("Roll") && stateInfo.normalizedTime >= 1.0f))
        {
            EndRoll();
        }
    }

    void EndRoll()
    {
        isRolling = false;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        
        // 重置动画状态
        animator.SetBool(IsRollingHash, false);
        
        // 恢复移动状态
        animator.SetFloat(SpeedHash, wasMoving ? 4f : 0f);
        
        // 调试信息
        Debug.Log("结束翻滚");
    }

    public bool IsRolling()
    {
        return isRolling;
    }
}