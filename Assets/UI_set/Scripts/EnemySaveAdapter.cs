using System.Collections.Generic;
using UnityEngine;
using System.Collections;

/// <summary>
/// 敌人存档适配器 - 处理现有Enemy类与存档系统的兼容性
/// </summary>
public class EnemySaveAdapter : MonoBehaviour
{
    [Header("敌人管理设置")]
    [SerializeField] private bool enableDebugLog = true;
    [SerializeField] private bool autoInitializeOnStart = true;
    
    // 公开属性，供SaveableEnemy访问
    public bool EnableDebugLog => enableDebugLog;
    
    private List<Enemy> sceneEnemies = new List<Enemy>();
    private Dictionary<string, Enemy> enemyLookup = new Dictionary<string, Enemy>();
    
    // 死亡敌人记录系统
    private Dictionary<string, EnemyData> deadEnemiesRecord = new Dictionary<string, EnemyData>();
    
    /// <summary>
    /// 单例实例
    /// </summary>
    public static EnemySaveAdapter Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (autoInitializeOnStart)
        {
            // 使用延迟初始化避免时机问题
            StartCoroutine(DelayedInitialization());
        }
    }
    
    /// <summary>
    /// 延迟初始化协程
    /// </summary>
    private IEnumerator DelayedInitialization()
    {
        // 等待一帧确保所有对象都已加载
        yield return null;
        
        // 再等待0.1秒确保场景完全稳定
        yield return new WaitForSeconds(0.1f);
        
        // 执行初始化并检查结果
        InitializeEnemies();
        
        // 如果初始化不成功，进行重试
        yield return StartCoroutine(RetryInitializationIfNeeded());
    }
    
    /// <summary>
    /// 重试初始化协程
    /// </summary>
    private IEnumerator RetryInitializationIfNeeded()
    {
        int maxRetries = 3;
        int retryCount = 0;
        
        while (retryCount < maxRetries)
        {
            // 检查当前初始化状态
            Enemy[] allEnemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            SaveableEnemy[] saveableEnemies = Object.FindObjectsByType<SaveableEnemy>(FindObjectsSortMode.None);
            
            // 如果敌人数量匹配，说明初始化成功
            if (allEnemies.Length > 0 && allEnemies.Length == saveableEnemies.Length)
            {
                if (enableDebugLog)
                    Debug.Log($"[EnemySaveAdapter] 初始化成功: {allEnemies.Length} 个敌人，{saveableEnemies.Length} 个SaveableEnemy组件");
                break;
            }
            
            // 如果不匹配，等待后重试
            retryCount++;
            if (enableDebugLog)
                Debug.LogWarning($"[EnemySaveAdapter] 初始化第{retryCount}次重试 (Enemy: {allEnemies.Length}, SaveableEnemy: {saveableEnemies.Length})");
            
            yield return new WaitForSeconds(0.5f);
            
            // 重新初始化
            InitializeEnemies();
        }
        
        // 最终检查
        if (retryCount >= maxRetries)
        {
            Enemy[] finalEnemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            SaveableEnemy[] finalSaveableEnemies = Object.FindObjectsByType<SaveableEnemy>(FindObjectsSortMode.None);
            
            if (finalEnemies.Length != finalSaveableEnemies.Length)
            {
                Debug.LogError($"[EnemySaveAdapter] 初始化失败: Enemy({finalEnemies.Length}) vs SaveableEnemy({finalSaveableEnemies.Length})");
            }
        }
    }
    
    /// <summary>
    /// 初始化场景中的敌人
    /// </summary>
    public void InitializeEnemies()
    {
        if (enableDebugLog)
            Debug.Log("[EnemySaveAdapter] 正在初始化场景敌人...");
        
        sceneEnemies.Clear();
        enemyLookup.Clear();
        // 注意：不再自动清理死亡记录，让RestoreEnemyData来管理
        
        // 查找场景中所有敌人
        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        
        for (int i = 0; i < enemies.Length; i++)
        {
            Enemy enemy = enemies[i];
            
            // 为敌人分配唯一ID
            string enemyID = GenerateEnemyID(enemy, i);
            
            // 检查这个敌人是否在死亡记录中
            if (deadEnemiesRecord.ContainsKey(enemyID))
            {
                // 这个敌人应该是死亡状态，立即禁用
                enemy.gameObject.SetActive(false);
                
                if (enableDebugLog)
                    Debug.Log($"[EnemySaveAdapter] 发现死亡敌人，禁用: {enemy.name} (ID: {enemyID})");
                
                continue; // 跳过这个死亡敌人的初始化
            }
            
            // 添加到列表和字典
            sceneEnemies.Add(enemy);
            enemyLookup[enemyID] = enemy;
            
            // 为敌人添加SaveableEnemy组件
            SaveableEnemy saveableComponent = enemy.GetComponent<SaveableEnemy>();
            if (saveableComponent == null)
            {
                saveableComponent = enemy.gameObject.AddComponent<SaveableEnemy>();
            }
            
            saveableComponent.Initialize(enemyID, enemy);
            
            // 监听敌人死亡事件
            RegisterEnemyDeathMonitoring(enemy, enemyID);
            
            if (enableDebugLog)
                Debug.Log($"[EnemySaveAdapter] 初始化敌人: {enemy.name} (ID: {enemyID})");
        }
        
        if (enableDebugLog)
            Debug.Log($"[EnemySaveAdapter] 总共初始化了 {sceneEnemies.Count} 个敌人，死亡记录: {deadEnemiesRecord.Count} 个");
    }
    
    /// <summary>
    /// 注册敌人死亡监控
    /// </summary>
    private void RegisterEnemyDeathMonitoring(Enemy enemy, string enemyID)
    {
        // 为敌人添加死亡监控组件
        EnemyDeathMonitor monitor = enemy.GetComponent<EnemyDeathMonitor>();
        if (monitor == null)
        {
            monitor = enemy.gameObject.AddComponent<EnemyDeathMonitor>();
        }
        
        monitor.Initialize(enemyID, this);
    }
    
    /// <summary>
    /// 清理所有死亡记录（仅在真正重新开始游戏时调用）
    /// </summary>
    public void ClearAllDeathRecords()
    {
        deadEnemiesRecord.Clear();
        
        if (enableDebugLog)
            Debug.Log("[EnemySaveAdapter] 已清理所有死亡记录");
    }
    
    /// <summary>
    /// 记录敌人死亡（由EnemyDeathMonitor调用）
    /// </summary>
    public void RecordEnemyDeath(string enemyID, EnemyData deathData)
    {
        deadEnemiesRecord[enemyID] = deathData;
        
        if (enableDebugLog)
            Debug.Log($"[EnemySaveAdapter] 记录敌人死亡: {enemyID} 位置: {deathData.position}");
    }
    
    /// <summary>
    /// 生成敌人唯一ID
    /// </summary>
    private string GenerateEnemyID(Enemy enemy, int index)
    {
        // 基于敌人位置和索引生成唯一ID
        Vector3 pos = enemy.transform.position;
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        return $"{sceneName}_Enemy_{index}_{pos.x:F1}_{pos.y:F1}";
    }
    
    /// <summary>
    /// 收集敌人数据（供SaveManager调用）
    /// </summary>
    public List<EnemyData> CollectEnemyData()
    {
        List<EnemyData> enemyDataList = new List<EnemyData>();
        
        if (enableDebugLog)
            Debug.Log("[EnemySaveAdapter] === 开始收集敌人数据 ===");
        
        // 首先统计场景中的敌人总数
        Enemy[] allEnemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        SaveableEnemy[] saveableEnemies = Object.FindObjectsByType<SaveableEnemy>(FindObjectsSortMode.None);
        
        if (enableDebugLog)
        {
            Debug.Log($"[EnemySaveAdapter] 场景Enemy组件总数: {allEnemies.Length}");
            Debug.Log($"[EnemySaveAdapter] SaveableEnemy组件总数: {saveableEnemies.Length}");
            Debug.Log($"[EnemySaveAdapter] 初始化敌人列表数量: {sceneEnemies.Count}");
        }
        
        // 收集活着的敌人数据
        int activeCount = 0;
        int inactiveCount = 0;
        int deadCount = 0;
        int errorCount = 0;
        
        foreach (SaveableEnemy saveable in saveableEnemies)
        {
            if (saveable == null)
            {
                errorCount++;
                continue;
            }
            
            string enemyID = saveable.GetEnemyID();
            bool isActive = saveable.IsActive();
            
            if (enableDebugLog)
                Debug.Log($"[EnemySaveAdapter] 检查敌人: {enemyID}, IsActive: {isActive}, GameObject.active: {saveable.gameObject.activeInHierarchy}");
            
            if (isActive)
            {
                EnemyData data = saveable.GetEnemyData();
                if (data != null)
                {
                    enemyDataList.Add(data);
                    activeCount++;
                    
                    if (enableDebugLog)
                        Debug.Log($"[EnemySaveAdapter] ✅ 收集活敌人数据: {data.enemyID} (血量: {data.currentHealth}/{data.maxHealth})");
                }
                else
                {
                    errorCount++;
                    if (enableDebugLog)
                        Debug.LogWarning($"[EnemySaveAdapter] ❌ 敌人数据为null: {enemyID}");
                }
            }
            else
            {
                // 检查敌人为什么不活跃
                Enemy enemyComponent = saveable.GetComponent<Enemy>();
                if (enemyComponent == null)
                {
                    errorCount++;
                    if (enableDebugLog)
                        Debug.LogWarning($"[EnemySaveAdapter] ❌ 敌人缺少Enemy组件: {enemyID}");
                }
                else
                {
                    // 通过反射检查敌人状态
                    var isDead = GetPrivateFieldValue<bool>(enemyComponent, "isDead");
                    var currentHealth = GetPrivateFieldValue<int>(enemyComponent, "currentHealth");
                    
                    if (isDead || currentHealth <= 0)
                    {
                        deadCount++;
                        if (enableDebugLog)
                            Debug.Log($"[EnemySaveAdapter] 💀 敌人已死亡: {enemyID} (血量: {currentHealth}, isDead: {isDead})");
                    }
                    else
                    {
                        inactiveCount++;
                        if (enableDebugLog)
                            Debug.LogWarning($"[EnemySaveAdapter] ⚠️ 敌人非活跃但未死亡: {enemyID} (血量: {currentHealth}, GameObject.active: {saveable.gameObject.activeInHierarchy})");
                    }
                }
            }
        }
        
        // 收集死亡敌人数据
        foreach (var deadEnemy in deadEnemiesRecord)
        {
            enemyDataList.Add(deadEnemy.Value);
            
            if (enableDebugLog)
                Debug.Log($"[EnemySaveAdapter] ✅ 收集死敌人数据: {deadEnemy.Key} (死亡状态)");
        }
        
        if (enableDebugLog)
        {
            Debug.Log($"[EnemySaveAdapter] === 数据收集统计 ===");
            Debug.Log($"[EnemySaveAdapter] 活跃敌人: {activeCount}");
            Debug.Log($"[EnemySaveAdapter] 非活跃敌人: {inactiveCount}");
            Debug.Log($"[EnemySaveAdapter] 死亡敌人(运行时): {deadCount}");
            Debug.Log($"[EnemySaveAdapter] 死亡敌人(记录): {deadEnemiesRecord.Count}");
            Debug.Log($"[EnemySaveAdapter] 错误数量: {errorCount}");
            Debug.Log($"[EnemySaveAdapter] 总共收集了 {enemyDataList.Count} 个敌人的数据");
        }
        
        return enemyDataList;
    }
    
    /// <summary>
    /// 恢复敌人数据（供SaveManager调用）
    /// </summary>
    public void RestoreEnemyData(List<EnemyData> enemyDataList)
    {
        if (enableDebugLog)
            Debug.Log($"[EnemySaveAdapter] 开始恢复 {enemyDataList.Count} 个敌人的数据");
        
        // 清理当前死亡记录，从存档数据重新构建
        deadEnemiesRecord.Clear();
        
        // 首先禁用所有现有敌人（包括那些可能在InitializeEnemies中被激活的）
        Enemy[] allEnemies = Object.FindObjectsByType<Enemy>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Enemy enemy in allEnemies)
        {
            enemy.gameObject.SetActive(false);
        }
        
        // 分析存档数据并分类处理
        List<EnemyData> aliveEnemies = new List<EnemyData>();
        List<EnemyData> deadEnemies = new List<EnemyData>();
        
        foreach (EnemyData data in enemyDataList)
        {
            if (data.currentHealth <= 0 || !data.isActive)
            {
                deadEnemies.Add(data);
            }
            else
            {
                aliveEnemies.Add(data);
            }
        }
        
        if (enableDebugLog)
            Debug.Log($"[EnemySaveAdapter] 存档分析: {aliveEnemies.Count} 个活敌人, {deadEnemies.Count} 个死敌人");
        
        // 恢复死亡敌人记录
        foreach (EnemyData deadData in deadEnemies)
        {
            deadEnemiesRecord[deadData.enemyID] = deadData;
            
            if (enableDebugLog)
                Debug.Log($"[EnemySaveAdapter] 恢复死亡敌人记录: {deadData.enemyID}");
        }
        
        // 恢复活着的敌人
        foreach (EnemyData aliveData in aliveEnemies)
        {
            RestoreSingleEnemy(aliveData);
        }
        
        if (enableDebugLog)
            Debug.Log($"[EnemySaveAdapter] 敌人数据恢复完成，活敌人: {aliveEnemies.Count}, 死亡记录: {deadEnemiesRecord.Count} 个");
    }
    
    /// <summary>
    /// 恢复单个敌人
    /// </summary>
    private void RestoreSingleEnemy(EnemyData data)
    {
        Enemy[] allEnemies = Object.FindObjectsByType<Enemy>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Enemy bestMatch = null;
        float minDistance = float.MaxValue;
        
        // 首先尝试通过SaveableEnemy的ID匹配
        SaveableEnemy[] allSaveableEnemies = Object.FindObjectsByType<SaveableEnemy>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (SaveableEnemy saveable in allSaveableEnemies)
        {
            if (saveable.GetEnemyID() == data.enemyID)
            {
                saveable.LoadEnemyData(data);
                
                if (data.isActive && data.currentHealth > 0)
                {
                    saveable.gameObject.SetActive(true);
                    
                    if (enableDebugLog)
                        Debug.Log($"[EnemySaveAdapter] 通过ID恢复敌人: {data.enemyID} (血量: {data.currentHealth}/{data.maxHealth})");
                }
                
                return; // 找到匹配的，直接返回
            }
        }
        
        // 如果通过ID没找到，尝试基于位置匹配
        foreach (Enemy enemy in allEnemies)
        {
            if (enemy == null) continue;
            
            float distance = Vector3.Distance(enemy.transform.position, data.position);
            if (distance < minDistance && distance < 1f) // 位置误差容忍度为1个单位
            {
                minDistance = distance;
                bestMatch = enemy;
            }
        }
        
        if (bestMatch != null)
        {
            // 为最佳匹配的敌人添加SaveableEnemy组件
            SaveableEnemy saveableComponent = bestMatch.GetComponent<SaveableEnemy>();
            if (saveableComponent == null)
            {
                saveableComponent = bestMatch.gameObject.AddComponent<SaveableEnemy>();
            }
            
            saveableComponent.Initialize(data.enemyID, bestMatch);
            saveableComponent.LoadEnemyData(data);
            
            if (data.isActive && data.currentHealth > 0)
            {
                bestMatch.gameObject.SetActive(true);
                
                if (enableDebugLog)
                    Debug.Log($"[EnemySaveAdapter] 通过位置恢复敌人: {data.enemyID} (血量: {data.currentHealth}/{data.maxHealth}, 距离: {minDistance:F2})");
            }
            else
            {
                if (enableDebugLog)
                    Debug.Log($"[EnemySaveAdapter] 敌人已死亡或非活跃，保持禁用状态: {data.enemyID}");
            }
        }
        else
        {
            if (enableDebugLog)
                Debug.LogWarning($"[EnemySaveAdapter] 未找到匹配的敌人: {data.enemyID} 位置: {data.position}");
        }
    }
    
    /// <summary>
    /// 获取场景中的敌人数量
    /// </summary>
    public int GetEnemyCount()
    {
        return sceneEnemies.Count;
    }
    
    /// <summary>
    /// 获取活跃敌人数量
    /// </summary>
    public int GetActiveEnemyCount()
    {
        int count = 0;
        foreach (Enemy enemy in sceneEnemies)
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                count++;
            }
        }
        return count;
    }
    
    /// <summary>
    /// 获取死亡敌人数量
    /// </summary>
    public int GetDeadEnemyCount()
    {
        return deadEnemiesRecord.Count;
    }
    
    /// <summary>
    /// 通过反射获取私有字段值 (用于调试)
    /// </summary>
    private T GetPrivateFieldValue<T>(object obj, string fieldName)
    {
        try
        {
            var field = obj.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                return (T)field.GetValue(obj);
            }
        }
        catch (System.Exception e)
        {
            if (enableDebugLog)
                Debug.LogWarning($"[EnemySaveAdapter] 无法获取字段 {fieldName}: {e.Message}");
        }
        
        return default(T);
    }
}

