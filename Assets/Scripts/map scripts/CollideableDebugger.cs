using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// Collideable图层调试器 - 实时检测和修复破坏问题
/// </summary>
public class CollideableDebugger : MonoBehaviour
{
    [Header("调试设置")]
    [SerializeField] private bool enableRealTimeDebug = true;
    [SerializeField] private KeyCode debugKey = KeyCode.F1;
    [SerializeField] private KeyCode forceFixKey = KeyCode.F2;
    [SerializeField] private KeyCode testAttackKey = KeyCode.F3;
    
    [Header("攻击测试")]
    [SerializeField] private bool showAttackIndicator = true;
    [SerializeField] private Color attackRangeColor = Color.red;
    
    private PlayerAttackSystem playerAttackSystem;
    private List<Tilemap> collideableTilemaps = new List<Tilemap>();
    
    void Start()
    {
        FindComponents();
        if (enableRealTimeDebug)
        {
            InvokeRepeating(nameof(PerformRealTimeCheck), 1f, 2f);
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(debugKey))
        {
            PerformFullDiagnosis();
        }
        
        if (Input.GetKeyDown(forceFixKey))
        {
            ForceFixCollideableIssues();
        }
        
        if (Input.GetKeyDown(testAttackKey))
        {
            TestAttackNearbyTiles();
        }
    }
    
    void FindComponents()
    {
        // 查找玩家
        playerAttackSystem = FindFirstObjectByType<PlayerAttackSystem>();
        if (playerAttackSystem == null)
        {
            Debug.LogError("[CollideableDebugger] 未找到PlayerAttackSystem!");
        }
        
        // 查找所有Collideable图层
        RefreshCollideableTilemaps();
    }
    
    void RefreshCollideableTilemaps()
    {
        collideableTilemaps.Clear();
        Tilemap[] allTilemaps = FindObjectsOfType<Tilemap>();
        
        foreach (var tilemap in allTilemaps)
        {
            if (tilemap.name.Contains("Collideable"))
            {
                collideableTilemaps.Add(tilemap);
            }
        }
        
        Debug.Log($"[CollideableDebugger] 找到 {collideableTilemaps.Count} 个Collideable图层");
    }
    
    void PerformRealTimeCheck()
    {
        if (!enableRealTimeDebug) return;
        
        // 检查每个Collideable图层的状态
        foreach (var tilemap in collideableTilemaps)
        {
            if (tilemap == null) continue;
            
            CheckTilemapConfiguration(tilemap);
        }
    }
    
    void CheckTilemapConfiguration(Tilemap tilemap)
    {
        bool hasIssues = false;
        List<string> issues = new List<string>();
        
        // 检查基本组件
        if (tilemap.GetComponent<TilemapCollider2D>() == null)
        {
            issues.Add("缺少TilemapCollider2D");
            hasIssues = true;
        }
        
        if (tilemap.GetComponent<BreakableTilemapManager>() == null)
        {
            issues.Add("缺少BreakableTilemapManager");
            hasIssues = true;
        }
        
        // 检查玩家组件
        if (playerAttackSystem != null)
        {
            if (playerAttackSystem.GetComponent<PlayerAttackTilemapIntegration>() == null)
            {
                issues.Add("玩家缺少PlayerAttackTilemapIntegration");
                hasIssues = true;
            }
        }
        
        if (hasIssues)
        {
            Debug.LogWarning($"[CollideableDebugger] {tilemap.name} 存在问题: {string.Join(", ", issues)}");
        }
    }
    
    [ContextMenu("完整诊断")]
    public void PerformFullDiagnosis()
    {
        Debug.Log("=== Collideable图层完整诊断 ===");
        
        RefreshCollideableTilemaps();
        
        // 诊断所有Collideable图层
        for (int i = 0; i < collideableTilemaps.Count; i++)
        {
            var tilemap = collideableTilemaps[i];
            Debug.Log($"\n--- 诊断 {tilemap.name} (第{i+1}个) ---");
            DiagnoseTilemap(tilemap);
        }
        
        // 诊断玩家设置
        Debug.Log("\n--- 诊断玩家设置 ---");
        DiagnosePlayer();
        
        // 诊断输入系统
        Debug.Log("\n--- 诊断输入系统 ---");
        DiagnoseInputSystem();
    }
    
