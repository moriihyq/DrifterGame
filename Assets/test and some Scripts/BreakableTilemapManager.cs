using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class BreakableTilemapManager : MonoBehaviour
{
    [Header("破坏系统设置")]
    [SerializeField] private Tilemap breakableTilemap; // 可破坏的Tilemap
    [SerializeField] private TileBase[] breakableTiles; // 可破坏的瓦片类型数组
    [SerializeField] private float detectionRadius = 1.5f; // 攻击检测半径
    
    [Header("破坏效果")]
    [SerializeField] private GameObject breakEffectPrefab; // 破坏特效预制体
    [SerializeField] private AudioClip[] breakSounds; // 破坏音效数组
    [SerializeField] private bool createDebris = true; // 是否创建碎片
    [SerializeField] private int debrisCount = 5; // 碎片数量
    [SerializeField] private float debrisForce = 3f; // 碎片力度
    [SerializeField] private Sprite debrisSprite; // 碎片精灵图
    
    [Header("掉落物品")]
    [SerializeField] private GameObject[] dropItems; // 可掉落的物品预制体
    [SerializeField] private float dropChance = 0.3f; // 掉落概率
    
    [Header("调试设置")]
    [SerializeField] private bool showDebugInfo = true; // 显示调试信息
    [SerializeField] private bool visualizeDetection = true; // 可视化检测区域
    
    // 私有变量
    private PlayerAttackSystem playerAttackSystem;
    private AudioSource audioSource;
    private Dictionary<Vector3Int, float> crackedTiles; // 记录已经受损的瓦片
    
    [Header("耐久度系统")]
    [SerializeField] private bool useHealthSystem = true; // 是否使用耐久度系统
    [SerializeField] private int tileHealth = 2; // 瓦片默认血量
    [SerializeField] private TileBase[] crackedTileSprites; // 受损瓦片精灵（可选）
    
    private void Start()
    {
        InitializeBreakableSystem();
    }
    
    private void Update()
    {
        CheckForPlayerAttack();
    }
    
    /// <summary>
    /// 初始化破坏系统
    /// </summary>
    private void InitializeBreakableSystem()
    {
        // 查找组件
        if (breakableTilemap == null)
        {
            breakableTilemap = GetComponent<Tilemap>();
            if (breakableTilemap == null)
            {
                breakableTilemap = FindObjectOfType<Tilemap>();
                if (breakableTilemap == null)
                {
                    Debug.LogError("[BreakableTilemapManager] 未找到Tilemap组件！");
                    return;
                }
            }
        }
        
        if (playerAttackSystem == null)
        {
            playerAttackSystem = FindObjectOfType<PlayerAttackSystem>();
            if (playerAttackSystem == null)
            {
                Debug.LogError("[BreakableTilemapManager] 未找到PlayerAttackSystem组件！");
            }
        }
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // 初始化受损瓦片字典
        crackedTiles = new Dictionary<Vector3Int, float>();
        
        if (showDebugInfo)
        {
            Debug.Log("[BreakableTilemapManager] 破坏系统初始化完成");
        }
    }
    
    /// <summary>
    /// 检查玩家攻击
    /// </summary>
    private void CheckForPlayerAttack()
    {
        if (playerAttackSystem == null) return;
        
        // 检查玩家是否在攻击（这里假设使用Fire1按键）
        if (Input.GetButtonDown("Fire1"))
        {
            CheckTilesInAttackRange();
        }
    }
    
    /// <summary>
    /// 检查攻击范围内的瓦片
    /// </summary>
    private void CheckTilesInAttackRange()
    {
        // 获取玩家位置
        Vector3 playerPosition = playerAttackSystem.transform.position;
        
        // 计算检测区域
        Vector3Int centerCell = breakableTilemap.WorldToCell(playerPosition);
        int radius = Mathf.CeilToInt(detectionRadius);
        
        // 检查范围内的所有瓦片
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector3Int cellPosition = centerCell + new Vector3Int(x, y, 0);
                Vector3 worldPosition = breakableTilemap.CellToWorld(cellPosition);
                
                // 检查距离
                float distance = Vector3.Distance(playerPosition, worldPosition);
                if (distance <= detectionRadius)
                {
                    TryBreakTile(cellPosition);
                }
            }
        }
    }
    
    /// <summary>
    /// 尝试破坏瓦片
    /// </summary>
    /// <param name="cellPosition">瓦片位置</param>
    public void TryBreakTile(Vector3Int cellPosition)
    {
        TileBase tile = breakableTilemap.GetTile(cellPosition);
        
        if (tile == null) return; // 没有瓦片
        
        // 检查是否为可破坏瓦片
        if (!IsBreakableTile(tile)) return;
        
        if (useHealthSystem)
        {
            // 使用耐久度系统
            DamageTile(cellPosition, tile);
        }
        else
        {
            // 直接破坏
            BreakTile(cellPosition, tile);
        }
    }
    
    /// <summary>
    /// 检查是否为可破坏瓦片
    /// </summary>
    /// <param name="tile">瓦片</param>
    /// <returns>是否可破坏</returns>
    private bool IsBreakableTile(TileBase tile)
    {
        if (breakableTiles == null || breakableTiles.Length == 0)
            return true; // 如果没有指定可破坏瓦片，则所有瓦片都可破坏
        
        foreach (TileBase breakableTile in breakableTiles)
        {
            if (tile == breakableTile)
                return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 对瓦片造成伤害
    /// </summary>
    /// <param name="cellPosition">瓦片位置</param>
    /// <param name="tile">瓦片</param>
    private void DamageTile(Vector3Int cellPosition, TileBase tile)
    {
        // 获取或初始化瓦片血量
        if (!crackedTiles.ContainsKey(cellPosition))
        {
            crackedTiles[cellPosition] = tileHealth;
        }
        
        // 造成伤害
        crackedTiles[cellPosition]--;
        
        if (showDebugInfo)
        {
            Debug.Log($"[BreakableTilemapManager] 瓦片 {cellPosition} 受到伤害，剩余血量: {crackedTiles[cellPosition]}");
        }
        
        // 更新瓦片外观（显示裂纹）
        UpdateTileAppearance(cellPosition, crackedTiles[cellPosition]);
        
        // 检查是否应该破坏
        if (crackedTiles[cellPosition] <= 0)
        {
            BreakTile(cellPosition, tile);
            crackedTiles.Remove(cellPosition);
        }
    }
    
    /// <summary>
    /// 更新瓦片外观
    /// </summary>
    /// <param name="cellPosition">瓦片位置</param>
    /// <param name="health">剩余血量</param>
    private void UpdateTileAppearance(Vector3Int cellPosition, float health)
    {
        if (crackedTileSprites == null || crackedTileSprites.Length == 0) return;
        
        // 根据血量比例选择不同的受损精灵
        float healthRatio = health / tileHealth;
        int spriteIndex = Mathf.Clamp(crackedTileSprites.Length - 1 - (int)(healthRatio * crackedTileSprites.Length), 0, crackedTileSprites.Length - 1);
        
        if (spriteIndex < crackedTileSprites.Length)
        {
            breakableTilemap.SetTile(cellPosition, crackedTileSprites[spriteIndex]);
        }
    }
    
    /// <summary>
    /// 破坏瓦片
    /// </summary>
    /// <param name="cellPosition">瓦片位置</param>
    /// <param name="tile">瓦片</param>
    private void BreakTile(Vector3Int cellPosition, TileBase tile)
    {
        Vector3 worldPosition = breakableTilemap.CellToWorld(cellPosition) + breakableTilemap.cellSize * 0.5f;
        
        if (showDebugInfo)
        {
            Debug.Log($"[BreakableTilemapManager] 破坏瓦片: {cellPosition} (世界坐标: {worldPosition})");
        }
        
        // 移除瓦片
        breakableTilemap.SetTile(cellPosition, null);
        
        // 播放破坏效果
        CreateBreakEffects(worldPosition);
        
        // 播放音效
        PlayBreakSound();
        
        // 创建碎片
        if (createDebris)
        {
            CreateDebris(worldPosition);
        }
        
        // 掉落物品
        TryDropItem(worldPosition);
    }
    
    /// <summary>
    /// 创建破坏特效
    /// </summary>
    /// <param name="position">位置</param>
    private void CreateBreakEffects(Vector3 position)
    {
        if (breakEffectPrefab != null)
        {
            GameObject effect = Instantiate(breakEffectPrefab, position, Quaternion.identity);
            
            // 自动销毁特效
            ParticleSystem particles = effect.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                Destroy(effect, particles.main.duration + particles.main.startLifetime.constantMax);
            }
            else
            {
                Destroy(effect, 2f); // 默认2秒后销毁
            }
        }
    }
    
    /// <summary>
    /// 播放破坏音效
    /// </summary>
    private void PlayBreakSound()
    {
        if (breakSounds != null && breakSounds.Length > 0 && audioSource != null)
        {
            AudioClip soundToPlay = breakSounds[Random.Range(0, breakSounds.Length)];
            audioSource.PlayOneShot(soundToPlay);
        }
    }
    
    /// <summary>
    /// 创建碎片
    /// </summary>
    /// <param name="position">位置</param>
    private void CreateDebris(Vector3 position)
    {
        for (int i = 0; i < debrisCount; i++)
        {
            GameObject debris = new GameObject("Debris");
            debris.transform.position = position + Random.insideUnitSphere * 0.2f;
            
            SpriteRenderer sr = debris.AddComponent<SpriteRenderer>();
            if (debrisSprite != null)
            {
                sr.sprite = debrisSprite;
            }
            else
            {
                // 创建简单的方形碎片
                sr.sprite = CreateDebrisSprite();
            }
            
            sr.sortingOrder = 10;
            sr.color = new Color(0.6f, 0.4f, 0.2f); // 棕色碎片
            
            Rigidbody2D rb = debris.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1f;
            
            // 添加随机力
            Vector2 force = new Vector2(Random.Range(-1f, 1f), Random.Range(0.5f, 1.5f)) * debrisForce;
            rb.AddForce(force, ForceMode2D.Impulse);
            rb.AddTorque(Random.Range(-100f, 100f));
            
            // 添加碰撞体
            BoxCollider2D col = debris.AddComponent<BoxCollider2D>();
            col.size = Vector2.one * 0.1f;
            
            // 设置碎片自动销毁
            Destroy(debris, Random.Range(3f, 6f));
        }
    }
    
    /// <summary>
    /// 创建简单的碎片精灵
    /// </summary>
    /// <returns>碎片精灵</returns>
    private Sprite CreateDebrisSprite()
    {
        Texture2D texture = new Texture2D(8, 8);
        Color[] colors = new Color[64];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = new Color(0.6f, 0.4f, 0.2f, 1f);
        }
        texture.SetPixels(colors);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 8, 8), Vector2.one * 0.5f, 100f);
    }
    
    /// <summary>
    /// 尝试掉落物品
    /// </summary>
    /// <param name="position">位置</param>
    private void TryDropItem(Vector3 position)
    {
        if (dropItems == null || dropItems.Length == 0) return;
        
        if (Random.Range(0f, 1f) <= dropChance)
        {
            GameObject itemToDrop = dropItems[Random.Range(0, dropItems.Length)];
            Instantiate(itemToDrop, position, Quaternion.identity);
            
            if (showDebugInfo)
            {
                Debug.Log($"[BreakableTilemapManager] 掉落物品: {itemToDrop.name}");
            }
        }
    }
    
    /// <summary>
    /// 外部调用的破坏方法
    /// </summary>
    /// <param name="worldPosition">世界坐标</param>
    public void BreakTileAtWorldPosition(Vector3 worldPosition)
    {
        Vector3Int cellPosition = breakableTilemap.WorldToCell(worldPosition);
        TryBreakTile(cellPosition);
    }
    
    /// <summary>
    /// 批量破坏区域内的瓦片
    /// </summary>
    /// <param name="centerPosition">中心位置（世界坐标）</param>
    /// <param name="radius">破坏半径</param>
    public void BreakTilesInArea(Vector3 centerPosition, float radius)
    {
        Vector3Int centerCell = breakableTilemap.WorldToCell(centerPosition);
        int cellRadius = Mathf.CeilToInt(radius);
        
        for (int x = -cellRadius; x <= cellRadius; x++)
        {
            for (int y = -cellRadius; y <= cellRadius; y++)
            {
                Vector3Int cellPosition = centerCell + new Vector3Int(x, y, 0);
                Vector3 worldPosition = breakableTilemap.CellToWorld(cellPosition);
                
                if (Vector3.Distance(centerPosition, worldPosition) <= radius)
                {
                    TryBreakTile(cellPosition);
                }
            }
        }
    }
    
    /// <summary>
    /// 设置瓦片类型为可破坏
    /// </summary>
    /// <param name="tiles">瓦片数组</param>
    public void SetBreakableTiles(TileBase[] tiles)
    {
        breakableTiles = tiles;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (visualizeDetection && playerAttackSystem != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerAttackSystem.transform.position, detectionRadius);
        }
    }
} 