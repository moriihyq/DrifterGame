using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人检测时机调试器 - 专门分析间歇性检测问题
/// </summary>
public class EnemyDetectionTimingDebugger : MonoBehaviour
{
    [Header("调试设置")]
    [SerializeField] private bool enableDetailedLogging = true;
    [SerializeField] private float detectionInterval = 1f; // 检测间隔（秒）
    [SerializeField] private bool autoStartDetection = true;
    
    [Header("统计信息")]
    [SerializeField] private int totalDetectionCycles = 0;
    [SerializeField] private int successfulDetections = 0;
    [SerializeField] private int failedDetections = 0;
    [SerializeField] private int lastDetectedEnemyCount = 0;
    
    private Dictionary<string, int> detectionHistory = new Dictionary<string, int>();
    private List<DetectionRecord> detectionRecords = new List<DetectionRecord>();
    private bool isDetecting = false;
    
    [System.Serializable]
    public class DetectionRecord
    {
        public float time;
        public int enemyCount;
        public int saveableEnemyCount;
        public string sceneState;
        public bool adapterExists;
        public bool adapterInitialized;
        
        public DetectionRecord(float t, int ec, int sec, string ss, bool ae, bool ai)
        {
            time = t;
            enemyCount = ec;
            saveableEnemyCount = sec;
            sceneState = ss;
            adapterExists = ae;
            adapterInitialized = ai;
        }
    }
    
    private void Start()
    {
        if (autoStartDetection)
        {
            StartDetection();
        }
    }
    
    private void Update()
    {
        // 键盘快捷键
        if (Input.GetKeyDown(KeyCode.D))
        {
            PerformSingleDetection();
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            StartDetection();
        }
        
        if (Input.GetKeyDown(KeyCode.S))
        {
            StopDetection();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetStatistics();
        }
        
        if (Input.GetKeyDown(KeyCode.A))
        {
            AnalyzeDetectionPatterns();
        }
    }
    
    /// <summary>
    /// 开始持续检测
    /// </summary>
    public void StartDetection()
    {
        if (!isDetecting)
        {
            isDetecting = true;
            StartCoroutine(ContinuousDetection());
            
            if (enableDetailedLogging)
                Debug.Log("[EnemyDetectionTimingDebugger] 开始持续检测，间隔: " + detectionInterval + "秒");
        }
    }
    
    /// <summary>
    /// 停止持续检测
    /// </summary>
    public void StopDetection()
    {
        isDetecting = false;
        
        if (enableDetailedLogging)
            Debug.Log("[EnemyDetectionTimingDebugger] 停止持续检测");
    }
    
    /// <summary>
    /// 持续检测协程
    /// </summary>
    private IEnumerator ContinuousDetection()
    {
        while (isDetecting)
        {
            PerformSingleDetection();
            yield return new WaitForSeconds(detectionInterval);
        }
    }
    
    /// <summary>
    /// 执行单次检测
    /// </summary>
    public void PerformSingleDetection()
    {
        totalDetectionCycles++;
        
        if (enableDetailedLogging)
            Debug.Log($"[EnemyDetectionTimingDebugger] === 检测周期 #{totalDetectionCycles} ===");
        
        // 获取场景状态信息
        string sceneState = GetSceneState();
        bool adapterExists = EnemySaveAdapter.Instance != null;
        bool adapterInitialized = false;
        
        // 检测敌人数量
        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        SaveableEnemy[] saveableEnemies = Object.FindObjectsByType<SaveableEnemy>(FindObjectsSortMode.None);
        
        if (adapterExists && EnemySaveAdapter.Instance != null)
        {
            adapterInitialized = EnemySaveAdapter.Instance.GetEnemyCount() > 0;
        }
        
        // 记录检测结果
        DetectionRecord record = new DetectionRecord(
            Time.time,
            enemies.Length,
            saveableEnemies.Length,
            sceneState,
            adapterExists,
            adapterInitialized
        );
        
        detectionRecords.Add(record);
        
        // 保持记录列表大小
        if (detectionRecords.Count > 50)
        {
            detectionRecords.RemoveAt(0);
        }
        
        // 分析检测结果
        AnalyzeSingleDetection(record, enemies, saveableEnemies);
        
        lastDetectedEnemyCount = enemies.Length;
    }
    
