using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    private static SaveManager instance;
    public static SaveManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SaveManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("SaveManager");
                    instance = go.AddComponent<SaveManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    private const int MAX_SAVE_SLOTS = 5; // 最多5个存档位
    private string saveDirectory;
    private float gameStartTime;
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        // 创建存档目录
        saveDirectory = Path.Combine(Application.persistentDataPath, "SaveGames");
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }
        
        gameStartTime = Time.time;
    }

    // 保存游戏到指定槽位
    public void SaveGame(int slot, string saveName = null)
    {
        if (slot < 0 || slot >= MAX_SAVE_SLOTS)
        {
            Debug.LogError("无效的存档槽位: " + slot);
            return;
        }

        SaveData saveData = new SaveData();
        saveData.saveSlot = slot;
        saveData.saveName = saveName ?? $"存档 {slot + 1}";
        saveData.saveTime = DateTime.Now;
        saveData.playTime = Time.time - gameStartTime;
        saveData.currentScene = SceneManager.GetActiveScene().name;

        // 获取玩家数据
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            saveData.playerPosition = player.transform.position;
            
            // 尝试获取玩家组件数据
            var playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                saveData.playerHealth = playerController.currentHealth;
                // PlayerController中暂时没有money属性，先使用默认值
                saveData.playerMoney = 0;
            }
        }

        // 保存到文件
        string json = JsonUtility.ToJson(saveData, true);
        string filePath = GetSaveFilePath(slot);
        File.WriteAllText(filePath, json);
        
        Debug.Log($"游戏已保存到槽位 {slot + 1}");
    }

    // 加载指定槽位的游戏
    public void LoadGame(int slot)
    {
        if (slot < 0 || slot >= MAX_SAVE_SLOTS)
        {
            Debug.LogError("无效的存档槽位: " + slot);
            return;
        }

        string filePath = GetSaveFilePath(slot);
        if (!File.Exists(filePath))
        {
            Debug.LogError($"槽位 {slot + 1} 没有存档");
            return;
        }

        string json = File.ReadAllText(filePath);
        SaveData saveData = JsonUtility.FromJson<SaveData>(json);

        // 加载场景
        SceneManager.LoadScene(saveData.currentScene);
        
        // 场景加载完成后恢复游戏数据
        SceneManager.sceneLoaded += OnSceneLoadedForSave;
        
        // 临时存储要恢复的数据
        tempSaveData = saveData;
    }
    
    private SaveData tempSaveData;

    private void OnSceneLoadedForSave(Scene scene, LoadSceneMode mode)
    {
        // 只执行一次
        SceneManager.sceneLoaded -= OnSceneLoadedForSave;
        
        if (tempSaveData == null) return;

        // 恢复玩家位置和数据
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = tempSaveData.playerPosition;
            
            var playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.currentHealth = tempSaveData.playerHealth;
                // PlayerController中暂时没有money属性，跳过恢复
            }
        }

        gameStartTime = Time.time - tempSaveData.playTime;
        Debug.Log($"游戏已从槽位 {tempSaveData.saveSlot + 1} 加载");
        
        tempSaveData = null;
    }

    // 检查槽位是否有存档
    public bool HasSaveInSlot(int slot)
    {
        if (slot < 0 || slot >= MAX_SAVE_SLOTS) return false;
        return File.Exists(GetSaveFilePath(slot));
    }

    // 获取所有存档信息
    public List<SaveData> GetAllSaves()
    {
        List<SaveData> saves = new List<SaveData>();
        
        for (int i = 0; i < MAX_SAVE_SLOTS; i++)
        {
            string filePath = GetSaveFilePath(i);
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                SaveData saveData = JsonUtility.FromJson<SaveData>(json);
                saves.Add(saveData);
            }
            else
            {
                saves.Add(null); // 空槽位
            }
        }
        
        return saves;
    }

    // 删除指定槽位的存档
    public void DeleteSave(int slot)
    {
        if (slot < 0 || slot >= MAX_SAVE_SLOTS) return;
        
        string filePath = GetSaveFilePath(slot);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log($"槽位 {slot + 1} 的存档已删除");
        }
    }

    private string GetSaveFilePath(int slot)
    {
        return Path.Combine(saveDirectory, $"save_slot_{slot}.json");
    }

    public int GetMaxSaveSlots()
    {
        return MAX_SAVE_SLOTS;
    }
} 