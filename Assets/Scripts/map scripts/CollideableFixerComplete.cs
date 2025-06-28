using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

/// <summary>
/// 完整的Collideable图层修复器 - 解决所有破坏问题
/// </summary>
public class CollideableFixerComplete : MonoBehaviour
{
    [Header("自动修复设置")]
    [SerializeField] private bool autoFixOnStart = true;
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private KeyCode manualFixKey = KeyCode.F9;
    [SerializeField] private KeyCode testAttackKey = KeyCode.F10;
    
    [Header("攻击设置")]
    [SerializeField] private KeyCode[] attackKeys = { KeyCode.LeftControl, KeyCode.Mouse0 };
    [SerializeField] private float attackRadius = 2f;
    [SerializeField] private LayerMask tilemapLayers = -1;
    
    private PlayerAttackSystem playerAttackSystem;
    private bool hasFixed = false;
    
    void Start()
    {
        if (autoFixOnStart)
        {
            StartCoroutine(DelayedFix());
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(manualFixKey))
        {
            PerformCompleteFix();
        }
        
        if (Input.GetKeyDown(testAttackKey))
        {
            TestBreakWalls();
        }
        
        // 处理攻击输入
        HandleAttackInput();
    }
    
    IEnumerator DelayedFix()
    {
        yield return new WaitForSeconds(1f); // 等待场景完全加载
        PerformCompleteFix();
    }
    
