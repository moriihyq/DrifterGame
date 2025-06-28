using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

/// <summary>
/// 敌人存档数据管理器 - 为每个存档槽位单独管理敌人状态
/// 解决不同存档间敌人死亡记录混乱的问题
/// </summary>
public class EnemySaveDataManager : MonoBehaviour
{
    [Header("敌人存档管理")]
    [SerializeField] private bool enableDebugLog = true;
    
    /// <summary>
    /// 单例实例
    /// </summary>
    public static EnemySaveDataManager Instance { get; private set; }
    
    // 每个存档槽位的敌人死亡记录
    private Dictionary<int, HashSet<string>> slotDeathRecords = new Dictionary<int, HashSet<string>>();
    
    // 当前活动的存档槽位
    private int currentSaveSlot = -1;
    
    // 敌人数据文件路径
    private string enemyDataDirectory;
    private const string ENEMY_DATA_FILE_PREFIX = "enemy_data_slot_";
    private const string ENEMY_DATA_FILE_EXTENSION = ".json";
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeEnemyDataSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 初始化敌人数据系统
    /// </summary>
    private void InitializeEnemyDataSystem()
    {
        // 创建敌人数据目录
        enemyDataDirectory = Path.Combine(Application.persistentDataPath, "EnemyData");
        if (!Directory.Exists(enemyDataDirectory))
        {
            Directory.CreateDirectory(enemyDataDirectory);
        }
        
        if (enableDebugLog)
            Debug.Log($"[EnemySaveDataManager] 敌人数据目录: {enemyDataDirectory}");
        
        // 初始化所有存档槽位的死亡记录
        for (int i = 0; i < 3; i++) // 假设最多3个存档槽位
        {
            slotDeathRecords[i] = new HashSet<string>();
            LoadEnemyDeathRecords(i);
        }
    }
    
    /// <summary>
    /// 设置当前活动的存档槽位
    /// </summary>
    public void SetCurrentSaveSlot(int slotIndex)
    {
        if (currentSaveSlot != slotIndex)
        {
            if (enableDebugLog)
                Debug.Log($"[EnemySaveDataManager] 切换存档槽位: {currentSaveSlot} -> {slotIndex}");
            
            currentSaveSlot = slotIndex;
            
            // 确保当前槽位的死亡记录已加载
            if (!slotDeathRecords.ContainsKey(slotIndex))
            {
                slotDeathRecords[slotIndex] = new HashSet<string>();
                LoadEnemyDeathRecords(slotIndex);
            }
            
            // 通知EnemySaveAdapter当前槽位的死亡记录
            if (EnemySaveAdapter.Instance != null)
            {
                EnemySaveAdapter.Instance.SetDeathRecordsForCurrentSlot(GetCurrentSlotDeathRecords());
            }
        }
    }
    
    /// <summary>
    /// 获取当前存档槽位
    /// </summary>
    public int GetCurrentSaveSlot()
    {
        return currentSaveSlot;
    }
    
    /// <summary>
    /// 记录敌人死亡（仅在当前存档槽位）
    /// </summary>
    public void RecordEnemyDeath(string enemyID)
    {
        if (currentSaveSlot < 0)
        {
            Debug.LogWarning("[EnemySaveDataManager] 当前存档槽位未设置，无法记录敌人死亡");
            return;
        }
        
        if (!slotDeathRecords.ContainsKey(currentSaveSlot))
        {
            slotDeathRecords[currentSaveSlot] = new HashSet<string>();
        }
        
        if (slotDeathRecords[currentSaveSlot].Add(enemyID))
        {
            if (enableDebugLog)
                Debug.Log($"[EnemySaveDataManager] 记录敌人死亡 - 槽位{currentSaveSlot}: {enemyID}");
            
            // 立即保存到文件
            SaveEnemyDeathRecords(currentSaveSlot);
        }
    }
    
    /// <summary>
    /// 检查敌人是否在当前存档槽位中已死亡
    /// </summary>
    public bool IsEnemyDeadInCurrentSlot(string enemyID)
    {
        if (currentSaveSlot < 0 || !slotDeathRecords.ContainsKey(currentSaveSlot))
        {
            return false;
        }
        
        return slotDeathRecords[currentSaveSlot].Contains(enemyID);
    }
    
    /// <summary>
    /// 获取当前槽位的所有死亡记录
    /// </summary>
    public HashSet<string> GetCurrentSlotDeathRecords()
    {
        if (currentSaveSlot < 0 || !slotDeathRecords.ContainsKey(currentSaveSlot))
        {
            return new HashSet<string>();
        }
        
        return new HashSet<string>(slotDeathRecords[currentSaveSlot]);
    }
    
    /// <summary>
    /// 清除指定存档槽位的所有死亡记录
    /// </summary>
    public void ClearSlotDeathRecords(int slotIndex)
    {
        if (slotDeathRecords.ContainsKey(slotIndex))
        {
            slotDeathRecords[slotIndex].Clear();
            SaveEnemyDeathRecords(slotIndex);
            
            if (enableDebugLog)
                Debug.Log($"[EnemySaveDataManager] 清除槽位{slotIndex}的死亡记录");
        }
    }
    
    /// <summary>
    /// 创建新存档时清除死亡记录
    /// </summary>
    public void CreateNewSaveSlot(int slotIndex)
    {
        ClearSlotDeathRecords(slotIndex);
        SetCurrentSaveSlot(slotIndex);
        
        if (enableDebugLog)
            Debug.Log($"[EnemySaveDataManager] 创建新存档槽位: {slotIndex}");
    }
    
