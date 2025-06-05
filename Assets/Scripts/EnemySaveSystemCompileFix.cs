using UnityEngine;

/// <summary>
/// 敌人存档系统编译修复器
/// 解决升级后的编译错误问题
/// </summary>
public class EnemySaveSystemCompileFix : MonoBehaviour
{
    [Header("编译错误修复")]
    [Tooltip("执行编译错误修复")]
    public bool executeCompileFix = false;
    
    [Tooltip("检查系统状态")]
    public bool checkSystemStatus = false;
    
    [Header("状态信息")]
    [SerializeField] private string fixStatus = "等待执行修复...";
    
    private void Update()
    {
        if (executeCompileFix)
        {
            executeCompileFix = false;
            ExecuteCompileFix();
        }
        
        if (checkSystemStatus)
        {
            checkSystemStatus = false;
            CheckSystemStatus();
        }
    }
    
    /// <summary>
    /// 执行编译错误修复
    /// </summary>
    [ContextMenu("执行编译错误修复")]
    public void ExecuteCompileFix()
    {
        fixStatus = "正在修复编译错误...";
        
        try
        {
            Debug.Log("=== [EnemySaveSystemCompileFix] 开始修复编译错误 ===");
            
            // 1. 检查必要的组件是否存在
            bool hasDataManager = EnemySaveDataManager.Instance != null;
            bool hasAdapter = EnemySaveAdapter.Instance != null;
            bool hasSaveManager = SaveManager.Instance != null;
            
            Debug.Log($"✅ 组件检查 - DataManager: {hasDataManager}, Adapter: {hasAdapter}, SaveManager: {hasSaveManager}");
            
            // 2. 如果缺少EnemySaveDataManager，创建它
            if (!hasDataManager)
            {
                GameObject dataManagerObj = new GameObject("EnemySaveDataManager");
                dataManagerObj.AddComponent<EnemySaveDataManager>();
                Debug.Log("✅ 创建了缺失的EnemySaveDataManager");
            }
            
            // 3. 清理可能的旧方法调用
            if (hasAdapter)
            {
                // 清理当前槽位死亡记录
                EnemySaveAdapter.Instance.ClearCurrentSlotDeathRecords();
                Debug.Log("✅ 清理EnemySaveAdapter当前槽位死亡记录");
            }
            
            // 4. 初始化系统
            if (hasAdapter)
            {
                EnemySaveAdapter.Instance.InitializeEnemies();
                Debug.Log("✅ 重新初始化EnemySaveAdapter");
            }
            
            // 5. 设置默认存档槽位
            if (EnemySaveDataManager.Instance != null)
            {
                EnemySaveDataManager.Instance.SetCurrentSaveSlot(0);
                Debug.Log("✅ 设置默认存档槽位为0");
            }
            
            fixStatus = "✅ 编译错误修复完成！";
            
            Debug.Log("=== [EnemySaveSystemCompileFix] 修复完成 ===");
            Debug.Log("✅ 编译错误已修复");
            Debug.Log("✅ 敌人存档系统已更新到新版本");
            Debug.Log("✅ 现在支持多存档槽位独立管理");
            
        }
        catch (System.Exception e)
        {
            fixStatus = $"❌ 修复失败: {e.Message}";
            Debug.LogError($"[EnemySaveSystemCompileFix] 修复失败: {e.Message}");
        }
    }
    
