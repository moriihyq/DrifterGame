using UnityEngine;

/// <summary>
/// 处理敌人受击后的击退效果
/// </summary>
public class EnemyKnockback : MonoBehaviour
{
    [Header("击退设置")]
    [SerializeField] private float knockbackForce = 5f; // 击退力度
    [SerializeField] private float knockbackUpwardForce = 2f; // 向上击退力度
    [SerializeField] private float knockbackDuration = 0.2f; // 击退持续时间
    
    // 组件引用
    private Rigidbody2D rb;
    
    // 状态变量
    private bool isKnockedBack = false;
    private float knockbackTimeLeft = 0f;
    private Vector2 knockbackDirection;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.gravityScale = 1f;
        }
    }
    
    private void Update()
    {
        // 处理击退计时
        if (isKnockedBack)
        {
            knockbackTimeLeft -= Time.deltaTime;
            
            if (knockbackTimeLeft <= 0)
            {
                // 击退结束
                isKnockedBack = false;
                
                // 重置速度
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
    }
    
    /// <summary>
    /// 对敌人应用击退效果
    /// </summary>
    /// <param name="attackerPosition">攻击者的位置</param>
    public void ApplyKnockback(Vector2 attackerPosition)
    {
        // 计算击退方向（从攻击者到敌人的方向）
        knockbackDirection = (transform.position - new Vector3(attackerPosition.x, attackerPosition.y, transform.position.z)).normalized;
        
        // 为方向添加上升力
        knockbackDirection.y = Mathf.Abs(knockbackDirection.y) + knockbackUpwardForce;
        
        // 开始击退
        isKnockedBack = true;
        knockbackTimeLeft = knockbackDuration;
        
        // 应用实际力量
        rb.linearVelocity = Vector2.zero; // 先清除当前速度
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
    }
    
    /// <summary>
    /// 设置击退参数
    /// </summary>
    /// <param name="force">击退力度</param>
    /// <param name="upwardForce">向上力度</param>
    /// <param name="duration">持续时间</param>
    public void SetKnockbackParameters(float force, float upwardForce, float duration)
    {
        knockbackForce = force;
        knockbackUpwardForce = upwardForce;
        knockbackDuration = duration;
    }
}
