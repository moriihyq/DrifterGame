using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CollideableLayerFixer : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/可破坏墙壁/修复Collideable图层")]
    public static void FixCollideableLayer()
    {
        Debug.Log("=== 开始修复Collideable图层 ===");
        
        // 查找所有名为"Collideable"的Tilemap
        Tilemap[] allTilemaps = FindObjectsOfType<Tilemap>();
        int fixedCount = 0;
        
        foreach (Tilemap tilemap in allTilemaps)
        {
            if (tilemap.name.Contains("Collideable"))
            {
                Debug.Log($"找到Collideable图层: {tilemap.name}");
                FixCollideableTilemap(tilemap);
                fixedCount++;
            }
        }
        
        if (fixedCount == 0)
        {
            Debug.LogWarning("未找到任何Collideable图层!");
            return;
        }
        
        // 确保玩家有必要的组件
        SetupPlayerComponents();
        
        Debug.Log($"✅ 修复完成! 共修复了 {fixedCount} 个Collideable图层");
        Debug.Log("现在可以使用 左Ctrl键 或 鼠标左键 攻击Collideable图层的墙壁!");
    }
    
    private static void FixCollideableTilemap(Tilemap tilemap)
    {
        GameObject tilemapObj = tilemap.gameObject;
        
        // 1. 确保有TilemapCollider2D
        TilemapCollider2D collider = tilemapObj.GetComponent<TilemapCollider2D>();
        if (collider == null)
        {
            collider = tilemapObj.AddComponent<TilemapCollider2D>();
            Debug.Log($"  ✓ 添加了TilemapCollider2D到 {tilemap.name}");
        }
        
        // 2. 确保有CompositeCollider2D
        CompositeCollider2D compositeCollider = tilemapObj.GetComponent<CompositeCollider2D>();
        if (compositeCollider == null)
        {
            compositeCollider = tilemapObj.AddComponent<CompositeCollider2D>();
            Debug.Log($"  ✓ 添加了CompositeCollider2D到 {tilemap.name}");
        }
        
        // 3. 确保有Rigidbody2D (CompositeCollider2D需要)
        Rigidbody2D rb = tilemapObj.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = tilemapObj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            Debug.Log($"  ✓ 添加了Rigidbody2D到 {tilemap.name}");
        }
        
        // 4. 配置TilemapCollider2D使用Composite
        collider.usedByComposite = true;
        
        // 5. 添加BreakableTilemapManager
        BreakableTilemapManager manager = tilemapObj.GetComponent<BreakableTilemapManager>();
        if (manager == null)
        {
            manager = tilemapObj.AddComponent<BreakableTilemapManager>();
            Debug.Log($"  ✓ 添加了BreakableTilemapManager到 {tilemap.name}");
            
            // 配置管理器
            ConfigureBreakableManager(manager, tilemap);
        }
        
        EditorUtility.SetDirty(tilemapObj);
    }
    
    private static void ConfigureBreakableManager(BreakableTilemapManager manager, Tilemap tilemap)
    {
        SerializedObject serializedManager = new SerializedObject(manager);
        
        // 基本设置
        serializedManager.FindProperty("breakableTilemap").objectReferenceValue = tilemap;
        serializedManager.FindProperty("detectionRadius").floatValue = 2f;
        
        // 耐久度系统
        serializedManager.FindProperty("useHealthSystem").boolValue = true;
        serializedManager.FindProperty("tileHealth").intValue = 2;
        
        // 效果设置
        serializedManager.FindProperty("createDebris").boolValue = true;
        serializedManager.FindProperty("debrisCount").intValue = 5;
        serializedManager.FindProperty("debrisForce").floatValue = 3f;
        serializedManager.FindProperty("dropChance").floatValue = 0.3f;
        
        // 调试设置
        serializedManager.FindProperty("showDebugInfo").boolValue = false; // 不显示调试信息
        serializedManager.FindProperty("visualizeDetection").boolValue = false;
        
        serializedManager.ApplyModifiedProperties();
        
        Debug.Log($"  ✓ 配置了BreakableTilemapManager参数");
    }
    
    private static void SetupPlayerComponents()
    {
        // 查找玩家
        PlayerAttackSystem playerAttackSystem = FindFirstObjectByType<PlayerAttackSystem>();
        if (playerAttackSystem == null)
        {
            Debug.LogError("❌ 未找到PlayerAttackSystem! 无法设置玩家组件");
            return;
        }
        
        GameObject playerObj = playerAttackSystem.gameObject;
        
        // 确保玩家有PlayerAttackTilemapIntegration
        PlayerAttackTilemapIntegration integration = playerObj.GetComponent<PlayerAttackTilemapIntegration>();
        if (integration == null)
        {
            integration = playerObj.AddComponent<PlayerAttackTilemapIntegration>();
            Debug.Log("  ✓ 添加了PlayerAttackTilemapIntegration到玩家");
            
            // 配置集成组件
            SerializedObject serializedIntegration = new SerializedObject(integration);
            serializedIntegration.FindProperty("tilemapLayers").intValue = -1; // 所有图层
            serializedIntegration.FindProperty("tilemapAttackRange").floatValue = 2f;
            serializedIntegration.FindProperty("showAttackRange").boolValue = false;
            serializedIntegration.ApplyModifiedProperties();
        }
        
        Debug.Log("  ✓ 玩家组件设置完成");
    }
    
    [MenuItem("Tools/可破坏墙壁/测试Collideable破坏")]
    public static void TestCollideableBreaking()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("请在游戏运行时进行测试!");
            return;
        }
        
        Debug.Log("=== 测试Collideable破坏功能 ===");
        
        PlayerAttackSystem player = FindFirstObjectByType<PlayerAttackSystem>();
        if (player == null)
        {
            Debug.LogError("未找到玩家!");
            return;
        }
        
        // 查找玩家附近的Collideable图层
        Vector3 playerPos = player.transform.position;
        float searchRadius = 5f;
        
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(playerPos, searchRadius);
        bool foundCollideable = false;
        
        foreach (var collider in nearbyColliders)
        {
            Tilemap tilemap = collider.GetComponent<Tilemap>();
            if (tilemap != null && tilemap.name.Contains("Collideable"))
            {
                foundCollideable = true;
                Vector3Int cellPos = tilemap.WorldToCell(playerPos);
                var tile = tilemap.GetTile(cellPos);
                
                Debug.Log($"找到Collideable图层: {tilemap.name}");
                Debug.Log($"玩家位置瓦片: {(tile != null ? "有瓦片" : "无瓦片")}");
                
                BreakableTilemapManager manager = tilemap.GetComponent<BreakableTilemapManager>();
                if (manager != null)
                {
                    Debug.Log("✓ 该图层有BreakableTilemapManager");
                }
                else
                {
                    Debug.LogError("✗ 该图层缺少BreakableTilemapManager!");
                }
            }
        }
        
        if (!foundCollideable)
        {
            Debug.LogWarning("玩家附近没有找到Collideable图层!");
        }
        
        Debug.Log("请使用 左Ctrl键 或 鼠标左键 攻击墙壁进行测试!");
    }
    
    [MenuItem("Tools/可破坏墙壁/检查当前场景设置")]
    public static void CheckCurrentSceneSetup()
    {
        Debug.Log("=== 检查当前场景的可破坏墙壁设置 ===");
        
        // 检查所有Tilemap
        Tilemap[] allTilemaps = FindObjectsOfType<Tilemap>();
        Debug.Log($"场景中共有 {allTilemaps.Length} 个Tilemap:");
        
        int collideableCount = 0;
        int breakableCount = 0;
        
        foreach (var tilemap in allTilemaps)
        {
            bool isCollideable = tilemap.name.Contains("Collideable");
            bool hasBreakable = tilemap.GetComponent<BreakableTilemapManager>() != null;
            bool hasCollider = tilemap.GetComponent<TilemapCollider2D>() != null;
            
            if (isCollideable)
            {
                collideableCount++;
                if (hasBreakable) breakableCount++;
            }
            
            string status = "";
            if (isCollideable) status += "[Collideable]";
            if (hasCollider) status += "[有碰撞体]";
            if (hasBreakable) status += "[可破坏]";
            
            Debug.Log($"  - {tilemap.name} {status}");
        }
        
        Debug.Log($"统计: Collideable图层 {collideableCount} 个, 其中可破坏 {breakableCount} 个");
        
        if (collideableCount > breakableCount)
        {
            Debug.LogWarning($"有 {collideableCount - breakableCount} 个Collideable图层还不能破坏!");
            Debug.Log("使用菜单: Tools → 可破坏墙壁 → 修复Collideable图层 来修复");
        }
        
        // 检查玩家设置
        PlayerAttackSystem player = FindFirstObjectByType<PlayerAttackSystem>();
        if (player != null)
        {
            bool hasIntegration = player.GetComponent<PlayerAttackTilemapIntegration>() != null;
            Debug.Log($"玩家设置: {(hasIntegration ? "✓ 已配置Tilemap攻击" : "✗ 缺少Tilemap攻击组件")}");
        }
        else
        {
            Debug.LogError("未找到玩家!");
        }
    }
#endif
} 