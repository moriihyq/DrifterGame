using UnityEngine.UI;
using UnityEngine;

public class PlayerAttackSystem : MonoBehaviour
{
    [Header("攻击设置")]
    [SerializeField] private int attackDamage = 50; // 玩家攻击伤害
    [SerializeField] private float attackCooldown = 0.2f; // 攻击冷却时间
    [SerializeField] private Transform attackPoint; // 攻击判定点
    [SerializeField] private float attackRadius = 3.0f; // 攻击范围半径（固定为3单位）
    [SerializeField] private LayerMask enemyLayers; // 敌人图层
      [Header("音效设置")]
    [SerializeField] private AudioSource audioSource; // 音效播放器
    [SerializeField] private AudioClip normalAttackSound; // 普通攻击音效
    [SerializeField] [Range(0f, 1f)] private float normalAttackVolume = 1f; // 普通攻击音量
    private AudioVolumeManager audioVolumeManager; // 音量管理器
    
    // 公共属性，允许其他脚本修改攻击伤害
    public int AttackDamage 
    { 
        get { return attackDamage; } 
        set { attackDamage = value; Debug.Log($"<color=yellow>[PlayerAttackSystem] 攻击伤害设置为: {attackDamage}</color>"); } 
    }
    
    [Header("生命值设置")]
    [SerializeField] private int maxHealth = 100; // 最大生命值
    private int currentHealth; // 当前生命值
    
    // 组件引用
    private Animator animator; // 动画控制器
    
    // 私有变量
    private float nextAttackTime = 0f; // 下次可攻击时间
    private bool isDead = false; // 是否死亡
    