/// <summary>
/// 敌人死亡监控组件 - 在敌人死亡时记录状态
/// </summary>
public class EnemyDeathMonitor : MonoBehaviour
{
    private string enemyID;
    private EnemySaveAdapter adapter;
    private Enemy enemyComponent;
    private bool hasRecordedDeath = false;
    
    public void Initialize(string id, EnemySaveAdapter saveAdapter)
    {
        enemyID = id;
        adapter = saveAdapter;
        enemyComponent = GetComponent<Enemy>();
    }
    
    private void Update()
    {
        // 检查敌人是否死亡但还没记录
        if (!hasRecordedDeath && enemyComponent != null && IsEnemyDead())
        {
            RecordDeathState();
            hasRecordedDeath = true;
        }
    }
    
    private bool IsEnemyDead()
    {
        if (enemyComponent == null) return true;
        
        // 通过反射获取isDead字段
        var isDead = GetPrivateFieldValue<bool>(enemyComponent, "isDead");
        var currentHealth = GetPrivateFieldValue<int>(enemyComponent, "currentHealth");
        
        return isDead || currentHealth <= 0;
    }
    
    private void RecordDeathState()
    {
        if (adapter != null && enemyComponent != null)
        {
            // 创建死亡时的敌人数据
            EnemyData deathData = new EnemyData
            {
                enemyID = enemyID,
                enemyType = "Enemy",
                position = transform.position,
                isActive = false, // 死亡敌人设为非活跃
                currentHealth = 0, // 死亡敌人血量为0
                maxHealth = GetPrivateFieldValue<int>(enemyComponent, "maxHealth")
            };
            
            adapter.RecordEnemyDeath(enemyID, deathData);
        }
    }
    
