using UnityEngine;

/// <summary>
/// 血量条测试脚本
/// 提供快捷键来测试血量条功能
/// </summary>
public class HealthBarTester : MonoBehaviour
{
    [Header("测试设置")]
    [SerializeField] private bool enableTesting = true; // 启用测试功能
    [SerializeField] private int damageAmount = 10; // 每次伤害量
    [SerializeField] private int healAmount = 15; // 每次治疗量
    
    [Header("快捷键设置")]
    [SerializeField] private KeyCode damageKey = KeyCode.H; // 减血快捷键
    [SerializeField] private KeyCode healKey = KeyCode.J; // 加血快捷键
    [SerializeField] private KeyCode resetKey = KeyCode.R; // 重置血量快捷键
    [SerializeField] private KeyCode maxDamageKey = KeyCode.K; // 大量伤害快捷键
    
    private PlayerAttackSystem playerAttackSystem;
    private HealthBarManager healthBarManager;
    
    private void Start()
    {
        if (!enableTesting) return;
        
        // 查找组件
        FindComponents();
        
        // 显示测试说明
        ShowTestInstructions();
    }
    
    private void Update()
    {
        if (!enableTesting) return;
        
        HandleTestInput();
    }
    
    /// <summary>
    /// 查找必要的组件
    /// </summary>
    private void FindComponents()
    {
        // 查找PlayerAttackSystem
        if (playerAttackSystem == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerAttackSystem = playerObject.GetComponent<PlayerAttackSystem>();
            }
            
            if (playerAttackSystem == null)
            {
                playerAttackSystem = FindObjectOfType<PlayerAttackSystem>();
            }
        }
        
        // 查找HealthBarManager
        if (healthBarManager == null)
        {
            healthBarManager = HealthBarManager.Instance;
            if (healthBarManager == null)
            {
                healthBarManager = FindObjectOfType<HealthBarManager>();
            }
        }
        
        // 验证组件
        if (playerAttackSystem == null)
        {
            Debug.LogError("[HealthBarTester] 未找到PlayerAttackSystem组件！");
        }
        
        if (healthBarManager == null)
        {
            Debug.LogWarning("[HealthBarTester] 未找到HealthBarManager组件，某些功能可能无法使用。");
        }
    }
    
    /// <summary>
    /// 显示测试说明
    /// </summary>
    private void ShowTestInstructions()
    {
        Debug.Log("=== 血量条测试脚本激活 ===");
        Debug.Log($"按 [{damageKey}] 键减少血量 ({damageAmount} 点)");
        Debug.Log($"按 [{healKey}] 键恢复血量 ({healAmount} 点)");
        Debug.Log($"按 [{resetKey}] 键重置血量到满血");
        Debug.Log($"按 [{maxDamageKey}] 键造成大量伤害 (50 点)");
        Debug.Log("========================");
    }
    
    /// <summary>
    /// 处理测试输入
    /// </summary>
    private void HandleTestInput()
    {
        // 减血测试
        if (Input.GetKeyDown(damageKey))
        {
            TestDamage();
        }
        
        // 加血测试
        if (Input.GetKeyDown(healKey))
        {
            TestHeal();
        }
        
        // 重置血量测试
        if (Input.GetKeyDown(resetKey))
        {
            TestResetHealth();
        }
        
        // 大量伤害测试
        if (Input.GetKeyDown(maxDamageKey))
        {
            TestMaxDamage();
        }
    }
    
    /// <summary>
    /// 测试伤害功能
    /// </summary>
    private void TestDamage()
    {
        if (playerAttackSystem == null) return;
        
        int currentHealth = playerAttackSystem.Health;
        playerAttackSystem.TakeDamage(damageAmount);
        
        Debug.Log($"[测试] 造成 {damageAmount} 点伤害，血量：{currentHealth} → {playerAttackSystem.Health}");
    }
    
    /// <summary>
    /// 测试治疗功能
    /// </summary>
    private void TestHeal()
    {
        if (playerAttackSystem == null) return;
        
        int currentHealth = playerAttackSystem.Health;
        playerAttackSystem.Heal(healAmount);
        
        Debug.Log($"[测试] 恢复 {healAmount} 点血量，血量：{currentHealth} → {playerAttackSystem.Health}");
    }
    
    /// <summary>
    /// 测试重置血量功能
    /// </summary>
    private void TestResetHealth()
    {
        if (playerAttackSystem == null) return;
        
        int currentHealth = playerAttackSystem.Health;
        playerAttackSystem.HealToFull();
        
        Debug.Log($"[测试] 重置血量到满血，血量：{currentHealth} → {playerAttackSystem.Health}");
        
        // 如果有血量条管理器，也重置血量条显示
        if (healthBarManager != null)
        {
            healthBarManager.ResetHealthBar();
        }
    }
    
    /// <summary>
    /// 测试大量伤害功能
    /// </summary>
    private void TestMaxDamage()
    {
        if (playerAttackSystem == null) return;
        
        int currentHealth = playerAttackSystem.Health;
        playerAttackSystem.TakeDamage(50);
        
        Debug.Log($"[测试] 造成大量伤害 (50点)，血量：{currentHealth} → {playerAttackSystem.Health}");
    }
    
    /// <summary>
    /// 在屏幕上显示测试信息
    /// </summary>
    private void OnGUI()
    {
        if (!enableTesting) return;
        
        // 显示测试面板
        GUILayout.BeginArea(new Rect(10, Screen.height - 150, 300, 140));
        GUILayout.Label("=== 血量条测试面板 ===");
        
        if (playerAttackSystem != null)
        {
            GUILayout.Label($"当前血量: {playerAttackSystem.Health}/{playerAttackSystem.MaxHealth}");
            GUILayout.Label($"血量百分比: {(float)playerAttackSystem.Health / playerAttackSystem.MaxHealth * 100:F1}%");
        }
        else
        {
            GUILayout.Label("未找到PlayerAttackSystem!");
        }
        
        GUILayout.Space(5);
        GUILayout.Label($"[{damageKey}] 减血  [{healKey}] 加血");
        GUILayout.Label($"[{resetKey}] 重置  [{maxDamageKey}] 大伤害");
        
        GUILayout.EndArea();
    }
    
    /// <summary>
    /// 设置测试参数
    /// </summary>
    /// <param name="damage">伤害量</param>
    /// <param name="heal">治疗量</param>
    public void SetTestAmounts(int damage, int heal)
    {
        damageAmount = damage;
        healAmount = heal;
    }
    
    /// <summary>
    /// 启用或禁用测试功能
    /// </summary>
    /// <param name="enabled">是否启用</param>
    public void SetTestingEnabled(bool enabled)
    {
        enableTesting = enabled;
        
        if (enabled)
        {
            ShowTestInstructions();
        }
    }
} 