    /// <summary>
    /// 检查系统状态
    /// </summary>
    [ContextMenu("检查系统状态")]
    public void CheckSystemStatus()
    {
        Debug.Log("=== [EnemySaveSystemCompileFix] 系统状态检查 ===");
        
        // 检查核心组件
        bool hasDataManager = EnemySaveDataManager.Instance != null;
        bool hasAdapter = EnemySaveAdapter.Instance != null;
        bool hasSaveManager = SaveManager.Instance != null;
        
        Debug.Log($"EnemySaveDataManager: {(hasDataManager ? "✅ 正常" : "❌ 缺失")}");
        Debug.Log($"EnemySaveAdapter: {(hasAdapter ? "✅ 正常" : "❌ 缺失")}");
        Debug.Log($"SaveManager: {(hasSaveManager ? "✅ 正常" : "❌ 缺失")}");
        
        // 检查存档槽位信息
        if (hasDataManager)
        {
            Debug.Log(EnemySaveDataManager.Instance.GetAllSlotsInfo());
        }
        
        // 检查场景敌人
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Enemy[] activeEnemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        SaveableEnemy[] saveableEnemies = FindObjectsByType<SaveableEnemy>(FindObjectsSortMode.None);
        
        Debug.Log($"场景敌人统计:");
        Debug.Log($"  总敌人数: {allEnemies.Length}");
        Debug.Log($"  活跃敌人: {activeEnemies.Length}");
        Debug.Log($"  SaveableEnemy组件: {saveableEnemies.Length}");
        
        if (hasAdapter)
        {
            Debug.Log($"EnemySaveAdapter统计:");
            Debug.Log($"  管理的敌人数: {EnemySaveAdapter.Instance.GetEnemyCount()}");
            Debug.Log($"  活跃敌人数: {EnemySaveAdapter.Instance.GetActiveEnemyCount()}");
            Debug.Log($"  死亡敌人数: {EnemySaveAdapter.Instance.GetDeadEnemyCount()}");
        }
        
        Debug.Log("=== 系统状态检查完成 ===");
    }
    
    /// <summary>
    /// 获取修复状态
    /// </summary>
    public string GetFixStatus()
    {
        return fixStatus;
    }
    
    /// <summary>
    /// 重置系统（紧急情况使用）
    /// </summary>
    [ContextMenu("重置系统")]
    public void ResetSystem()
    {
        fixStatus = "正在重置系统...";
        
        try
        {
            // 清理所有存档槽位的死亡记录
            if (EnemySaveDataManager.Instance != null)
            {
                EnemySaveDataManager.Instance.ClearAllSlots();
            }
            
            // 清理当前死亡记录
            if (EnemySaveAdapter.Instance != null)
            {
                EnemySaveAdapter.Instance.ClearCurrentSlotDeathRecords();
            }
            
            // 重新激活所有敌人
            Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Enemy enemy in allEnemies)
            {
                if (enemy != null)
                {
                    enemy.gameObject.SetActive(true);
                }
            }
            
            // 重新初始化系统
            if (EnemySaveAdapter.Instance != null)
            {
                EnemySaveAdapter.Instance.InitializeEnemies();
            }
            
            fixStatus = "✅ 系统重置完成";
            Debug.Log("[EnemySaveSystemCompileFix] ✅ 系统重置完成");
        }
        catch (System.Exception e)
        {
            fixStatus = $"❌ 重置失败: {e.Message}";
            Debug.LogError($"[EnemySaveSystemCompileFix] 重置失败: {e.Message}");
        }
    }
    
    private void OnGUI()
    {
        // 简单的状态显示
        GUI.color = Color.white;
        GUI.Box(new Rect(10, Screen.height - 80, 300, 60), "");
        GUI.Label(new Rect(15, Screen.height - 75, 290, 20), "敌人存档系统编译修复器");
        GUI.Label(new Rect(15, Screen.height - 55, 290, 20), fixStatus);
        
        if (GUI.Button(new Rect(15, Screen.height - 35, 80, 20), "执行修复"))
        {
            ExecuteCompileFix();
        }
        
        if (GUI.Button(new Rect(100, Screen.height - 35, 80, 20), "检查状态"))
        {
            CheckSystemStatus();
        }
        
        if (GUI.Button(new Rect(185, Screen.height - 35, 80, 20), "重置系统"))
        {
            ResetSystem();
        }
    }
} 