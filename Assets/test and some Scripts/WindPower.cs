using System.Collections;
using UnityEngine;

public class WindPower : MonoBehaviour
{
    [Header("风力技能设置")]
    [SerializeField] private float windPowerCooldown = 15f; // 风力技能冷却时间(秒)
    [SerializeField] private float windPowerDuration = 5f; // 风力技能持续时间(秒)
    [SerializeField] private float speedMultiplier = 1.5f; // 速度提升倍数
    [SerializeField] private float damageMultiplier = 1.5f; // 伤害提升倍数
    
    [Header("音效设置")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip windPowerSound;
    
    // 私有变量
    private bool isWindPowerActive = false; // 风力技能是否激活
    private bool isWindPowerOnCooldown = false; // 风力技能是否在冷却
    private float currentCooldownTime = 0f; // 当前剩余冷却时间
    
    // 组件引用
    private Animator playerAnimator;
    private PlayerRun playerRun;
    private PlayerAttackSystem playerAttackSystem;
    
    // 缓存原始值
    private float originalWalkSpeed;
    private float originalRunSpeed;
    private int originalAttackDamage;
    
    // 动画触发器常量
    private const string WINDPOWER_TRIGGER = "windpower";
    
    // 公共属性，供其他脚本读取状态
    public bool IsWindPowerActive => isWindPowerActive;
    public bool IsWindPowerOnCooldown => isWindPowerOnCooldown;
    public float CurrentCooldownTime => currentCooldownTime;
    public float CurrentDuration { get; private set; } = 0f;
    
    void Start()
    {
        // 获取必要组件
        playerAnimator = GetComponent<Animator>();
        playerRun = GetComponent<PlayerRun>();
        playerAttackSystem = GetComponent<PlayerAttackSystem>();
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        if (playerAnimator == null)
        {
            Debug.LogError("WindPower: 未找到Animator组件！");
        }
        
        if (playerRun == null)
        {
            Debug.LogError("WindPower: 未找到PlayerRun组件！");
        }
        
        if (playerAttackSystem == null)
        {
            Debug.LogError("WindPower: 未找到PlayerAttackSystem组件！");
        }
        
        // 缓存原始值
        if (playerRun != null)
        {
            originalWalkSpeed = playerRun.walkSpeed;
            originalRunSpeed = playerRun.runSpeed;
        }
        
        if (playerAttackSystem != null)
        {
            originalAttackDamage = playerAttackSystem.AttackDamage;
        }
    }
    
    void Update()
    {
        HandleInput();
        UpdateCooldown();
    }
    
    void HandleInput()
    {
        // 检测R键输入，触发风力技能
        if (Input.GetKeyDown(KeyCode.R))
        {
            TryActivateWindPower();
        }
    }
    
    void TryActivateWindPower()
    {
        // 如果技能在冷却中，不能使用
        if (isWindPowerOnCooldown)
        {
            Debug.Log($"风力技能还在冷却中，剩余时间: {currentCooldownTime:F1}秒");
            return;
        }
        
        // 如果技能已激活，不能重复使用
        if (isWindPowerActive)
        {
            Debug.Log("风力技能已处于激活状态！");
            return;
        }
        
        // 激活技能
        ActivateWindPower();
    }
    
    void ActivateWindPower()
    {
        Debug.Log("激活风力技能！");
        
        // 播放风力技能动画
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger(WINDPOWER_TRIGGER);
            Debug.Log($"播放风力技能动画: {WINDPOWER_TRIGGER}");
        }
        else
        {
            Debug.LogError("无法播放风力技能动画，Animator组件不存在！");
        }
        
        // 播放音效
        if (audioSource != null && windPowerSound != null)
        {
            audioSource.PlayOneShot(windPowerSound);
        }
        
        // 提升移动速度
        if (playerRun != null)
        {
            playerRun.walkSpeed *= speedMultiplier;
            playerRun.runSpeed *= speedMultiplier;
            Debug.Log($"移动速度提升至 {speedMultiplier} 倍！步行: {playerRun.walkSpeed}, 奔跑: {playerRun.runSpeed}");
        }
        
        // 提升攻击伤害
        if (playerAttackSystem != null)
        {
            playerAttackSystem.AttackDamage = (int)(originalAttackDamage * damageMultiplier);
            Debug.Log($"攻击伤害提升至 {damageMultiplier} 倍！当前攻击伤害: {playerAttackSystem.AttackDamage}");
        }
        
        // 开始技能持续时间协程
        StartCoroutine(WindPowerDurationCoroutine());
        
        // 设置技能状态
        isWindPowerActive = true;
        
        // 开始冷却
        StartCooldown();
    }
    
