using UnityEngine;

// 这个类用于自动设置游戏中的图层关系
public class LayerSetup : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("LayerSetup: 正在设置游戏图层...");
        
        // 检查是否存在Enemy和Player图层
        int enemyLayerIndex = LayerMask.NameToLayer("Enemy");
        int playerLayerIndex = LayerMask.NameToLayer("Player");
        
        Debug.Log("Enemy层索引: " + enemyLayerIndex + ", Player层索引: " + playerLayerIndex);
        
        // 如果Enemy图层不存在，尝试找一个可用的图层并重命名
        if (enemyLayerIndex == -1)
        {
            Debug.LogWarning("未找到Enemy图层！请在Unity编辑器中创建此图层 (Edit > Project Settings > Tags and Layers)");
            
            // 提示如何创建图层
            Debug.Log("=== 创建图层指南 ===");
            Debug.Log("1. 在Unity菜单中选择 Edit > Project Settings");
            Debug.Log("2. 选择左侧的Tags and Layers");
            Debug.Log("3. 在Layers部分，找到一个未使用的层(例如User Layer 8)");
            Debug.Log("4. 将其命名为'Enemy'");
            Debug.Log("5. 将所有敌人对象设置为此图层");
            Debug.Log("================");
        }
        
        // 自动查找所有敌人对象并设置到Enemy层(如果该层存在)
        if (enemyLayerIndex != -1)
        {
            Enemy[] enemies = FindObjectsOfType<Enemy>();
            foreach (Enemy enemy in enemies)
            {
                if (enemy.gameObject.layer != enemyLayerIndex)
                {
                    enemy.gameObject.layer = enemyLayerIndex;
                    Debug.Log("已将敌人 " + enemy.name + " 设置为Enemy层");
                }
            }
        }
        
        // 查找玩家对象并确认其在Player层
        PlayerAttackSystem[] players = FindObjectsOfType<PlayerAttackSystem>();
        if (players.Length > 0 && playerLayerIndex != -1)
        {
            foreach (PlayerAttackSystem player in players)
            {
                if (player.gameObject.layer != playerLayerIndex)
                {
                    player.gameObject.layer = playerLayerIndex;
                    Debug.Log("已将玩家 " + player.name + " 设置为Player层");
                }
            }
        }
        
        // 为所有没有碰撞器的敌人添加碰撞器
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in allEnemies)
        {
            Collider2D collider = enemy.GetComponent<Collider2D>();
            if (collider == null)
            {
                BoxCollider2D boxCollider = enemy.gameObject.AddComponent<BoxCollider2D>();
                boxCollider.size = new Vector2(1f, 1f); // 默认大小，可能需要调整
                Debug.Log("为敌人 " + enemy.name + " 添加了碰撞器");
            }
        }
    }
}
