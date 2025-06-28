using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// 智能Collideable图层攻击器 - 专门识别和攻击Collideable图层
/// </summary>
public class SmartCollideableAttacker : MonoBehaviour
{
    [Header("攻击设置")]
    [SerializeField] private KeyCode[] attackKeys = { KeyCode.LeftControl, KeyCode.Mouse0 };
    [SerializeField] private float attackRadius = 3f;
    [SerializeField] private bool showDebugInfo = true;
    
    [Header("图层识别")]
    [SerializeField] private string[] collideableKeywords = { "Collideable", "Breakable", "可破坏" };
    [SerializeField] private string[] ignoreKeywords = { "Wall", "Ground", "Floor", "Background" };
    
    private PlayerAttackSystem playerAttackSystem;
    private List<Tilemap> collideableTilemaps = new List<Tilemap>();
    
    void Start()
    {
        FindPlayerAndTilemaps();
    }
    
    void Update()
    {
        CheckForAttackInput();
    }
    
    void FindPlayerAndTilemaps()
    {
        // 查找玩家
        playerAttackSystem = FindFirstObjectByType<PlayerAttackSystem>();
        if (playerAttackSystem == null)
        {
            Debug.LogError("[SmartCollideableAttacker] 未找到PlayerAttackSystem!");
            return;
        }
        
        // 查找所有Collideable图层
        RefreshCollideableTilemaps();
    }
    
    void RefreshCollideableTilemaps()
    {
        collideableTilemaps.Clear();
        Tilemap[] allTilemaps = FindObjectsOfType<Tilemap>();
        
        Debug.Log($"[SmartCollideableAttacker] 检查 {allTilemaps.Length} 个Tilemap图层");
        
        foreach (var tilemap in allTilemaps)
        {
            if (IsCollideableLayer(tilemap))
            {
                collideableTilemaps.Add(tilemap);
                Debug.Log($"[SmartCollideableAttacker] 识别为Collideable图层: {tilemap.name}");
                
                // 确保图层有必要的组件
                EnsureBreakableComponents(tilemap);
            }
            else
            {
                Debug.Log($"[SmartCollideableAttacker] 跳过图层: {tilemap.name}");
            }
        }
        
        Debug.Log($"[SmartCollideableAttacker] 总共找到 {collideableTilemaps.Count} 个Collideable图层");
    }
    
    bool IsCollideableLayer(Tilemap tilemap)
    {
        string layerName = tilemap.name.ToLower();
        
        // 检查是否包含忽略关键词
        foreach (string ignoreKeyword in ignoreKeywords)
        {
            if (layerName.Contains(ignoreKeyword.ToLower()))
            {
                Debug.Log($"[SmartCollideableAttacker] 图层 {tilemap.name} 包含忽略关键词: {ignoreKeyword}");
                return false;
            }
        }
        
        // 检查是否包含Collideable关键词
        foreach (string keyword in collideableKeywords)
        {
            if (layerName.Contains(keyword.ToLower()))
            {
                Debug.Log($"[SmartCollideableAttacker] 图层 {tilemap.name} 匹配关键词: {keyword}");
                return true;
            }
        }
        
        return false;
    }
    
    void EnsureBreakableComponents(Tilemap tilemap)
    {
        GameObject tilemapObj = tilemap.gameObject;
        
        // 确保有必要的组件
        if (tilemapObj.GetComponent<TilemapCollider2D>() == null)
        {
            tilemapObj.AddComponent<TilemapCollider2D>().usedByComposite = true;
            Debug.Log($"[SmartCollideableAttacker] 为 {tilemap.name} 添加TilemapCollider2D");
        }
        
        if (tilemapObj.GetComponent<CompositeCollider2D>() == null)
        {
            tilemapObj.AddComponent<CompositeCollider2D>();
            Debug.Log($"[SmartCollideableAttacker] 为 {tilemap.name} 添加CompositeCollider2D");
        }
        
        var rb = tilemapObj.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = tilemapObj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            Debug.Log($"[SmartCollideableAttacker] 为 {tilemap.name} 添加Rigidbody2D");
        }
        