    /// <summary>
    /// 分析单次检测结果
    /// </summary>
    private void AnalyzeSingleDetection(DetectionRecord record, Enemy[] enemies, SaveableEnemy[] saveableEnemies)
    {
        bool detectionSuccessful = enemies.Length > 0 && enemies.Length == saveableEnemies.Length;
        
        if (detectionSuccessful)
        {
            successfulDetections++;
        }
        else
        {
            failedDetections++;
        }
        
        if (enableDetailedLogging)
        {
            Debug.Log($"[EnemyDetectionTimingDebugger] 检测结果: {(detectionSuccessful ? "✅ 成功" : "❌ 失败")}");
            Debug.Log($"[EnemyDetectionTimingDebugger] Enemy组件数量: {enemies.Length}");
            Debug.Log($"[EnemyDetectionTimingDebugger] SaveableEnemy组件数量: {saveableEnemies.Length}");
            Debug.Log($"[EnemyDetectionTimingDebugger] 场景状态: {record.sceneState}");
            Debug.Log($"[EnemyDetectionTimingDebugger] 适配器存在: {record.adapterExists}");
            Debug.Log($"[EnemyDetectionTimingDebugger] 适配器已初始化: {record.adapterInitialized}");
            
            // 详细分析每个敌人
            for (int i = 0; i < enemies.Length; i++)
            {
                Enemy enemy = enemies[i];
                SaveableEnemy saveable = enemy.GetComponent<SaveableEnemy>();
                
                Debug.Log($"[EnemyDetectionTimingDebugger] 敌人 #{i}: {enemy.name} " +
                         $"(位置: {enemy.transform.position}) " +
                         $"SaveableEnemy: {(saveable != null ? "✅" : "❌")} " +
                         $"活跃: {enemy.gameObject.activeInHierarchy}");
            }
        }
        
        // 检查异常情况
        CheckForAnomalies(record, enemies, saveableEnemies);
    }
    
    /// <summary>
    /// 检查异常情况
    /// </summary>
    private void CheckForAnomalies(DetectionRecord record, Enemy[] enemies, SaveableEnemy[] saveableEnemies)
    {
        List<string> anomalies = new List<string>();
        
        // 检查1: Enemy和SaveableEnemy数量不匹配
        if (enemies.Length != saveableEnemies.Length)
        {
            anomalies.Add($"组件数量不匹配: Enemy({enemies.Length}) vs SaveableEnemy({saveableEnemies.Length})");
        }
        
        // 检查2: 适配器不存在但有敌人
        if (!record.adapterExists && enemies.Length > 0)
        {
            anomalies.Add("适配器不存在但场景中有敌人");
        }
        
        // 检查3: 适配器存在但未初始化
        if (record.adapterExists && !record.adapterInitialized && enemies.Length > 0)
        {
            anomalies.Add("适配器存在但未初始化");
        }
        
        // 检查4: 敌人数量突然变化
        if (detectionRecords.Count > 1)
        {
            var previousRecord = detectionRecords[detectionRecords.Count - 2];
            if (Mathf.Abs(record.enemyCount - previousRecord.enemyCount) > 0)
            {
                anomalies.Add($"敌人数量变化: {previousRecord.enemyCount} -> {record.enemyCount}");
            }
        }
        
        // 检查5: 有敌人但SaveableEnemy为0
        if (enemies.Length > 0 && saveableEnemies.Length == 0)
        {
            anomalies.Add("有敌人但SaveableEnemy组件缺失");
        }
        
        // 输出异常信息
        if (anomalies.Count > 0)
        {
            Debug.LogWarning($"[EnemyDetectionTimingDebugger] ⚠️ 检测到 {anomalies.Count} 个异常:");
            foreach (string anomaly in anomalies)
            {
                Debug.LogWarning($"[EnemyDetectionTimingDebugger] - {anomaly}");
            }
        }
    }
    