    /// <summary>
    /// 加载指定槽位的敌人死亡记录
    /// </summary>
    private void LoadEnemyDeathRecords(int slotIndex)
    {
        string fileName = $"{ENEMY_DATA_FILE_PREFIX}{slotIndex}{ENEMY_DATA_FILE_EXTENSION}";
        string filePath = Path.Combine(enemyDataDirectory, fileName);
        
        if (File.Exists(filePath))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                EnemyDeathRecordData recordData = JsonUtility.FromJson<EnemyDeathRecordData>(json);
                
                if (!slotDeathRecords.ContainsKey(slotIndex))
                {
                    slotDeathRecords[slotIndex] = new HashSet<string>();
                }
                
                slotDeathRecords[slotIndex].Clear();
                foreach (string enemyID in recordData.deadEnemyIDs)
                {
                    slotDeathRecords[slotIndex].Add(enemyID);
                }
                
                if (enableDebugLog)
                    Debug.Log($"[EnemySaveDataManager] 加载槽位{slotIndex}死亡记录: {slotDeathRecords[slotIndex].Count}个敌人");
            }
            catch (Exception e)
            {
                Debug.LogError($"[EnemySaveDataManager] 加载槽位{slotIndex}死亡记录失败: {e.Message}");
            }
        }
        else
        {
            // 文件不存在，初始化为空记录
            if (!slotDeathRecords.ContainsKey(slotIndex))
            {
                slotDeathRecords[slotIndex] = new HashSet<string>();
            }
        }
    }
    
    /// <summary>
    /// 保存指定槽位的敌人死亡记录
    /// </summary>
    private void SaveEnemyDeathRecords(int slotIndex)
    {
        if (!slotDeathRecords.ContainsKey(slotIndex))
        {
            return;
        }
        
        string fileName = $"{ENEMY_DATA_FILE_PREFIX}{slotIndex}{ENEMY_DATA_FILE_EXTENSION}";
        string filePath = Path.Combine(enemyDataDirectory, fileName);
        
        try
        {
            EnemyDeathRecordData recordData = new EnemyDeathRecordData();
            recordData.deadEnemyIDs = new List<string>(slotDeathRecords[slotIndex]);
            recordData.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            string json = JsonUtility.ToJson(recordData, true);
            File.WriteAllText(filePath, json);
            
            if (enableDebugLog)
                Debug.Log($"[EnemySaveDataManager] 保存槽位{slotIndex}死亡记录: {recordData.deadEnemyIDs.Count}个敌人");
        }
        catch (Exception e)
        {
            Debug.LogError($"[EnemySaveDataManager] 保存槽位{slotIndex}死亡记录失败: {e.Message}");
        }
    }
    
    /// <summary>
    /// 删除指定存档槽位的数据文件
    /// </summary>
    public void DeleteSlotData(int slotIndex)
    {
        // 清除内存中的记录
        if (slotDeathRecords.ContainsKey(slotIndex))
        {
            slotDeathRecords[slotIndex].Clear();
        }
        
        // 删除文件
        string fileName = $"{ENEMY_DATA_FILE_PREFIX}{slotIndex}{ENEMY_DATA_FILE_EXTENSION}";
        string filePath = Path.Combine(enemyDataDirectory, fileName);
        
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
                if (enableDebugLog)
                    Debug.Log($"[EnemySaveDataManager] 删除槽位{slotIndex}的敌人数据文件");
            }
            catch (Exception e)
            {
                Debug.LogError($"[EnemySaveDataManager] 删除槽位{slotIndex}数据文件失败: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// 获取所有存档槽位的统计信息
    /// </summary>
    public string GetAllSlotsInfo()
    {
        string info = "[EnemySaveDataManager] 存档槽位信息:\n";
        
        for (int i = 0; i < 3; i++)
        {
            int deadCount = slotDeathRecords.ContainsKey(i) ? slotDeathRecords[i].Count : 0;
            string current = (i == currentSaveSlot) ? " (当前)" : "";
            info += $"  槽位{i}: {deadCount}个死亡敌人{current}\n";
        }
        
        return info;
    }
    
    /// <summary>
    /// 强制刷新当前槽位（用于调试）
    /// </summary>
    [ContextMenu("刷新当前槽位")]
    public void RefreshCurrentSlot()
    {
        if (currentSaveSlot >= 0)
        {
            LoadEnemyDeathRecords(currentSaveSlot);
            
            // 通知EnemySaveAdapter
            if (EnemySaveAdapter.Instance != null)
            {
                EnemySaveAdapter.Instance.SetDeathRecordsForCurrentSlot(GetCurrentSlotDeathRecords());
            }
            
            if (enableDebugLog)
                Debug.Log($"[EnemySaveDataManager] 刷新槽位{currentSaveSlot}完成");
        }
    }
    
    /// <summary>
    /// 清除所有存档槽位的死亡记录（用于重置游戏）
    /// </summary>
    [ContextMenu("清除所有存档")]
    public void ClearAllSlots()
    {
        for (int i = 0; i < 3; i++)
        {
            ClearSlotDeathRecords(i);
        }
        
        if (enableDebugLog)
            Debug.Log("[EnemySaveDataManager] 清除所有存档槽位的死亡记录");
    }
}

/// <summary>
/// 敌人死亡记录数据结构
/// </summary>
[System.Serializable]
public class EnemyDeathRecordData
{
    public List<string> deadEnemyIDs = new List<string>();
    public string saveTime;
} 