    public void PerformCompleteFix()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== 开始完整修复Collideable图层 ===");
        }
        
        // 1. 查找和修复所有Collideable图层
        FixAllCollideableLayers();
        
        // 2. 设置玩家攻击系统
        SetupPlayerAttackSystem();
        
        // 3. 验证修复结果
        ValidateFixResult();
        
        hasFixed = true;
        
        if (showDebugInfo)
        {
            Debug.Log("=== Collideable图层修复完成 ===");
            Debug.Log("现在可以使用以下按键攻击墙壁:");
            Debug.Log("- 左Ctrl键");
            Debug.Log("- 鼠标左键");
            Debug.Log($"- {testAttackKey} 键测试攻击");
        }
    }
    
    void FixAllCollideableLayers()
    {
        Tilemap[] allTilemaps = FindObjectsOfType<Tilemap>();
        int fixedCount = 0;
        
        foreach (var tilemap in allTilemaps)
        {
            if (tilemap.name.Contains("Collideable"))
            {
                if (FixSingleCollideableLayer(tilemap))
                {
                    fixedCount++;
                }
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"修复了 {fixedCount} 个Collideable图层");
        }
    }
    
    bool FixSingleCollideableLayer(Tilemap tilemap)
    {
        bool wasFixed = false;
        GameObject tilemapObj = tilemap.gameObject;
        
        if (showDebugInfo)
        {
            Debug.Log($"修复图层: {tilemap.name}");
        }
        
        // 1. 添加TilemapCollider2D
        TilemapCollider2D tilemapCollider = tilemapObj.GetComponent<TilemapCollider2D>();
        if (tilemapCollider == null)
        {
            tilemapCollider = tilemapObj.AddComponent<TilemapCollider2D>();
            wasFixed = true;
            if (showDebugInfo) Debug.Log("  ✓ 添加TilemapCollider2D");
        }
        
        // 2. 添加CompositeCollider2D
        CompositeCollider2D compositeCollider = tilemapObj.GetComponent<CompositeCollider2D>();
        if (compositeCollider == null)
        {
            compositeCollider = tilemapObj.AddComponent<CompositeCollider2D>();
            wasFixed = true;
            if (showDebugInfo) Debug.Log("  ✓ 添加CompositeCollider2D");
        }
        
        // 3. 添加Rigidbody2D
        Rigidbody2D rb = tilemapObj.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = tilemapObj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            wasFixed = true;
            if (showDebugInfo) Debug.Log("  ✓ 添加Rigidbody2D");
        }
        
        // 4. 配置TilemapCollider2D
        tilemapCollider.usedByComposite = true;
        
        // 5. 添加BreakableTilemapManager
        BreakableTilemapManager breakableManager = tilemapObj.GetComponent<BreakableTilemapManager>();
        if (breakableManager == null)
        {
            breakableManager = tilemapObj.AddComponent<BreakableTilemapManager>();
            wasFixed = true;
            if (showDebugInfo) Debug.Log("  ✓ 添加BreakableTilemapManager");
        }
        
        return wasFixed;
    }
    
    void SetupPlayerAttackSystem()
    {
        // 查找玩家
        playerAttackSystem = FindFirstObjectByType<PlayerAttackSystem>();
        if (playerAttackSystem == null)
        {
            if (showDebugInfo)
            {
                Debug.LogError("未找到PlayerAttackSystem! 请确保场景中有玩家对象");
            }
            return;
        }
        
        // 添加PlayerAttackTilemapIntegration（如果没有）
        PlayerAttackTilemapIntegration integration = playerAttackSystem.GetComponent<PlayerAttackTilemapIntegration>();
        if (integration == null)
        {
            integration = playerAttackSystem.gameObject.AddComponent<PlayerAttackTilemapIntegration>();
            if (showDebugInfo) Debug.Log("✓ 添加PlayerAttackTilemapIntegration到玩家");
        }
        
        if (showDebugInfo)
        {
            Debug.Log("✓ 玩家攻击系统设置完成");
        }
    }
    
    void ValidateFixResult()
    {
        Tilemap[] collideableTilemaps = GetCollideableTilemaps();
        int validCount = 0;
        
        foreach (var tilemap in collideableTilemaps)
        {
            bool isValid = ValidateTilemap(tilemap);
            if (isValid) validCount++;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"验证结果: {validCount}/{collideableTilemaps.Length} 个Collideable图层配置正确");
        }
    }
    
    bool ValidateTilemap(Tilemap tilemap)
    {
        bool hasCollider = tilemap.GetComponent<TilemapCollider2D>() != null;
        bool hasComposite = tilemap.GetComponent<CompositeCollider2D>() != null;
        bool hasRigidbody = tilemap.GetComponent<Rigidbody2D>() != null;
        bool hasBreakable = tilemap.GetComponent<BreakableTilemapManager>() != null;
        
        return hasCollider && hasComposite && hasRigidbody && hasBreakable;
    }
    
    Tilemap[] GetCollideableTilemaps()
    {
        Tilemap[] allTilemaps = FindObjectsOfType<Tilemap>();
        System.Collections.Generic.List<Tilemap> collideableList = new System.Collections.Generic.List<Tilemap>();
        
        foreach (var tilemap in allTilemaps)
        {
            if (tilemap.name.Contains("Collideable"))
            {
                collideableList.Add(tilemap);
            }
        }
        
        return collideableList.ToArray();
    }
    
    void HandleAttackInput()
    {
        if (playerAttackSystem == null) return;
        
        // 检查攻击按键
        bool attackPressed = false;
        foreach (var key in attackKeys)
        {
            if (Input.GetKeyDown(key))
            {
                attackPressed = true;
                break;
            }
        }
        
        if (attackPressed)
        {
            PerformDirectAttack();
        }
    }
    
    void PerformDirectAttack()
    {
        Vector3 playerPos = playerAttackSystem.transform.position;
        
        if (showDebugInfo)
        {
            Debug.Log($"[CollideableFixerComplete] 玩家攻击: {playerPos}");
        }
        
        // 检测攻击范围内的所有碰撞体
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(playerPos, attackRadius, tilemapLayers);
        
        foreach (var collider in hitColliders)
        {
            // 检查是否为Tilemap
            Tilemap tilemap = collider.GetComponent<Tilemap>();
            if (tilemap != null && tilemap.name.Contains("Collideable"))
            {
                BreakableTilemapManager manager = tilemap.GetComponent<BreakableTilemapManager>();
                if (manager != null)
                {
                    // 尝试破坏玩家位置附近的瓦片
                    Vector3Int playerCell = tilemap.WorldToCell(playerPos);
                    
                    // 检查3x3范围内的瓦片
                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            Vector3Int cellPos = playerCell + new Vector3Int(x, y, 0);
                            TileBase tile = tilemap.GetTile(cellPos);
                            
                            if (tile != null)
                            {
                                if (showDebugInfo)
                                {
                                    Debug.Log($"[CollideableFixerComplete] 破坏瓦片: {cellPos}");
                                }
                                manager.TryBreakTile(cellPos);
                                return; // 只破坏一个瓦片
                            }
                        }
                    }
                }
                else if (showDebugInfo)
                {
                    Debug.LogWarning($"图层 {tilemap.name} 没有BreakableTilemapManager!");
                }
            }
        }
    }
    
    public void TestBreakWalls()
    {
        if (playerAttackSystem == null)
        {
            Debug.LogError("未找到玩家，无法测试");
            return;
        }
        
        Debug.Log("=== 测试破坏墙壁功能 ===");
        
        Vector3 playerPos = playerAttackSystem.transform.position;
        Tilemap[] collideableTilemaps = GetCollideableTilemaps();
        
        Debug.Log($"玩家位置: {playerPos}");
        Debug.Log($"找到 {collideableTilemaps.Length} 个Collideable图层");
        
        bool foundNearbyWalls = false;
        
        foreach (var tilemap in collideableTilemaps)
        {
            Vector3Int playerCell = tilemap.WorldToCell(playerPos);
            
            // 检查玩家周围是否有瓦片
            for (int x = -2; x <= 2; x++)
            {
                for (int y = -2; y <= 2; y++)
                {
                    Vector3Int cellPos = playerCell + new Vector3Int(x, y, 0);
                    TileBase tile = tilemap.GetTile(cellPos);
                    
                    if (tile != null)
                    {
                        foundNearbyWalls = true;
                        Vector3 worldPos = tilemap.CellToWorld(cellPos);
                        float distance = Vector3.Distance(playerPos, worldPos);
                        Debug.Log($"  发现墙壁: {tilemap.name} 位置{cellPos} 距离{distance:F1}m");
                        
                        if (distance <= attackRadius)
                        {
                            Debug.Log($"    → 在攻击范围内!");
                        }
                    }
                }
            }
        }
        
        if (!foundNearbyWalls)
        {
            Debug.LogWarning("玩家附近没有发现任何墙壁瓦片");
        }
        
        Debug.Log($"使用 左Ctrl 或 鼠标左键 攻击 (攻击范围: {attackRadius}m)");
    }
    
    void OnDrawGizmosSelected()
    {
        if (playerAttackSystem != null)
        {
            // 显示攻击范围
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerAttackSystem.transform.position, attackRadius);
            
            // 显示Collideable图层
            Gizmos.color = Color.yellow;
            Tilemap[] collideableTilemaps = GetCollideableTilemaps();
            foreach (var tilemap in collideableTilemaps)
            {
                if (tilemap != null)
                {
                    var bounds = tilemap.localBounds;
                    Gizmos.DrawWireCube(tilemap.transform.position + bounds.center, bounds.size);
                }
            }
        }
    }
    
    void OnGUI()
    {
        if (!hasFixed)
        {
            GUI.Label(new Rect(10, 50, 400, 40), 
                $"Collideable修复器\n{manualFixKey}: 手动修复 | {testAttackKey}: 测试攻击");
        }
        else if (showDebugInfo)
        {
            GUI.Label(new Rect(10, 50, 400, 60), 
                $"Collideable修复完成\n左Ctrl/鼠标左键: 攻击墙壁\n{testAttackKey}: 测试功能");
        }
    }
} 