    IEnumerator WindPowerDurationCoroutine()
    {
        CurrentDuration = windPowerDuration;
        
        while (CurrentDuration > 0)
        {
            yield return new WaitForSeconds(0.1f);
            CurrentDuration -= 0.1f;
        }
        
        // 技能持续时间结束，恢复原始状态
        DeactivateWindPower();
        
        CurrentDuration = 0f;
    }
    
    void DeactivateWindPower()
    {
        // 恢复原始移动速度
        if (playerRun != null)
        {
            playerRun.walkSpeed = originalWalkSpeed;
            playerRun.runSpeed = originalRunSpeed;
            Debug.Log($"移动速度恢复正常！步行: {playerRun.walkSpeed}, 奔跑: {playerRun.runSpeed}");
        }
        
        // 恢复原始攻击伤害
        if (playerAttackSystem != null)
        {
            playerAttackSystem.AttackDamage = originalAttackDamage;
            Debug.Log($"攻击伤害恢复正常！当前攻击伤害: {playerAttackSystem.AttackDamage}");
        }
        
        isWindPowerActive = false;
        Debug.Log("风力技能效果结束！");
    }
    
    void StartCooldown()
    {
        isWindPowerOnCooldown = true;
        currentCooldownTime = windPowerCooldown;
        
        Debug.Log($"风力技能进入冷却，冷却时间: {windPowerCooldown} 秒");
    }
    
    void UpdateCooldown()
    {
        if (isWindPowerOnCooldown)
        {
            // 更新冷却时间
            currentCooldownTime -= Time.deltaTime;
            
            // 冷却结束
            if (currentCooldownTime <= 0)
            {
                isWindPowerOnCooldown = false;
                currentCooldownTime = 0f;
                Debug.Log("风力技能冷却完成，可以再次使用！");
            }
        }
    }
    
    // 用于调试的可视化
    void OnGUI()
    {
        // 简单的调试UI，在游戏窗口显示技能状态信息
        GUI.Label(new Rect(10, 10, 200, 20), $"风力技能状态: {(isWindPowerActive ? "激活中" : "未激活")}");
        if (isWindPowerActive)
        {
            GUI.Label(new Rect(10, 30, 200, 20), $"剩余持续时间: {CurrentDuration:F1}秒");
        }
        
        if (isWindPowerOnCooldown)
        {
            GUI.Label(new Rect(10, 50, 200, 20), $"冷却时间: {currentCooldownTime:F1}秒");
        }
        else
        {
            GUI.Label(new Rect(10, 50, 200, 20), "技能就绪！按R使用");
        }
    }
    
    // 可视化技能范围 (仅在编辑器调试用)
    void OnDrawGizmosSelected()
    {
        if (isWindPowerActive)
        {
            // 绘制一个风力效果的可视化
            Gizmos.color = new Color(0, 0.8f, 1f, 0.2f);
            Gizmos.DrawSphere(transform.position, 1f);
            
            // 绘制风的方向
            float playerDirection = transform.localScale.x > 0 ? 1f : -1f;
            Vector3 startPos = transform.position;
            
            for (int i = 0; i < 3; i++)
            {
                float offset = i * 0.5f;
                Vector3 endPos = startPos + new Vector3(playerDirection * 2f + offset, 0, 0);
                Gizmos.DrawLine(startPos, endPos);
            }
        }
    }
}
