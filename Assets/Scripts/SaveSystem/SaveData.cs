using System;
using UnityEngine;

[Serializable]
public class SaveData
{
    // 存档元数据
    public string saveName;
    public DateTime saveTime;
    public int saveSlot;
    
    // 玩家数据
    public Vector3 playerPosition;
    public int playerHealth;
    public int playerMoney;
    
    // 场景数据
    public string currentScene;
    
    // 游戏进度数据
    public float playTime;
    public int currentLevel;
    
    // 构造函数
    public SaveData()
    {
        saveName = "新存档";
        saveTime = DateTime.Now;
        saveSlot = 0;
        playerPosition = Vector3.zero;
        playerHealth = 100;
        playerMoney = 0;
        currentScene = "5.5 map";
        playTime = 0f;
        currentLevel = 1;
    }
} 