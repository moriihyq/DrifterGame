using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 敌人存档系统修复器 - 自动检测和修复敌人存档相关问题
/// </summary>
public class EnemySaveSystemFixer : MonoBehaviour
{
    [Header("修复设置")]
    [SerializeField] private bool autoFixOnStart = true;
    [SerializeField] private bool enableDebugLog = true;
    [SerializeField] private bool continuousMonitoring = true;
    [SerializeField] private float monitoringInterval = 2f;
    
    [Header("紧急修复")]
    [SerializeField] private KeyCode emergencyFixKey = KeyCode.F9;
    [SerializeField] private KeyCode fullRebuildKey = KeyCode.F10;
    
    private void Start()
    {
        if (autoFixOnStart)
        {
            StartCoroutine(DelayedAutoFix());
        }
        
        if (continuousMonitoring)
        {
            StartCoroutine(ContinuousMonitoring());
        }
    }
    
    private void Update()
    {
        // 紧急修复热键
        if (Input.GetKeyDown(emergencyFixKey))
        {
            StartCoroutine(EmergencyFix());
        }
        
        // 完全重建热键
        if (Input.GetKeyDown(fullRebuildKey))
        {
            StartCoroutine(FullSystemRebuild());
        }
    }
    
    /// <summary>
    /// 延迟自动修复 - 确保场景完全加载后执行
    /// </summary>
    private IEnumerator DelayedAutoFix()
    {
        // 等待场景完全稳定
        yield return new WaitForSeconds(0.5f);
        yield return new WaitForEndOfFrame();
        yield return new WaitForFixedUpdate();
        
        yield return StartCoroutine(FullSystemRebuild());
    }
    
    /// <summary>
    /// 持续监控系统状态
    /// </summary>
    private IEnumerator ContinuousMonitoring()
    {
        while (continuousMonitoring)
        {
            yield return new WaitForSeconds(monitoringInterval);
            
            if (DetectSystemProblems())
            {
                yield return StartCoroutine(EmergencyFix());
            }
        }
    }
    
    /// <summary>
    /// 检测系统问题
    /// </summary>
    private bool DetectSystemProblems()
    {
        // 检查EnemySaveAdapter
        if (EnemySaveAdapter.Instance == null)
        {
            return true;
        }
        
        // 检查敌人组件匹配
        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        SaveableEnemy[] saveableEnemies = Object.FindObjectsByType<SaveableEnemy>(FindObjectsSortMode.None);
        
        if (enemies.Length > 0 && saveableEnemies.Length != enemies.Length)
        {
            return true;
        }
        
        // 检查敌人ID完整性
        foreach (SaveableEnemy saveable in saveableEnemies)
        {
            if (string.IsNullOrEmpty(saveable.GetEnemyID()))
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 紧急修复 - 修复当前检测到的问题
    /// </summary>
    private IEnumerator EmergencyFix()
    {
        
        // 等待一帧确保稳定性
        yield return null;
        
        // 1. 确保EnemySaveAdapter存在
        yield return StartCoroutine(EnsureEnemySaveAdapter());
        
        // 2. 修复缺失的SaveableEnemy组件
        yield return StartCoroutine(FixMissingSaveableEnemyComponents());
        
        // 3. 验证修复结果
        yield return StartCoroutine(ValidateSystemIntegrity());
    }
    
    /// <summary>
    /// 完全重建系统
    /// </summary>
    private IEnumerator FullSystemRebuild()
    {
        
        // 等待系统稳定
        yield return new WaitForEndOfFrame();
        
        // 1. 清理现有的SaveableEnemy组件
        yield return StartCoroutine(CleanupExistingSaveableComponents());
        
        // 2. 确保EnemySaveAdapter存在并正确配置
        yield return StartCoroutine(EnsureEnemySaveAdapter());
        
        // 3. 为所有敌人重新添加SaveableEnemy组件
        yield return StartCoroutine(RebuildSaveableEnemyComponents());
        
        // 4. 重新初始化EnemySaveAdapter
        yield return StartCoroutine(ReinitializeEnemySaveAdapter());
        
        // 5. 最终验证
        yield return StartCoroutine(ValidateSystemIntegrity());
    }
    
    /// <summary>
    /// 确保EnemySaveAdapter存在
    /// </summary>
    private IEnumerator EnsureEnemySaveAdapter()
    {
        if (EnemySaveAdapter.Instance == null)
        {
            if (enableDebugLog)
                Debug.Log("[EnemySaveSystemFixer] 📦 创建EnemySaveAdapter实例");
            
            // 查找现有的EnemySaveAdapter对象
            EnemySaveAdapter existingAdapter = Object.FindFirstObjectByType<EnemySaveAdapter>();
            
            if (existingAdapter == null)
            {
                // 创建新的EnemySaveAdapter对象
                GameObject adapterObject = new GameObject("EnemySaveAdapter");
                existingAdapter = adapterObject.AddComponent<EnemySaveAdapter>();
                
                
            }
            else
            {
                
            }
            
            // 等待实例初始化
            yield return new WaitForSeconds(0.1f);
        }
        else
        {
        }
    }
    
    /// <summary>
    /// 清理现有的SaveableEnemy组件
    /// </summary>
    private IEnumerator CleanupExistingSaveableComponents()
    {
        SaveableEnemy[] existingComponents = Object.FindObjectsByType<SaveableEnemy>(FindObjectsSortMode.None);
        
        if (existingComponents.Length > 0)
        {
            
            foreach (SaveableEnemy component in existingComponents)
            {
                if (component != null)
                {
                    DestroyImmediate(component);
                }
            }
            
            yield return null;
        }
    }
    
    /// <summary>
    /// 修复缺失的SaveableEnemy组件
    /// </summary>
    private IEnumerator FixMissingSaveableEnemyComponents()
    {
        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        int fixedCount = 0;
        

        
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null && enemy.GetComponent<SaveableEnemy>() == null)
            {
                SaveableEnemy saveableComponent = enemy.gameObject.AddComponent<SaveableEnemy>();
                string enemyID = GenerateUniqueEnemyID(enemy, fixedCount);
                saveableComponent.Initialize(enemyID, enemy);
                fixedCount++;
                
                
            }
            
            // 每处理几个敌人后暂停一帧，避免卡顿
            if (fixedCount % 5 == 0)
            {
                yield return null;
            }
        }
        

    }
    
