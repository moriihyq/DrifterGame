using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BreakableWallFixer : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/可破坏墙壁/自动修复可破坏墙壁系统")]
    public static void AutoFixBreakableWalls()
    {
        Debug.Log("=== 开始自动修复可破坏墙壁系统 ===");
        
        // 1. 查找玩家对象
        PlayerAttackSystem playerAttackSystem = FindFirstObjectByType<PlayerAttackSystem>();
        if (playerAttackSystem == null)
        {
            Debug.LogError("未找到PlayerAttackSystem！请确保玩家对象正确设置。");
            return;
        }
        
        Debug.Log($"✓ 找到玩家: {playerAttackSystem.name}");
        
        // 2. 查找所有Tilemap对象
        Tilemap[] allTilemaps = FindObjectsOfType<Tilemap>();
        Debug.Log($"找到 {allTilemaps.Length} 个Tilemap对象");
        
        // 3. 为可能的可破坏墙壁Tilemap添加组件
        int fixedTilemaps = 0;
        foreach (var tilemap in allTilemaps)
        {
            if (ShouldBeBreakable(tilemap))
            {
                SetupBreakableTilemap(tilemap);
                fixedTilemaps++;
            }
        }
        
        // 4. 确保玩家有PlayerAttackTilemapIntegration
        SetupPlayerTilemapIntegration(playerAttackSystem.gameObject);
        
        Debug.Log($"=== 修复完成！处理了 {fixedTilemaps} 个可破坏Tilemap ===");
        
        if (fixedTilemaps > 0)
        {
            EditorUtility.SetDirty(playerAttackSystem.gameObject);
            AssetDatabase.SaveAssets();
        }
    }
    
    [MenuItem("Tools/可破坏墙壁/创建测试可破坏墙壁")]
    public static void CreateTestBreakableWall()
    {
        PlayerAttackSystem playerAttackSystem = FindFirstObjectByType<PlayerAttackSystem>();
        if (playerAttackSystem == null)
        {
            Debug.LogError("未找到PlayerAttackSystem！");
            return;
        }
        
        // 创建测试墙壁
        GameObject testWallObj = new GameObject("测试可破坏墙壁");
        testWallObj.transform.position = playerAttackSystem.transform.position + Vector3.right * 3f;
        
        // 添加Grid组件
        Grid grid = testWallObj.AddComponent<Grid>();
        
        // 创建子对象作为Tilemap
        GameObject tilemapObj = new GameObject("Walls");
        tilemapObj.transform.SetParent(testWallObj.transform);
        
        // 添加Tilemap相关组件
        Tilemap tilemap = tilemapObj.AddComponent<Tilemap>();
        TilemapRenderer tilemapRenderer = tilemapObj.AddComponent<TilemapRenderer>();
        TilemapCollider2D tilemapCollider = tilemapObj.AddComponent<TilemapCollider2D>();
        BreakableTilemapManager breakableManager = tilemapObj.AddComponent<BreakableTilemapManager>();
        
        // 设置渲染排序
        tilemapRenderer.sortingOrder = 1;
        tilemapCollider.usedByComposite = false;
        
        // 创建测试瓦片
        CreateTestTiles(tilemap);
        
        // 配置BreakableTilemapManager
        var serializedObject = new SerializedObject(breakableManager);
        serializedObject.FindProperty("detectionRadius").floatValue = 1.5f;
        serializedObject.FindProperty("useHealthSystem").boolValue = true;
        serializedObject.FindProperty("tileHealth").intValue = 2;
        serializedObject.FindProperty("createDebris").boolValue = true;
        serializedObject.FindProperty("debrisCount").intValue = 5;
        serializedObject.FindProperty("showDebugInfo").boolValue = true;
        serializedObject.ApplyModifiedProperties();
        
        // 确保玩家有PlayerAttackTilemapIntegration
        SetupPlayerTilemapIntegration(playerAttackSystem.gameObject);
        
        Debug.Log("✓ 创建了测试可破坏墙壁！使用左Ctrl或鼠标左键攻击测试！");
        
        // 选中创建的对象
        Selection.activeGameObject = testWallObj;
    }
    
    private static bool ShouldBeBreakable(Tilemap tilemap)
    {
        string name = tilemap.name.ToLower();
        
        // 检查名称是否包含墙壁相关关键词
        bool hasWallKeyword = name.Contains("wall") || 
                             name.Contains("破坏") || 
                             name.Contains("可破坏") ||
                             name.Contains("brick") ||
                             name.Contains("block");
        
        // 检查是否已经有破坏管理器
        bool hasBreakableManager = tilemap.GetComponent<BreakableTilemapManager>() != null;
        
        // 检查是否有碰撞体
        bool hasCollider = tilemap.GetComponent<TilemapCollider2D>() != null;
        
        // 检查图层（避免处理地面等图层）
        bool isNotFloor = !name.Contains("floor") && !name.Contains("ground") && !name.Contains("地面");
        
        return hasWallKeyword && !hasBreakableManager && isNotFloor;
    }
    
    private static void SetupBreakableTilemap(Tilemap tilemap)
    {
        Debug.Log($"正在设置可破坏Tilemap: {tilemap.name}");
        
        // 添加TilemapCollider2D（如果没有）
        TilemapCollider2D collider = tilemap.GetComponent<TilemapCollider2D>();
        if (collider == null)
        {
            collider = tilemap.gameObject.AddComponent<TilemapCollider2D>();
            Debug.Log($"  ✓ 添加了TilemapCollider2D");
        }
        
        // 添加BreakableTilemapManager
        BreakableTilemapManager manager = tilemap.gameObject.AddComponent<BreakableTilemapManager>();
        
        // 配置BreakableTilemapManager
        var serializedObject = new SerializedObject(manager);
        serializedObject.FindProperty("breakableTilemap").objectReferenceValue = tilemap;
        serializedObject.FindProperty("detectionRadius").floatValue = 1.5f;
        serializedObject.FindProperty("useHealthSystem").boolValue = true;
        serializedObject.FindProperty("tileHealth").intValue = 2;
        serializedObject.FindProperty("createDebris").boolValue = true;
        serializedObject.FindProperty("debrisCount").intValue = 5;
        serializedObject.FindProperty("showDebugInfo").boolValue = true;
        serializedObject.ApplyModifiedProperties();
        
        Debug.Log($"  ✓ 添加并配置了BreakableTilemapManager");
        
        EditorUtility.SetDirty(tilemap.gameObject);
    }
    
    private static void SetupPlayerTilemapIntegration(GameObject playerObj)
    {
        PlayerAttackTilemapIntegration integration = playerObj.GetComponent<PlayerAttackTilemapIntegration>();
        if (integration == null)
        {
            integration = playerObj.AddComponent<PlayerAttackTilemapIntegration>();
            Debug.Log("  ✓ 添加了PlayerAttackTilemapIntegration到玩家");
        }
        
        // 配置PlayerAttackTilemapIntegration
        var serializedObject = new SerializedObject(integration);
        serializedObject.FindProperty("tilemapLayers").intValue = -1; // 所有图层
        serializedObject.FindProperty("tilemapAttackRange").floatValue = 2f;
        serializedObject.FindProperty("showAttackRange").boolValue = true;
        serializedObject.ApplyModifiedProperties();
        
        Debug.Log("  ✓ 配置了PlayerAttackTilemapIntegration");
        
        EditorUtility.SetDirty(playerObj);
    }
    
    private static void CreateTestTiles(Tilemap tilemap)
    {
        // 创建一个简单的测试瓦片
        Texture2D texture = new Texture2D(32, 32);
        Color[] colors = new Color[32 * 32];
        
        // 创建棕色砖块纹理
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = new Color(0.6f, 0.3f, 0.1f, 1f); // 棕色
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        // 创建精灵
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), Vector2.one * 0.5f, 32f);
        
        // 创建瓦片
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        
        // 在Tilemap上放置几个瓦片形成墙壁
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                tilemap.SetTile(position, tile);
            }
        }
    }
    
    [MenuItem("Tools/可破坏墙壁/检查可破坏墙壁状态")]
    public static void CheckBreakableWallStatus()
    {
        Debug.Log("=== 检查可破坏墙壁系统状态 ===");
        
        PlayerAttackSystem playerAttackSystem = FindFirstObjectByType<PlayerAttackSystem>();
        if (playerAttackSystem == null)
        {
            Debug.LogError("✗ 未找到PlayerAttackSystem");
            return;
        }
        
        PlayerAttackTilemapIntegration integration = playerAttackSystem.GetComponent<PlayerAttackTilemapIntegration>();
        if (integration == null)
        {
            Debug.LogWarning("⚠ 玩家缺少PlayerAttackTilemapIntegration组件");
        }
        else
        {
            Debug.Log("✓ 玩家有PlayerAttackTilemapIntegration组件");
        }
        
        BreakableTilemapManager[] managers = FindObjectsOfType<BreakableTilemapManager>();
        Debug.Log($"找到 {managers.Length} 个BreakableTilemapManager");
        
        foreach (var manager in managers)
        {
            var tilemap = manager.GetComponent<Tilemap>();
            var collider = manager.GetComponent<TilemapCollider2D>();
            
            string status = $"  - {manager.name}: ";
            if (tilemap != null) status += "[有Tilemap]";
            if (collider != null) status += "[有碰撞体]";
            
            Debug.Log(status);
        }
        
        Debug.Log("=== 检查完成 ===");
    }
#endif
} 