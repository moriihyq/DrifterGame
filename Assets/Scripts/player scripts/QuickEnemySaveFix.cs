using UnityEngine;

/// <summary>
/// 一键敌人存档修复 - 快速设置所有必要组件
/// </summary>
public class QuickEnemySaveFix : MonoBehaviour
{
    [Header("🚀 一键修复")]
    [Space(5)]
    [Tooltip("勾选此项将执行完整的敌人存档系统修复")]
    [SerializeField] private bool executeOneClickFix;
    
    [Space(10)]
    [Header("🔍 状态检查")]
    [Space(5)]
    [Tooltip("勾选此项将检查当前系统状态")]
    [SerializeField] private bool executeStatusCheck;
    
    [Space(10)]
    [Header("🧪 功能测试")]
    [Space(5)]
    [Tooltip("勾选此项将测试存档加载功能")]
    [SerializeField] private bool executeSaveLoadTest;
    
    [Header("修复选项")]
    [SerializeField] private bool createEnemySaveAdapter = true;
    [SerializeField] private bool createSystemFixer = true;
    [SerializeField] private bool fixEnemyTags = true;
    [SerializeField] private bool enableDebugLogs = true;
    
    [Header("🔄 自动执行")]
    [Space(5)]
    [Tooltip("启动时自动执行一键修复")]
    [SerializeField] private bool autoFixOnStart = false;
    
    [Space(10)]
    [Header("📊 当前状态")]
    [Space(5)]
    [SerializeField] [TextArea(3, 6)] private string currentSystemStatus = "点击'执行状态检查'查看当前状态...";
    
    private void Start()
    {
        if (autoFixOnStart)
        {
            Debug.Log("🚀 [QuickEnemySaveFix] 自动执行启动修复");
            PerformOneClickFix();
        }
        
        // 更新状态显示
        UpdateStatusDisplay();
    }
    
    private void OnValidate()
    {
        if (executeOneClickFix)
        {
            executeOneClickFix = false;
            PerformOneClickFix();
        }
        
        if (executeStatusCheck)
        {
            executeStatusCheck = false;
            CheckSystemStatus();
        }
        
        if (executeSaveLoadTest)
        {
            executeSaveLoadTest = false;
            TestSaveLoadFunctionality();
        }
    }
    
    /// <summary>
    /// 执行一键修复
    /// </summary>
    [ContextMenu("一键修复")]
    public void PerformOneClickFix()
    {
        Debug.Log("🚀 [QuickEnemySaveFix] 开始一键修复敌人存档系统");
        
        try
        {
            // 1. 创建或确保EnemySaveAdapter存在
            if (createEnemySaveAdapter)
            {
                EnsureEnemySaveAdapter();
            }
            
            // 2. 创建或确保EnemySaveSystemFixer存在
            if (createSystemFixer)
            {
                EnsureSystemFixer();
            }
            
            // 3. 修复敌人标签
            if (fixEnemyTags)
            {
                FixEnemyTags();
            }
            
            // 4. 等待一帧后进行系统验证
            Invoke(nameof(DelayedSystemValidation), 0.1f);
            
            // 5. 更新状态显示
            UpdateStatusDisplay();
            
            Debug.Log("✅ [QuickEnemySaveFix] 一键修复完成！");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [QuickEnemySaveFix] 修复过程中出现错误: {e.Message}");
        }
    }
    
    /// <summary>
    /// 确保EnemySaveAdapter存在
    /// </summary>
    private void EnsureEnemySaveAdapter()
    {
        EnemySaveAdapter existingAdapter = FindFirstObjectByType<EnemySaveAdapter>();
        
        if (existingAdapter == null)
        {
            GameObject adapterObj = new GameObject("EnemySaveAdapter");
            existingAdapter = adapterObj.AddComponent<EnemySaveAdapter>();
            
            Debug.Log("📦 [QuickEnemySaveFix] 已创建EnemySaveAdapter");
        }
        else
        {
            Debug.Log("✅ [QuickEnemySaveFix] EnemySaveAdapter已存在");
        }
    }
    