    /// <summary>
    /// 通过反射获取私有字段值
    /// </summary>
    private T GetPrivateFieldValue<T>(object obj, string fieldName)
    {
        try
        {
            var field = obj.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                return (T)field.GetValue(obj);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[EnemyDeathMonitor] 无法获取字段 {fieldName}: {e.Message}");
        }
        
        return default(T);
    }
}

/// <summary>
/// 可保存的敌人组件 - 附加到Enemy对象上
/// </summary>
public class SaveableEnemy : MonoBehaviour
{
    private string enemyID;
    private Enemy enemyComponent;
    
    /// <summary>
    /// 初始化
    /// </summary>
    public void Initialize(string id, Enemy enemy)
    {
        enemyID = id;
        enemyComponent = enemy;
    }
    
    /// <summary>
    /// 获取敌人ID
    /// </summary>
    public string GetEnemyID()
    {
        return enemyID;
    }
    
    /// <summary>
    /// 检查敌人是否活跃
    /// </summary>
    public bool IsActive()
    {
        return enemyComponent != null && gameObject.activeInHierarchy && !IsEnemyDead();
    }
    
    /// <summary>
    /// 检查敌人是否死亡
    /// </summary>
    private bool IsEnemyDead()
    {
        if (enemyComponent == null) return true;
        
        // 通过反射获取isDead字段
        var isDead = GetPrivateFieldValue<bool>(enemyComponent, "isDead");
        var currentHealth = GetPrivateFieldValue<int>(enemyComponent, "currentHealth");
        
        return isDead || currentHealth <= 0;
    }
    
