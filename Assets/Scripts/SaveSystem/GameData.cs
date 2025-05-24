using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    // 存档信息
    public string saveName;
    public DateTime saveTime;
    public string sceneName;
    
    // 玩家数据
    public PlayerData playerData;
    
    // 敌人数据列表
    public List<EnemyData> enemiesData;
    
    // 游戏进度数据
    public GameProgressData progressData;
    
    public GameData()
    {
        playerData = new PlayerData();
        enemiesData = new List<EnemyData>();
        progressData = new GameProgressData();
        saveTime = DateTime.Now;
    }
}

[System.Serializable]
public class PlayerData
{
    public int currentHealth;
    public int maxHealth;
    public Vector3 position;
    public bool isFacingRight;
    
    public PlayerData()
    {
        currentHealth = 10;
        maxHealth = 10;
        position = Vector3.zero;
        isFacingRight = true;
    }
}

[System.Serializable]
public class EnemyData
{
    public string enemyID;
    public string enemyType;
    public int currentHealth;
    public int maxHealth;
    public Vector3 position;
    public bool isActive;
    
    public EnemyData()
    {
        enemyID = "";
        enemyType = "";
        currentHealth = 100;
        maxHealth = 100;
        position = Vector3.zero;
        isActive = true;
    }
}

[System.Serializable]
public class GameProgressData
{
    public int currentLevel;
    public float playTime;
    public int score;
    
    public GameProgressData()
    {
        currentLevel = 1;
        playTime = 0f;
        score = 0;
    }
} 