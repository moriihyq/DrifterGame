using UnityEngine;

/// <summary>
/// 角色落地时触发动画、特效或音效的辅助脚本。
/// 挂载到角色对象上，需配合Animator的Landing触发器使用。
/// </summary>
public class PlayerLandingEffect : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private bool wasGroundedLastFrame = true;
    private float leaveGroundY = 0f; // 离开地面时的y坐标
    private float maxFallDistance = 0f; // 记录下落最大高度
    private bool landingPlayed = false; // 标记落地前是否已播放landing动画
    private bool isFalling = false; // 标记是否正在下落
    [Header("地面检测参数（需与主控制器一致）")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.1f;
    public Vector2 groundCheckSize = new Vector2(0.8f, 0.2f);
    public Vector2 groundCheckOffset = new Vector2(0f, -0.5f);
    [Header("格子高度（单位：Unity单位）")]
    public float tileHeight = 1f;
    [Header("提前播放Landing动画的距离（单位：格）")]
    public float preLandDistanceInTiles = 1.5f;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        Vector2 boxPosition = (Vector2)transform.position + groundCheckOffset;
        bool isGrounded = Physics2D.BoxCast(boxPosition, groundCheckSize, 0f, Vector2.down, groundCheckDistance, groundLayer);

        // 检测离开地面，记录起跳点
        if (wasGroundedLastFrame && !isGrounded)
        {
            leaveGroundY = transform.position.y;
            maxFallDistance = 0f;
            landingPlayed = false;
            isFalling = true;
        }
        // 空中时累计最大下落高度
        if (!isGrounded && isFalling)
        {
            float fallDist = leaveGroundY - transform.position.y;
            if (fallDist > maxFallDistance)
                maxFallDistance = fallDist;
        }
        // 落地时判断是否需要播放landing
        if (!wasGroundedLastFrame && isGrounded)
        {
            isFalling = false;
            if (maxFallDistance >= tileHeight * 2f && !landingPlayed)
            {
                if (animator != null)
                {
                    animator.ResetTrigger("Landing");
                    animator.SetTrigger("Landing");
                }
                landingPlayed = true;
            }
        }
        wasGroundedLastFrame = isGrounded;
    }
}