    /// <summary>
    /// 获取敌人数据 - 使用现有的EnemyData结构
    /// </summary>
    public EnemyData GetEnemyData()
    {
        if (enemyComponent == null)
        {
            Debug.LogWarning($"SaveableEnemy {enemyID}: Enemy组件为空");
            return null;
        }
        
        int currentHealth = GetPrivateFieldValue<int>(enemyComponent, "currentHealth");
        int maxHealth = GetPrivateFieldValue<int>(enemyComponent, "maxHealth");
        bool isActive = IsActive();
        
        EnemyData data = new EnemyData
        {
            enemyID = enemyID,
            enemyType = "Enemy", // 默认敌人类型
            position = transform.position,
            isActive = isActive,
            currentHealth = currentHealth,
            maxHealth = maxHealth
        };
        
        // 添加调试信息
        if (EnemySaveAdapter.Instance != null && EnemySaveAdapter.Instance.EnableDebugLog)
        {
            Debug.Log($"[SaveableEnemy] 生成敌人数据: {enemyID} " +
                     $"位置: {transform.position} " +
                     $"血量: {currentHealth}/{maxHealth} " +
                     $"活跃: {isActive} " +
                     $"GameObject.active: {gameObject.activeInHierarchy}");
        }
        
        return data;
    }
    