    /// <summary>
    /// 获取场景状态
    /// </summary>
    private string GetSceneState()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        return $"{scene.name}(加载状态: {(scene.isLoaded ? "已加载" : "未加载")})";
    }
    
    /// <summary>
    /// 分析检测模式
    /// </summary>
    public void AnalyzeDetectionPatterns()
    {
        if (detectionRecords.Count < 5)
        {
            Debug.LogWarning("[EnemyDetectionTimingDebugger] 检测记录不足，需要至少5次检测才能分析模式");
            return;
        }
        
        Debug.Log("[EnemyDetectionTimingDebugger] === 检测模式分析 ===");
        
        // 分析成功率
        float successRate = (float)successfulDetections / totalDetectionCycles * 100f;
        Debug.Log($"[EnemyDetectionTimingDebugger] 检测成功率: {successRate:F1}% ({successfulDetections}/{totalDetectionCycles})");
        
        // 分析敌人数量变化
        var enemyCounts = new List<int>();
        var saveableEnemyCounts = new List<int>();
        
        foreach (var record in detectionRecords)
        {
            enemyCounts.Add(record.enemyCount);
            saveableEnemyCounts.Add(record.saveableEnemyCount);
        }
        
        int minEnemies = enemyCounts.Count > 0 ? Mathf.Min(enemyCounts.ToArray()) : 0;
        int maxEnemies = enemyCounts.Count > 0 ? Mathf.Max(enemyCounts.ToArray()) : 0;
        int minSaveable = saveableEnemyCounts.Count > 0 ? Mathf.Min(saveableEnemyCounts.ToArray()) : 0;
        int maxSaveable = saveableEnemyCounts.Count > 0 ? Mathf.Max(saveableEnemyCounts.ToArray()) : 0;
        
        Debug.Log($"[EnemyDetectionTimingDebugger] Enemy数量范围: {minEnemies} - {maxEnemies}");
        Debug.Log($"[EnemyDetectionTimingDebugger] SaveableEnemy数量范围: {minSaveable} - {maxSaveable}");
        
        // 查找问题模式
        if (maxEnemies > minEnemies)
        {
            Debug.LogWarning("[EnemyDetectionTimingDebugger] ⚠️ 检测到敌人数量不稳定");
        }
        
        if (maxSaveable != maxEnemies)
        {
            Debug.LogWarning("[EnemyDetectionTimingDebugger] ⚠️ 检测到SaveableEnemy组件数量异常");
        }
        
        // 最近5次检测的详细信息
        Debug.Log("[EnemyDetectionTimingDebugger] 最近5次检测:");
        int startIndex = Mathf.Max(0, detectionRecords.Count - 5);
        for (int i = startIndex; i < detectionRecords.Count; i++)
        {
            var record = detectionRecords[i];
            Debug.Log($"[EnemyDetectionTimingDebugger] #{i + 1}: E={record.enemyCount}, SE={record.saveableEnemyCount}, 时间={record.time:F1}s");
        }
    }
    
    /// <summary>
    /// 重置统计信息
    /// </summary>
    public void ResetStatistics()
    {
        totalDetectionCycles = 0;
        successfulDetections = 0;
        failedDetections = 0;
        detectionRecords.Clear();
        detectionHistory.Clear();
        
        if (enableDetailedLogging)
            Debug.Log("[EnemyDetectionTimingDebugger] 统计信息已重置");
    }
    
    /// <summary>
    /// 强制重新初始化敌人适配器
    /// </summary>
    public void ForceReinitializeAdapter()
    {
        if (EnemySaveAdapter.Instance != null)
        {
            EnemySaveAdapter.Instance.InitializeEnemies();
            Debug.Log("[EnemyDetectionTimingDebugger] 已强制重新初始化敌人适配器");
        }
        else
        {
            Debug.LogWarning("[EnemyDetectionTimingDebugger] 敌人适配器不存在，无法重新初始化");
        }
    }
    
    private void OnGUI()
    {
        // 图形化界面已禁用 - 如需重新启用，将下面的return注释掉
        return;
        
        if (!enableDetailedLogging) return;
        
        GUILayout.BeginArea(new Rect(10, 150, 400, 300));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("敌人检测时机调试器", GUI.skin.label);
        GUILayout.Space(5);
        
        GUILayout.Label($"检测周期: {totalDetectionCycles}");
        GUILayout.Label($"成功: {successfulDetections} | 失败: {failedDetections}");
        GUILayout.Label($"最后检测敌人数: {lastDetectedEnemyCount}");
        GUILayout.Label($"状态: {(isDetecting ? "正在检测" : "已停止")}");
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("D - 单次检测"))
        {
            PerformSingleDetection();
        }
        
        if (GUILayout.Button(isDetecting ? "S - 停止检测" : "C - 开始检测"))
        {
            if (isDetecting)
                StopDetection();
            else
                StartDetection();
        }
        
        if (GUILayout.Button("A - 分析模式"))
        {
            AnalyzeDetectionPatterns();
        }
        
        if (GUILayout.Button("强制重新初始化"))
        {
            ForceReinitializeAdapter();
        }
        
        if (GUILayout.Button("R - 重置统计"))
        {
            ResetStatistics();
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
} 