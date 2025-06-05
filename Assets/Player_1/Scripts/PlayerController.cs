using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 4.5f; // 原为3f，提升1.5倍
    public float runSpeed = 9f;   // 原为6f，提升1.5倍
    public float crouchSpeed = 2.25f; // 原为1.5f，提升1.5倍
    public float rollSpeed = 12f; // 原为8f，提升1.5倍
    public float wallJumpForce = 15f; // 原为10f，提升1.5倍
    public float climbSpeed = 3f; // 原为2f，提升1.5倍

    [Header("Jump Settings")]
    public float jumpForce = 5f; // 大幅减少跳跃力度
    public int maxJumpCount = 2; // 允许二段跳
    private int jumpCount;

    [Header("Health Settings")]
    public int maxHealth = 10;
    public int currentHealth;

    [Header("Spell System (Extensible)")]
    public string spellCategory; // 预留法术类别接口

    [Header("Ground Check Settings")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.1f; // 减小检测距离，避免提前检测
    public Vector2 groundCheckSize = new Vector2(0.8f, 0.2f); // 增加检测框宽度，提高高度
    private Vector2 groundCheckOffset = new Vector2(0f, -0.5f); // 添加偏移量以调整检测位置

    [Header("渲染层级设置")]
    [Tooltip("玩家的排序顺序，确保显示在墙壁之上（墙壁通常为0-5，玩家应设为10或更高）")]
    public int playerSortingOrder = 10;
    [Tooltip("是否在启动时自动修复渲染层级")]
    public bool autoFixLayerOnStart = true;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;
    private PlayerJumpManager jumpManager;
    private RollingManager rollingManager;

    public bool isGrounded { get; private set; }
    private bool isCrouching;
    private bool isClimbing;
    private bool isTouchingWall;
    private float moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        jumpManager = GetComponent<PlayerJumpManager>();
        rollingManager = GetComponent<RollingManager>();
        
        // 自动修复渲染层级
        if (autoFixLayerOnStart && sr != null)
        {
            FixPlayerRenderingLayer();
        }
    }

    /// <summary>
    /// 修复玩家的渲染层级，确保显示在墙壁之上
    /// </summary>
    private void FixPlayerRenderingLayer()
    {
        if (sr == null) return;
        
        // 设置玩家的排序顺序
        sr.sortingOrder = playerSortingOrder;
        
        Debug.Log($"<color=green>[PlayerController] 已修复玩家 {gameObject.name} 的渲染层级:</color>");
        Debug.Log($"<color=green>- 排序层: {sr.sortingLayerName}</color>");
        Debug.Log($"<color=green>- 排序顺序: {sr.sortingOrder}</color>");
    }
    
    /// <summary>
    /// 手动设置玩家的排序顺序
    /// </summary>
    /// <param name="order">新的排序顺序</param>
    public void SetPlayerSortingOrder(int order)
    {
        playerSortingOrder = order;
        if (sr != null)
        {
            sr.sortingOrder = order;
            Debug.Log($"<color=green>[PlayerController] 已将玩家排序顺序设置为 {order}</color>");
        }
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        HandleMovement();
        HandleJump();
        HandleCrouch();
        HandleClimb();
        UpdateAnimator();
    }

    void HandleMovement()
    {
        // 在翻滚时完全跳过移动控制
        if (rollingManager != null && rollingManager.IsRolling()) return;
        if (isClimbing) return;

        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        if (isCrouching) speed = crouchSpeed;
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
        if (moveInput != 0)
            sr.flipX = moveInput < 0;

        // Trigger running animation
        if (animator != null)
        {
            animator.SetBool("isRunning", Input.GetKey(KeyCode.LeftShift) && moveInput != 0);
        }
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (jumpCount < maxJumpCount)
            {
                // 计算跳跃力度
                float force = (jumpCount == 0) ? jumpForce : 2f;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
                
                // 只在第一次跳跃（从地面起跳）时播放jump_off动画
                if (jumpCount == 0 && jumpManager != null && isGrounded)
                {
                    jumpManager.TriggerJumpStart();  // 修改这里：TriggerJumpAnimation -> TriggerJumpStart
                }
                
                jumpCount++;
                Debug.Log($"[Jump] 执行跳跃！当前跳跃次数：{jumpCount}，跳跃力度：{force}");
            }
            else
            {
                Debug.Log($"[Jump] 无法跳跃，已达到最大跳跃次数：{jumpCount}/{maxJumpCount}");
            }
        }
    }

    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.S))
            isCrouching = true;
        if (Input.GetKeyUp(KeyCode.S))
            isCrouching = false;
    }    // 移除了HandleRoll方法，现在由RollingManager处理

    void HandleClimb()
    {
        if (isTouchingWall && Input.GetKey(KeyCode.W))
        {
            isClimbing = true;
            rb.gravityScale = 0;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, climbSpeed);
        }
        else if (isClimbing && !Input.GetKey(KeyCode.W))
        {
            isClimbing = false;
            rb.gravityScale = 1;
        }
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        // Update Speed parameter
        float speedValue = Mathf.Abs(rb.linearVelocity.x);
        animator.SetFloat("Speed", speedValue);

        // Update isRunning parameter
        bool isRunningValue = Input.GetKey(KeyCode.LeftShift) && moveInput != 0;
        animator.SetBool("isRunning", isRunningValue);

        // 注释掉会产生警告的动画参数，避免控制台错误
        // animator.SetBool("isGrounded", isGrounded);
        // animator.SetBool("isCrouching", isCrouching);
        // animator.SetBool("isClimbing", isClimbing);
        // animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
        
        // Rolling状态由RollingManager控制
    }

    void FixedUpdate()
    {
        Vector2 boxPosition = (Vector2)transform.position + groundCheckOffset;
        isGrounded = Physics2D.BoxCast(boxPosition, groundCheckSize, 0f, Vector2.down, groundCheckDistance, groundLayer);
        
        // 墙体检测保持不变
        isTouchingWall = Physics2D.OverlapCircle(transform.position + Vector3.right * (sr.flipX ? -0.5f : 0.5f), 0.2f, LayerMask.GetMask("Wall"));

        // 落地检测
        if (isGrounded)
        {
            if (jumpCount > 0)
            {
                jumpCount = 0;
                if (jumpManager != null)
                {
                    jumpManager.ResetJumpState();  // 修改这里：ResetJumpAnimation -> ResetJumpState
                }
                Debug.Log($"[Ground] 检测到落地！跳跃次数重置为：{jumpCount}");
            }
        }
    }

    // 修改 OnDrawGizmos 方法来可视化新的检测范围
    void OnDrawGizmos()
    {
        // 使用新的偏移量绘制检测范围
        Gizmos.color = Color.red;
        Vector2 boxPosition = (Vector2)transform.position + groundCheckOffset;
        Gizmos.DrawWireCube(boxPosition, groundCheckSize);
        
        // 绘制检测距离
        Vector2 bottomCenter = boxPosition + Vector2.down * (groundCheckSize.y / 2);
        Gizmos.DrawLine(bottomCenter, bottomCenter + Vector2.down * groundCheckDistance);
    }

    // 血量相关方法
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0)
        {
            currentHealth = 0;
            Die();
        }
    }
    
    void Die()
    {
        // 死亡动画和逻辑
        Debug.Log("Player Died");
        if (animator != null)
            animator.SetTrigger("Die");
            
        // 触发复活
        if (RespawnManager.Instance != null)
        {
            // 延迟一点时间让死亡动画播放
            Invoke("TriggerRespawn", 1f);
        }
        else
        {
            Debug.LogWarning("[PlayerController] 找不到RespawnManager，无法复活！");
        }
    }
    
    /// <summary>
    /// 触发复活
    /// </summary>
    void TriggerRespawn()
    {
        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.RespawnPlayer();
        }
    }
    
    /// <summary>
    /// 手动复活（用于调试或特殊需求）
    /// </summary>
    public void Respawn()
    {
        TriggerRespawn();
    }

    // 法术类别接口预留
    public void SetSpellCategory(string category)
    {
        spellCategory = category;
        // TODO: 扩展法术系统
    }
}