        if (tilemapObj.GetComponent<BreakableTilemapManager>() == null)
        {
            tilemapObj.AddComponent<BreakableTilemapManager>();
            Debug.Log($"[SmartCollideableAttacker] 为 {tilemap.name} 添加BreakableTilemapManager");
        }
    }
    
    void CheckForAttackInput()
    {
        if (playerAttackSystem == null) return;
        
        bool shouldAttack = false;
        
        foreach (var key in attackKeys)
        {
            if (Input.GetKeyDown(key))
            {
                shouldAttack = true;
                if (showDebugInfo)
                {
                    Debug.Log($"[SmartCollideableAttacker] 检测到攻击输入: {key}");
                }
                break;
            }
        }
        
        if (shouldAttack)
        {
            PerformSmartAttack();
        }
    }
    
    void PerformSmartAttack()
    {
        Vector3 playerPos = playerAttackSystem.transform.position;
        
        if (showDebugInfo)
        {
            Debug.Log($"[SmartCollideableAttacker] 开始智能攻击 - 玩家位置: {playerPos}");
        }
        
        // 刷新Collideable图层列表
        if (collideableTilemaps.Count == 0)
        {
            RefreshCollideableTilemaps();
        }
        
        // 查找距离最近的Collideable图层
        Tilemap nearestTilemap = null;
        float nearestDistance = float.MaxValue;
        Vector3Int nearestCellPos = Vector3Int.zero;
        
        foreach (var tilemap in collideableTilemaps)
        {
            if (tilemap == null) continue;
            
            Vector3Int playerCell = tilemap.WorldToCell(playerPos);
            
            // 搜索玩家周围的瓦片
            for (int x = -2; x <= 2; x++)
            {
                for (int y = -2; y <= 2; y++)
                {
                    Vector3Int cellPos = playerCell + new Vector3Int(x, y, 0);
                    TileBase tile = tilemap.GetTile(cellPos);
                    
                    if (tile != null)
                    {
                        Vector3 worldPos = tilemap.CellToWorld(cellPos);
                        float distance = Vector3.Distance(playerPos, worldPos);
                        
                        if (distance <= attackRadius && distance < nearestDistance)
                        {
                            nearestTilemap = tilemap;
                            nearestDistance = distance;
                            nearestCellPos = cellPos;
                        }
                    }
                }
            }
        }
        
        if (nearestTilemap != null)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[SmartCollideableAttacker] 攻击最近的Collideable瓦片: {nearestTilemap.name} 位置 {nearestCellPos} 距离 {nearestDistance:F2}");
            }
            
            AttackTile(nearestTilemap, nearestCellPos);
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.LogWarning($"[SmartCollideableAttacker] 攻击范围({attackRadius}m)内没有找到Collideable瓦片");
                LogNearbyTilemaps(playerPos);
            }
        }
    }
    
    void AttackTile(Tilemap tilemap, Vector3Int cellPos)
    {
        BreakableTilemapManager manager = tilemap.GetComponent<BreakableTilemapManager>();
        
        if (manager != null)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[SmartCollideableAttacker] 使用BreakableTilemapManager破坏瓦片 {cellPos}");
            }
            manager.TryBreakTile(cellPos);
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.Log($"[SmartCollideableAttacker] 直接破坏瓦片 {cellPos}");
            }
            
            // 直接破坏瓦片
            tilemap.SetTile(cellPos, null);
            CreateBreakEffect(tilemap.CellToWorld(cellPos));
        }
    }
    
    void CreateBreakEffect(Vector3 position)
    {
        // 简单的破坏效果
        GameObject effect = new GameObject("BreakEffect");
        effect.transform.position = position;
        
        for (int i = 0; i < 3; i++)
        {
            GameObject debris = new GameObject("Debris");
            debris.transform.parent = effect.transform;
            debris.transform.position = position + Random.insideUnitSphere * 0.3f;
            
            SpriteRenderer sr = debris.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite();
            sr.color = new Color(0.8f, 0.6f, 0.4f);
            sr.sortingOrder = 10;
            
            Rigidbody2D rb = debris.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1f;
            Vector2 force = Random.insideUnitCircle * 2f + Vector2.up * 1.5f;
            rb.AddForce(force, ForceMode2D.Impulse);
        }
        
        Destroy(effect, 2f);
    }
    
    Sprite CreateSquareSprite()
    {
        Texture2D texture = new Texture2D(4, 4);
        Color[] colors = new Color[16];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.white;
        }
        texture.SetPixels(colors);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 4, 4), Vector2.one * 0.5f, 100f);
    }
    
    void LogNearbyTilemaps(Vector3 playerPos)
    {
        Debug.Log("[SmartCollideableAttacker] 附近的图层信息:");
        
        Tilemap[] allTilemaps = FindObjectsOfType<Tilemap>();
        foreach (var tilemap in allTilemaps)
        {
            float distance = Vector3.Distance(playerPos, tilemap.transform.position);
            bool hasCollider = tilemap.GetComponent<TilemapCollider2D>() != null;
            bool hasBreakable = tilemap.GetComponent<BreakableTilemapManager>() != null;
            
            Debug.Log($"  - {tilemap.name}: 距离{distance:F1}m, 碰撞体:{hasCollider}, 可破坏:{hasBreakable}");
        }
    }
    
    [ContextMenu("刷新Collideable图层")]
    public void ManualRefresh()
    {
        RefreshCollideableTilemaps();
    }
    
    [ContextMenu("测试攻击")]
    public void TestAttack()
    {
        PerformSmartAttack();
    }
    
    void OnDrawGizmosSelected()
    {
        if (playerAttackSystem != null)
        {
            // 显示攻击范围
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerAttackSystem.transform.position, attackRadius);
            
            // 显示Collideable图层
            Gizmos.color = Color.green;
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
        if (playerAttackSystem == null) return;
        
        GUILayout.BeginArea(new Rect(10, 200, 350, 150));
        GUILayout.Label("=== 智能Collideable攻击器 ===");
        GUILayout.Label($"玩家位置: {playerAttackSystem.transform.position}");
        GUILayout.Label($"攻击范围: {attackRadius}m");
        GUILayout.Label($"Collideable图层: {collideableTilemaps.Count}个");
        
        GUILayout.Label("攻击按键:");
        foreach (var key in attackKeys)
        {
            GUILayout.Label($"  - {key}");
        }
        
        if (GUILayout.Button("手动测试攻击"))
        {
            TestAttack();
        }
        
        GUILayout.EndArea();
    }
} 