using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLevelSkillSystem : MonoBehaviour
{
    [Header("等级系统")]
    [SerializeField] private int playerLevel = 1;
    
    [Header("空中攻击技能设置")]
    [SerializeField] private int aerialAttackRequiredLevel = 3;
    [SerializeField] private float aerialAttackCooldown = 20f;
    [SerializeField] private float aerialAttackRange = 3f;
    [SerializeField] private float aerialAttackDamage = 50f;
    [SerializeField] private float slowEffectDuration = 3f;
    [SerializeField] private float slowEffectStrength = 0.5f; // 减速到原速度的50%
    
    [Header("技能UI")]
    [SerializeField] private GameObject skillCooldownUI;
    [SerializeField] private Text cooldownText;
    [SerializeField] private Image cooldownFillImage;
    [SerializeField] private Text levelRequirementText;
    
    [Header("音效")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip aerialAttackSound;
    
    // 私有变量
    private bool isAerialAttackOnCooldown = false;
    private float currentCooldownTime = 0f;
    private Animator playerAnimator;
    private PlayerAttackSystem playerAttackSystem;      // 公共属性
    public int PlayerLevel 
    { 
        get { return playerLevel; } 
        set { playerLevel = value; UpdateUI(); } 
    }
      // 技能状态的公共只读属性
    public bool IsAerialAttackOnCooldown => isAerialAttackOnCooldown;
    public float CurrentCooldownTime => currentCooldownTime;
    
    // 公共方法：增加等级
    public void AddLevel(int levelToAdd)
    {
        int oldLevel = playerLevel;
        playerLevel += levelToAdd;
        UpdateUI();
        Debug.Log($"玩家等级从 {oldLevel} 提升到 {playerLevel}");
    }
    
    void Start()
    {
        // 获取组件引用
        playerAnimator = GetComponent<Animator>();
        playerAttackSystem = GetComponent<PlayerAttackSystem>();
        
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        // 初始化UI
        InitializeUI();
        UpdateUI();
    }
    
    void Update()
    {
        HandleInput();
        UpdateCooldownUI();
    }
      void HandleInput()
    {
        // 检测X键输入
        if (Input.GetKeyDown(KeyCode.X))
        {
            TryUseAerialAttack();
        }
        
        // 测试用：按L键提升等级到3级
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (playerLevel < 3)
            {
                PlayerLevel = 3;
                Debug.Log($"<color=#00FF00>测试：玩家等级提升到 {playerLevel} 级！现在可以使用空中攻击技能</color>");
            }
            else
            {
                Debug.Log($"<color=#FFFF00>玩家已经是 {playerLevel} 级</color>");
            }
        }
        
        // 测试用：按K键直接重置冷却时间
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (isAerialAttackOnCooldown)
            {
                isAerialAttackOnCooldown = false;
                currentCooldownTime = 0f;
                Debug.Log($"<color=#00FF00>测试：重置技能冷却时间</color>");
            }
            else
            {
                Debug.Log($"<color=#FFFF00>技能当前没有在冷却中</color>");
            }
        }
    }
      void TryUseAerialAttack()
    {
        Debug.Log($"<color=#FFFF00>尝试使用空中攻击技能...</color>");
        Debug.Log($"<color=#FFFF00>当前玩家等级: {playerLevel}, 需要等级: {aerialAttackRequiredLevel}</color>");
        Debug.Log($"<color=#FFFF00>技能是否在冷却: {isAerialAttackOnCooldown}, 剩余冷却时间: {currentCooldownTime:F1}秒</color>");
        
        // 检查等级要求
        if (playerLevel < aerialAttackRequiredLevel)
        {
            Debug.Log($"<color=#FF0000>等级不足！当前等级 {playerLevel}，需要等级 {aerialAttackRequiredLevel}</color>");
            ShowLevelRequirementMessage();
            return;
        }
        
        // 检查冷却时间
        if (isAerialAttackOnCooldown)
        {
            Debug.Log($"<color=#FF0000>空中攻击还在冷却中，剩余时间: {currentCooldownTime:F1}秒</color>");
            return;
        }
        
        Debug.Log($"<color=#00FF00>所有条件满足，执行空中攻击！</color>");
        
        // 执行空中攻击
        PerformAerialAttack();
    }
      void PerformAerialAttack()
    {
        Debug.Log("执行空中攻击技能！");
        
        // 播放动画
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("Aerial_Attack");
            Debug.Log("播放空中攻击动画：Aerial_Attack");
        }
        
        // 播放音效
        if (audioSource != null && aerialAttackSound != null)
        {
            audioSource.PlayOneShot(aerialAttackSound);
        }
        
        // 检测前方敌人并造成伤害和减速
        DetectAndDamageEnemies();
        
        // 开始冷却
        StartCooldown();
    }
      // 获取玩家面向方向的方法
    float GetPlayerDirection()
    {
        // 方法1：通过SpriteRenderer.flipX判断 (PlayerController使用这种方式)
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        float spriteDirection = 1f; // 默认向右
        if (sr != null)
        {
            // flipX=true表示面向左，flipX=false表示面向右
            spriteDirection = sr.flipX ? -1f : 1f;
        }
        
        // 方法2：通过localScale.x判断 (备用方法)
        float scaleDirection = transform.localScale.x > 0 ? 1f : -1f;
        
        // 方法3：通过Rigidbody2D的velocity判断移动方向
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        float velocityDirection = 0f;
        if (rb != null && Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        {
            velocityDirection = rb.linearVelocity.x > 0 ? 1f : -1f;
        }
        
        // 方法4：通过输入方向判断
        float inputDirection = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            inputDirection = -1f;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            inputDirection = 1f;
        }
        
        // 调试信息
        Debug.Log($"<color=#FFFF00>面向检测 - Sprite方向: {spriteDirection} (flipX: {(sr != null ? sr.flipX.ToString() : "null")}), Scale方向: {scaleDirection}, 速度方向: {velocityDirection}, 输入方向: {inputDirection}</color>");
        
        // 优先级：输入方向 > 速度方向 > Sprite方向 > Scale方向
        if (inputDirection != 0f)
        {
            Debug.Log($"<color=#00FF00>使用输入方向: {inputDirection}</color>");
            return inputDirection;
        }
        else if (velocityDirection != 0f)
        {
            Debug.Log($"<color=#00FF00>使用速度方向: {velocityDirection}</color>");
            return velocityDirection;
        }
        else if (sr != null)
        {
            Debug.Log($"<color=#00FF00>使用Sprite方向: {spriteDirection} (基于flipX)</color>");
            return spriteDirection;
        }
        else
        {
            Debug.Log($"<color=#00FF00>使用Scale方向: {scaleDirection}</color>");
            return scaleDirection;
        }
    }void DetectAndDamageEnemies()
    {
        // 获取玩家面向方向 - 提供多种检测方式
        float direction = GetPlayerDirection();
          // 详细调试面向信息
        Debug.Log($"<color=#00FFFF>===== 空中攻击检测开始 =====</color>");
        Debug.Log($"<color=#00FFFF>玩家位置: {transform.position}</color>");
        
        // 显示所有面向检测信息
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Debug.Log($"<color=#00FFFF>SpriteRenderer.flipX: {sr.flipX} (true=面向左, false=面向右)</color>");
        }
        Debug.Log($"<color=#00FFFF>transform.localScale.x: {transform.localScale.x}</color>");
        
        Debug.Log($"<color=#00FFFF>计算的方向值: {direction}</color>");
        Debug.Log($"<color=#00FFFF>玩家面向: {(direction > 0 ? "右" : "左")}</color>");
        
        // 计算检测区域的中心点
        Vector2 attackCenter = (Vector2)transform.position + Vector2.right * direction * (aerialAttackRange / 2f);
        
        // 创建检测区域（矩形）
        Vector2 boxSize = new Vector2(aerialAttackRange, 2f); // 宽度为设定范围，高度为2个单位
        
        Debug.Log($"<color=#00FFFF>检测中心: {attackCenter}</color>");
        Debug.Log($"<color=#00FFFF>检测大小: {boxSize}</color>");
        Debug.Log($"<color=#00FFFF>伤害值: {aerialAttackDamage}</color>");
        
        // 检测区域内的所有碰撞器
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(attackCenter, boxSize, 0f);
        
        Debug.Log($"<color=#00FFFF>检测到 {hitColliders.Length} 个碰撞器</color>");
        
        int enemiesHit = 0;
        
        for (int i = 0; i < hitColliders.Length; i++)
        {
            Collider2D hitCollider = hitColliders[i];
            Debug.Log($"<color=#FFFF00>碰撞器 {i+1}: {hitCollider.name} (Tag: {hitCollider.tag})</color>");
            
            // 检查是否为敌人
            Enemy enemy = hitCollider.GetComponent<Enemy>();
            BossController boss = hitCollider.GetComponent<BossController>();
            
            if (enemy != null)
            {
                Debug.Log($"<color=#FF6600>发现敌人: {enemy.name}，准备造成伤害...</color>");
                
                // 对普通敌人造成伤害
                enemy.TakeDamage((int)aerialAttackDamage);
                
                // 应用减速效果
                ApplySlowEffect(enemy);
                
                enemiesHit++;
                Debug.Log($"<color=#00FF00>✓ 空中攻击命中敌人 {enemy.name}，造成 {aerialAttackDamage} 点伤害</color>");
            }
            else if (boss != null)
            {
                Debug.Log($"<color=#FF6600>发现Boss: {boss.name}，准备造成伤害...</color>");
                
                // 对Boss造成伤害
                boss.TakeDamage((int)aerialAttackDamage);
                
                // 应用减速效果到Boss
                ApplySlowEffectToBoss(boss);
                
                enemiesHit++;
                Debug.Log($"<color=#00FF00>✓ 空中攻击命中Boss {boss.name}，造成 {aerialAttackDamage} 点伤害</color>");
            }
            else
            {
                Debug.Log($"<color=#CCCCCC>碰撞器 {hitCollider.name} 不是敌人或Boss</color>");
            }
        }
        
        Debug.Log($"<color=#00FFFF>===== 空中攻击检测结束，共命中 {enemiesHit} 个敌人 =====</color>");
        
        // 如果没有命中任何敌人，提供额外的调试信息
        if (enemiesHit == 0)
        {
            Debug.Log($"<color=#FF0000>警告：空中攻击没有命中任何敌人！</color>");
            
            // 尝试查找附近的敌人
            GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            Debug.Log($"<color=#FF0000>场景中共有 {allEnemies.Length} 个敌人标签的对象</color>");
            
            foreach (GameObject enemyObj in allEnemies)
            {
                float distance = Vector2.Distance(transform.position, enemyObj.transform.position);
                Debug.Log($"<color=#FF0000>敌人 {enemyObj.name} 距离: {distance}</color>");
            }
            
            // 查找Boss
            BossController[] allBosses = FindObjectsOfType<BossController>();
            Debug.Log($"<color=#FF0000>场景中共有 {allBosses.Length} 个Boss</color>");
            
            foreach (BossController bossObj in allBosses)
            {
                float distance = Vector2.Distance(transform.position, bossObj.transform.position);
                Debug.Log($"<color=#FF0000>Boss {bossObj.name} 距离: {distance}</color>");
            }
        }
    }
    
    void ApplySlowEffect(Enemy enemy)
    {
        // 启动减速效果协程
        StartCoroutine(SlowEnemyCoroutine(enemy));
    }
    
    void ApplySlowEffectToBoss(BossController boss)
    {
        // 启动Boss减速效果协程
        StartCoroutine(SlowBossCoroutine(boss));
    }
      IEnumerator SlowEnemyCoroutine(Enemy enemy)
    {
        if (enemy == null) yield break;
        
        // 由于Enemy类没有moveSpeed字段，我们需要添加一个减速组件
        EnemySlowEffect slowEffect = enemy.GetComponent<EnemySlowEffect>();
        if (slowEffect == null)
        {
            slowEffect = enemy.gameObject.AddComponent<EnemySlowEffect>();
        }
        
        // 应用减速效果
        slowEffect.ApplySlowEffect(slowEffectStrength, slowEffectDuration);
        
        Debug.Log($"敌人被减速，减速强度 {slowEffectStrength}，持续 {slowEffectDuration} 秒");
        
        // 等待减速持续时间
        yield return new WaitForSeconds(slowEffectDuration);
        
        Debug.Log($"敌人减速效果结束");
    }
      IEnumerator SlowBossCoroutine(BossController boss)
    {
        if (boss == null) yield break;
        
        // 由于BossController的moveSpeed是私有的，我们需要使用一个减速组件
        BossSlowEffect slowEffect = boss.GetComponent<BossSlowEffect>();
        if (slowEffect == null)
        {
            slowEffect = boss.gameObject.AddComponent<BossSlowEffect>();
        }
        
        // 应用减速效果
        slowEffect.ApplySlowEffect(slowEffectStrength, slowEffectDuration);
        
        Debug.Log($"Boss被减速，减速强度 {slowEffectStrength}，持续 {slowEffectDuration} 秒");
        
        // 等待减速持续时间
        yield return new WaitForSeconds(slowEffectDuration);
        
        Debug.Log($"Boss减速效果结束");
    }
    
    void StartCooldown()
    {
        isAerialAttackOnCooldown = true;
        currentCooldownTime = aerialAttackCooldown;
        
        StartCoroutine(CooldownCoroutine());
    }
    
    IEnumerator CooldownCoroutine()
    {
        while (currentCooldownTime > 0)
        {
            yield return new WaitForSeconds(0.1f);
            currentCooldownTime -= 0.1f;
        }
        
        isAerialAttackOnCooldown = false;
        currentCooldownTime = 0f;
        
        Debug.Log("空中攻击技能冷却完成！");
    }
    
    void InitializeUI()
    {
        // 如果没有指定UI元素，尝试自动查找
        if (skillCooldownUI == null)
        {
            skillCooldownUI = GameObject.Find("SkillCooldownUI");
        }
        
        if (cooldownText == null && skillCooldownUI != null)
        {
            cooldownText = skillCooldownUI.transform.Find("CooldownText")?.GetComponent<Text>();
        }
        
        if (cooldownFillImage == null && skillCooldownUI != null)
        {
            cooldownFillImage = skillCooldownUI.transform.Find("CooldownFill")?.GetComponent<Image>();
        }
        
        if (levelRequirementText == null)
        {
            levelRequirementText = GameObject.Find("LevelRequirementText")?.GetComponent<Text>();
        }
    }
    
    void UpdateUI()
    {
        // 更新等级要求提示
        if (levelRequirementText != null)
        {
            if (playerLevel < aerialAttackRequiredLevel)
            {
                levelRequirementText.gameObject.SetActive(true);
                levelRequirementText.text = $"空中攻击需要等级 {aerialAttackRequiredLevel} (当前: {playerLevel})";
            }
            else
            {
                levelRequirementText.gameObject.SetActive(false);
            }
        }
    }
    
    void UpdateCooldownUI()
    {
        if (skillCooldownUI == null) return;
        
        // 根据冷却状态显示/隐藏UI
        if (isAerialAttackOnCooldown && playerLevel >= aerialAttackRequiredLevel)
        {
            skillCooldownUI.SetActive(true);
            
            // 更新冷却时间文本
            if (cooldownText != null)
            {
                cooldownText.text = $"空中攻击: {currentCooldownTime:F1}s";
            }
            
            // 更新冷却填充图像
            if (cooldownFillImage != null)
            {
                float fillAmount = currentCooldownTime / aerialAttackCooldown;
                cooldownFillImage.fillAmount = fillAmount;
            }
        }
        else
        {
            skillCooldownUI.SetActive(false);
        }
    }
    
    void ShowLevelRequirementMessage()
    {
        Debug.Log($"等级不足！需要等级 {aerialAttackRequiredLevel}，当前等级 {playerLevel}");
        
        // 如果有UI文本，临时显示等级要求
        if (levelRequirementText != null)
        {
            StartCoroutine(ShowTemporaryLevelMessage());
        }
    }
    
    IEnumerator ShowTemporaryLevelMessage()
    {
        if (levelRequirementText != null)
        {
            levelRequirementText.gameObject.SetActive(true);
            levelRequirementText.text = $"需要等级 {aerialAttackRequiredLevel}！当前等级: {playerLevel}";
            levelRequirementText.color = Color.red;
            
            yield return new WaitForSeconds(2f);
            
            levelRequirementText.color = Color.white;
            UpdateUI(); // 恢复正常UI状态
        }
    }    // 可视化检测范围（仅在Scene视图中显示）
    void OnDrawGizmosSelected()
    {
        // 只在达到等级要求时绘制攻击范围
        if (playerLevel >= aerialAttackRequiredLevel)
        {
            // 获取玩家面向方向
            float direction = GetPlayerDirection();
            
            // 计算检测区域的中心点
            Vector2 attackCenter = (Vector2)transform.position + Vector2.right * direction * (aerialAttackRange / 2f);
            Vector2 boxSize = new Vector2(aerialAttackRange, 2f);
            
            // 根据冷却状态选择颜色
            Gizmos.color = isAerialAttackOnCooldown ? Color.red : Color.cyan;
            
            // 绘制检测范围边框
            Gizmos.DrawWireCube(attackCenter, boxSize);
            
            // 绘制半透明填充
            Gizmos.color = isAerialAttackOnCooldown ? 
                new Color(1f, 0f, 0f, 0.2f) : 
                new Color(0f, 1f, 1f, 0.2f);
            Gizmos.DrawCube(attackCenter, boxSize);
            
            // 绘制方向箭头
            Gizmos.color = Color.yellow;
            Vector3 arrowStart = transform.position;
            Vector3 arrowEnd = arrowStart + Vector3.right * direction * aerialAttackRange;
            Gizmos.DrawLine(arrowStart, arrowEnd);
            
            // 绘制箭头头部
            Vector3 arrowHeadRight = arrowEnd + Vector3.left * direction * 0.3f + Vector3.up * 0.2f;
            Vector3 arrowHeadLeft = arrowEnd + Vector3.left * direction * 0.3f + Vector3.down * 0.2f;
            Gizmos.DrawLine(arrowEnd, arrowHeadRight);
            Gizmos.DrawLine(arrowEnd, arrowHeadLeft);
            
            // 绘制技能信息文字（在Scene视图中）
            #if UNITY_EDITOR
            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
                $"空中攻击 ({(direction > 0 ? "右" : "左")})\n伤害: {aerialAttackDamage}\n范围: {aerialAttackRange}\n冷却: {(isAerialAttackOnCooldown ? $"{currentCooldownTime:F1}s" : "就绪")}");
            #endif
        }        else
        {
            // 等级不足时显示锁定状态
            Gizmos.color = Color.gray;
            
            // 使用相同的面向检测逻辑
            float direction = 1f; // 默认向右
            if (Application.isPlaying)
            {
                direction = GetPlayerDirection();
            }
            else
            {
                // 编辑器模式下的面向检测
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    direction = sr.flipX ? -1f : 1f;
                }
                else
                {
                    direction = transform.localScale.x > 0 ? 1f : -1f;
                }
            }
            
            Vector2 attackCenter = (Vector2)transform.position + Vector2.right * direction * (aerialAttackRange / 2f);
            Vector2 boxSize = new Vector2(aerialAttackRange, 2f);
            Gizmos.DrawWireCube(attackCenter, boxSize);
            
            #if UNITY_EDITOR
            UnityEditor.Handles.color = Color.red;
                    UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
                $"空中攻击 (锁定)\n需要等级: {aerialAttackRequiredLevel}\n当前等级: {playerLevel}");
            #endif
        }
    }
    
    // 公共方法供外部调用
    public bool CanUseAerialAttack()
    {
        return playerLevel >= aerialAttackRequiredLevel && !isAerialAttackOnCooldown;
    }
}
