using UnityEngine;

/// <summary>
/// 简化版敌人存档修复器 - 最基本的修复功能
/// </summary>
public class SimpleEnemySaveFix : MonoBehaviour
{
    [Header("⚡ 快速修复")]
    [Space(10)]
    [Tooltip("勾选这个选项来执行完整的敌人存档系统修复")]
    public bool 执行一键修复 = false;
    
    [Space(10)]
    [Tooltip("勾选这个选项来检查当前系统状态")]
    public bool 检查系统状态 = false;
    
    [Space(10)]
    [Tooltip("启动游戏时自动修复")]
    public bool 启动时自动修复 = true;
    
    [Header("📋 系统信息")]
    [Space(5)]
    [TextArea(4, 8)]
    [SerializeField] private string 当前状态 = "等待检查...";
    
    void Start()
    {
        if (启动时自动修复)
        {
            Debug.Log("🚀 [SimpleEnemySaveFix] 启动时自动修复");
            执行完整修复();
        }
        
        更新状态显示();
    }
    
    void Update()
    {
        if (执行一键修复)
        {
            执行一键修复 = false;
            执行完整修复();
        }
        
        if (检查系统状态)
        {
            检查系统状态 = false;
            检查并更新状态();
        }
    }
    
    /// <summary>
    /// 执行完整修复
    /// </summary>
    [ContextMenu("执行完整修复")]
    public void 执行完整修复()
    {
        Debug.Log("🔧 [SimpleEnemySaveFix] === 开始执行完整修复 ===");
        
        try
        {
            // 1. 确保EnemySaveAdapter存在
            确保存档适配器存在();
            
            // 2. 确保EnemySaveSystemFixer存在
            确保系统修复器存在();
            
            // 3. 修复敌人标签
            修复敌人标签();
            
            // 4. 为所有敌人添加存档组件
            为敌人添加存档组件();
            
            // 5. 重新初始化适配器
            重新初始化适配器();
            
            Debug.Log("✅ [SimpleEnemySaveFix] 完整修复完成！");
            
            // 延迟更新状态
            Invoke(nameof(检查并更新状态), 0.5f);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [SimpleEnemySaveFix] 修复过程出错: {e.Message}");
        }
    }
    
    /// <summary>
    /// 确保存档适配器存在
    /// </summary>
    private void 确保存档适配器存在()
    {
        EnemySaveAdapter adapter = FindFirstObjectByType<EnemySaveAdapter>();
        
        if (adapter == null)
        {
            GameObject adapterObj = new GameObject("EnemySaveAdapter");
            adapter = adapterObj.AddComponent<EnemySaveAdapter>();
            Debug.Log("📦 [SimpleEnemySaveFix] 已创建EnemySaveAdapter");
        }
        else
        {
            Debug.Log("✅ [SimpleEnemySaveFix] EnemySaveAdapter已存在");
        }
    }
    
    /// <summary>
    /// 确保系统修复器存在
    /// </summary>
    private void 确保系统修复器存在()
    {
        EnemySaveSystemFixer fixer = FindFirstObjectByType<EnemySaveSystemFixer>();
        
        if (fixer == null)
        {
            GameObject fixerObj = new GameObject("EnemySaveSystemFixer");
            fixer = fixerObj.AddComponent<EnemySaveSystemFixer>();
            Debug.Log("🔧 [SimpleEnemySaveFix] 已创建EnemySaveSystemFixer");
        }
        else
        {
            Debug.Log("✅ [SimpleEnemySaveFix] EnemySaveSystemFixer已存在");
        }
    }
    
    /// <summary>
    /// 修复敌人标签
    /// </summary>
    private void 修复敌人标签()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        int 修复数量 = 0;
        
        foreach (Enemy enemy in enemies)
        {
            if (!enemy.CompareTag("Enemy"))
            {
                enemy.tag = "Enemy";
                修复数量++;
            }
        }
        
