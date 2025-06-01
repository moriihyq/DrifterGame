using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 4.5f;
    public float runSpeed = 9f;
    public float crouchSpeed = 2.25f;
    public float rollSpeed = 12f;
    public float wallJumpForce = 15f;
    public float climbSpeed = 3f;

    [Header("Jump Settings")]
    public float jumpForce = 5f;
    public int maxJumpCount = 2;
    private int jumpCount;

    [Header("Health Settings")]
    public int maxHealth = 10;
    public int currentHealth;

    [Header("Spell System (Extensible)")]
    public string spellCategory;

    [Header("Ground Check Settings")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.1f;
    public Vector2 groundCheckSize = new Vector2(0.8f, 0.2f);
    private Vector2 groundCheckOffset = new Vector2(0f, -0.5f);

    // --- 管道交互新增变量 ---
    [Header("Pipe Interaction Settings")]
    public float pipeMoveSpeed = 3f; // 在管道内的移动速度
    public KeyCode interactKey = KeyCode.E; // 进入/离开管道的交互键
    public KeyCode exitPipeKey = KeyCode.Space; // 在管道内按此键也可离开
    private bool isInPipeZone = false;    // 玩家是否在管道触发区域内
    private bool isInsidePipe = false;    // 玩家是否正在管道内部移动
    private GameObject currentPipe = null; // 当前接触的管道对象
    private Vector2 pipeMovementInput;
    // --- END 管道交互新增变量 ---

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;
    private PlayerJumpManager jumpManager;
    private RollingManager rollingManager;
    public bool isGrounded { get; private set; }
    private bool isCrouching;
    private bool isClimbing; // 这个 isClimbing 可能会与管道内的 isInsidePipe 状态冲突，需要注意
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
    }

    void Update()
    {
        if (isInsidePipe) // 如果在管道内
        {
            HandlePipeMovement(); // 只处理管道移动
        }
        else // 如果不在管道内，执行原有逻辑
        {
            moveInput = Input.GetAxisRaw("Horizontal"); // 普通移动时才获取水平输入
            HandleMovement();
            HandleJump();
            HandleCrouch();
            HandleClimb(); // 注意：如果攀爬和管道可以同时触发，这里可能有逻辑冲突
            CheckForPipeEntry(); // 检测是否要进入管道
        }
        UpdateAnimator(); // Animator更新放在最后，基于当前所有状态
    }

    void HandleMovement()
    {
        // 在翻滚时完全跳过移动控制
        if (rollingManager != null && rollingManager.IsRolling()) return;
        if (isClimbing) return; // 如果正在攀爬，不进行普通水平移动

        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        if (isCrouching) speed = crouchSpeed;

        // 只有在不翻滚、不攀爬、不在管道内时才应用普通移动速度
        if (!(rollingManager != null && rollingManager.IsRolling()) && !isClimbing && !isInsidePipe)
        {
            rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y); // 使用 velocity 而非 linearVelocity
            if (moveInput != 0)
                sr.flipX = moveInput < 0;
        }
    }

    void HandleJump()
    {
        // 如果正在攀爬或在管道内，不允许跳跃
        if (isClimbing || isInsidePipe) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (jumpCount < maxJumpCount)
            {
                float force = (jumpCount == 0) ? jumpForce : jumpForce * 0.8f; // 二段跳力度可以稍作调整
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // 重置Y轴速度再施加力，确保跳跃高度一致
                rb.AddForce(new Vector2(0f, force), ForceMode2D.Impulse); // 使用AddForce和Impulse模式

                if (jumpCount == 0 && jumpManager != null && isGrounded)
                {
                    jumpManager.TriggerJumpStart();
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
        // 如果在管道内，不允许蹲下
        if (isInsidePipe)
        {
            isCrouching = false; // 确保离开管道时蹲伏状态被重置
            return;
        }

        if (Input.GetKeyDown(KeyCode.S))
            isCrouching = true;
        if (Input.GetKeyUp(KeyCode.S))
            isCrouching = false;
    }

    void HandleClimb()
    {
        // 如果在管道内，不允许攀爬
        if (isInsidePipe)
        {
            // 如果之前在攀爬，现在进入管道，需要重置攀爬状态
            if (isClimbing)
            {
                isClimbing = false;
                if (rb.isKinematic) { // 只有当攀爬使其变为kinematic时才恢复
                     // rb.gravityScale = 1; // 进入管道后会再次设置为kinematic，所以这里可以不急着改gravityScale
                }
            }
            return;
        }


        if (isTouchingWall && Input.GetKey(KeyCode.W) && !isGrounded) // 通常在空中才能攀爬
        {
            isClimbing = true;
            rb.gravityScale = 0; // 攀爬时不受重力
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, climbSpeed); // 使用 velocity
        }
        else if (isClimbing) // 任何导致攀爬结束的条件 (比如不按W，或离开墙壁)
        {
            isClimbing = false;
            rb.gravityScale = 1; // 恢复重力 (如果不是因为进入管道而结束攀爬)
        }
    }

    // --- 新增管道相关方法 ---
    void CheckForPipeEntry()
    {
        if (isInPipeZone && Input.GetKeyDown(interactKey) && !isClimbing && !(rollingManager != null && rollingManager.IsRolling()))
        {
            EnterPipe();
        }
    }

    void EnterPipe()
    {
        isInsidePipe = true;
        isCrouching = false; // 进入管道时取消蹲伏
        isClimbing = false; // 进入管道时取消攀爬

        rb.linearVelocity = Vector2.zero; // 清除进入管道前的速度
        rb.gravityScale = 0; // 不再受重力影响 (虽然isKinematic也会这样做，但明确设置一下)
        rb.isKinematic = true; // 将刚体设置为运动学，由脚本控制位置

        // (可选) 将玩家位置对齐到管道中心或入口点
        if (currentPipe != null)
        {
            // 假设管道触发器的父对象是管道本身，并且管道的X坐标是中心线
            transform.position = new Vector2(currentPipe.transform.parent.position.x, transform.position.y);
        }

        Debug.Log("玩家进入管道");
        if (animator != null) animator.SetBool("isInsidePipe", true); // (需要添加isInsidePipe动画参数)
    }

    void HandlePipeMovement()
    {
        float verticalInput = Input.GetAxisRaw("Vertical");
        pipeMovementInput = new Vector2(0, verticalInput);

        Vector2 newPosition = (Vector2)transform.position + pipeMovementInput * pipeMoveSpeed * Time.deltaTime;

        // 限制在管道的顶部和底部范围内 (假设管道对象有PipePath脚本来定义路径点)
        if (currentPipe != null)
        {
            PipePath pipePath = currentPipe.transform.parent.GetComponent<PipePath>(); // 假设管道父对象有PipePath脚本
            if (pipePath != null)
            {
                if (pipePath.topPoint != null && newPosition.y > pipePath.topPoint.position.y)
                {
                    newPosition.y = pipePath.topPoint.position.y;
                    if (verticalInput > 0.1f && Input.GetKeyDown(interactKey)) // 在顶部按交互键离开
                    {
                        ExitPipe(pipePath.topPoint.position);
                        return; // 离开后直接返回，不再执行后续移动
                    }
                }
                if (pipePath.bottomPoint != null && newPosition.y < pipePath.bottomPoint.position.y)
                {
                    newPosition.y = pipePath.bottomPoint.position.y;
                    if (verticalInput < -0.1f && Input.GetKeyDown(interactKey)) // 在底部按交互键离开
                    {
                        ExitPipe(pipePath.bottomPoint.position);
                        return; // 离开后直接返回
                    }
                }
            }
        }
        transform.position = newPosition; // 直接修改transform.position，因为是Kinematic

        // 按下特定键离开管道
        if (Input.GetKeyDown(exitPipeKey) || (Input.GetKeyDown(interactKey) && verticalInput == 0)) // 按跳跃键，或在管道中间按交互键离开
        {
            ExitPipe(transform.position);
        }
    }

    void ExitPipe(Vector2 exitPosition)
    {
        isInsidePipe = false;
        transform.position = exitPosition;

        rb.isKinematic = false;
        rb.gravityScale = 1; // 恢复正常的重力

        // (可选) 给予一个小的向上的力，使其看起来是从管道“弹出”
        // rb.velocity = new Vector2(rb.velocity.x, 2f); // 或者AddForce

        Debug.Log("玩家离开管道");
        if (animator != null) animator.SetBool("isInsidePipe", false);
        // 离开管道后，可能需要重置一些状态，比如确保isClimbing也是false
        isClimbing = false;
    }
    // --- END 管道相关方法 ---

    void UpdateAnimator()
    {
        if (animator == null) return;

        // 只有不在管道内时，才根据rb.velocity更新Speed和isRunning
        if (!isInsidePipe)
        {
            float speedValue = Mathf.Abs(rb.linearVelocity.x); // 使用 velocity
            animator.SetFloat("Speed", speedValue);

            bool isRunningValue = Input.GetKey(KeyCode.LeftShift) && moveInput != 0;
            animator.SetBool("isRunning", isRunningValue);
            animator.SetFloat("VerticalVelocity", rb.linearVelocity.y); // 使用 velocity
        }
        else
        {
            // 在管道内时，Speed和VerticalVelocity可以基于pipeMovementInput
            animator.SetFloat("Speed", 0); // 在管道内通常不播行走动画
            animator.SetFloat("VerticalVelocity", pipeMovementInput.y * pipeMoveSpeed); // 用管道移动的输入来驱动垂直速度动画
            animator.SetBool("isRunning", false);
        }

        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isCrouching", isCrouching && !isInsidePipe); // 蹲伏动画只在不在管道内时
        animator.SetBool("isClimbing", isClimbing && !isInsidePipe); // 攀爬动画只在不在管道内时
        animator.SetBool("isInsidePipe", isInsidePipe); // (需要添加isInsidePipe动画参数)
    }

    void FixedUpdate()
    {
        // 只有不在管道内时才进行地面和墙壁检测
        if (!isInsidePipe)
        {
            Vector2 boxPosition = (Vector2)transform.position + groundCheckOffset;
            isGrounded = Physics2D.BoxCast(boxPosition, groundCheckSize, 0f, Vector2.down, groundCheckDistance, groundLayer);

            isTouchingWall = Physics2D.OverlapCircle(transform.position + Vector3.right * (sr.flipX ? -0.5f : 0.5f), 0.2f, LayerMask.GetMask("Wall"));

            if (isGrounded)
            {
                if (jumpCount > 0)
                {
                    jumpCount = 0;
                    if (jumpManager != null)
                    {
                        jumpManager.ResetJumpState();
                    }
                    Debug.Log($"[Ground] 检测到落地！跳跃次数重置为：{jumpCount}");
                }
                // 如果在攀爬时落地，也应该结束攀爬
                if (isClimbing)
                {
                    isClimbing = false;
                    rb.gravityScale = 1;
                }
            }
        }
        else // 在管道内时，强制isGrounded和isTouchingWall为false，避免状态冲突
        {
            isGrounded = false;
            isTouchingWall = false;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector2 boxPosition = (Vector2)transform.position + groundCheckOffset;
        Gizmos.DrawWireCube(boxPosition, groundCheckSize);
        Vector2 bottomCenter = boxPosition + Vector2.down * (groundCheckSize.y / 2);
        Gizmos.DrawLine(bottomCenter, bottomCenter + Vector2.down * groundCheckDistance);

        // 墙体检测Gizmo (如果需要)
        // Gizmos.color = Color.blue;
        // Gizmos.DrawWireSphere(transform.position + Vector3.right * (sr.flipX ? -0.5f : 0.5f), 0.2f);
    }

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
        Debug.Log("Player Died");
        if (animator != null)
            animator.SetTrigger("Die");
        // 在这里可以禁用玩家控制等
        isInsidePipe = false; // 确保死亡时不在管道状态
        // this.enabled = false; // 例如，禁用此脚本
    }

    public void SetSpellCategory(string category)
    {
        spellCategory = category;
    }

    // --- OnTriggerEnter2D 和 OnTriggerExit2D ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PipeTrigger")) // 假设管道触发器的Tag是 "PipeTrigger"
        {
            if (!isInsidePipe) // 避免在管道内时重复设置
            {
                isInPipeZone = true;
                currentPipe = other.gameObject; // 保存的是触发器对象
                Debug.Log("进入管道区域，当前管道触发器: " + other.name);
                // (可选) 显示交互提示UI
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("PipeTrigger"))
        {
            if (other.gameObject == currentPipe) // 确保离开的是当前接触的管道区域
            {
                isInPipeZone = false;
                // 不要在这里将 currentPipe 设置为 null，除非确定玩家已完全离开管道交互范围
                // 如果玩家仍在管道内，但碰撞体暂时离开了入口触发器，currentPipe应该保留
                Debug.Log("离开管道区域: " + other.name);
                // (可选) 隐藏交互提示UI
            }
        }
    }
}

// --- 辅助脚本：PipePath.cs (需要创建这个新脚本并附加到管道的父对象上) ---
// 文件名: PipePath.cs
// using UnityEngine;
// public class PipePath : MonoBehaviour
// {
//     public Transform topPoint;
//     public Transform bottomPoint;
//
//     void OnDrawGizmosSelected()
//     {
//         if (topPoint != null && bottomPoint != null)
//         {
//             Gizmos.color = Color.green;
//             Gizmos.DrawLine(transform.position, topPoint.position); // 从管道对象中心到顶部
//             Gizmos.DrawLine(transform.position, bottomPoint.position); // 从管道对象中心到底部
//             Gizmos.DrawWireSphere(topPoint.position, 0.2f);
//             Gizmos.DrawWireSphere(bottomPoint.position, 0.2f);
//         }
//     }
// }