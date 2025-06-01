using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    [Header("基本属性")]
    [SerializeField] private int maxHealth = 500; // Boss的最大生命值
    [SerializeField] private int meleeAttackDamage = 40; // 近战攻击伤害
    [SerializeField] private int rangedAttackDamage = 60; // 远程攻击伤害
    [SerializeField] private float meleeAttackCooldown = 1.5f; // 近战攻击冷却时间
    [SerializeField] private float meleeAttackRange = 1.5f; // 近战攻击范围
    [SerializeField] private float rangedAttackRange = 8f; // 远程攻击范围
    [SerializeField] private float moveSpeed = 3f; // Boss移动速度
    [SerializeField] private Transform meleeAttackPoint; // 近战攻击判定点
    [SerializeField] private float rangedAttackRadius = 5f; // 远程攻击范围半径
    [SerializeField] private LayerMask playerLayer; // 玩家图层
    [SerializeField] private float meleeAttackDelay = 0.4f; // 近战攻击判定延迟
    [SerializeField] private float rangedAttackDelay = 0.8f; // 远程攻击判定延迟
    
    // 组件引用
    private Animator anim;
    private Rigidbody2D rb;
    
    // 状态变量
    private int currentHealth;
    private bool isDead = false;
    private bool isActive = false; // Boss是否处于激活状态
    private bool isAttacking = false; // 当前是否正在攻击
    private float distanceToPlayer; // 与玩家的距离
    private GameObject player;
    private float nextAttackTime = 0f; // 下次可攻击时间
    private float originScaleX; // 原始X缩放值
    
    // Boss特殊行为变量
    private int consecutiveMeleeAttacks = 0; // 已执行的连续近战攻击次数
    private int meleeAttacksBeforeRanged; // 执行远程攻击前需要的近战攻击次数
    private bool initialAttackPerformed = false; // 是否已执行初始攻击
    
    void Start()
    {
        // 初始化组件引用
        anim = GetComponentInChildren<Animator>();
        if (anim == null) anim = GetComponent<Animator>();
        
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.gravityScale = 3f; // 确保Boss受重力影响
        
        // 初始化状态
        currentHealth = maxHealth;
        originScaleX = transform.localScale.x;
        isActive = false;
        isAttacking = false;
        
        // 查找玩家
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = GameObject.Find("Player");
            Debug.LogWarning("无法通过标签找到玩家对象，尝试通过名称查找");
        }
        
        // 确保有近战攻击点
        if (meleeAttackPoint == null)
        {
            GameObject attackPointObj = new GameObject("BossMeleeAttackPoint");
            attackPointObj.transform.SetParent(transform);
            attackPointObj.transform.localPosition = new Vector3(1.5f, 0f, 0f);
            meleeAttackPoint = attackPointObj.transform;
            Debug.Log("为Boss创建了默认近战攻击点");
        }
        
        // 确保有碰撞器
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.size = new Vector2(2f, 3f); // Boss通常比普通敌人大
            boxCollider.offset = new Vector2(0f, 1.5f);
            Debug.LogWarning("Boss没有碰撞器，添加了默认的BoxCollider2D");
        }
        
        // 确保Boss在正确的层级上
        if (LayerMask.NameToLayer("Enemy") != -1)
        {
            gameObject.layer = LayerMask.NameToLayer("Enemy");
        }
        
        // 随机生成执行远程攻击前需要的近战攻击次数
        RegenerateAttackPattern();
    }
    
    void Update()
    {
        // 如果Boss已死亡，不执行任何逻辑
        if (isDead) return;
        
        // 检测P键激活Boss
        if (Input.GetKeyDown(KeyCode.P) && !isActive)
        {
            ActivateBoss();
        }
        
        if (isActive)
        {
            UpdatePlayerDistance();
            UpdateMovement();
            UpdateAttackState();
        }
        
        UpdateAnimator();
    }
    
    // 激活Boss
    private void ActivateBoss()
    {
        isActive = true;
        consecutiveMeleeAttacks = 0;
        initialAttackPerformed = false;
        Debug.Log("<color=#FF0000>Boss已激活！</color>");
        
        if (anim != null)
        {
            anim.SetBool("isActive", true);
        }
    }
    
    // 更新与玩家的距离
    private void UpdatePlayerDistance()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                distanceToPlayer = 100f; // 如果找不到玩家，设置一个很大的距离
                return;
            }
        }
        
        // 计算与玩家的X轴距离（2D游戏通常关注X轴距离）
        distanceToPlayer = player.transform.position.x - transform.position.x;
    }
    
    // 更新Boss移动
    private void UpdateMovement()
    {
        // 如果正在攻击，不允许移动
        if (isAttacking)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }
        
        // 如果是初次激活且尚未执行初始攻击，追踪玩家
        if (!initialAttackPerformed)
        {
            // 如果距离大于近战攻击范围，向玩家移动
            if (Mathf.Abs(distanceToPlayer) > meleeAttackRange)
            {
                float moveDirection = distanceToPlayer > 0 ? 1 : -1;
                rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);
            }
            else
            {
                // 停止并准备攻击
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
        }
        else
        {
            // 如果需要执行近战攻击但不在范围内，则靠近玩家
            if (consecutiveMeleeAttacks < meleeAttacksBeforeRanged && Mathf.Abs(distanceToPlayer) > meleeAttackRange)
            {
                float moveDirection = distanceToPlayer > 0 ? 1 : -1;
                rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);
            }
            else
            {
                // 其他情况下停止移动
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
        }
    }
    
    // 更新攻击状态
    private void UpdateAttackState()
    {
        // 如果正在攻击或冷却中，不处理攻击
        if (isAttacking || Time.time < nextAttackTime) return;
        
        // 初次激活时的特殊处理
        if (!initialAttackPerformed)
        {
            // 当靠近玩家时执行第一次攻击
            if (Mathf.Abs(distanceToPlayer) <= meleeAttackRange)
            {
                PerformMeleeAttack();
                initialAttackPerformed = true;
            }
            return;
        }
        
        // 正常攻击模式
        if (consecutiveMeleeAttacks < meleeAttacksBeforeRanged)
        {
            // 执行近战攻击
            if (Mathf.Abs(distanceToPlayer) <= meleeAttackRange)
            {
                PerformMeleeAttack();
                consecutiveMeleeAttacks++;
            }
        }
        else
        {
            // 执行远程攻击
            PerformRangedAttack();
            
            // 重置攻击模式
            consecutiveMeleeAttacks = 0;
            RegenerateAttackPattern();
        }
    }
    
    // 生成新的攻击模式
    private void RegenerateAttackPattern()
    {
        meleeAttacksBeforeRanged = Random.Range(1, 4); // 1-3次近战攻击后执行远程攻击
        Debug.Log($"<color=#FF8C00>Boss将在执行{meleeAttacksBeforeRanged}次近战攻击后使用远程攻击</color>");
    }
    
    // 执行近战攻击
    private void PerformMeleeAttack()
    {
        isAttacking = true;
        
        // 触发近战攻击动画
        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }
        
        Debug.Log("<color=#FF4500>Boss发动近战攻击！</color>");
        
        // 延迟进行攻击判定
        Invoke("ApplyMeleeAttackDamage", meleeAttackDelay);
        
        // 设置冷却和结束攻击状态
        nextAttackTime = Time.time + meleeAttackCooldown;
        Invoke("EndAttackState", 0.7f); // 假设攻击动画持续0.7秒
    }
    
    // 执行远程攻击
    private void PerformRangedAttack()
    {
        isAttacking = true;
        
        // 触发远程攻击动画
        if (anim != null)
        {
            anim.SetTrigger("Shoot");
        }
        
        Debug.Log("<color=#8B0000>Boss发动远程攻击！</color>");
        
        // 延迟进行攻击判定
        Invoke("ApplyRangedAttackDamage", rangedAttackDelay);
        
        // 设置冷却和结束攻击状态
        nextAttackTime = Time.time + meleeAttackCooldown * 1.5f;
        Invoke("EndAttackState", 1.2f); // 远程攻击动画可能更长
    }
    
    // 应用近战攻击伤害
    private void ApplyMeleeAttackDamage()
    {
        if (isDead || !isActive) return;
        
        // 检测攻击范围内的玩家
        Collider2D playerCollider = Physics2D.OverlapCircle(meleeAttackPoint.position, meleeAttackRange, playerLayer);
        if (playerCollider != null)
        {
            PlayerAttackSystem playerHealth = playerCollider.GetComponent<PlayerAttackSystem>();
            if (playerHealth != null)
            {
                // 对玩家造成伤害
                playerHealth.TakeDamage(meleeAttackDamage);
                
                // 记录到战斗管理器
                if (CombatManager.Instance != null)
                {
                    CombatManager.Instance.LogDamage(gameObject, player, meleeAttackDamage);
                }
                
                Debug.Log($"<color=#FF4500>Boss的近战攻击命中玩家，造成 {meleeAttackDamage} 点伤害！</color>");
            }
        }
    }
    
    // 应用远程攻击伤害
    private void ApplyRangedAttackDamage()
    {
        if (isDead || !isActive) return;
        
        // 远程攻击使用更大的范围
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, rangedAttackRadius, playerLayer);
        if (playerCollider != null)
        {
            PlayerAttackSystem playerHealth = playerCollider.GetComponent<PlayerAttackSystem>();
            if (playerHealth != null)
            {
                // 对玩家造成伤害
                playerHealth.TakeDamage(rangedAttackDamage);
                
                // 记录到战斗管理器
                if (CombatManager.Instance != null)
                {
                    CombatManager.Instance.LogDamage(gameObject, player, rangedAttackDamage);
                }
                
                Debug.Log($"<color=#8B0000>Boss的远程攻击命中玩家，造成 {rangedAttackDamage} 点伤害！</color>");
            }
        }
    }
    
    // 结束攻击状态
    private void EndAttackState()
    {
        isAttacking = false;
    }
    
    // 更新动画器
    private void UpdateAnimator()
    {
        if (anim == null) return;
        
        // 设置朝向（根据玩家位置）
        if (player != null)
        {
            Vector3 currentScale = transform.localScale;
            currentScale.x = distanceToPlayer > 0 ? originScaleX : -originScaleX;
            transform.localScale = currentScale;
        }
        
        // 更新移动动画
        bool isMoving = Mathf.Abs(rb.velocity.x) > 0.1f;
        anim.SetBool("isMoving", isMoving);
        
        // 设置激活状态参数
        anim.SetBool("isActive", isActive);
    }
    
    // Boss受到伤害
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        int prevHealth = currentHealth;
        currentHealth -= damage;
        
        // 在控制台输出Boss血量变化信息
        Debug.Log($"<color=#FFA500>Boss受到 {damage} 点伤害！血量变化：{prevHealth} -> {currentHealth}</color>");
        
        // 播放受伤动画
        if (anim != null)
        {
            anim.SetTrigger("Hurt");
        }
        
        // 检查是否死亡
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log("<color=#FF0000>Boss已被击败！</color>");
            Die();
        }
        
        // 通知战斗管理器记录伤害
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.LogDamage(GameObject.FindGameObjectWithTag("Player"), gameObject, damage);
        }
    }
    
    // Boss死亡
    private void Die()
    {
        isDead = true;
        isActive = false;
        isAttacking = false;
        
        // 播放死亡动画
        if (anim != null)
        {
            anim.SetBool("IsDead", true);
        }
        
        // 禁用碰撞器和刚体
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        
        // Boss可能不会立即销毁，而是有一个死亡动画
        Debug.Log("<color=#FF0000>Boss已死亡！游戏胜利！</color>");
    }
    
    // 在编辑器中绘制攻击范围（调试用）
    void OnDrawGizmosSelected()
    {
        // 绘制近战攻击范围
        if (meleeAttackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(meleeAttackPoint.position, meleeAttackRange);
        }
        
        // 绘制远程攻击范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangedAttackRadius);
    }
}