    /// <summary>
    /// 重建SaveableEnemy组件
    /// </summary>
    private IEnumerator RebuildSaveableEnemyComponents()
    {
        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        
    
        
        for (int i = 0; i < enemies.Length; i++)
        {
            Enemy enemy = enemies[i];
            if (enemy != null)
            {
                // 确保敌人有正确的标签
                if (!enemy.CompareTag("Enemy"))
                {
                    enemy.tag = "Enemy";
                }
                
                // 添加SaveableEnemy组件
                SaveableEnemy saveableComponent = enemy.gameObject.AddComponent<SaveableEnemy>();
                string enemyID = GenerateUniqueEnemyID(enemy, i);
                saveableComponent.Initialize(enemyID, enemy);
                
                
            }
            
            // 每处理几个敌人后暂停一帧
            if (i % 3 == 0)
            {
                yield return null;
            }
        }
    }
    
    /// <summary>
    /// 重新初始化EnemySaveAdapter
    /// </summary>
    private IEnumerator ReinitializeEnemySaveAdapter()
    {
        if (EnemySaveAdapter.Instance != null)
        {
            
            
            // 等待一帧确保所有SaveableEnemy组件都已添加
            yield return null;
            
            // 调用初始化方法
            EnemySaveAdapter.Instance.InitializeEnemies();
            
            // 等待初始化完成
            yield return new WaitForSeconds(0.2f);
            
            
        }
    }
    
    /// <summary>
    /// 验证系统完整性
    /// </summary>
    private IEnumerator ValidateSystemIntegrity()
    {
        yield return new WaitForSeconds(0.1f);
        
        
        
        // 统计组件数量
        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        SaveableEnemy[] saveableEnemies = Object.FindObjectsByType<SaveableEnemy>(FindObjectsSortMode.None);
        EnemyDeathMonitor[] deathMonitors = Object.FindObjectsByType<EnemyDeathMonitor>(FindObjectsSortMode.None);
        
        bool adapterExists = EnemySaveAdapter.Instance != null;
        int adapterEnemyCount = adapterExists ? EnemySaveAdapter.Instance.GetEnemyCount() : 0;
        
        // 输出验证结果 
        
        
        
        // 检查一致性
        bool systemHealthy = true;
        List<string> issues = new List<string>();
        
        if (!adapterExists)
        {
            issues.Add("EnemySaveAdapter缺失");
            systemHealthy = false;
        }
        
        if (enemies.Length != saveableEnemies.Length)
        {
            issues.Add($"组件数量不匹配: Enemy({enemies.Length}) vs SaveableEnemy({saveableEnemies.Length})");
            systemHealthy = false;
        }
        
        if (adapterExists && adapterEnemyCount != enemies.Length)
        {
            issues.Add($"适配器管理数量不匹配: Adapter({adapterEnemyCount}) vs Enemy({enemies.Length})");
            systemHealthy = false;
        }
        
        // 输出结果
        if (systemHealthy)
        {
        }   
        
        
        yield return null;
    }
    
    /// <summary>
    /// 生成唯一敌人ID
    /// </summary>
    private string GenerateUniqueEnemyID(Enemy enemy, int index)
    {
        Vector3 pos = enemy.transform.position;
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        return $"{sceneName}_Enemy_{index}_{pos.x:F1}_{pos.y:F1}_{Time.time:F1}";
    }
    
    /// <summary>
    /// 获取系统状态摘要（用于UI显示）
    /// </summary>
    public string GetSystemStatusSummary()
    {
        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        SaveableEnemy[] saveableEnemies = Object.FindObjectsByType<SaveableEnemy>(FindObjectsSortMode.None);
        bool adapterExists = EnemySaveAdapter.Instance != null;
        
        return $"敌人: {enemies.Length} | 存档组件: {saveableEnemies.Length} | 适配器: {(adapterExists ? "✅" : "❌")}";
    }
    
    /// <summary>
    /// 手动触发紧急修复（可从其他脚本调用）
    /// </summary>
    public void TriggerEmergencyFix()
    {
        StartCoroutine(EmergencyFix());
    }
    
    /// <summary>
    /// 手动触发完全重建（可从其他脚本调用）
    /// </summary>
    public void TriggerFullRebuild()
    {
        StartCoroutine(FullSystemRebuild());
    }
} 