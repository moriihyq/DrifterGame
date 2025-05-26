using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("组件引用")]
    [SerializeField] private Rigidbody2D rb;

    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 7f;

    [Header("跳跃参数")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("二段跳参数")] // <--- 新增部分 ---
    [SerializeField] private int maxJumps = 2; // 最大跳跃次数（1为普通跳，2为二段跳）
    [SerializeField] private float airJumpForceMultiplier = 1f; // 空中跳跃力度乘数 (1表示与地面跳跃力度相同)
    private int jumpsRemaining; // 当前剩余跳跃次数   <--- 新增变量 ---

    // 私有变量
    private float horizontalInput;
    private bool isGrounded;
    private bool isFacingRight = true;

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogError("错误：PlayerMovement 脚本未能找到 Rigidbody2D 组件！", gameObject);
        if (groundCheckPoint == null) Debug.LogError("错误：PlayerMovement 脚本需要一个 Ground Check Point！", gameObject);
        if (groundLayer == 0) Debug.LogWarning("警告：PlayerMovement 脚本的 Ground Layer 未设置。", gameObject);

        jumpsRemaining = maxJumps; // 初始化剩余跳跃次数 <--- 新增初始化 ---
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // --- 修改跳跃逻辑 ---
        if (Input.GetButtonDown("Jump"))
        {
            AttemptJump(); // 调用尝试跳跃的方法
        }

        FlipCharacter();
    }

    void FixedUpdate()
    {
        bool wasGrounded = isGrounded; // 记录上一帧是否在地面
        CheckIfGrounded();

        // 如果从空中落到地面，或者刚开始时在地面，重置跳跃次数
        if (isGrounded && !wasGrounded) // 刚接触地面
        {
            jumpsRemaining = maxJumps;
            Debug.Log("刚接触地面，跳跃次数重置为: " + jumpsRemaining);
        }
        else if (isGrounded && jumpsRemaining < maxJumps && rb.linearVelocity.y <= 0.01f) // 确保已经在地面上稳定一段时间才重置(防止刚跳起就重置)
        {
             jumpsRemaining = maxJumps;
             // Debug.Log("已在地面，跳跃次数重置为: " + jumpsRemaining); // 这个可能会频繁打印，酌情开启
        }


        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    void CheckIfGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
    }

    // --- 新增尝试跳跃的方法 ---
    void AttemptJump()
    {
        if (jumpsRemaining > 0)
        {
            float currentJumpForce = jumpForce;
            if (!isGrounded && jumpsRemaining < maxJumps) // 如果是空中跳跃 (jumpsRemaining < maxJumps 暗示不是第一次跳)
            {
                currentJumpForce *= airJumpForceMultiplier; // 应用空中跳跃力度乘数
                Debug.Log("执行空中跳跃！剩余次数（跳跃前）: " + jumpsRemaining + ", 力度: " + currentJumpForce);
            }
            else if (isGrounded) // 如果是地面跳跃
            {
                Debug.Log("执行地面跳跃！剩余次数（跳跃前）: " + jumpsRemaining + ", 力度: " + currentJumpForce);
            }

            // 为了让二段跳感觉更可控，通常会在施加新的向上力之前，先将Y轴速度清零或减小
            // 这样可以避免上一次跳跃的剩余向上的速度与本次跳跃的力叠加过高，或者向下速度过大导致跳不起来
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // 将垂直速度清零，确保每次跳跃高度一致

            rb.AddForce(Vector2.up * currentJumpForce, ForceMode2D.Impulse);
            jumpsRemaining--; // 减少一次剩余跳跃次数

            // 重要：在跳跃后，立即将isGrounded设为false（即使地面检测可能下一帧才更新）
            // 这样可以防止在同一帧内因为isGrounded仍然为true而连续执行多次跳跃（如果跳跃键按住时间稍长）。
            // 但这只是一种处理方式，更好的方式是确保GetButtonDown只在按下的那一帧触发。
            // isGrounded = false; // 这行可以考虑是否需要，通常GetButtonDown已经处理了单次触发

            Debug.Log("跳跃后，剩余次数: " + jumpsRemaining);
        }
        else
        {
            Debug.Log("无法跳跃，剩余跳跃次数为 0。");
        }
    }

    void FlipCharacter()
    {
        if (horizontalInput > 0 && !isFacingRight) Flip();
        else if (horizontalInput < 0 && isFacingRight) Flip();
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
    }
}