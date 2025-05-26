using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("敌人预制体")]
    public GameObject[] enemyPrefabs;
    
    [Header("敌人列表")]
    public List<GameObject> activeEnemies = new List<GameObject>();
    
    private void Start()
    {
        // 收集场景中所有的敌人
        RefreshEnemyList();
    }
    
    public void RefreshEnemyList()
    {
        activeEnemies.Clear();
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        activeEnemies.AddRange(enemies);
    }
    
    public void RestoreEnemies(List<EnemyData> enemiesData)
    {
        // 清除当前所有敌人
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }
        activeEnemies.Clear();
        
        // 根据存档数据重新生成敌人
        foreach (EnemyData data in enemiesData)
        {
            if (data.isActive)
            {
                // 根据敌人类型找到对应的预制体
                GameObject prefab = GetEnemyPrefabByType(data.enemyType);
                if (prefab != null)
                {
                    GameObject newEnemy = Instantiate(prefab, data.position, Quaternion.identity);
                    EnemyBase enemyBase = newEnemy.GetComponent<EnemyBase>();
                    if (enemyBase != null)
                    {
                        enemyBase.LoadEnemyData(data);
                    }
                    activeEnemies.Add(newEnemy);
                }
            }
        }
    }
    
    private GameObject GetEnemyPrefabByType(string enemyType)
    {
        // 根据敌人类型返回对应的预制体
        foreach (GameObject prefab in enemyPrefabs)
        {
            if (prefab.name.Contains(enemyType))
            {
                return prefab;
            }
        }
        
        // 如果没有找到匹配的，返回第一个预制体
        return enemyPrefabs.Length > 0 ? enemyPrefabs[0] : null;
    }
    
    public void RegisterEnemy(GameObject enemy)
    {
        if (!activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
        }
    }
    
    public void UnregisterEnemy(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }
    }
} 