    void DiagnoseTilemap(Tilemap tilemap)
    {
        // 检查基本组件
        var tilemapCollider = tilemap.GetComponent<TilemapCollider2D>();
        var compositeCollider = tilemap.GetComponent<CompositeCollider2D>();
        var rigidbody = tilemap.GetComponent<Rigidbody2D>();
        var breakableManager = tilemap.GetComponent<BreakableTilemapManager>();
        
        Debug.Log($"  TilemapCollider2D: {(tilemapCollider != null ? "✓" : "✗")}");
        Debug.Log($"  CompositeCollider2D: {(compositeCollider != null ? "✓" : "✗")}");
        Debug.Log($"  Rigidbody2D: {(rigidbody != null ? "✓" : "✗")}");
        Debug.Log($"  BreakableTilemapManager: {(breakableManager != null ? "✓" : "✗")}");
        
        if (tilemapCollider != null)
        {
            Debug.Log($"    UsedByComposite: {tilemapCollider.usedByComposite}");
        }
        
        if (rigidbody != null)
        {
            Debug.Log($"    BodyType: {rigidbody.bodyType}");
        }
        
        // 检查瓦片数量
        int tileCount = GetTileCount(tilemap);
        Debug.Log($"  瓦片数量: {tileCount}");
        
        // 检查图层
        Debug.Log($"  图层: {tilemap.gameObject.layer} ({LayerMask.LayerToName(tilemap.gameObject.layer)})");
    }
    
    int GetTileCount(Tilemap tilemap)
    {
        int count = 0;
        BoundsInt bounds = tilemap.cellBounds;
        
        foreach (var position in bounds.allPositionsWithin)
        {
            if (tilemap.HasTile(position))
            {
                count++;
                if (count >= 50) break; // 限制检查数量避免卡顿
            }
        }
        
        return count;
    }
    
