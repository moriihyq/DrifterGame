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
    private bool wasRunning = false;     // 记录翻滚前是否在跑步

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
    }    void StartRoll()
    {
        if (!canRoll || isRolling) return;

        // 记录当前是否在跑步
        wasRunning = animator.GetBool("isRunning");
        
        isRolling = true;
        canRoll = false;
        currentRollTime = 0f;
        
        // 设置翻滚方向
        rollDirection = spriteRenderer.flipX ? -1f : 1f;
        
        // 设置为跑步状态
        animator.SetBool("isRunning", true);
        
        // 根据当前是否在跑步来设置初始翻滚速度
        float initialRollSpeed = wasRunning ? rollSpeed * 1.5f : rollSpeed;
        rb.linearVelocity = new Vector2(rollDirection * initialRollSpeed, rb.linearVelocity.y);
        
        // 触发翻滚动画
        animator.SetTrigger("rolling");
    }    void UpdateRoll()
    {
        currentRollTime += Time.deltaTime;
        
        // 在翻滚过程中根据是否在跑步状态来设置速度
        float currentRollSpeed = wasRunning ? rollSpeed * 1.5f : rollSpeed;
        rb.linearVelocity = new Vector2(rollDirection * currentRollSpeed, rb.linearVelocity.y);
        
        // 保持跑步动画状态
        animator.SetBool("isRunning", true);

        // 检查翻滚是否结束
        if (currentRollTime >= rollDuration)
        {
            EndRoll();
        }
    }

    void EndRoll()
    {
        isRolling = false;
        rb.linearVelocity = Vector2.zero;
        
        // 恢复到之前的跑步状态
        animator.SetBool("isRunning", wasRunning);
    }

    public bool IsRolling()
    {
        return isRolling;
    }
}