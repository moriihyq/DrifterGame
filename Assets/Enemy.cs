using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("基本属性")]
    [SerializeField] private int maxHealth = 100; // 最大生命值
    [SerializeField] private int attackDamage = 25; // 攻击伤害
    [SerializeField] private float attackCooldown = 1f; // 攻击冷却时间 (精确为1秒)
    [SerializeField] private float attackRange = 1.2f; // 攻击范围
    [SerializeField] private Transform attackPoint; // 攻击判定点
    [SerializeField] private LayerMask playerLayer; // 玩家图层
    [SerializeField] private float attackDelay = 0.3f; // 从攻击动画开始到实际造成伤害的延迟
    
    // 组件引用
    private Animator anim;
    private Rigidbody2D rb;
    
    // 状态变量
    private bool isMoving;
    private bool approach;
    public float distanceToPlayer;
    private int currentHealth;
    private bool isDead;
    private float originScaleX;
    private float lastSpeed;
    private float nextAttackTime = 0f; // 下次可攻击时间
    private GameObject player;
    private bool isAttacking = false; // 当前是否正在攻击
    private bool isActive = false; // 敌人是否处于激活状态
    void Start()
    {
        isMoving = false;
        approach = false;
        isAttacking = false;
        isActive = false;
        
        // 获取组件
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        // 初始化变量
        lastSpeed = 0;
        currentHealth = maxHealth;
        isDead = false;
        originScaleX = transform.localScale.x;
        
        // 查找玩家
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = GameObject.Find("Player");
            if (player == null)
            {
                Debug.LogWarning("无法找到玩家对象，请确保玩家对象已设置'Player'标签或名为'Player'");
            }
        }
        
        // 确保有攻击点
        if (attackPoint == null)
        {
            GameObject attackPointObj = new GameObject("EnemyAttackPoint");
            attackPointObj.transform.SetParent(transform);
            attackPointObj.transform.localPosition = new Vector3(1f, 0f, 0f); // 设置在敌人前方
            attackPoint = attackPointObj.transform;
            
            Debug.Log("为敌人创建了默认攻击点");
        }
        
        // 确保有碰撞器
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            Debug.LogWarning("Enemy没有碰撞器，添加BoxCollider2D");
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.size = new Vector2(1f, 1f);
            boxCollider.offset = Vector2.zero;
        }
        
        // 确保敌人在正确的层级上
        if (LayerMask.NameToLayer("Enemy") != -1)
        {
            gameObject.layer = LayerMask.NameToLayer("Enemy");
        }
    }
      // 敌人受到伤害
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        int prevHealth = currentHealth;
        currentHealth -= damage;
        
        // 在控制台输出敌人血量变化信息
        Debug.Log($"<color=#FFA500>敌人 {gameObject.name} 受到 {damage} 点伤害！血量变化：{prevHealth} -> {currentHealth}</color>");
        
        // 播放受伤动画
        if (anim != null)
        {
            anim.SetTrigger("Hurt");
        }
        
        // 检查是否死亡
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log($"<color=#FF0000>敌人 {gameObject.name} 死亡！</color>");
            Die();
        }
        
        // 通知战斗管理器记录伤害
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.LogDamage(GameObject.FindGameObjectWithTag("Player"), gameObject, damage);
            
            // 检查是否击杀
            if (currentHealth <= 0)
            {
                CombatManager.Instance.HandleKill(GameObject.FindGameObjectWithTag("Player"), gameObject);
            }
        }
    }
    
    // 敌人死亡
    private void Die()
    {
        isDead = true;
        
        // 播放死亡动画
        if (anim != null)
        {
            anim.SetBool("IsDead", true);
        }
        
        // 禁用组件
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
        
        // 在一段时间后销毁对象
        Destroy(gameObject, 2f);
    }    // Update is called once per frame
    void Update()
    {
        // 如果敌人已死亡，不执行任何逻辑
        if (isDead)
            return;
        
        UpdatePlayerDistance();
        UpdateMovement();
        UpdateAttackState();
        UpdateAnimator();
    }
    
    // 更新与玩家的距离
    private void UpdatePlayerDistance()
    {
        // 如果player引用丢失，尝试重新查找
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                distanceToPlayer = 0;
                isActive = false;
                return;
            }
        }
        
        distanceToPlayer = player.transform.position.x - transform.position.x;
        
        // 当玩家在一定范围内时激活敌人
        isActive = Mathf.Abs(distanceToPlayer) < 5f;
    }
    
    // 更新敌人移动
    private void UpdateMovement()
    {
        // 如果正在攻击或未激活，不处理移动
        if (isAttacking || !isActive)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }
        
        // 在攻击范围内停止并准备攻击
        if (Mathf.Abs(distanceToPlayer) <= attackRange)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        // 否则向玩家移动
        else if (Mathf.Abs(distanceToPlayer) < 5f)
        {
            float moveDirection = distanceToPlayer > 0 ? 1 : -1;
            rb.linearVelocity = new Vector2(moveDirection * 5f, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }
    
    // 更新攻击状态
    private void UpdateAttackState()
    {
        // 只有在激活状态且不在攻击过程中时才考虑攻击
        if (!isActive || isAttacking)
            return;
        
        // 如果玩家在攻击范围内且冷却时间已过
        if (Mathf.Abs(distanceToPlayer) <= attackRange && Time.time >= nextAttackTime)
        {
            // 开始攻击
            PerformAttack();
            
            // 设置下次攻击时间
            nextAttackTime = Time.time + attackCooldown;
        }
    }
    
    // 执行攻击
    private void PerformAttack()
    {
        isAttacking = true;
        
        // 触发攻击动画
        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }
        
        // 延迟进行攻击判定
        Invoke("ApplyAttackDamage", attackDelay);
        
        // 延迟结束攻击状态
        Invoke("EndAttackState", 0.5f); // 假设攻击动画持续0.5秒左右
    }
    
    // 应用攻击伤害
    private void ApplyAttackDamage()
    {
        if (isDead) return;
        
        // 如果玩家不存在或已离开攻击范围，则不造成伤害
        if (player == null || Mathf.Abs(distanceToPlayer) > attackRange * 1.2f)
            return;
        
        // 检查玩家是否有生命值组件
        PlayerAttackSystem playerHealth = player.GetComponent<PlayerAttackSystem>();
        if (playerHealth != null)
        {
            // 对玩家造成伤害
            playerHealth.TakeDamage(attackDamage);
              // 记录到战斗管理器
            if (CombatManager.Instance != null)
            {
                CombatManager.Instance.LogDamage(gameObject, player, attackDamage);
            }
            
            Debug.Log($"<color=#FF4500>敌人 {gameObject.name} 攻击了玩家，造成 {attackDamage} 点伤害！</color>");
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
        Vector3 currentScale = transform.localScale;
        currentScale.x = distanceToPlayer > 0 ? originScaleX : -originScaleX;
        transform.localScale = currentScale;
        
        // 更新移动动画
        lastSpeed = rb.linearVelocity.x;
        isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.1f;
        anim.SetBool("isMoving", isMoving);
        
        // 更新接近状态
        approach = Mathf.Abs(distanceToPlayer) <= attackRange;
        anim.SetBool("approach", approach);
        
        // 设置激活状态参数
        anim.SetBool("isActive", isActive);
    }
}
