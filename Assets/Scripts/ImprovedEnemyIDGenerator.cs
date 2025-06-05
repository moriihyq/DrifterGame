using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 改进的敌人ID生成器 - 确保ID在不同游戏会话间保持一致
/// </summary>
public static class ImprovedEnemyIDGenerator
{
    /// <summary>
    /// 为场景中的所有敌人生成一致的ID
    /// </summary>
    public static Dictionary<Enemy, string> GenerateConsistentEnemyIDs()
    {
        Dictionary<Enemy, string> enemyIDMap = new Dictionary<Enemy, string>();
        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        
        // 按位置排序以确保一致性
        var sortedEnemies = enemies.OrderBy(e => e.transform.position.x)
                                 .ThenBy(e => e.transform.position.y)
                                 .ThenBy(e => e.transform.position.z)
                                 .ToArray();
        
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        for (int i = 0; i < sortedEnemies.Length; i++)
        {
            Enemy enemy = sortedEnemies[i];
            string enemyID = GenerateConsistentID(enemy, i, sceneName);
            enemyIDMap[enemy] = enemyID;
        }
        
        return enemyIDMap;
    }
    
    /// <summary>
    /// 为单个敌人生成一致的ID
    /// </summary>
    public static string GenerateConsistentID(Enemy enemy, int sortedIndex, string sceneName)
    {
        Vector3 pos = enemy.transform.position;
        
        // 使用位置的哈希值来生成一致的ID
        int positionHash = GetPositionHash(pos);
        
        // 组合场景名、排序索引和位置哈希
        return $"{sceneName}_Enemy_{sortedIndex:D3}_{positionHash:X8}";
    }
    
    /// <summary>
    /// 计算位置的哈希值
    /// </summary>
    private static int GetPositionHash(Vector3 position)
    {
        // 四舍五入到小数点后1位以避免浮点精度问题
        int x = Mathf.RoundToInt(position.x * 10);
        int y = Mathf.RoundToInt(position.y * 10);
        int z = Mathf.RoundToInt(position.z * 10);
        
        // 使用位运算组合哈希值
        return (x << 16) ^ (y << 8) ^ z;
    }
    
    /// <summary>
    /// 验证ID的唯一性
    /// </summary>
    public static bool ValidateIDUniqueness(Dictionary<Enemy, string> enemyIDMap)
    {
        var allIDs = enemyIDMap.Values.ToList();
        var uniqueIDs = allIDs.Distinct().ToList();
        
        bool isUnique = allIDs.Count == uniqueIDs.Count;
        
        if (!isUnique)
        {
            Debug.LogWarning($"[ImprovedEnemyIDGenerator] 发现重复ID: 总数 {allIDs.Count}, 唯一数 {uniqueIDs.Count}");
            
            // 找出重复的ID
            var duplicates = allIDs.GroupBy(id => id)
                                  .Where(g => g.Count() > 1)
                                  .Select(g => g.Key);
            
            foreach (string duplicateID in duplicates)
            {
                Debug.LogWarning($"[ImprovedEnemyIDGenerator] 重复ID: {duplicateID}");
            }
        }
        
        return isUnique;
    }
    
    /// <summary>
    /// 根据位置查找最匹配的敌人
    /// </summary>
    public static Enemy FindBestMatchingEnemy(Vector3 targetPosition, Enemy[] availableEnemies, float maxDistance = 2f)
    {
        Enemy bestMatch = null;
        float minDistance = float.MaxValue;
        
        foreach (Enemy enemy in availableEnemies)
        {
            if (enemy == null) continue;
            
            float distance = Vector3.Distance(enemy.transform.position, targetPosition);
            if (distance < minDistance && distance <= maxDistance)
            {
                minDistance = distance;
                bestMatch = enemy;
            }
        }
        
        return bestMatch;
    }
    
    /// <summary>
    /// 重新映射存档中的敌人ID到当前场景的敌人
    /// </summary>
    public static Dictionary<string, Enemy> RemapSavedEnemyIDs(List<EnemyData> savedEnemyData)
    {
        Dictionary<string, Enemy> remappedEnemies = new Dictionary<string, Enemy>();
        Enemy[] currentEnemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        List<Enemy> unmatchedEnemies = currentEnemies.ToList();
        
        // 首先尝试精确位置匹配
        foreach (EnemyData savedData in savedEnemyData)
        {
            Enemy matchedEnemy = FindBestMatchingEnemy(savedData.position, unmatchedEnemies.ToArray(), 0.5f);
            if (matchedEnemy != null)
            {
                remappedEnemies[savedData.enemyID] = matchedEnemy;
                unmatchedEnemies.Remove(matchedEnemy);
            }
        }
        
        // 然后尝试较宽松的位置匹配
        foreach (EnemyData savedData in savedEnemyData)
        {
            if (remappedEnemies.ContainsKey(savedData.enemyID)) continue;
            
            Enemy matchedEnemy = FindBestMatchingEnemy(savedData.position, unmatchedEnemies.ToArray(), 2f);
            if (matchedEnemy != null)
            {
                remappedEnemies[savedData.enemyID] = matchedEnemy;
                unmatchedEnemies.Remove(matchedEnemy);
                
                Debug.LogWarning($"[ImprovedEnemyIDGenerator] 使用宽松匹配: {savedData.enemyID} -> {matchedEnemy.name}");
            }
        }
        
        return remappedEnemies;
    }
} 