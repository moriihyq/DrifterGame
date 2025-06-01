using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    // 存档信息
    public string saveName;
    public string saveTimeString; // 使用string代替DateTime
    public string sceneName;
    
    // 玩家数据
    public PlayerData playerData;
    
    // 敌人数据列表
    public List<EnemyData> enemiesData;
    
    // 游戏进度数据
    public GameProgressData progressData;
    
    // 用于访问时间的属性
    public DateTime saveTime 
    { 
        get 
        { 
            if (DateTime.TryParse(saveTimeString, out DateTime result))
                return result;
            return DateTime.MinValue;
        }
        set 
        { 
            saveTimeString = value.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
    
    public GameData()
    {
        playerData = new PlayerData();
        enemiesData = new List<EnemyData>();
        progressData = new GameProgressData();
        // 移除 saveTime = DateTime.Now; 避免反序列化时覆盖保存的时间
    }
}

[System.Serializable]
public class PlayerData
{
    // 基础血量信息
    public int currentHealth;
    public int maxHealth;
    
    // 位置和朝向信息
    public Vector3 position;
    public bool isFacingRight;
    
    // PlayerAttackSystem专用字段
    public int attackDamage;
    public float attackCooldown;
    public float attackRadius;
    public bool isDead;
    
    // 攻击状态
    public float nextAttackTime;
    
    public PlayerData()
    {
        // 默认血量值（从PlayerAttackSystem获取）
        currentHealth = 100;
        maxHealth = 100;
        
        // 默认位置和朝向
        position = Vector3.zero;
        isFacingRight = true;
        
        // 默认攻击设置
        attackDamage = 50;
        attackCooldown = 0.2f;
        attackRadius = 3.0f;
        isDead = false;
        nextAttackTime = 0f;
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