    private void Start()
    {
        // 获取组件引用
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogError("未找到Animator组件！请确保玩家对象或其子对象上有Animator组件");
            }
        }
        
        // 如果没有设置音频源，则获取或添加一个
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                Debug.Log("自动添加AudioSource组件到玩家对象");
            }
        }
        
        // 初始化生命值
        currentHealth = maxHealth;
        
        // 如果没有设置攻击点，则创建一个
        if (attackPoint == null)
        {
            GameObject attackPointObj = new GameObject("AttackPoint");
            attackPointObj.transform.parent = this.transform;
            attackPointObj.transform.localPosition = new Vector3(1f, 0f, 0f); // 在角色前方创建攻击点
            attackPoint = attackPointObj.transform;
        }
          // 检查enemyLayers设置
        if (enemyLayers.value == 0)
        {
            Debug.LogWarning("enemyLayers未设置！请在Unity编辑器中设置正确的敌人图层");
        }
        
        // 查找音量管理器
        audioVolumeManager = FindFirstObjectByType<AudioVolumeManager>();
        if (audioVolumeManager == null)
        {
            Debug.LogWarning("PlayerAttackSystem: 未找到AudioVolumeManager，音效将使用默认音量");
        }
    }
    
    private void Update()
    {
        // 如果已死亡，不执行后续逻辑
        if (isDead) return;
        
        // 检查是否可以攻击
        if (Time.time >= nextAttackTime)
        {
            // 检查攻击输入 (假设使用 "Fire1" 按钮进行攻击)
            if (Input.GetButtonDown("Fire1"))
            {
                Kick();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }
      // 踢击攻击
    private void Kick()
    {
        // 在控制台输出玩家攻击信息
        Debug.Log("<color=#00BFFF>玩家发动踢击攻击！</color>");
        
        // 播放踢击动画
        if (animator != null)
        {
            animator.SetTrigger("Kick");
        }
        else
        {
            Debug.LogError("没有找到Animator组件，无法播放攻击动画!");
        }
          // 播放普通攻击音效
        if (audioSource != null && normalAttackSound != null)
        {            float finalVolume = normalAttackVolume;
            if (audioVolumeManager != null)
            {
                finalVolume *= audioVolumeManager.GetCurrentVolume();
            }
            audioSource.PlayOneShot(normalAttackSound, finalVolume);
        }
        else
        {
            Debug.LogWarning("音效播放器或普通攻击音效未设置，无法播放音效");
        }
        
        // 检查enemyLayers是否已设置，如果未设置则尝试自动设置
        if (enemyLayers.value == 0)
        {
            Debug.LogError("enemyLayers未设置! 尝试自动设置敌人图层");
            // 尝试自动设置为默认的"Enemy"层
            enemyLayers = LayerMask.GetMask("Enemy");
            if (enemyLayers.value == 0)
            {
                // 如果"Enemy"层不存在，则尝试使用其他可能的层名称
                enemyLayers = LayerMask.GetMask("Enemies", "Monster", "Hostile");
                if (enemyLayers.value == 0)
                {
                    // 如果还是找不到合适的层，使用除"Ignore Raycast"外的所有层
                    enemyLayers = ~(LayerMask.GetMask("Ignore Raycast"));
                    Debug.LogWarning("未找到专门的敌人图层，使用所有层作为敌人检测层");
                }
            }
        }
        
        // 检测攻击范围内的敌人
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayers);
        
        // 对每个敌人造成伤害
        foreach (Collider2D enemy in hitEnemies)
        {
            // 尝试获取敌人组件并造成伤害
            Enemy enemyComponent = enemy.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                Debug.Log($"<color=#4169E1>玩家攻击命中敌人 {enemy.name}！</color>");
                enemyComponent.TakeDamage(attackDamage);
                continue;
            }

            // 检测BossController
            BossController boss = enemy.GetComponent<BossController>();
            if (boss != null)
            {
                Debug.Log($"<color=yellow>玩家攻击命中Boss {enemy.name}！</color>");
                boss.TakeDamage(attackDamage);
                continue;
            }

            // 尝试在父对象或子对象中查找Enemy组件
            enemyComponent = enemy.GetComponentInParent<Enemy>();
            if (enemyComponent != null)
            {
                enemyComponent.TakeDamage(attackDamage);
                continue;
            }
            enemyComponent = enemy.GetComponentInChildren<Enemy>();
            if (enemyComponent != null)
            {
                enemyComponent.TakeDamage(attackDamage);
                continue;
            }

            // 如果使用的是 EnemyBase 类，则使用该类的伤害方法
            EnemyBase enemyBaseComponent = enemy.GetComponent<EnemyBase>();
            if (enemyBaseComponent != null)
            {
                enemyBaseComponent.TakeDamage(attackDamage);
            }
        }
    }
      // 玩家受到伤害
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        int prevHealth = currentHealth;
        currentHealth -= damage;
        
        // 在控制台输出玩家血量变化信息
        Debug.Log($"<color=#FF6347>玩家受到 {damage} 点伤害！血量变化：{prevHealth} -> {currentHealth}</color>");
        
        // 播放受伤动画
        if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }
        
        // 检查是否死亡
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log("<color=#FF0000>玩家死亡！</color>");
            Die();
        }
    }
      // 恢复生命值
    public void Heal(int amount)
    {
        if (isDead) return;
        
        int prevHealth = currentHealth;
        currentHealth += amount;
        
        // 限制最大生命值
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        
        // 在控制台输出玩家血量恢复信息
        Debug.Log($"<color=#00FF00>玩家恢复 {amount} 点生命值！血量变化：{prevHealth} -> {currentHealth}</color>");
    }
    
    // 恢复全部生命值（血瓶专用）
    public void HealToFull()
    {
        if (isDead) return;
        
        int prevHealth = currentHealth;
        currentHealth = maxHealth;
        
        // 在控制台输出玩家血量恢复信息
        Debug.Log($"<color=#00FF00>玩家使用血瓶恢复全部生命值！血量变化：{prevHealth} -> {currentHealth}</color>");
    }
    
    // 玩家死亡
    private void Die()
    {
        isDead = true;
        
        // 播放死亡动画
        if (animator != null)
        {
            animator.SetBool("IsDead", true);
        }
        
        // 禁用玩家控制器
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
        
        // 禁用当前组件
        this.enabled = false;
    }
    
    // 绘制攻击范围 (仅在编辑器中可见，用于调试)
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
    
    // 获取当前生命值
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    // 获取最大生命值
    public int GetMaxHealth()
    {
        return maxHealth;
    }
    
    // 直接设置当前生命值
    public void SetHealth(int newHealth)
    {
        if (isDead) return;
        
        currentHealth = newHealth;
        
        // 限制生命值范围
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }
    
    // 获取最大生命值（供外部访问）
    public int MaxHealth
    {
        get { return maxHealth; }
    }
    
    // 外部只读访问当前生命值
    public int Health => currentHealth;
}