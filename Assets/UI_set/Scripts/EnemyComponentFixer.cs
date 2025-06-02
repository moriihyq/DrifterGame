using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
/// <summary>
/// 敌人组件修复器 - 专门解决SaveableEnemy组件缺失问题
/// </summary>
public class EnemyComponentFixer : MonoBehaviour
{
    [Header("修复设置")]
    [SerializeField] private bool enableAutoFix = true;
    [SerializeField] private bool enableDebugLog = true;
    [SerializeField] private float autoFixInterval = 2f; // 自动修复间隔
    
    [Header("统计信息")]
    [SerializeField] private int totalFixAttempts = 0;
    [SerializeField] private int successfulFixes = 0;
    [SerializeField] private int lastEnemyCount = 0;
    [SerializeField] private int lastSaveableEnemyCount = 0;
    
    private bool isAutoFixing = false;
    
    private void Start()
    {
        if (enableAutoFix)
        {
            StartAutoFix();
        }
    }
    
    private void Update()
    {
        // 键盘快捷键
        if (Input.GetKeyDown(KeyCode.F))
        {
            PerformManualFix();
        }
        
        if (Input.GetKeyDown(KeyCode.G))
        {
            CheckComponentStatus();
        }
        
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (isAutoFixing)
                StopAutoFix();
            else
                StartAutoFix();
        }
    }
    
    /// <summary>
    /// 开始自动修复
    /// </summary>
    public void StartAutoFix()
    {
        if (!isAutoFixing)
        {
            isAutoFixing = true;
            StartCoroutine(AutoFixCoroutine());
            
            if (enableDebugLog)
                Debug.Log("[EnemyComponentFixer] 开始自动修复，间隔: " + autoFixInterval + "秒");
        }
    }
    
    /// <summary>
    /// 停止自动修复
    /// </summary>
    public void StopAutoFix()
    {
        isAutoFixing = false;
        
        if (enableDebugLog)
            Debug.Log("[EnemyComponentFixer] 停止自动修复");
    }
    
    /// <summary>
    /// 自动修复协程
    /// </summary>
    private IEnumerator AutoFixCoroutine()
    {
        while (isAutoFixing)
        {
            CheckAndFix();
            yield return new WaitForSeconds(autoFixInterval);
        }
    }
    
    /// <summary>
    /// 执行手动修复
    /// </summary>
    public void PerformManualFix()
    {
        if (enableDebugLog)
            Debug.Log("[EnemyComponentFixer] 执行手动修复...");
        
        CheckAndFix();
    }
    
    /// <summary>
    /// 检查并修复组件
    /// </summary>
    private void CheckAndFix()
    {
        totalFixAttempts++;
        
        // 获取当前敌人状态
        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        SaveableEnemy[] saveableEnemies = Object.FindObjectsByType<SaveableEnemy>(FindObjectsSortMode.None);
        
        lastEnemyCount = enemies.Length;
        lastSaveableEnemyCount = saveableEnemies.Length;
        
        if (enableDebugLog)
        {
            Debug.Log($"[EnemyComponentFixer] 修复尝试 #{totalFixAttempts}");
            Debug.Log($"[EnemyComponentFixer] Enemy数量: {enemies.Length}, SaveableEnemy数量: {saveableEnemies.Length}");
        }
        
        // 如果数量匹配，无需修复
        if (enemies.Length == saveableEnemies.Length && enemies.Length > 0)
        {
            if (enableDebugLog)
                Debug.Log("[EnemyComponentFixer] ✅ 组件数量匹配，无需修复");
            return;
        }
        
        // 如果没有敌人，也无需修复
        if (enemies.Length == 0)
        {
            if (enableDebugLog)
                Debug.Log("[EnemyComponentFixer] 场景中没有敌人，无需修复");
            return;
        }
        
        // 开始修复过程
        int fixedCount = 0;
        
        // 为缺少SaveableEnemy组件的敌人添加组件
        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;
            
            SaveableEnemy saveableComponent = enemy.GetComponent<SaveableEnemy>();
            if (saveableComponent == null)
            {
                // 添加SaveableEnemy组件
                saveableComponent = enemy.gameObject.AddComponent<SaveableEnemy>();
                
                // 生成敌人ID
                string enemyID = GenerateEnemyID(enemy, fixedCount);
                
                // 初始化组件
                saveableComponent.Initialize(enemyID, enemy);
                
                // 添加死亡监控组件
                EnemyDeathMonitor monitor = enemy.GetComponent<EnemyDeathMonitor>();
                if (monitor == null)
                {
                    monitor = enemy.gameObject.AddComponent<EnemyDeathMonitor>();
                    monitor.Initialize(enemyID, EnemySaveAdapter.Instance);
                }
                
                fixedCount++;
                
                if (enableDebugLog)
                    Debug.Log($"[EnemyComponentFixer] ✅ 为敌人添加SaveableEnemy组件: {enemy.name} (ID: {enemyID})");
            }
        }
        
        if (fixedCount > 0)
        {
            successfulFixes++;
            
            if (enableDebugLog)
                Debug.Log($"[EnemyComponentFixer] 🎊 修复完成！共修复了 {fixedCount} 个敌人组件");
            
            // 通知EnemySaveAdapter重新初始化
            if (EnemySaveAdapter.Instance != null)
            {
                EnemySaveAdapter.Instance.InitializeEnemies();
                if (enableDebugLog)
                    Debug.Log("[EnemyComponentFixer] 已通知EnemySaveAdapter重新初始化");
            }
        }
        else
        {
            if (enableDebugLog)
                Debug.LogWarning("[EnemyComponentFixer] ⚠️ 没有找到需要修复的敌人组件");
        }
    }
    
    /// <summary>
    /// 检查组件状态
    /// </summary>
    public void CheckComponentStatus()
    {
        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        SaveableEnemy[] saveableEnemies = Object.FindObjectsByType<SaveableEnemy>(FindObjectsSortMode.None);
        
        Debug.Log("[EnemyComponentFixer] === 组件状态检查 ===");
        Debug.Log($"[EnemyComponentFixer] Enemy组件数量: {enemies.Length}");
        Debug.Log($"[EnemyComponentFixer] SaveableEnemy组件数量: {saveableEnemies.Length}");
        Debug.Log($"[EnemyComponentFixer] EnemySaveAdapter存在: {EnemySaveAdapter.Instance != null}");
        
        if (EnemySaveAdapter.Instance != null)
        {
            Debug.Log($"[EnemyComponentFixer] 适配器敌人数量: {EnemySaveAdapter.Instance.GetEnemyCount()}");
            Debug.Log($"[EnemyComponentFixer] 适配器活跃敌人数量: {EnemySaveAdapter.Instance.GetActiveEnemyCount()}");
            Debug.Log($"[EnemyComponentFixer] 适配器死亡敌人数量: {EnemySaveAdapter.Instance.GetDeadEnemyCount()}");
        }
        
        // 详细检查每个敌人
        for (int i = 0; i < enemies.Length; i++)
        {
            Enemy enemy = enemies[i];
            SaveableEnemy saveable = enemy.GetComponent<SaveableEnemy>();
            EnemyDeathMonitor monitor = enemy.GetComponent<EnemyDeathMonitor>();
            
            Debug.Log($"[EnemyComponentFixer] 敌人 #{i}: {enemy.name} " +
                     $"SaveableEnemy: {(saveable != null ? "✅" : "❌")} " +
                     $"DeathMonitor: {(monitor != null ? "✅" : "❌")} " +
                     $"活跃: {enemy.gameObject.activeInHierarchy}");
        }
        
        Debug.Log("[EnemyComponentFixer] === 检查完成 ===");
    }
    
    /// <summary>
    /// 生成敌人ID
    /// </summary>
    private string GenerateEnemyID(Enemy enemy, int index)
    {
        Vector3 pos = enemy.transform.position;
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        return $"{sceneName}_Enemy_{index}_{pos.x:F1}_{pos.y:F1}_{pos.z:F1}";
    }
    
    /// <summary>
    /// 清理重复的SaveableEnemy组件
    /// </summary>
    public void CleanupDuplicateComponents()
    {
        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        int cleanedCount = 0;
        
        foreach (Enemy enemy in enemies)
        {
            SaveableEnemy[] saveableComponents = enemy.GetComponents<SaveableEnemy>();
            if (saveableComponents.Length > 1)
            {
                // 保留第一个，删除其余的
                for (int i = 1; i < saveableComponents.Length; i++)
                {
                    DestroyImmediate(saveableComponents[i]);
                    cleanedCount++;
                }
                
                if (enableDebugLog)
                    Debug.Log($"[EnemyComponentFixer] 清理敌人重复组件: {enemy.name} (删除了 {saveableComponents.Length - 1} 个重复组件)");
            }
        }
        
        if (cleanedCount > 0)
        {
            if (enableDebugLog)
                Debug.Log($"[EnemyComponentFixer] 清理完成，共删除了 {cleanedCount} 个重复组件");
        }
    }
    
    private void OnGUI()
    {
        // 图形化界面已禁用 - 如需重新启用，将下面的return注释掉
        return;
        
        if (!enableDebugLog) return;
        
        GUILayout.BeginArea(new Rect(420, 150, 350, 250));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("敌人组件修复器", GUI.skin.label);
        GUILayout.Space(5);
        
        GUILayout.Label($"修复尝试: {totalFixAttempts}");
        GUILayout.Label($"成功修复: {successfulFixes}");
        GUILayout.Label($"Enemy数量: {lastEnemyCount}");
        GUILayout.Label($"SaveableEnemy数量: {lastSaveableEnemyCount}");
        GUILayout.Label($"自动修复: {(isAutoFixing ? "运行中" : "已停止")}");
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("F - 手动修复"))
        {
            PerformManualFix();
        }
        
        if (GUILayout.Button("G - 检查状态"))
        {
            CheckComponentStatus();
        }
        
        if (GUILayout.Button(isAutoFixing ? "H - 停止自动修复" : "H - 开始自动修复"))
        {
            if (isAutoFixing)
                StopAutoFix();
            else
                StartAutoFix();
        }
        
        if (GUILayout.Button("清理重复组件"))
        {
            CleanupDuplicateComponents();
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
} 