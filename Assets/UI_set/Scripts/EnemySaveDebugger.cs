using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人存档调试器 - 用于测试和调试敌人存档功能
/// </summary>
public class EnemySaveDebugger : MonoBehaviour
{
    [Header("调试设置")]
    [SerializeField] private bool enableKeyboardShortcuts = true;
    [SerializeField] private bool showDebugGUI = true;
    [SerializeField] private KeyCode quickSaveKey = KeyCode.F5;
    [SerializeField] private KeyCode quickLoadKey = KeyCode.F9;
    [SerializeField] private KeyCode showEnemyInfoKey = KeyCode.F6;
    [SerializeField] private KeyCode initializeEnemiesKey = KeyCode.F7;
    
    [Header("测试敌人")]
    [SerializeField] private GameObject testEnemyPrefab;
    [SerializeField] private Vector3 spawnPosition = new Vector3(5f, 0f, 0f);
    
    private bool showGUI = false;
    private Vector2 scrollPosition;
    private string debugInfo = "";
    
    private void Update()
    {
        if (!enableKeyboardShortcuts) return;
        
        // F5 - 快速保存
        if (Input.GetKeyDown(quickSaveKey))
        {
            QuickSave();
        }
        
        // F9 - 快速加载
        if (Input.GetKeyDown(quickLoadKey))
        {
            QuickLoad();
        }
        
        // F6 - 显示敌人信息
        if (Input.GetKeyDown(showEnemyInfoKey))
        {
            ShowEnemyInfo();
        }
        
        // F7 - 初始化敌人
        if (Input.GetKeyDown(initializeEnemiesKey))
        {
            InitializeEnemies();
        }
        
        // Tab - 切换GUI显示
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            showGUI = !showGUI;
        }
    }
    
    private void OnGUI()
    {
        // 图形化界面已禁用 - 如需重新启用，将下面的return注释掉
        return;
        
        if (!showDebugGUI || !showGUI) return;
        
        // 创建GUI窗口
        GUILayout.BeginArea(new Rect(10, 10, 400, Screen.height - 20));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("🛠️ 敌人存档调试器", new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold });
        GUILayout.Space(10);
        
        // 快捷键说明
        GUILayout.Label("快捷键操作:");
        GUILayout.Label($"• {quickSaveKey} - 快速保存到插槽0");
        GUILayout.Label($"• {quickLoadKey} - 快速加载插槽0");
        GUILayout.Label($"• {showEnemyInfoKey} - 显示敌人信息");
        GUILayout.Label($"• {initializeEnemiesKey} - 初始化敌人适配器");
        GUILayout.Label("• Tab - 切换调试界面");
        
        GUILayout.Space(10);
        
        // 操作按钮
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("快速保存"))
        {
            QuickSave();
        }
        if (GUILayout.Button("快速加载"))
        {
            QuickLoad();
        }
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("显示敌人信息"))
        {
            ShowEnemyInfo();
        }
        if (GUILayout.Button("初始化敌人"))
        {
            InitializeEnemies();
        }
        GUILayout.EndHorizontal();
        
        if (GUILayout.Button("生成测试敌人"))
        {
            SpawnTestEnemy();
        }
        
        if (GUILayout.Button("伤害所有敌人"))
        {
            DamageAllEnemies();
        }
        
        if (GUILayout.Button("清理调试信息"))
        {
            debugInfo = "";
        }
        
        if (GUILayout.Button("清理死亡记录"))
        {
            ClearDeathRecords();
        }
        
        GUILayout.Space(10);
        
        // 敌人状态信息
        ShowEnemyStatus();
        
        GUILayout.Space(10);
        
        // 调试信息滚动区域
        GUILayout.Label("调试信息:");
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
        GUILayout.TextArea(debugInfo, GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    /// <summary>
    /// 显示敌人状态
    /// </summary>
    private void ShowEnemyStatus()
    {
        GUILayout.Label("敌人状态:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
        
        // 显示EnemySaveAdapter状态
        if (EnemySaveAdapter.Instance != null)
        {
            GUILayout.Label($"✅ EnemySaveAdapter已启用");
            GUILayout.Label($"敌人总数: {EnemySaveAdapter.Instance.GetEnemyCount()}");
            GUILayout.Label($"活跃敌人: {EnemySaveAdapter.Instance.GetActiveEnemyCount()}");
            GUILayout.Label($"死亡敌人: {EnemySaveAdapter.Instance.GetDeadEnemyCount()}");
        }
        else
        {
            GUILayout.Label("❌ EnemySaveAdapter未找到", new GUIStyle(GUI.skin.label) { normal = { textColor = Color.red } });
        }
        
        // 显示场景中的敌人
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        SaveableEnemy[] saveableEnemies = FindObjectsOfType<SaveableEnemy>();
        EnemyDeathMonitor[] deathMonitors = FindObjectsOfType<EnemyDeathMonitor>();
        
        GUILayout.Label($"场景Enemy组件: {enemies.Length}");
        GUILayout.Label($"SaveableEnemy组件: {saveableEnemies.Length}");
        GUILayout.Label($"死亡监控组件: {deathMonitors.Length}");
        
        // 显示SaveManager状态
        if (SaveManager.Instance != null)
        {
            GUILayout.Label("✅ SaveManager已启用");
        }
        else
        {
            GUILayout.Label("❌ SaveManager未找到", new GUIStyle(GUI.skin.label) { normal = { textColor = Color.red } });
        }
    }
    
    /// <summary>
    /// 快速保存
    /// </summary>
    private void QuickSave()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame(0);
            AddDebugMessage("📀 快速保存完成 (插槽0)");
        }
        else
        {
            AddDebugMessage("❌ SaveManager未找到，无法保存");
        }
    }
    
    /// <summary>
    /// 快速加载
    /// </summary>
    private void QuickLoad()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.LoadGameAndApply(0);
            AddDebugMessage("📂 快速加载完成 (插槽0)");
        }
        else
        {
            AddDebugMessage("❌ SaveManager未找到，无法加载");
        }
    }
    
    /// <summary>
    /// 显示敌人信息
    /// </summary>
    private void ShowEnemyInfo()
    {
        AddDebugMessage("=== 敌人信息 ===");
        
        if (EnemySaveAdapter.Instance != null)
        {
            var enemyData = EnemySaveAdapter.Instance.CollectEnemyData();
            AddDebugMessage($"通过EnemySaveAdapter收集到 {enemyData.Count} 个敌人数据:");
            
            foreach (var data in enemyData)
            {
                AddDebugMessage($"• {data.enemyID}: 血量 {data.currentHealth}/{data.maxHealth}, 活跃: {data.isActive}, 位置: {data.position}");
            }
        }
        else
        {
            Enemy[] enemies = FindObjectsOfType<Enemy>();
            AddDebugMessage($"场景中发现 {enemies.Length} 个Enemy组件:");
            
            for (int i = 0; i < enemies.Length; i++)
            {
                Enemy enemy = enemies[i];
                AddDebugMessage($"• {enemy.name}: 位置 {enemy.transform.position}, 活跃: {enemy.gameObject.activeInHierarchy}");
            }
        }
    }
    
    /// <summary>
    /// 初始化敌人
    /// </summary>
    private void InitializeEnemies()
    {
        if (EnemySaveAdapter.Instance != null)
        {
            EnemySaveAdapter.Instance.InitializeEnemies();
            AddDebugMessage("🔄 敌人适配器重新初始化完成");
        }
        else
        {
            AddDebugMessage("❌ 未找到EnemySaveAdapter，无法初始化");
        }
    }
    
    /// <summary>
    /// 生成测试敌人
    /// </summary>
    private void SpawnTestEnemy()
    {
        if (testEnemyPrefab != null)
        {
            GameObject newEnemy = Instantiate(testEnemyPrefab, spawnPosition, Quaternion.identity);
            newEnemy.name = $"TestEnemy_{Time.time:F1}";
            
            // 确保有Enemy组件
            Enemy enemyComponent = newEnemy.GetComponent<Enemy>();
            if (enemyComponent == null)
            {
                enemyComponent = newEnemy.AddComponent<Enemy>();
            }
            
            // 确保有正确的标签
            newEnemy.tag = "Enemy";
            
            AddDebugMessage($"✨ 生成测试敌人: {newEnemy.name} 在位置 {spawnPosition}");
            
            // 重新初始化敌人适配器
            if (EnemySaveAdapter.Instance != null)
            {
                EnemySaveAdapter.Instance.InitializeEnemies();
            }
        }
        else
        {
            AddDebugMessage("❌ 未设置测试敌人预制体");
        }
    }
    
    /// <summary>
    /// 伤害所有敌人
    /// </summary>
    private void DamageAllEnemies()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        int damagedCount = 0;
        
        foreach (Enemy enemy in enemies)
        {
            if (enemy.gameObject.activeInHierarchy)
            {
                enemy.TakeDamage(25); // 造成25点伤害
                damagedCount++;
            }
        }
        
        AddDebugMessage($"⚔️ 对 {damagedCount} 个敌人造成了25点伤害");
    }
    
    /// <summary>
    /// 清理死亡记录
    /// </summary>
    private void ClearDeathRecords()
    {
        if (EnemySaveAdapter.Instance != null)
        {
            int oldCount = EnemySaveAdapter.Instance.GetDeadEnemyCount();
            EnemySaveAdapter.Instance.ClearAllDeathRecords();
            AddDebugMessage($"🧹 已清理 {oldCount} 个死亡敌人记录");
            
            // 重新初始化敌人
            EnemySaveAdapter.Instance.InitializeEnemies();
            AddDebugMessage("🔄 重新初始化敌人适配器");
        }
        else
        {
            AddDebugMessage("❌ 未找到EnemySaveAdapter，无法清理死亡记录");
        }
    }
    
    /// <summary>
    /// 添加调试消息
    /// </summary>
    private void AddDebugMessage(string message)
    {
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        debugInfo += $"[{timestamp}] {message}\n";
        Debug.Log($"[EnemySaveDebugger] {message}");
        
        // 限制调试信息长度
        if (debugInfo.Length > 2000)
        {
            debugInfo = debugInfo.Substring(debugInfo.Length - 1500);
        }
    }
}