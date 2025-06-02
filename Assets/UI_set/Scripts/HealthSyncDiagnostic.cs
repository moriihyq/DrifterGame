using UnityEngine;

/// <summary>
/// 血量同步诊断脚本
/// 用于调试存档加载后血量不同步的问题
/// </summary>
public class HealthSyncDiagnostic : MonoBehaviour
{
    [Header("诊断设置")]
    [SerializeField] private bool enableDiagnostic = true;
    [SerializeField] private float checkInterval = 1f; // 检查间隔
    
    [Header("快捷键")]
    [SerializeField] private KeyCode diagnosticKey = KeyCode.F12; // 手动诊断快捷键
    
    private PlayerAttackSystem playerAttackSystem;
    private HealthBarManager healthBarManager;
    private HealthBarUI healthBarUI;
    
    private void Start()
    {
        if (!enableDiagnostic) return;
        
        // 查找组件
        FindComponents();
        
        // 开始定期检查
        InvokeRepeating(nameof(PerformDiagnostic), 1f, checkInterval);
        
        Debug.Log("🔧 血量同步诊断工具已启动");
        Debug.Log($"按 [{diagnosticKey}] 进行手动诊断");
    }
    
    private void Update()
    {
        if (!enableDiagnostic) return;
        
        if (Input.GetKeyDown(diagnosticKey))
        {
            PerformDetailedDiagnostic();
        }
    }
    
    /// <summary>
    /// 查找相关组件
    /// </summary>
    private void FindComponents()
    {
        // 查找PlayerAttackSystem
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerAttackSystem = playerObject.GetComponent<PlayerAttackSystem>();
        }
        
        if (playerAttackSystem == null)
        {
            playerAttackSystem = FindFirstObjectByType<PlayerAttackSystem>();
        }
        
        // 查找HealthBarManager
        healthBarManager = HealthBarManager.Instance;
        if (healthBarManager == null)
        {
            healthBarManager = FindFirstObjectByType<HealthBarManager>();
        }
        
        // 查找HealthBarUI
        healthBarUI = FindFirstObjectByType<HealthBarUI>();
    }
    
    /// <summary>
    /// 执行定期诊断
    /// </summary>
    private void PerformDiagnostic()
    {
        if (playerAttackSystem == null || healthBarManager == null) return;
        
        int playerHealth = playerAttackSystem.Health;
        int playerMaxHealth = playerAttackSystem.MaxHealth;
        float healthPercentage = healthBarManager.GetHealthPercentage();
        
        // 检查是否有不同步问题
        float expectedPercentage = (float)playerHealth / playerMaxHealth;
        float difference = Mathf.Abs(healthPercentage - expectedPercentage);
        
        if (difference > 0.01f) // 如果差异超过1%
        {
            Debug.LogWarning($"⚠️ 血量同步问题检测到!");
            Debug.LogWarning($"PlayerAttackSystem: {playerHealth}/{playerMaxHealth} ({expectedPercentage * 100:F1}%)");
            Debug.LogWarning($"血量条显示: {healthPercentage * 100:F1}%");
            Debug.LogWarning($"差异: {difference * 100:F1}%");
            
            // 尝试修复
            AttemptFix();
        }
    }
    
    /// <summary>
    /// 执行详细诊断
    /// </summary>
    private void PerformDetailedDiagnostic()
    {
        Debug.Log("🔧 === 详细血量同步诊断 ===");
        
        // 重新查找组件
        FindComponents();
        
        // PlayerAttackSystem 状态
        if (playerAttackSystem != null)
        {
            Debug.Log($"✅ PlayerAttackSystem: {playerAttackSystem.Health}/{playerAttackSystem.MaxHealth}");
            Debug.Log($"   位置: {playerAttackSystem.transform.position}");
            Debug.Log($"   GameObject: {playerAttackSystem.gameObject.name}");
        }
        else
        {
            Debug.LogError("❌ PlayerAttackSystem 未找到!");
        }
        
        // HealthBarManager 状态
        if (healthBarManager != null)
        {
            Debug.Log($"✅ HealthBarManager 存在");
            var connectedPlayer = healthBarManager.GetConnectedPlayerSystem();
            if (connectedPlayer != null)
            {
                Debug.Log($"   连接的PlayerAttackSystem: {connectedPlayer.gameObject.name}");
                Debug.Log($"   连接的血量: {connectedPlayer.Health}/{connectedPlayer.MaxHealth}");
                Debug.Log($"   是否为同一对象: {connectedPlayer == playerAttackSystem}");
            }
            else
            {
                Debug.LogWarning("⚠️ HealthBarManager 未连接到PlayerAttackSystem");
            }
            
            float percentage = healthBarManager.GetHealthPercentage();
            Debug.Log($"   显示百分比: {percentage * 100:F1}%");
        }
        else
        {
            Debug.LogError("❌ HealthBarManager 未找到!");
        }
        
        // HealthBarUI 状态
        if (healthBarUI != null)
        {
            Debug.Log($"✅ HealthBarUI 存在");
            float uiPercentage = healthBarUI.GetCurrentPercentage();
            Debug.Log($"   UI显示百分比: {uiPercentage * 100:F1}%");
        }
        else
        {
            Debug.LogError("❌ HealthBarUI 未找到!");
        }
        
        Debug.Log("🔧 === 诊断完成 ===");
    }
    
    /// <summary>
    /// 尝试修复同步问题
    /// </summary>
    private void AttemptFix()
    {
        Debug.Log("🔧 尝试修复血量同步问题...");
        
        if (healthBarManager != null && playerAttackSystem != null)
        {
            // 强制重新连接
            healthBarManager.ForceReconnectPlayerSystem();
            
            // 手动设置连接
            healthBarManager.SetPlayerAttackSystem(playerAttackSystem);
            
            // 强制更新显示
            healthBarManager.UpdateHealthDisplay(true);
            
            Debug.Log("🔧 修复尝试完成");
        }
    }
    
    /// <summary>
    /// 在屏幕上显示诊断信息
    /// </summary>
    private void OnGUI()
    {
        // 图形化界面已禁用 - 如需重新启用，将下面的return注释掉
        return;
        
        if (!enableDiagnostic) return;
        
        GUILayout.BeginArea(new Rect(Screen.width - 350, 10, 340, 150));
        GUILayout.Label("=== 血量同步诊断 ===");
        
        if (playerAttackSystem != null)
        {
            GUILayout.Label($"PlayerAttackSystem: {playerAttackSystem.Health}/{playerAttackSystem.MaxHealth}");
        }
        else
        {
            GUILayout.Label("PlayerAttackSystem: 未找到");
        }
        
        if (healthBarManager != null)
        {
            float percentage = healthBarManager.GetHealthPercentage();
            GUILayout.Label($"血量条显示: {percentage * 100:F1}%");
        }
        else
        {
            GUILayout.Label("HealthBarManager: 未找到");
        }
        
        GUILayout.Label($"[{diagnosticKey}] 详细诊断");
        
        if (GUILayout.Button("手动修复同步"))
        {
            AttemptFix();
        }
        
        GUILayout.EndArea();
    }
    
    /// <summary>
    /// 启用或禁用诊断
    /// </summary>
    public void SetDiagnosticEnabled(bool enabled)
    {
        enableDiagnostic = enabled;
        
        if (enabled)
        {
            Debug.Log("🔧 血量同步诊断已启用");
        }
        else
        {
            Debug.Log("🔧 血量同步诊断已禁用");
        }
    }
} 