using UnityEngine;
using UnityEngine.Tilemaps;

public class BreakableWallDiagnostic : MonoBehaviour
{
    [Header("诊断设置")]
    public bool enableDebugLogs = true;
    public bool showGizmos = true;
    public KeyCode testKey = KeyCode.T;
    
    private PlayerAttackSystem playerAttackSystem;
    private BreakableTilemapManager breakableTilemapManager;
    private PlayerAttackTilemapIntegration tilemapIntegration;
    private Tilemap[] allTilemaps;
    
    private void Start()
    {
        PerformDiagnosis();
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            TestBreakableWalls();
        }
    }
    
    private void PerformDiagnosis()
    {
        if (!enableDebugLogs) return;
        
        Debug.Log("=== 可破坏墙壁系统诊断开始 ===");
        
        // 检查PlayerAttackSystem
        playerAttackSystem = FindFirstObjectByType<PlayerAttackSystem>();
        if (playerAttackSystem != null)
        {
            Debug.Log($"✓ 找到PlayerAttackSystem: {playerAttackSystem.name}");
        }
        else
        {
            Debug.LogError("✗ 未找到PlayerAttackSystem！");
        }
        
        // 检查BreakableTilemapManager
        breakableTilemapManager = FindFirstObjectByType<BreakableTilemapManager>();
        if (breakableTilemapManager != null)
        {
            Debug.Log($"✓ 找到BreakableTilemapManager: {breakableTilemapManager.name}");
            CheckBreakableTilemapManagerSettings();
        }
        else
        {
            Debug.LogWarning("⚠ 未找到BreakableTilemapManager");
        }
        
        // 检查PlayerAttackTilemapIntegration
        tilemapIntegration = FindFirstObjectByType<PlayerAttackTilemapIntegration>();
        if (tilemapIntegration != null)
        {
            Debug.Log($"✓ 找到PlayerAttackTilemapIntegration: {tilemapIntegration.name}");
        }
        else
        {
            Debug.LogWarning("⚠ 未找到PlayerAttackTilemapIntegration");
        }
        
        // 检查所有Tilemap
        allTilemaps = FindObjectsOfType<Tilemap>();
        Debug.Log($"场景中共有 {allTilemaps.Length} 个Tilemap:");
        foreach (var tilemap in allTilemaps)
        {
            CheckTilemapSettings(tilemap);
        }
        
        Debug.Log("=== 可破坏墙壁系统诊断完成 ===");
        
        if (breakableTilemapManager == null && tilemapIntegration == null)
        {
            Debug.LogError("❌ 没有找到任何可破坏墙壁组件！请添加BreakableTilemapManager或PlayerAttackTilemapIntegration");
            ProvideFixSuggestion();
        }
    }
    
    private void CheckBreakableTilemapManagerSettings()
    {
        var tilemap = breakableTilemapManager.GetComponent<Tilemap>();
        if (tilemap == null)
        {
            Debug.LogError("✗ BreakableTilemapManager所在对象没有Tilemap组件！");
        }
        
        var tilemapCollider = breakableTilemapManager.GetComponent<TilemapCollider2D>();
        if (tilemapCollider == null)
        {
            Debug.LogError("✗ BreakableTilemapManager所在对象没有TilemapCollider2D组件！");
        }
        else
        {
            Debug.Log("✓ TilemapCollider2D正常");
        }
    }
    
    private void CheckTilemapSettings(Tilemap tilemap)
    {
        var collider = tilemap.GetComponent<TilemapCollider2D>();
        var manager = tilemap.GetComponent<BreakableTilemapManager>();
        
        string status = "";
        if (collider != null) status += "[有碰撞体]";
        if (manager != null) status += "[有破坏管理器]";
        
        Debug.Log($"  - {tilemap.name} (图层:{tilemap.gameObject.layer}) {status}");
        
        // 检查是否有瓦片
        int tileCount = 0;
        BoundsInt bounds = tilemap.cellBounds;
        foreach (var position in bounds.allPositionsWithin)
        {
            if (tilemap.HasTile(position))
            {
                tileCount++;
                if (tileCount >= 10) break; // 只检查前10个瓦片
            }
        }
        
        if (tileCount > 0)
        {
            Debug.Log($"    瓦片数量: {tileCount}+ 个");
        }
        else
        {
            Debug.LogWarning($"    ⚠ {tilemap.name} 中没有瓦片！");
        }
    }
    
    private void TestBreakableWalls()
    {
        Debug.Log($"=== 按下 {testKey} 键测试可破坏墙壁 ===");
        
        if (playerAttackSystem == null)
        {
            Debug.LogError("无法测试：没有PlayerAttackSystem");
            return;
        }
        
        Vector3 playerPos = playerAttackSystem.transform.position;
        Debug.Log($"玩家位置: {playerPos}");
        
        // 测试是否能检测到附近的Tilemap
        float testRadius = 3f;
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(playerPos, testRadius);
        
        Debug.Log($"玩家周围 {testRadius} 米内的碰撞体:");
        foreach (var collider in nearbyColliders)
        {
            var tilemapCollider = collider.GetComponent<TilemapCollider2D>();
            if (tilemapCollider != null)
            {
                var tilemap = tilemapCollider.GetComponent<Tilemap>();
                if (tilemap != null)
                {
                    Vector3Int cellPos = tilemap.WorldToCell(playerPos);
                    var tile = tilemap.GetTile(cellPos);
                    Debug.Log($"  - Tilemap: {tilemap.name}, 玩家位置瓦片: {(tile != null ? tile.name : "无瓦片")}");
                }
            }
        }
        
        // 尝试手动破坏瓦片
        if (breakableTilemapManager != null)
        {
            Debug.Log("尝试使用BreakableTilemapManager破坏瓦片...");
            var tilemap = breakableTilemapManager.GetComponent<Tilemap>();
            if (tilemap != null)
            {
                Vector3Int cellPos = tilemap.WorldToCell(playerPos);
                breakableTilemapManager.TryBreakTile(cellPos);
            }
        }
    }
    
    private void ProvideFixSuggestion()
    {
        Debug.Log("=== 修复建议 ===");
        Debug.Log("1. 找到包含可破坏墙壁的Tilemap对象");
        Debug.Log("2. 添加BreakableTilemapManager脚本到该对象上");
        Debug.Log("3. 确保该对象有TilemapCollider2D组件");
        Debug.Log("4. 在玩家对象上添加PlayerAttackTilemapIntegration脚本");
        Debug.Log("5. 配置相应的图层遮罩和攻击范围");
    }
    
    [ContextMenu("创建测试可破坏墙壁")]
    public void CreateTestBreakableWall()
    {
        if (playerAttackSystem == null)
        {
            Debug.LogError("需要先找到玩家对象");
            return;
        }
        
        // 在玩家前方创建一个测试用的可破坏墙壁
        GameObject testWall = new GameObject("测试可破坏墙壁");
        testWall.transform.position = playerAttackSystem.transform.position + Vector3.right * 2f;
        
        // 添加Tilemap组件
        var grid = testWall.AddComponent<Grid>();
        var tilemap = testWall.AddComponent<Tilemap>();
        var tilemapRenderer = testWall.AddComponent<TilemapRenderer>();
        var tilemapCollider = testWall.AddComponent<TilemapCollider2D>();
        var breakableManager = testWall.AddComponent<BreakableTilemapManager>();
        
        // 创建一个简单的瓦片
        var texture = new Texture2D(32, 32);
        Color brownColor = new Color(0.6f, 0.3f, 0.1f, 1f); // 自定义棕色
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                texture.SetPixel(x, y, brownColor);
            }
        }
        texture.Apply();
        
        var sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), Vector2.one * 0.5f, 32f);
        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        
        // 放置瓦片
        Vector3Int cellPos = Vector3Int.zero;
        tilemap.SetTile(cellPos, tile);
        
        Debug.Log("创建了测试可破坏墙壁，请尝试攻击它！");
    }
    
    private void OnDrawGizmos()
    {
        if (!showGizmos || playerAttackSystem == null) return;
        
        // 绘制玩家攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(playerAttackSystem.transform.position, 2f);
        
        // 绘制检测到的Tilemap位置
        if (allTilemaps != null)
        {
            Gizmos.color = Color.green;
            foreach (var tilemap in allTilemaps)
            {
                if (tilemap != null)
                {
                    var bounds = tilemap.cellBounds;
                    Vector3 center = tilemap.transform.TransformPoint(bounds.center);
                    Vector3 size = tilemap.transform.TransformVector(bounds.size);
                    Gizmos.DrawWireCube(center, size);
                }
            }
        }
    }
} 