    /// <summary>
    /// 加载敌人数据 - 适配现有的EnemyData结构
    /// </summary>
    public void LoadEnemyData(EnemyData data)
    {
        if (enemyComponent == null)
        {
            Debug.LogWarning($"SaveableEnemy {enemyID}: Enemy组件为空，无法加载数据");
            return;
        }
        
        // 设置位置
        transform.position = data.position;
        
        // 设置血量
        SetPrivateFieldValue(enemyComponent, "currentHealth", data.currentHealth);
        SetPrivateFieldValue(enemyComponent, "maxHealth", data.maxHealth);
        
        // 根据血量判断是否死亡
        bool isDead = data.currentHealth <= 0;
        SetPrivateFieldValue(enemyComponent, "isDead", isDead);
        
        // 如果敌人已死亡，禁用相关组件
        if (isDead)
        {
            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }
            
            enemyComponent.enabled = false;
        }
        else
        {
            // 如果敌人活着，确保组件启用
            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = true;
            }
            
            enemyComponent.enabled = true;
        }
        
        // 设置活跃状态
        gameObject.SetActive(data.isActive);
    }
    
    /// <summary>
    /// 通过反射获取私有字段值
    /// </summary>
    private T GetPrivateFieldValue<T>(object obj, string fieldName)
    {
        try
        {
            var field = obj.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                return (T)field.GetValue(obj);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[SaveableEnemy] 无法获取字段 {fieldName}: {e.Message}");
        }
        
        return default(T);
    }
    
    /// <summary>
    /// 通过反射设置私有字段值
    /// </summary>
    private void SetPrivateFieldValue(object obj, string fieldName, object value)
    {
        try
        {
            var field = obj.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                field.SetValue(obj, value);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[SaveableEnemy] 无法设置字段 {fieldName}: {e.Message}");
        }
    }
} 