        if (修复数量 > 0)
        {
            Debug.Log($"🏷️ [SimpleEnemySaveFix] 修复了 {修复数量} 个敌人的标签");
        }
        else
        {
            Debug.Log("✅ [SimpleEnemySaveFix] 所有敌人标签都正确");
        }
    }
    
    /// <summary>
    /// 为敌人添加存档组件
    /// </summary>
    private void 为敌人添加存档组件()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        int 添加数量 = 0;
        
        for (int i = 0; i < enemies.Length; i++)
        {
            Enemy enemy = enemies[i];
            
            // 移除现有的SaveableEnemy组件
            SaveableEnemy existingComponent = enemy.GetComponent<SaveableEnemy>();
            if (existingComponent != null)
            {
                DestroyImmediate(existingComponent);
            }
            
            // 添加新的SaveableEnemy组件
            SaveableEnemy newComponent = enemy.gameObject.AddComponent<SaveableEnemy>();
            
            // 生成ID
            string enemyID = 生成敌人ID(enemy, i);
            newComponent.Initialize(enemyID, enemy);
            
            添加数量++;
        }
        
        Debug.Log($"📋 [SimpleEnemySaveFix] 为 {添加数量} 个敌人添加了存档组件");
    }
    
    /// <summary>
    /// 生成敌人ID
    /// </summary>
    private string 生成敌人ID(Enemy enemy, int index)
    {
        Vector3 pos = enemy.transform.position;
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        return $"{sceneName}_Enemy_{index:D3}_{pos.x:F1}_{pos.y:F1}";
    }
    
    /// <summary>
    /// 重新初始化适配器
    /// </summary>
    private void 重新初始化适配器()
    {
        EnemySaveAdapter adapter = FindFirstObjectByType<EnemySaveAdapter>();
        if (adapter != null)
        {
            adapter.InitializeEnemies();
            Debug.Log("🔄 [SimpleEnemySaveFix] 适配器重新初始化完成");
        }
    }
    
    /// <summary>
    /// 检查并更新状态
    /// </summary>
    [ContextMenu("检查系统状态")]
    public void 检查并更新状态()
    {
        Debug.Log("🔍 [SimpleEnemySaveFix] 正在检查系统状态...");
        更新状态显示();
        
        // 打印控制台信息
        bool hasAdapter = FindFirstObjectByType<EnemySaveAdapter>() != null;
        bool hasFixer = FindFirstObjectByType<EnemySaveSystemFixer>() != null;
        bool hasSaveManager = FindFirstObjectByType<SaveManager>() != null;
        int enemyCount = FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length;
        int saveableCount = FindObjectsByType<SaveableEnemy>(FindObjectsSortMode.None).Length;
        
        Debug.Log($"📊 EnemySaveAdapter: {(hasAdapter ? "✅" : "❌")}");
        Debug.Log($"📊 EnemySaveSystemFixer: {(hasFixer ? "✅" : "❌")}");
        Debug.Log($"📊 SaveManager: {(hasSaveManager ? "✅" : "❌")}");
        Debug.Log($"📊 敌人数量: {enemyCount}");
        Debug.Log($"📊 存档组件数量: {saveableCount}");
        
        bool 系统健康 = hasAdapter && hasFixer && enemyCount == saveableCount;
        Debug.Log($"🎯 系统状态: {(系统健康 ? "✅ 良好" : "❌ 需要修复")}");
    }
    
    /// <summary>
    /// 更新状态显示
    /// </summary>
    private void 更新状态显示()
    {
        bool hasAdapter = FindFirstObjectByType<EnemySaveAdapter>() != null;
        bool hasFixer = FindFirstObjectByType<EnemySaveSystemFixer>() != null;
        bool hasSaveManager = FindFirstObjectByType<SaveManager>() != null;
        int enemyCount = FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length;
        int saveableCount = FindObjectsByType<SaveableEnemy>(FindObjectsSortMode.None).Length;
        
        当前状态 = "🔍 系统状态检查结果:\n\n";
        当前状态 += $"📦 存档适配器: {(hasAdapter ? "✅" : "❌")}\n";
        当前状态 += $"🔧 系统修复器: {(hasFixer ? "✅" : "❌")}\n";
        当前状态 += $"💾 存档管理器: {(hasSaveManager ? "✅" : "❌")}\n";
        当前状态 += $"👹 敌人数量: {enemyCount}\n";
        当前状态 += $"📋 存档组件: {saveableCount}\n\n";
        
        bool 系统健康 = hasAdapter && hasFixer && enemyCount == saveableCount;
        当前状态 += 系统健康 ? "🎉 系统状态: 良好" : "⚠️ 系统状态: 需要修复";
        
        if (!系统健康)
        {
            当前状态 += "\n\n💡 建议: 勾选'执行一键修复'来解决问题";
        }
    }
} 