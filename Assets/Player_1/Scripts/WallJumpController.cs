using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class WallJumpController : MonoBehaviour
{
    [Header("Wall Settings")]
    public float wallSlideSpeed = 2f;        // 墙壁滑落速度
    public float wallStickTime = 0.25f;      // 墙壁粘附时间
    public float wallJumpForce = 12f;        // 蹬墙跳跃力度
    public float wallJumpDirectionX = 1f;    // 水平方向的跳跃力度
    public float wallJumpDirectionY = 2f;    // 垂直方向的跳跃力度

    [Header("Wall Detection")]
    public Vector2 wallCheckSize = new Vector2(0.2f, 1f);  // 墙壁检测范围
    public float wallCheckDistance = 0.1f;    // 墙壁检测距离
    public LayerMask wallLayer;              // 墙壁层级

    // 组件引用
    private Rigidbody2D rb;
    private PlayerController playerController;
    private Animator animator;
    private bool isFacingRight = true;

    // 墙壁状态
    private bool isTouchingWall;
    private bool isWallSliding;
    private float wallStickCounter;
    private int wallDirX;

    private void Start()
    {
        // 获取必要组件
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
        
        if (rb == null || playerController == null)
        {
            Debug.LogError("Missing required components on " + gameObject.name);
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        CheckWallSlide();
        HandleWallJump();
    }

    private void CheckWallSlide()
    {
        // 检测墙壁
        isTouchingWall = Physics2D.BoxCast(
            transform.position,
            wallCheckSize,
            0f,
            new Vector2(isFacingRight ? 1 : -1, 0),
            wallCheckDistance,
            wallLayer
        );

        // 判断是否处于滑墙状态
        isWallSliding = isTouchingWall && !playerController.isGrounded && rb.linearVelocity.y < 0;

        if (isWallSliding)
        {
            wallStickCounter = wallStickTime;
            
            // 限制下落速度
            if (rb.linearVelocity.y < -wallSlideSpeed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
            }

            // 更新动画状态
            if (animator != null)
            {
                animator.SetBool("isWallSliding", true);
            }
        }
        else
        {
            if (wallStickCounter > 0)
                wallStickCounter -= Time.deltaTime;

            if (animator != null)
            {
                animator.SetBool("isWallSliding", false);
            }
        }
    }

    private void HandleWallJump()
    {
        if (isWallSliding && Input.GetKeyDown(KeyCode.Space))
        {
            // 计算跳跃方向
            Vector2 jumpDirection = new Vector2(
                wallJumpDirectionX * (isFacingRight ? -1 : 1),
                wallJumpDirectionY
            ).normalized;

            // 应用跳跃力
            rb.linearVelocity = Vector2.zero; // 清除当前速度
            rb.AddForce(jumpDirection * wallJumpForce, ForceMode2D.Impulse);

            // 翻转朝向
            isFacingRight = !isFacingRight;
            transform.localScale = new Vector3(isFacingRight ? 1 : -1, 1, 1);

            // 触发跳跃动画
            if (animator != null)
            {
                animator.SetTrigger("wallJump");
            }

            // 重置墙壁粘附计时器
            wallStickCounter = 0;
        }
    }

    private void OnDrawGizmos()
    {
        // 可视化墙壁检测范围
        Gizmos.color = Color.yellow;
        Vector2 checkPosition = (Vector2)transform.position + 
            new Vector2(isFacingRight ? wallCheckDistance : -wallCheckDistance, 0);
        Gizmos.DrawWireCube(checkPosition, wallCheckSize);
    }
}