    void DiagnosePlayer()
    {
        if (playerAttackSystem == null)
        {
            Debug.LogError("  ✗ 未找到PlayerAttackSystem");
            return;
        }
        
        Debug.Log($"  ✓ PlayerAttackSystem: {playerAttackSystem.name}");
        
        var tilemapIntegration = playerAttackSystem.GetComponent<PlayerAttackTilemapIntegration>();
        Debug.Log($"  PlayerAttackTilemapIntegration: {(tilemapIntegration != null ? "✓" : "✗")}");
        
        if (tilemapIntegration != null)
        {
            // 使用反射获取私有字段
            var tilemapLayersField = typeof(PlayerAttackTilemapIntegration).GetField("tilemapLayers", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (tilemapLayersField != null)
            {
                var tilemapLayers = (LayerMask)tilemapLayersField.GetValue(tilemapIntegration);
                Debug.Log($"    TilemapLayers: {tilemapLayers.value}");
            }
            
            var attackRangeField = typeof(PlayerAttackTilemapIntegration).GetField("tilemapAttackRange", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (attackRangeField != null)
            {
                var attackRange = (float)attackRangeField.GetValue(tilemapIntegration);
                Debug.Log($"    AttackRange: {attackRange}");
            }
        }
    }
    
    void DiagnoseInputSystem()
    {
        // 测试Fire1输入
        bool fire1Available = false;
        try
        {
            Input.GetButtonDown("Fire1");
            fire1Available = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"  Fire1输入错误: {e.Message}");
        }
        
        Debug.Log($"  Fire1输入可用: {(fire1Available ? "✓" : "✗")}");
        
        // 检查当前输入状态
        Debug.Log($"  当前Fire1状态: {Input.GetButton("Fire1")}");
        Debug.Log($"  鼠标左键状态: {Input.GetMouseButton(0)}");
        Debug.Log($"  左Ctrl状态: {Input.GetKey(KeyCode.LeftControl)}");
    }
    
    [ContextMenu("强制修复所有问题")]
    public void ForceFixCollideableIssues()
    {
        Debug.Log("=== 开始强制修复Collideable问题 ===");
        
        RefreshCollideableTilemaps();
        
        foreach (var tilemap in collideableTilemaps)
        {
            if (tilemap == null) continue;
            FixTilemapIssues(tilemap);
        }
        
        FixPlayerIssues();
        
        Debug.Log("=== 强制修复完成 ===");
    }
    
    void FixTilemapIssues(Tilemap tilemap)
    {
        Debug.Log($"修复 {tilemap.name}...");
        
        // 添加缺失组件
        var tilemapCollider = tilemap.GetComponent<TilemapCollider2D>();
        if (tilemapCollider == null)
        {
            tilemapCollider = tilemap.gameObject.AddComponent<TilemapCollider2D>();
            Debug.Log("  ✓ 添加TilemapCollider2D");
        }
        
        var compositeCollider = tilemap.GetComponent<CompositeCollider2D>();
        if (compositeCollider == null)
        {
            compositeCollider = tilemap.gameObject.AddComponent<CompositeCollider2D>();
            Debug.Log("  ✓ 添加CompositeCollider2D");
        }
        
        var rigidbody = tilemap.GetComponent<Rigidbody2D>();
        if (rigidbody == null)
        {
            rigidbody = tilemap.gameObject.AddComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Static;
            Debug.Log("  ✓ 添加Rigidbody2D");
        }
        
        // 配置组件
        tilemapCollider.usedByComposite = true;
        
        var breakableManager = tilemap.GetComponent<BreakableTilemapManager>();
        if (breakableManager == null)
        {
            breakableManager = tilemap.gameObject.AddComponent<BreakableTilemapManager>();
            Debug.Log("  ✓ 添加BreakableTilemapManager");
            
            // 配置BreakableTilemapManager
            ConfigureBreakableManager(breakableManager, tilemap);
        }
    }
    
    void ConfigureBreakableManager(BreakableTilemapManager manager, Tilemap tilemap)
    {
        // 使用反射设置私有字段
        SetPrivateField(manager, "breakableTilemap", tilemap);
        SetPrivateField(manager, "detectionRadius", 2f);
        SetPrivateField(manager, "useHealthSystem", true);
        SetPrivateField(manager, "tileHealth", 2);
        SetPrivateField(manager, "createDebris", true);
        SetPrivateField(manager, "debrisCount", 5);
        SetPrivateField(manager, "showDebugInfo", false);
    }
    
    void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(obj, value);
        }
    }
    
    void FixPlayerIssues()
    {
        if (playerAttackSystem == null) return;
        
        var tilemapIntegration = playerAttackSystem.GetComponent<PlayerAttackTilemapIntegration>();
        if (tilemapIntegration == null)
        {
            tilemapIntegration = playerAttackSystem.gameObject.AddComponent<PlayerAttackTilemapIntegration>();
            Debug.Log("  ✓ 添加PlayerAttackTilemapIntegration到玩家");
        }
    }
    
    [ContextMenu("测试攻击附近瓦片")]
    public void TestAttackNearbyTiles()
    {
        if (playerAttackSystem == null)
        {
            Debug.LogError("无法测试：未找到玩家");
            return;
        }
        
        Debug.Log("=== 测试攻击附近瓦片 ===");
        
        Vector3 playerPos = playerAttackSystem.transform.position;
        Debug.Log($"玩家位置: {playerPos}");
        
        // 检查附近的Collideable图层
        float testRadius = 3f;
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(playerPos, testRadius);
        
        bool foundAnyTilemap = false;
        foreach (var collider in nearbyColliders)
        {
            var tilemap = collider.GetComponent<Tilemap>();
            if (tilemap != null && tilemap.name.Contains("Collideable"))
            {
                foundAnyTilemap = true;
                Vector3Int cellPos = tilemap.WorldToCell(playerPos);
                var tile = tilemap.GetTile(cellPos);
                
                Debug.Log($"找到Collideable图层: {tilemap.name}");
                Debug.Log($"玩家位置瓦片: {(tile != null ? tile.name : "无瓦片")}");
                
                // 尝试手动破坏瓦片
                var manager = tilemap.GetComponent<BreakableTilemapManager>();
                if (manager != null)
                {
                    Debug.Log("尝试破坏瓦片...");
                    
                    // 在玩家周围寻找瓦片
                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            Vector3Int testPos = cellPos + new Vector3Int(x, y, 0);
                            var testTile = tilemap.GetTile(testPos);
                            if (testTile != null)
                            {
                                Debug.Log($"尝试破坏位置 {testPos} 的瓦片");
                                manager.TryBreakTile(testPos);
                                return; // 只破坏一个瓦片
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError("该图层没有BreakableTilemapManager!");
                }
            }
        }
        
        if (!foundAnyTilemap)
        {
            Debug.LogWarning("玩家附近没有找到Collideable图层!");
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showAttackIndicator || playerAttackSystem == null) return;
        
        // 显示攻击范围
        Gizmos.color = attackRangeColor;
        Gizmos.DrawWireSphere(playerAttackSystem.transform.position, 2f);
        
        // 显示Collideable图层位置
        Gizmos.color = Color.yellow;
        foreach (var tilemap in collideableTilemaps)
        {
            if (tilemap != null)
            {
                var bounds = tilemap.localBounds;
                Gizmos.DrawWireCube(tilemap.transform.position + bounds.center, bounds.size);
            }
        }
    }
    
    void OnGUI()
    {
        if (!enableRealTimeDebug) return;
        
        GUILayout.BeginArea(new Rect(10, 100, 300, 200));
        GUILayout.Label("=== Collideable调试器 ===");
        GUILayout.Label($"F1: 完整诊断");
        GUILayout.Label($"F2: 强制修复");
        GUILayout.Label($"F3: 测试攻击");
        GUILayout.Label($"");
        GUILayout.Label($"Collideable图层: {collideableTilemaps.Count}个");
        if (playerAttackSystem != null)
        {
            var hasIntegration = playerAttackSystem.GetComponent<PlayerAttackTilemapIntegration>() != null;
            GUILayout.Label($"玩家Tilemap攻击: {(hasIntegration ? "已配置" : "未配置")}");
        }
        GUILayout.EndArea();
    }
} 