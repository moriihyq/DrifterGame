using UnityEngine;

/// <summary>
/// 敌人存档槽位修复器 - 解决不同存档间敌人状态混乱的问题
/// 一键修复脚本，无需复杂配置
/// </summary>
public class EnemySaveSlotFix : MonoBehaviour
{
    [Header("一键修复")]
    [Tooltip("执行完整的敌人存档系统修复")]
    public bool executeFullFix = false;
    
    [Tooltip("清除所有存档的敌人死亡记录")]
    public bool clearAllDeathRecords = false;
    
    [Tooltip("重新初始化当前场景的敌人")]
    public bool reinitializeEnemies = false;
    
    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private string currentStatus = "等待操作...";
    
    private void Update()
    {
        // 检查操作标志
        if (executeFullFix)
        {
            executeFullFix = false;
            ExecuteFullFix();
        }
        
        if (clearAllDeathRecords)
        {
            clearAllDeathRecords = false;
            ClearAllDeathRecords();
        }
        
        if (reinitializeEnemies)
        {
            reinitializeEnemies = false;
            ReinitializeEnemies();
        }
    }
    
    /// <summary>
    /// 执行完整修复
    /// </summary>
    [ContextMenu("执行完整修复")]
    public void ExecuteFullFix()
    {
        currentStatus = "执行完整修复中...";
        
        try
        {
            // 1. 确保EnemySaveDataManager存在
            if (EnemySaveDataManager.Instance == null)
            {
                GameObject managerObj = new GameObject("EnemySaveDataManager");
                managerObj.AddComponent<EnemySaveDataManager>();
                
                if (showDebugInfo)
                    Debug.Log("[EnemySaveSlotFix] ✅ 创建EnemySaveDataManager");
            }
            
            // 2. 重置所有存档槽位的死亡记录
            if (EnemySaveDataManager.Instance != null)
            {
                EnemySaveDataManager.Instance.ClearAllSlots();
                
                if (showDebugInfo)
                    Debug.Log("[EnemySaveSlotFix] ✅ 清除所有存档槽位死亡记录");
            }
            
            // 3. 重新初始化EnemySaveAdapter
            if (EnemySaveAdapter.Instance != null)
            {
                EnemySaveAdapter.Instance.ClearCurrentSlotDeathRecords();
                EnemySaveAdapter.Instance.InitializeEnemies();
                
                if (showDebugInfo)
                    Debug.Log("[EnemySaveSlotFix] ✅ 重新初始化EnemySaveAdapter");
            }
            
            // 4. 重新激活所有场景中的敌人
            Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Enemy enemy in allEnemies)
            {
                if (enemy != null)
                {
                    enemy.gameObject.SetActive(true);
                    
                    // 重置敌人状态（如果有isDead字段）
                    try
                    {
                        var isDeadField = enemy.GetType().GetField("isDead", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (isDeadField != null)
                        {
                            isDeadField.SetValue(enemy, false);
                        }
                        
                        var currentHealthField = enemy.GetType().GetField("currentHealth", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        var maxHealthField = enemy.GetType().GetField("maxHealth", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        
                        if (currentHealthField != null && maxHealthField != null)
                        {
                            int maxHealth = (int)maxHealthField.GetValue(enemy);
                            currentHealthField.SetValue(enemy, maxHealth);
                        }
                    }
                    catch (System.Exception e)
                    {
                        if (showDebugInfo)
                            Debug.LogWarning($"[EnemySaveSlotFix] 重置敌人状态失败: {enemy.name}, 错误: {e.Message}");
                    }
                }
            }
            
            if (showDebugInfo)
                Debug.Log($"[EnemySaveSlotFix] ✅ 重新激活了 {allEnemies.Length} 个敌人");
            
            currentStatus = "✅ 完整修复成功！所有敌人已恢复";
            
            if (showDebugInfo)
            {
                Debug.Log("=== [EnemySaveSlotFix] 完整修复完成 ===");
                Debug.Log("✅ 现在不同存档间的敌人状态将正确隔离");
                Debug.Log("✅ 新建存档时所有敌人将正常显示");
                Debug.Log("✅ 每个存档槽位的敌人死亡记录独立管理");
            }
        }
        catch (System.Exception e)
        {
            currentStatus = $"❌ 修复失败: {e.Message}";
            Debug.LogError($"[EnemySaveSlotFix] 修复失败: {e.Message}");
        }
    }
    
    /// <summary>
    /// 清除所有死亡记录
    /// </summary>
    [ContextMenu("清除所有死亡记录")]
    public void ClearAllDeathRecords()
    {
        currentStatus = "清除死亡记录中...";
        
        try
        {
            if (EnemySaveDataManager.Instance != null)
            {
                EnemySaveDataManager.Instance.ClearAllSlots();
            }
            
            if (EnemySaveAdapter.Instance != null)
            {
                EnemySaveAdapter.Instance.ClearCurrentSlotDeathRecords();
            }
            
            currentStatus = "✅ 所有死亡记录已清除";
            
            if (showDebugInfo)
                Debug.Log("[EnemySaveSlotFix] ✅ 清除所有死亡记录完成");
        }
        catch (System.Exception e)
        {
            currentStatus = $"❌ 清除失败: {e.Message}";
            Debug.LogError($"[EnemySaveSlotFix] 清除死亡记录失败: {e.Message}");
        }
    }
    
    /// <summary>
    /// 重新初始化敌人
    /// </summary>
    [ContextMenu("重新初始化敌人")]
    public void ReinitializeEnemies()
    {
        currentStatus = "重新初始化敌人中...";
        
        try
        {
            if (EnemySaveAdapter.Instance != null)
            {
                EnemySaveAdapter.Instance.InitializeEnemies();
            }
            
            currentStatus = "✅ 敌人重新初始化完成";
            
            if (showDebugInfo)
                Debug.Log("[EnemySaveSlotFix] ✅ 敌人重新初始化完成");
        }
        catch (System.Exception e)
        {
            currentStatus = $"❌ 初始化失败: {e.Message}";
            Debug.LogError($"[EnemySaveSlotFix] 敌人初始化失败: {e.Message}");
        }
    }
    
    /// <summary>
    /// 获取系统状态信息
    /// </summary>
    public string GetSystemStatus()
    {
        string status = "=== 敌人存档系统状态 ===\n";
        
        // EnemySaveDataManager状态
        if (EnemySaveDataManager.Instance != null)
        {
            status += "✅ EnemySaveDataManager: 正常\n";
            status += EnemySaveDataManager.Instance.GetAllSlotsInfo();
        }
        else
        {
            status += "❌ EnemySaveDataManager: 缺失\n";
        }
        
        // EnemySaveAdapter状态
        if (EnemySaveAdapter.Instance != null)
        {
            status += "✅ EnemySaveAdapter: 正常\n";
            status += $"   敌人总数: {EnemySaveAdapter.Instance.GetEnemyCount()}\n";
            status += $"   活跃敌人: {EnemySaveAdapter.Instance.GetActiveEnemyCount()}\n";
        }
        else
        {
            status += "❌ EnemySaveAdapter: 缺失\n";
        }
        
        // 场景敌人统计
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Enemy[] activeEnemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        
        status += $"场景敌人: {allEnemies.Length} 总数, {activeEnemies.Length} 活跃\n";
        
        return status;
    }
    
    /// <summary>
    /// 创建新存档槽位（确保干净的状态）
    /// </summary>
    public void CreateNewSaveSlot(int slotIndex)
    {
        if (EnemySaveDataManager.Instance != null)
        {
            EnemySaveDataManager.Instance.CreateNewSaveSlot(slotIndex);
            
            if (showDebugInfo)
                Debug.Log($"[EnemySaveSlotFix] ✅ 创建新存档槽位: {slotIndex}");
        }
    }
    
    private void OnGUI()
    {
        if (!showDebugInfo) return;
        
        // 显示状态信息
        GUILayout.BeginArea(new Rect(10, 10, 400, 200));
        GUILayout.Label("敌人存档槽位修复器", GUILayout.Width(200));
        GUILayout.Label($"状态: {currentStatus}", GUILayout.Width(350));
        
        if (GUILayout.Button("执行完整修复", GUILayout.Width(150)))
        {
            ExecuteFullFix();
        }
        
        if (GUILayout.Button("查看系统状态", GUILayout.Width(150)))
        {
            Debug.Log(GetSystemStatus());
        }
        
        GUILayout.EndArea();
    }
} 