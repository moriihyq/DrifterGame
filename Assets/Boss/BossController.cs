using UnityEngine;
using System.Collections;

using UnityEngine.UI;
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
    
    [Header("音效设置")]
    [SerializeField] private AudioSource audioSource; // 音效播放器
    [SerializeField] private AudioClip meleeAttackSound; // 近战攻击音效
    [SerializeField] private AudioClip rangedAttackSound; // 远程攻击音效
    [SerializeField] [Range(0f, 1f)] private float meleeAttackVolume = 1f; // 近战攻击音量
    [SerializeField] [Range(0f, 1f)] private float rangedAttackVolume = 1f; // 远程攻击音量
    
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
        
        // 初始化音频组件
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                Debug.Log("自动添加AudioSource组件到Boss对象");
            }
        }
        
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
        
        // 确保playerLayer已正确设置
        if (playerLayer.value == 0)
        {
            Debug.LogWarning("<color=yellow>Boss的playerLayer未设置，尝试自动设置为'Player'层...</color>");
            playerLayer = LayerMask.GetMask("Player");
            
            if (playerLayer.value == 0)
            {
                Debug.LogError("<color=red>严重错误：找不到'Player'层！请确保已在项目中创建此层。尝试寻找替代层...</color>");
                
                // 尝试其他可能的层名称
                string[] possibleLayerNames = new string[] {"Character", "PlayerCharacter", "Hero", "Protagonist"};
                foreach (string layerName in possibleLayerNames)
                {
                    playerLayer = LayerMask.GetMask(layerName);
                    if (playerLayer.value != 0)
                    {
                        Debug.Log($"<color=yellow>使用'{layerName}'层作为替代的玩家层</color>");
                        break;
                    }
                }
                
                // 如果仍未找到，则使用一些常见层
                if (playerLayer.value == 0)
                {
                    playerLayer = LayerMask.GetMask("Default");
                    Debug.LogWarning("<color=orange>无法找到专门的玩家层，使用'Default'层。这可能导致攻击检测不准确！</color>");
                }
            }
        }
        
        Debug.Log($"<color=cyan>Boss初始化完成 - 玩家引用: {(player != null ? player.name : "未找到")}, 玩家层: {playerLayer.value}</color>");
        
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
    }      void Update()
    {
        // 如果Boss已死亡，不执行任何逻辑
        if (isDead) return;
          // 检测P键激活或重置Boss
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("<color=#FF1493>P键被按下 - 当前Boss激活状态: " + isActive + "</color>");
            
            // 重置所有相关状态，确保完全重新激活
            StopAllCoroutines(); // 停止所有可能的协程
            isAttacking = false;
            initialAttackPerformed = false;
            
            // 无论当前状态如何，都重新查找玩家并激活Boss
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
            {
                player = foundPlayer;
                Debug.Log($"<color=#FF1493>找到玩家，位置: {player.transform.position}</color>");
                
                // 完全重新激活Boss
                isActive = false;
                StartCoroutine(DelayedActivation());
            }
            else
            {
                Debug.LogError("<color=#FF0000>无法找到玩家对象！Boss无法激活！</color>");
            }
        }
        
        if (isActive)
        {
            UpdatePlayerDistance();
            UpdateMovement();
            UpdateAttackState();
        }
        
        UpdateAnimator();
    }    // 用于延迟激活Boss的协程，解决可能的帧同步问题
    private IEnumerator DelayedActivation()
    {
        // 先等待一帧，确保场景中的所有状态都已更新
        yield return null;
        
        // 然后调用激活方法
        ActivateBoss();
        
        // 额外的调试检查
        yield return new WaitForSeconds(0.5f);
        
        if (!isActive || rb.linearVelocity.x == 0)
        {
            Debug.LogWarning("<color=#FFFF00>Boss可能未正确激活，尝试再次激活...</color>");
            ActivateBoss();
        }
    }
    
    // 激活Boss
    public void ActivateBoss()
    {
        // 设置激活状态
        isActive = true;
        consecutiveMeleeAttacks = 0;
        initialAttackPerformed = false;
        
        // 记录激活时间
        float activationTime = Time.time;
        Debug.Log($"<color=#FF0000>Boss激活！时间: {activationTime}, 当前帧: {Time.frameCount}</color>");
        
        // 确保找到玩家
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                player = GameObject.Find("Player");
                if (player == null)
                {
                    Debug.LogError("<color=#FF0000>激活Boss时无法找到玩家对象！Boss将无法追踪</color>");
                    return;
                }
            }
        }
        
        Debug.Log($"<color=#FF9900>找到玩家: {player.name}, 位置: {player.transform.position}</color>");
        
        // 更新玩家距离
        UpdatePlayerDistance();
        
        // 立即更新动画状态
        if (anim != null)
        {
            anim.SetBool("isActive", true);
            anim.SetBool("isMoving", true);
            Debug.Log("<color=#32CD32>Boss动画状态已更新：isActive=true, isMoving=true</color>");
        }
        else
        {
            Debug.LogError("<color=#FF0000>Boss没有动画控制器！</color>");
        }
        
        // 强制立即更新移动
        if (rb != null)
        {
            float moveDirection = distanceToPlayer > 0 ? 1 : -1;
            rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
            Debug.Log($"<color=#FF9900>Boss开始移动！位置: {transform.position}, 速度: {rb.linearVelocity}, 移动方向: {moveDirection}, 玩家距离: {distanceToPlayer}</color>");
            
            // 添加强制移动确认
            StartCoroutine(ConfirmMovement());
        }
        else
        {
            Debug.LogError("<color=#FF0000>Boss没有Rigidbody2D组件！</color>");
        }
    }
    
    // 确认移动已经成功启动的协程
    private IEnumerator ConfirmMovement()
    {
        yield return new WaitForSeconds(0.2f);
        
        if (isActive && rb.linearVelocity.x == 0 && Mathf.Abs(distanceToPlayer) > meleeAttackRange)
        {
            Debug.LogWarning("<color=#FFFF00>Boss移动似乎未成功启动，尝试手动设置速度...</color>");
            float moveDirection = distanceToPlayer > 0 ? 1 : -1;
            rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
        }
        
        // 多次检查，确保移动状态正常
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(0.2f);
            if (isActive && !isAttacking && Mathf.Abs(distanceToPlayer) > meleeAttackRange)
            {
                float moveDirection = distanceToPlayer > 0 ? 1 : -1;
                float currentVelocityX = rb.linearVelocity.x;
                float expectedVelocityX = moveDirection * moveSpeed;
                
                // 如果速度不符合预期，尝试修正
                if (Mathf.Abs(currentVelocityX - expectedVelocityX) > 0.5f)
                {
                    Debug.Log($"<color=#FFA07A>校正Boss移动速度: {currentVelocityX} -> {expectedVelocityX}</color>");
                    rb.linearVelocity = new Vector2(expectedVelocityX, rb.linearVelocity.y);
                }
            }
        }
    }// 更新与玩家的距离
    private void UpdatePlayerDistance()
    {
        // 如果玩家引用丢失，重新查找
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                player = GameObject.Find("Player");
                if (player == null)
                {
                    Debug.LogError("<color=#FF0000>Boss无法找到玩家！请确保场景中有带有'Player'标签的玩家对象</color>");
                    distanceToPlayer = 100f; // 如果找不到玩家，设置一个很大的距离
                    return;
                }
            }
        }
        
        // 计算与玩家的X轴距离（2D游戏通常关注X轴距离）
        distanceToPlayer = player.transform.position.x - transform.position.x;
        
        // 计算实际2D距离和垂直差距（用于攻击判定）
        Vector2 playerPosition = player.transform.position;
        Vector2 bossPosition = transform.position;
        float actualDistance = Vector2.Distance(playerPosition, bossPosition);
        float verticalDistance = Mathf.Abs(playerPosition.y - bossPosition.y);
    }// 更新Boss移动
    private void UpdateMovement()
    {
        // 检查Boss是否激活
        if (!isActive)
        {
            if (rb.linearVelocity.x != 0)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            return;
        }
    
        // 如果正在攻击，不允许移动
        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }
        
        // 确保再次检查玩家位置
        UpdatePlayerDistance();
        
        // 如果找不到玩家，不要移动
        if (player == null)
        {
            Debug.LogWarning("<color=yellow>UpdateMovement: 找不到玩家，Boss无法移动</color>");
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }
          // 获取玩家和Boss的Y轴距离
        float yDistance = Mathf.Abs(player.transform.position.y - transform.position.y);
        
        // 如果玩家和Boss在不同层级且差距较大，可以考虑垂直移动（如果游戏设计支持）
        bool shouldTrackPlayer = true;
        if (yDistance > 3.0f) // 如果玩家在明显不同的高度（可调整此阈值）
        {
            Debug.Log($"<color=orange>警告：玩家与Boss垂直距离过大: {yDistance}，可能需要调整寻路逻辑</color>");
            shouldTrackPlayer = false; // 可以设计为Boss放弃追击或使用特殊方式移动
        }
        
        // 如果是初次激活且尚未执行初始攻击，优先追踪玩家
        if (!initialAttackPerformed && shouldTrackPlayer)
        {
            // 如果距离大于近战攻击范围，向玩家移动
            if (Mathf.Abs(distanceToPlayer) > meleeAttackRange)
            {
                float moveDirection = distanceToPlayer > 0 ? 1 : -1;
                rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
            }
            else
            {
                // 停止并准备攻击
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
        else if (shouldTrackPlayer)
        {
            // 正常追踪逻辑：如果需要执行近战攻击但不在范围内，则靠近玩家
            if (consecutiveMeleeAttacks < meleeAttacksBeforeRanged && Mathf.Abs(distanceToPlayer) > meleeAttackRange)
            {
                float moveDirection = distanceToPlayer > 0 ? 1 : -1;
                rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
            }
            else
            {
                // 其他情况下停止移动
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
        else
        {
            // 如果不应该追踪（例如，Y轴距离过大），则停止移动
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
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
        
        // 播放近战攻击音效
        if (audioSource != null && meleeAttackSound != null)
        {
            audioSource.PlayOneShot(meleeAttackSound, meleeAttackVolume);
            Debug.Log("<color=#FF4500>播放近战攻击音效</color>");
        }
        else
        {
            Debug.LogWarning("<color=yellow>近战攻击音效未设置或音效播放器缺失</color>");
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
        
        // 播放远程攻击音效
        if (audioSource != null && rangedAttackSound != null)
        {
            audioSource.PlayOneShot(rangedAttackSound, rangedAttackVolume);
            Debug.Log("<color=#8B0000>播放远程攻击音效</color>");
        }
        else
        {
            Debug.LogWarning("<color=yellow>远程攻击音效未设置或音效播放器缺失</color>");
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
        
        // 确认playerLayer是否设置正确
        if (playerLayer.value == 0)
        {
            Debug.LogError("<color=red>错误: Boss的playerLayer未设置！尝试自动设置...</color>");
            playerLayer = LayerMask.GetMask("Player");
            if (playerLayer.value == 0)
            {
                Debug.LogError("<color=red>错误: 找不到Player层，请在Inspector中正确设置playerLayer</color>");
                return;
            }
        }
        
        // 调试信息
        Debug.Log($"<color=cyan>Boss近战攻击检测 - 攻击点位置: {meleeAttackPoint.position}, 攻击范围: {meleeAttackRange}, 玩家层掩码: {playerLayer.value}</color>");
        
        // 检测攻击范围内的玩家
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(meleeAttackPoint.position, meleeAttackRange, playerLayer);
        
        // 检查是否找到任何玩家碰撞体
        if (hitPlayers.Length == 0)
        {
            Debug.LogWarning($"<color=yellow>Boss近战攻击未检测到玩家碰撞体! 尝试直接查找玩家...</color>");
            
            // 尝试直接通过玩家引用查找
            if (player != null)
            {
                float distanceToPlayerReal = Vector2.Distance(meleeAttackPoint.position, player.transform.position);
                Debug.Log($"<color=cyan>直接距离检测: 与玩家实际距离 = {distanceToPlayerReal}，攻击范围 = {meleeAttackRange}</color>");
                
                if (distanceToPlayerReal <= meleeAttackRange * 1.2f) // 增加一些容错空间
                {
                    PlayerAttackSystem playerHealth = player.GetComponent<PlayerAttackSystem>();
                    if (playerHealth != null)
                    {
                        // 对玩家造成伤害
                        playerHealth.TakeDamage(meleeAttackDamage);
                        
                        // 记录到战斗管理器
                        if (CombatManager.Instance != null)
                        {
                            CombatManager.Instance.LogDamage(gameObject, player, meleeAttackDamage);
                        }
                        
                        Debug.Log($"<color=#FF4500>Boss的近战攻击命中玩家(距离检测)，造成 {meleeAttackDamage} 点伤害！</color>");
                        return; // 已处理完攻击
                    }
                }
            }
        }
        else
        {
            // 正常的碰撞检测流程
            foreach (Collider2D playerCollider in hitPlayers)
            {
                PlayerAttackSystem playerHealth = playerCollider.GetComponent<PlayerAttackSystem>();
                if (playerHealth == null)
                {
                    playerHealth = playerCollider.GetComponentInParent<PlayerAttackSystem>();
                }
                
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
                else
                {
                    Debug.LogWarning($"<color=yellow>检测到碰撞体 {playerCollider.name}，但未找到PlayerAttackSystem组件!</color>");
                }
            }
        }
    }
      // 应用远程攻击伤害
    private void ApplyRangedAttackDamage()
    {
        if (isDead || !isActive) return;
        
        // 确认playerLayer是否设置正确
        if (playerLayer.value == 0)
        {
            Debug.LogError("<color=red>错误: Boss的playerLayer未设置！尝试自动设置...</color>");
            playerLayer = LayerMask.GetMask("Player");
            if (playerLayer.value == 0)
            {
                Debug.LogError("<color=red>错误: 找不到Player层，请在Inspector中正确设置playerLayer</color>");
                return;
            }
        }
        
        // 调试信息
        Debug.Log($"<color=cyan>Boss远程攻击检测 - 攻击中心位置: {transform.position}, 攻击范围: {rangedAttackRadius}, 玩家层掩码: {playerLayer.value}</color>");
        
        // 远程攻击使用更大的范围，获取所有在范围内的玩家
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, rangedAttackRadius, playerLayer);
        
        // 检查是否找到任何玩家碰撞体
        if (hitPlayers.Length == 0)
        {
            Debug.LogWarning($"<color=yellow>Boss远程攻击未检测到玩家碰撞体! 尝试直接查找玩家...</color>");
            
            // 尝试直接通过玩家引用查找
            if (player != null)
            {
                float distanceToPlayerReal = Vector2.Distance(transform.position, player.transform.position);
                Debug.Log($"<color=cyan>直接距离检测: 与玩家实际距离 = {distanceToPlayerReal}，攻击范围 = {rangedAttackRadius}</color>");
                
                if (distanceToPlayerReal <= rangedAttackRadius * 1.1f) // 增加一些容错空间
                {
                    PlayerAttackSystem playerHealth = player.GetComponent<PlayerAttackSystem>();
                    if (playerHealth != null)
                    {
                        // 对玩家造成伤害
                        playerHealth.TakeDamage(rangedAttackDamage);
                        
                        // 记录到战斗管理器
                        if (CombatManager.Instance != null)
                        {
                            CombatManager.Instance.LogDamage(gameObject, player, rangedAttackDamage);
                        }
                        
                        Debug.Log($"<color=#8B0000>Boss的远程攻击命中玩家(距离检测)，造成 {rangedAttackDamage} 点伤害！</color>");
                        return; // 已处理完攻击
                    }
                }
            }
        }
        else
        {
            foreach (Collider2D playerCollider in hitPlayers)
            {
                PlayerAttackSystem playerHealth = playerCollider.GetComponent<PlayerAttackSystem>();
                if (playerHealth == null)
                {
                    playerHealth = playerCollider.GetComponentInParent<PlayerAttackSystem>();
                }
                
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
                else
                {
                    Debug.LogWarning($"<color=yellow>检测到碰撞体 {playerCollider.name}，但未找到PlayerAttackSystem组件!</color>");
                }
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
        bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.1f;
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
        
        rb.linearVelocity = Vector2.zero;
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
    
    // 调试功能：测试对玩家造成伤害
    public void TestDamagePlayer(int damageAmount)
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("<color=red>找不到玩家对象，无法测试伤害！</color>");
                return;
            }
        }
        
        PlayerAttackSystem playerHealth = player.GetComponent<PlayerAttackSystem>();
        if (playerHealth == null)
        {
            Debug.LogError("<color=red>玩家对象上找不到PlayerAttackSystem组件！</color>");
            return;
        }
        
        // 测试造成伤害
        playerHealth.TakeDamage(damageAmount);
        Debug.Log($"<color=lime>测试成功：对玩家造成了{damageAmount}点测试伤害</color>");
    }
      // Update方法中添加测试键绑定
    private void OnGUI()
    {
        // 图形化界面已禁用 - 如需重新启用，将下面的return注释掉
        return;
        
        // 创建一个简单的调试按钮，在游戏运行时按下可以测试对玩家造成伤害
        if (GUI.Button(new Rect(Screen.width - 150, 10, 140, 30), "测试Boss伤害"))
        {
            TestDamagePlayer(10);
        }
    }
    
    // 外部只读访问Boss激活状态
    public bool IsActive => isActive;
}

