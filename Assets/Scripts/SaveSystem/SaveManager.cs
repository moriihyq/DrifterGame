using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    
    private string saveDirectory;
    private const string SAVE_FILE_PREFIX = "save_";
    private const string SAVE_FILE_EXTENSION = ".json";
    private const int MAX_SAVE_SLOTS = 3;
    
    // 自动存档设置
    [Header("自动存档设置")]
    public float autoSaveInterval = 60f; // 每60秒自动存档一次
    private float autoSaveTimer;
    private bool isAutoSaveEnabled = true;
    
    // UI引用
    [Header("UI引用")]
    public SaveMessageUI saveMessageUI; // 存档成功提示UI
    public float messageDisplayTime = 2f;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSaveSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeSaveSystem()
    {
        // 创建存档目录
        saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }
        
        Debug.Log($"存档目录: {saveDirectory}");
    }
    
    private void Update()
    {
        // 自动存档计时
        if (isAutoSaveEnabled && SceneManager.GetActiveScene().name != "MainMenuScene")
        {
            autoSaveTimer += Time.deltaTime;
            if (autoSaveTimer >= autoSaveInterval)
            {
                AutoSave();
                autoSaveTimer = 0f;
            }
        }
    }
    
    public void SaveGame(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MAX_SAVE_SLOTS)
        {
            Debug.LogError($"无效的存档槽位: {slotIndex}");
            return;
        }
        
        GameData gameData = CollectGameData();
        gameData.saveName = $"存档 {slotIndex + 1}";
        
        string fileName = $"{SAVE_FILE_PREFIX}{slotIndex}{SAVE_FILE_EXTENSION}";
        string filePath = Path.Combine(saveDirectory, fileName);
        
        try
        {
            string json = JsonUtility.ToJson(gameData, true);
            File.WriteAllText(filePath, json);
            Debug.Log($"游戏已保存到槽位 {slotIndex}");
            ShowSaveMessage("游戏已保存");
        }
        catch (Exception e)
        {
            Debug.LogError($"保存游戏失败: {e.Message}");
            ShowSaveMessage("保存失败", true);
        }
    }
    
    public void AutoSave()
    {
        // 找到最近使用的存档槽位或使用第一个槽位
        int slotToUse = GetAutoSaveSlot();
        SaveGame(slotToUse);
        Debug.Log("自动存档完成");
    }
    
    private int GetAutoSaveSlot()
    {
        // 这里可以实现更复杂的逻辑，比如找到最近使用的槽位
        // 现在简单地使用第一个槽位
        return 0;
    }
    
    public GameData LoadGame(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MAX_SAVE_SLOTS)
        {
            Debug.LogError($"无效的存档槽位: {slotIndex}");
            return null;
        }
        
        string fileName = $"{SAVE_FILE_PREFIX}{slotIndex}{SAVE_FILE_EXTENSION}";
        string filePath = Path.Combine(saveDirectory, fileName);
        
        if (File.Exists(filePath))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                GameData gameData = JsonUtility.FromJson<GameData>(json);
                Debug.Log($"从槽位 {slotIndex} 加载游戏");
                return gameData;
            }
            catch (Exception e)
            {
                Debug.LogError($"加载游戏失败: {e.Message}");
                return null;
            }
        }
        else
        {
            Debug.Log($"槽位 {slotIndex} 没有存档");
            return null;
        }
    }
    
    public void LoadGameAndApply(int slotIndex)
    {
        GameData gameData = LoadGame(slotIndex);
        if (gameData != null)
        {
            StartCoroutine(LoadGameCoroutine(gameData));
        }
    }
    
    private System.Collections.IEnumerator LoadGameCoroutine(GameData gameData)
    {
        // 加载场景
        if (SceneManager.GetActiveScene().name != gameData.sceneName)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(gameData.sceneName);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
        
        // 等待一帧确保场景完全加载
        yield return null;
        
        // 应用存档数据
        ApplyGameData(gameData);
    }
    
    private GameData CollectGameData()
    {
        GameData gameData = new GameData();
        gameData.sceneName = SceneManager.GetActiveScene().name;
        
        // 收集玩家数据
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            gameData.playerData = new PlayerData
            {
                currentHealth = player.currentHealth,
                maxHealth = player.maxHealth,
                position = player.transform.position,
                isFacingRight = !player.GetComponent<SpriteRenderer>().flipX
            };
        }
        
        // 收集敌人数据
        gameData.enemiesData = new List<EnemyData>();
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemyObj in enemies)
        {
            EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();
            if (enemy != null && enemy.isActive)
            {
                gameData.enemiesData.Add(enemy.GetEnemyData());
            }
        }
        
        return gameData;
    }
    
    private void ApplyGameData(GameData gameData)
    {
        // 应用玩家数据
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && gameData.playerData != null)
        {
            player.currentHealth = gameData.playerData.currentHealth;
            player.maxHealth = gameData.playerData.maxHealth;
            player.transform.position = gameData.playerData.position;
            player.GetComponent<SpriteRenderer>().flipX = !gameData.playerData.isFacingRight;
        }
        
        // 应用敌人数据
        // 这里需要根据你的敌人系统来实现
        // 例如：通过敌人管理器重新生成敌人，或者寻找场景中的敌人并更新它们的状态
    }
    
    public List<SaveInfo> GetAllSaveInfos()
    {
        List<SaveInfo> saveInfos = new List<SaveInfo>();
        
        for (int i = 0; i < MAX_SAVE_SLOTS; i++)
        {
            string fileName = $"{SAVE_FILE_PREFIX}{i}{SAVE_FILE_EXTENSION}";
            string filePath = Path.Combine(saveDirectory, fileName);
            
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    GameData gameData = JsonUtility.FromJson<GameData>(json);
                    
                    SaveInfo info = new SaveInfo
                    {
                        slotIndex = i,
                        saveName = gameData.saveName,
                        saveTime = gameData.saveTime,
                        sceneName = gameData.sceneName,
                        playerHealth = gameData.playerData.currentHealth,
                        playTime = gameData.progressData.playTime
                    };
                    
                    saveInfos.Add(info);
                }
                catch (Exception e)
                {
                    Debug.LogError($"读取存档信息失败: {e.Message}");
                }
            }
            else
            {
                // 空槽位
                SaveInfo info = new SaveInfo
                {
                    slotIndex = i,
                    saveName = $"空槽位 {i + 1}",
                    isEmpty = true
                };
                saveInfos.Add(info);
            }
        }
        
        return saveInfos;
    }
    
    public void DeleteSave(int slotIndex)
    {
        string fileName = $"{SAVE_FILE_PREFIX}{slotIndex}{SAVE_FILE_EXTENSION}";
        string filePath = Path.Combine(saveDirectory, fileName);
        
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log($"删除槽位 {slotIndex} 的存档");
        }
    }
    
    private void ShowSaveMessage(string message, bool isError = false)
    {
        if (saveMessageUI != null)
        {
            saveMessageUI.ShowMessage(message, isError);
        }
    }
    
    public void SetAutoSaveEnabled(bool enabled)
    {
        isAutoSaveEnabled = enabled;
        if (!enabled)
        {
            autoSaveTimer = 0f;
        }
    }
}

[System.Serializable]
public class SaveInfo
{
    public int slotIndex;
    public string saveName;
    public DateTime saveTime;
    public string sceneName;
    public int playerHealth;
    public float playTime;
    public bool isEmpty;
} 