    /// <summary>
    /// 确保EnemySaveSystemFixer存在
    /// </summary>
    private void EnsureSystemFixer()
    {
        EnemySaveSystemFixer existingFixer = FindFirstObjectByType<EnemySaveSystemFixer>();
        
        if (existingFixer == null)
        {
            GameObject fixerObj = new GameObject("EnemySaveSystemFixer");
            existingFixer = fixerObj.AddComponent<EnemySaveSystemFixer>();
            
            Debug.Log("🔧 [QuickEnemySaveFix] 已创建EnemySaveSystemFixer");
        }
        else
        {
            Debug.Log("✅ [QuickEnemySaveFix] EnemySaveSystemFixer已存在");
        }
    }
    
    /// <summary>
    /// 修复敌人标签
    /// </summary>
    private void FixEnemyTags()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        int fixedCount = 0;
        
        foreach (Enemy enemy in enemies)
        {
            if (!enemy.CompareTag("Enemy"))
            {
                enemy.tag = "Enemy";
                fixedCount++;
            }
        }
        
        if (fixedCount > 0)
        {
            Debug.Log($"🏷️ [QuickEnemySaveFix] 修复了 {fixedCount} 个敌人的标签");
        }
        else
        {
            Debug.Log("✅ [QuickEnemySaveFix] 所有敌人标签都正确");
        }
    }
    
    /// <summary>
    /// 延迟系统验证
    /// </summary>
    private void DelayedSystemValidation()
    {
        CheckSystemStatus();
        
        // 触发自动修复
        EnemySaveSystemFixer fixer = FindFirstObjectByType<EnemySaveSystemFixer>();
        if (fixer != null)
        {
            fixer.TriggerFullRebuild();
            Debug.Log("🔄 [QuickEnemySaveFix] 已触发系统完全重建");
        }
    }
    
    /// <summary>
    /// 检查系统状态
    /// </summary>
    [ContextMenu("检查系统状态")]
    public void CheckSystemStatus()
    {
        Debug.Log("🔍 [QuickEnemySaveFix] === 系统状态检查 ===");
        
        // 检查基本组件
        bool hasEnemySaveAdapter = FindFirstObjectByType<EnemySaveAdapter>() != null;
        bool hasSystemFixer = FindFirstObjectByType<EnemySaveSystemFixer>() != null;
        bool hasSaveManager = FindFirstObjectByType<SaveManager>() != null;
        
        // 检查敌人状态
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        SaveableEnemy[] saveableEnemies = FindObjectsByType<SaveableEnemy>(FindObjectsSortMode.None);
        
        int enemiesWithCorrectTag = 0;
        foreach (Enemy enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
                enemiesWithCorrectTag++;
        }
        
        // 输出状态报告
        Debug.Log($"📊 [QuickEnemySaveFix] EnemySaveAdapter: {(hasEnemySaveAdapter ? "✅" : "❌")}");
        Debug.Log($"📊 [QuickEnemySaveFix] EnemySaveSystemFixer: {(hasSystemFixer ? "✅" : "❌")}");
        Debug.Log($"📊 [QuickEnemySaveFix] SaveManager: {(hasSaveManager ? "✅" : "❌")}");
        Debug.Log($"📊 [QuickEnemySaveFix] Enemy组件: {enemies.Length}");
        Debug.Log($"📊 [QuickEnemySaveFix] SaveableEnemy组件: {saveableEnemies.Length}");
        Debug.Log($"📊 [QuickEnemySaveFix] 正确标签的敌人: {enemiesWithCorrectTag}/{enemies.Length}");
        
        // 系统健康状态评估
        bool systemHealthy = hasEnemySaveAdapter && hasSystemFixer && 
                           enemies.Length == saveableEnemies.Length && 
                           enemiesWithCorrectTag == enemies.Length;
        
        if (systemHealthy)
        {
            Debug.Log("🎉 [QuickEnemySaveFix] 系统状态良好！");
        }
        else
        {
            Debug.LogWarning("⚠️ [QuickEnemySaveFix] 系统需要修复！建议运行一键修复");
        }
        
        // 详细诊断
        if (!hasEnemySaveAdapter) Debug.LogWarning("🚨 缺少EnemySaveAdapter");
        if (!hasSystemFixer) Debug.LogWarning("🚨 缺少EnemySaveSystemFixer");
        if (!hasSaveManager) Debug.LogWarning("🚨 缺少SaveManager");
        if (enemies.Length != saveableEnemies.Length) Debug.LogWarning("🚨 敌人组件数量不匹配");
        if (enemiesWithCorrectTag != enemies.Length) Debug.LogWarning("🚨 部分敌人标签不正确");
        
        // 更新状态显示
        UpdateStatusDisplay();
    }
    
    /// <summary>
    /// 更新Inspector中的状态显示
    /// </summary>
    private void UpdateStatusDisplay()
    {
        bool hasAdapter = FindFirstObjectByType<EnemySaveAdapter>() != null;
        bool hasFixer = FindFirstObjectByType<EnemySaveSystemFixer>() != null;
        bool hasSaveManager = FindFirstObjectByType<SaveManager>() != null;
        int enemyCount = FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length;
        int saveableCount = FindObjectsByType<SaveableEnemy>(FindObjectsSortMode.None).Length;
        
        string status = $"🔍 系统状态检查结果:\n\n";
        status += $"📦 EnemySaveAdapter: {(hasAdapter ? "✅ 已存在" : "❌ 缺失")}\n";
        status += $"🔧 EnemySaveSystemFixer: {(hasFixer ? "✅ 已存在" : "❌ 缺失")}\n";
        status += $"💾 SaveManager: {(hasSaveManager ? "✅ 已存在" : "❌ 缺失")}\n";
        status += $"👹 Enemy组件: {enemyCount}\n";
        status += $"📋 SaveableEnemy组件: {saveableCount}\n\n";
        
        bool systemHealthy = hasAdapter && hasFixer && enemyCount == saveableCount;
        status += systemHealthy ? "🎉 系统状态: 良好" : "⚠️ 系统状态: 需要修复";
        
        currentSystemStatus = status;
    }
    
    /// <summary>
    /// 测试存档加载功能
    /// </summary>
    [ContextMenu("测试存档功能")]
    public void TestSaveLoadFunctionality()
    {
        Debug.Log("🧪 [QuickEnemySaveFix] 开始测试存档功能");
        
        SaveManager saveManager = FindFirstObjectByType<SaveManager>();
        if (saveManager == null)
        {
            Debug.LogError("❌ [QuickEnemySaveFix] 未找到SaveManager，无法测试");
            return;
        }
        
        EnemySaveAdapter adapter = FindFirstObjectByType<EnemySaveAdapter>();
        if (adapter == null)
        {
            Debug.LogError("❌ [QuickEnemySaveFix] 未找到EnemySaveAdapter，无法测试");
            return;
        }
        
        try
        {
            // 收集当前敌人数据
            var enemyData = adapter.CollectEnemyData();
            Debug.Log($"📦 [QuickEnemySaveFix] 收集到 {enemyData.Count} 个敌人的数据");
            
            // 测试保存（使用临时槽位）
            saveManager.SaveGame(2); // 使用槽位2进行测试
            Debug.Log("💾 [QuickEnemySaveFix] 测试保存完成");
            
            // 模拟加载测试
            var loadedData = saveManager.LoadGame(2);
            if (loadedData != null)
            {
                Debug.Log($"📂 [QuickEnemySaveFix] 测试加载成功，敌人数据: {loadedData.enemiesData.Count}");
                Debug.Log("✅ [QuickEnemySaveFix] 存档功能测试通过！");
            }
            else
            {
                Debug.LogWarning("⚠️ [QuickEnemySaveFix] 加载测试失败");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [QuickEnemySaveFix] 存档功能测试失败: {e.Message}");
        }
    }
    
    /// <summary>
    /// 获取快速状态摘要
    /// </summary>
    public string GetQuickStatusSummary()
    {
        bool hasAdapter = FindFirstObjectByType<EnemySaveAdapter>() != null;
        bool hasFixer = FindFirstObjectByType<EnemySaveSystemFixer>() != null;
        int enemyCount = FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length;
        int saveableCount = FindObjectsByType<SaveableEnemy>(FindObjectsSortMode.None).Length;
        
        return $"适配器:{(hasAdapter ? "✅" : "❌")} 修复器:{(hasFixer ? "✅" : "❌")} 敌人:{enemyCount} 存档组件:{saveableCount}";
    }
}

 