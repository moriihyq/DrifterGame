using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 可破坏Collideable瓦片系统
/// 为Tilemap中的collideable添加可被角色普攻打破的特性
/// </summary>
public class CollidableTileBreaker : MonoBehaviour
{
    [Header("瓦片破坏设置")]
    [SerializeField] private Tilemap collidableTilemap; // Collideable瓦片图
    [SerializeField] private int tileHitPoints = 1; // 瓦片耐久度
    [SerializeField] private float attackDetectionRange = 2.5f; // 攻击检测范围
    [SerializeField] private LayerMask playerLayer = 1; // 玩家图层
    
    [Header("破坏效果")]
    [SerializeField] private bool enableBreakEffects = true; // 启用破坏效果
    [SerializeField] private Color particleColor = new Color(0.6f, 0.4f, 0.2f); // 碎片颜色（棕色）
    [SerializeField] private int particleCount = 8; // 碎片数量
    [SerializeField] private float particleSpeed = 5f; // 碎片速度
    [SerializeField] private float particleLifetime = 1f; // 碎片存在时间
    
    [Header("音效设置")]
    [SerializeField] private AudioClip[] breakSounds; // 破坏音效数组
    [SerializeField] private float soundVolume = 0.8f; // 音效音量
    
    [Header("掉落设置")]
    [SerializeField] private bool enableDrops = false; // 启用掉落
    [SerializeField] private GameObject[] dropPrefabs; // 掉落物预制体
    [SerializeField] private float dropChance = 0.3f; // 掉落概率
    
    [Header("调试设置")]
    [SerializeField] private bool showDebugGizmos = true; // 显示调试Gizmos
    [SerializeField] private bool logDebugInfo = true; // 记录调试信息
    
    // 私有变量
    private Dictionary<Vector3Int, int> tileHealth; // 瓦片血量记录
    private AudioSource audioSource; // 音频源
    private Camera mainCamera; // 主摄像机
    
    // 输入检测
    private bool wasAttackPressed = false; // 上一帧是否按下攻击键
    
    private void Start()
    {
        InitializeSystem();
    }
    
    private void Update()
    {
        CheckPlayerAttack();
    }
    
    /// <summary>
    /// 初始化系统
    /// </summary>
    private void InitializeSystem()
    {
        // 初始化瓦片血量字典
        tileHealth = new Dictionary<Vector3Int, int>();
        
        // 查找或创建音频源
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // 获取主摄像机
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        // 自动查找Collideable Tilemap
        if (collidableTilemap == null)
        {
            FindCollidableTilemap();
        }
        
        if (logDebugInfo)
        {
            Debug.Log("[CollidableTileBreaker] 系统初始化完成");
            if (collidableTilemap != null)
            {
                Debug.Log($"[CollidableTileBreaker] 找到Collideable Tilemap: {collidableTilemap.name}");
            }
        }
    }
    
    /// <summary>
    /// 查找Collideable Tilemap
    /// </summary>
    private void FindCollidableTilemap()
    {
        Tilemap[] tilemaps = FindObjectsOfType<Tilemap>();
        
        if (logDebugInfo)
        {
            Debug.Log($"[CollidableTileBreaker] 场景中找到 {tilemaps.Length} 个Tilemap:");
            foreach (Tilemap tm in tilemaps)
            {
                Debug.Log($"[CollidableTileBreaker] - Tilemap: '{tm.name}' (GameObject: '{tm.gameObject.name}')");
            }
        }
        
        foreach (Tilemap tilemap in tilemaps)
        {
            string tilemapNameLower = tilemap.name.ToLower();
            string gameObjectNameLower = tilemap.gameObject.name.ToLower();
            
            if (tilemapNameLower.Contains("collideable") || 
                tilemapNameLower.Contains("collision") ||
                tilemapNameLower.Contains("wall") ||
                gameObjectNameLower.Contains("collideable") ||
                gameObjectNameLower.Contains("collision") ||
                gameObjectNameLower.Contains("wall"))
            {
                collidableTilemap = tilemap;
                if (logDebugInfo)
                {
                    Debug.Log($"[CollidableTileBreaker] 自动找到Collideable Tilemap: {tilemap.name} (GameObject: {tilemap.gameObject.name})");
                }
                break;
            }
        }
        
        if (collidableTilemap == null)
        {
            if (logDebugInfo)
            {
                Debug.LogWarning("[CollidableTileBreaker] 未找到Collideable Tilemap，请手动指定或确保Tilemap名称包含'collideable'、'collision'或'wall'");
            }
            
            // 如果没找到符合条件的，尝试使用第一个Tilemap
            if (tilemaps.Length > 0)
            {
                collidableTilemap = tilemaps[0];
                if (logDebugInfo)
                {
                    Debug.Log($"[CollidableTileBreaker] 作为备选，使用第一个找到的Tilemap: {tilemaps[0].name}");
                }
            }
        }
    }
    
    /// <summary>
    /// 检查玩家攻击
    /// </summary>
    private void CheckPlayerAttack()
    {
        bool isAttackPressed = false;
        
        // 检查多种攻击输入
        try
        {
            isAttackPressed = Input.GetButton("Fire1");
        }
        catch
        {
            // Fire1可能未配置，使用其他输入
        }
        
        // 备用输入检测
        if (!isAttackPressed)
        {
            isAttackPressed = Input.GetMouseButton(0) || 
                             Input.GetKey(KeyCode.LeftControl) || 
                             Input.GetKey(KeyCode.Z) || 
                             Input.GetKey(KeyCode.X);
        }
        
        // 检测攻击按键按下（从未按下变为按下）
        if (isAttackPressed && !wasAttackPressed)
        {
            PerformAttackCheck();
        }
        
        wasAttackPressed = isAttackPressed;
    }
    
    /// <summary>
    /// 执行攻击检查
    /// </summary>
    private void PerformAttackCheck()
    {
        if (collidableTilemap == null) return;
        
        // 查找玩家
        GameObject player = FindPlayer();
        if (player == null) return;
        
        Vector3 playerPosition = player.transform.position;
        
        if (logDebugInfo)
        {
            Debug.Log($"[CollidableTileBreaker] 玩家攻击检查，位置: {playerPosition}");
        }
        
        // 检查攻击范围内的瓦片
        CheckTilesInRange(playerPosition);
    }
    
    /// <summary>
    /// 查找玩家对象
    /// </summary>
    private GameObject FindPlayer()
    {
        // 首先尝试通过标签查找
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) return player;
        
        // 通过名称查找
        player = GameObject.Find("Player");
        if (player != null) return player;
        
        // 通过图层查找
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (((1 << obj.layer) & playerLayer) != 0)
            {
                return obj;
            }
        }
        
        // 最后尝试查找包含"player"的对象
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.ToLower().Contains("player"))
            {
                return obj;
            }
        }
        
        if (logDebugInfo)
        {
            Debug.LogWarning("[CollidableTileBreaker] 未找到玩家对象");
        }
        
        return null;
    }
    
    /// <summary>
    /// 检查范围内的瓦片
    /// </summary>
    private void CheckTilesInRange(Vector3 centerPosition)
    {
        if (collidableTilemap == null)
        {
            if (logDebugInfo)
            {
                Debug.LogError("[CollidableTileBreaker] Collideable Tilemap 未设置！");
            }
            return;
        }
        
        // 计算检查范围
        Vector3Int centerCell = collidableTilemap.WorldToCell(centerPosition);
        int checkRadius = Mathf.CeilToInt(attackDetectionRange);
        
        if (logDebugInfo)
        {
            Debug.Log($"[CollidableTileBreaker] 检查中心位置: {centerPosition}, 转换为瓦片坐标: {centerCell}, 检查半径: {checkRadius}");
            Debug.Log($"[CollidableTileBreaker] 使用的Tilemap: {collidableTilemap.name}");
        }
        
        List<Vector3Int> tilesToBreak = new List<Vector3Int>();
        int checkedTiles = 0;
        int foundTiles = 0;
        
        // 遍历范围内的所有瓦片
        for (int x = -checkRadius; x <= checkRadius; x++)
        {
            for (int y = -checkRadius; y <= checkRadius; y++)
            {
                Vector3Int tilePosition = centerCell + new Vector3Int(x, y, 0);
                Vector3 worldPosition = collidableTilemap.CellToWorld(tilePosition) + collidableTilemap.cellSize * 0.5f;
                
                checkedTiles++;
                
                // 检查距离
                float distance = Vector3.Distance(centerPosition, worldPosition);
                
                // 详细调试每个位置
                if (logDebugInfo && (x >= -1 && x <= 1 && y >= -1 && y <= 1)) // 只显示中心3x3区域避免日志过多
                {
                    TileBase debugTile = collidableTilemap.GetTile(tilePosition);
                    Debug.Log($"[CollidableTileBreaker] 检查位置 ({x},{y}): 瓦片坐标={tilePosition}, 世界坐标={worldPosition:F2}, 距离={distance:F2}, 瓦片={debugTile?.name ?? "null"}");
                }
                
                if (distance <= attackDetectionRange)
                {
                    // 检查该位置是否有瓦片
                    TileBase tile = collidableTilemap.GetTile(tilePosition);
                    if (tile != null)
                    {
                        foundTiles++;
                        tilesToBreak.Add(tilePosition);
                        
                        if (logDebugInfo)
                        {
                            Debug.Log($"[CollidableTileBreaker] ✓ 找到可破坏瓦片: {tilePosition}, 世界坐标: {worldPosition}, 距离: {distance:F2}, 瓦片类型: {tile.name}");
                        }
                    }
                    else if (logDebugInfo && distance <= attackDetectionRange * 0.5f) // 只在很近的距离报告空瓦片
                    {
                        Debug.Log($"[CollidableTileBreaker] ○ 范围内但无瓦片: {tilePosition}, 距离: {distance:F2}");
                    }
                }
            }
        }
        
        if (logDebugInfo)
        {
            Debug.Log($"[CollidableTileBreaker] 检查了 {checkedTiles} 个位置，找到了 {foundTiles} 个瓦片，准备破坏 {tilesToBreak.Count} 个瓦片");
        }
        
        // 破坏找到的瓦片
        foreach (Vector3Int tilePos in tilesToBreak)
        {
            DamageTile(tilePos);
        }
        
        if (tilesToBreak.Count == 0 && logDebugInfo)
        {
            Debug.LogWarning($"[CollidableTileBreaker] 在攻击范围 {attackDetectionRange} 内未找到任何瓦片！");
        }
    }
    
    /// <summary>
    /// 对瓦片造成伤害
    /// </summary>
    private void DamageTile(Vector3Int tilePosition)
    {
        // 获取或初始化瓦片血量
        if (!tileHealth.ContainsKey(tilePosition))
        {
            tileHealth[tilePosition] = tileHitPoints;
        }
        
        // 减少血量
        tileHealth[tilePosition]--;
        
        if (logDebugInfo)
        {
            Debug.Log($"[CollidableTileBreaker] 瓦片 {tilePosition} 受到伤害，剩余血量: {tileHealth[tilePosition]}");
        }
        
        // 检查是否应该破坏瓦片
        if (tileHealth[tilePosition] <= 0)
        {
            BreakTile(tilePosition);
        }
    }
    
    /// <summary>
    /// 破坏瓦片
    /// </summary>
    private void BreakTile(Vector3Int tilePosition)
    {
        if (collidableTilemap == null) return;
        
        // 获取瓦片世界位置
        Vector3 worldPosition = collidableTilemap.CellToWorld(tilePosition) + collidableTilemap.cellSize * 0.5f;
        
        // 移除瓦片
        collidableTilemap.SetTile(tilePosition, null);
        
        // 从血量记录中移除
        tileHealth.Remove(tilePosition);
        
        if (logDebugInfo)
        {
            Debug.Log($"[CollidableTileBreaker] 瓦片 {tilePosition} 已被破坏");
        }
        
        // 创建破坏效果
        if (enableBreakEffects)
        {
            CreateBreakEffects(worldPosition);
        }
        
        // 播放破坏音效
        PlayBreakSound();
        
        // 处理掉落
        if (enableDrops)
        {
            HandleDrop(worldPosition);
        }
    }
    
    /// <summary>
    /// 创建破坏效果
    /// </summary>
    private void CreateBreakEffects(Vector3 position)
    {
        // 创建粒子效果
        for (int i = 0; i < particleCount; i++)
        {
            CreateParticle(position);
        }
    }
    
    /// <summary>
    /// 创建单个粒子
    /// </summary>
    private void CreateParticle(Vector3 position)
    {
        // 创建粒子对象
        GameObject particle = new GameObject("BreakParticle");
        particle.transform.position = position;
        
        // 添加SpriteRenderer
        SpriteRenderer renderer = particle.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateParticleSprite();
        renderer.color = particleColor;
        renderer.sortingOrder = 10;
        
        // 添加Rigidbody2D用于物理运动
        Rigidbody2D rb = particle.AddComponent<Rigidbody2D>();
        rb.gravityScale = 1f;
        rb.linearDamping = 0.5f;
        
        // 随机初始速度
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        rb.linearVelocity = randomDirection * particleSpeed + Vector2.up * particleSpeed * 0.5f;
        rb.angularVelocity = Random.Range(-360f, 360f);
        
        // 设置粒子自动销毁
        StartCoroutine(DestroyParticleAfterTime(particle, particleLifetime));
    }
    
    /// <summary>
    /// 创建粒子精灵
    /// </summary>
    private Sprite CreateParticleSprite()
    {
        // 创建简单的方形粒子
        Texture2D texture = new Texture2D(4, 4);
        Color[] pixels = new Color[16];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 4, 4), Vector2.one * 0.5f);
    }
    
    /// <summary>
    /// 在指定时间后销毁粒子
    /// </summary>
    private IEnumerator DestroyParticleAfterTime(GameObject particle, float time)
    {
        // 渐隐效果
        SpriteRenderer renderer = particle.GetComponent<SpriteRenderer>();
        Color originalColor = renderer.color;
        
        float elapsed = 0f;
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / time);
            renderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        
        if (particle != null)
        {
            Destroy(particle);
        }
    }
    
    /// <summary>
    /// 播放破坏音效
    /// </summary>
    private void PlayBreakSound()
    {
        if (audioSource != null && breakSounds != null && breakSounds.Length > 0)
        {
            AudioClip soundToPlay = breakSounds[Random.Range(0, breakSounds.Length)];
            audioSource.PlayOneShot(soundToPlay, soundVolume);
        }
    }
    
    /// <summary>
    /// 处理掉落
    /// </summary>
    private void HandleDrop(Vector3 position)
    {
        if (dropPrefabs == null || dropPrefabs.Length == 0) return;
        
        if (Random.Range(0f, 1f) <= dropChance)
        {
            GameObject dropPrefab = dropPrefabs[Random.Range(0, dropPrefabs.Length)];
            if (dropPrefab != null)
            {
                Instantiate(dropPrefab, position, Quaternion.identity);
                
                if (logDebugInfo)
                {
                    Debug.Log($"[CollidableTileBreaker] 在 {position} 掉落了 {dropPrefab.name}");
                }
            }
        }
    }
    
    /// <summary>
    /// 公共方法：直接破坏指定位置的瓦片
    /// </summary>
    public void BreakTileAtPosition(Vector3 worldPosition)
    {
        if (collidableTilemap == null) return;
        
        Vector3Int tilePosition = collidableTilemap.WorldToCell(worldPosition);
        TileBase tile = collidableTilemap.GetTile(tilePosition);
        
        if (tile != null)
        {
            BreakTile(tilePosition);
        }
    }
    
    /// <summary>
    /// 公共方法：破坏指定范围内的所有瓦片
    /// </summary>
    public void BreakTilesInRange(Vector3 centerPosition, float radius)
    {
        if (collidableTilemap == null) return;
        
        Vector3Int centerCell = collidableTilemap.WorldToCell(centerPosition);
        int checkRadius = Mathf.CeilToInt(radius);
        
        for (int x = -checkRadius; x <= checkRadius; x++)
        {
            for (int y = -checkRadius; y <= checkRadius; y++)
            {
                Vector3Int tilePosition = centerCell + new Vector3Int(x, y, 0);
                Vector3 worldPosition = collidableTilemap.CellToWorld(tilePosition) + collidableTilemap.cellSize * 0.5f;
                
                float distance = Vector3.Distance(centerPosition, worldPosition);
                if (distance <= radius)
                {
                    TileBase tile = collidableTilemap.GetTile(tilePosition);
                    if (tile != null)
                    {
                        BreakTile(tilePosition);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 设置瓦片耐久度
    /// </summary>
    public void SetTileHitPoints(int hitPoints)
    {
        tileHitPoints = Mathf.Max(1, hitPoints);
    }
    
    /// <summary>
    /// 设置攻击检测范围
    /// </summary>
    public void SetAttackRange(float range)
    {
        attackDetectionRange = Mathf.Max(0.1f, range);
    }
    
    /// <summary>
    /// 调试方法：强制执行攻击检查
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugForceAttack()
    {
        if (logDebugInfo)
        {
            Debug.Log("[CollidableTileBreaker] 手动触发攻击检查");
        }
        PerformAttackCheck();
    }
    
    /// <summary>
    /// 调试方法：显示当前系统状态
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugShowStatus()
    {
        Debug.Log("=== CollidableTileBreaker 状态 ===");
        Debug.Log($"Collideable Tilemap: {(collidableTilemap != null ? collidableTilemap.name : "未设置")}");
        Debug.Log($"攻击检测范围: {attackDetectionRange}");
        Debug.Log($"瓦片耐久度: {tileHitPoints}");
        Debug.Log($"调试信息: {logDebugInfo}");
        
        GameObject player = FindPlayer();
        if (player != null)
        {
            Debug.Log($"玩家对象: {player.name}, 位置: {player.transform.position}");
        }
        else
        {
            Debug.LogWarning("未找到玩家对象！");
        }
        
        if (collidableTilemap != null && player != null)
        {
            Vector3 playerPos = player.transform.position;
            Vector3Int cellPos = collidableTilemap.WorldToCell(playerPos);
            TileBase tileAtPlayer = collidableTilemap.GetTile(cellPos);
            Debug.Log($"玩家位置的瓦片: {(tileAtPlayer != null ? tileAtPlayer.name : "无瓦片")}");
            Debug.Log($"Tilemap 单元格大小: {collidableTilemap.cellSize}");
            Debug.Log($"Tilemap 世界坐标: {collidableTilemap.transform.position}");
            Debug.Log($"玩家瓦片坐标: {cellPos}");
            
            // 检查周围9个格子
            Debug.Log("--- 周围瓦片检查 ---");
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Vector3Int checkPos = cellPos + new Vector3Int(x, y, 0);
                    TileBase tile = collidableTilemap.GetTile(checkPos);
                    Vector3 worldPos = collidableTilemap.CellToWorld(checkPos);
                    float distance = Vector3.Distance(playerPos, worldPos);
                    Debug.Log($"  位置({x},{y}): 瓦片坐标={checkPos}, 世界坐标={worldPos:F2}, 距离={distance:F2}, 瓦片={(tile != null ? tile.name : "null")}");
                }
            }
        }
        
        Debug.Log("================================");
    }
    
    /// <summary>
    /// 调试方法：测试指定位置的瓦片检测
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugTestTileAt(Vector3 worldPosition)
    {
        if (collidableTilemap == null)
        {
            Debug.LogError("[CollidableTileBreaker] Tilemap 未设置！");
            return;
        }
        
        Vector3Int tilePos = collidableTilemap.WorldToCell(worldPosition);
        TileBase tile = collidableTilemap.GetTile(tilePos);
        Vector3 tileCenterWorld = collidableTilemap.CellToWorld(tilePos) + collidableTilemap.cellSize * 0.5f;
        
        Debug.Log($"=== 瓦片检测测试 ===");
        Debug.Log($"测试世界位置: {worldPosition}");
        Debug.Log($"转换瓦片坐标: {tilePos}");
        Debug.Log($"瓦片中心世界坐标: {tileCenterWorld}");
        Debug.Log($"瓦片对象: {(tile != null ? tile.name + " (类型: " + tile.GetType().Name + ")" : "null")}");
        
        if (tile != null)
        {
            Debug.Log($"✓ 该位置有瓦片，可以被破坏");
        }
        else
        {
            Debug.Log($"✗ 该位置没有瓦片");
        }
    }
    
    /// <summary>
    /// 调试方法：扫描所有Tilemap，找到在玩家位置附近有瓦片的Tilemap
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugScanAllTilemaps()
    {
        GameObject player = FindPlayer();
        if (player == null)
        {
            Debug.LogError("[CollidableTileBreaker] 未找到玩家！");
            return;
        }
        
        Vector3 playerPos = player.transform.position;
        Tilemap[] allTilemaps = FindObjectsOfType<Tilemap>();
        
        Debug.Log($"=== 扫描所有Tilemap (玩家位置: {playerPos}) ===");
        
        foreach (Tilemap tilemap in allTilemaps)
        {
            Debug.Log($"\n检查Tilemap: '{tilemap.name}' (GameObject: '{tilemap.gameObject.name}')");
            Debug.Log($"  Transform位置: {tilemap.transform.position}");
            Debug.Log($"  单元格大小: {tilemap.cellSize}");
            
            // 检查玩家位置周围的瓦片
            Vector3Int centerCell = tilemap.WorldToCell(playerPos);
            int foundTiles = 0;
            
            for (int x = -2; x <= 2; x++)
            {
                for (int y = -2; y <= 2; y++)
                {
                    for (int z = -1; z <= 1; z++) // 也检查Z轴
                    {
                        Vector3Int checkPos = centerCell + new Vector3Int(x, y, z);
                        TileBase tile = tilemap.GetTile(checkPos);
                        
                        if (tile != null)
                        {
                            foundTiles++;
                            Vector3 worldPos = tilemap.CellToWorld(checkPos);
                            float distance = Vector3.Distance(playerPos, worldPos);
                            Debug.Log($"  ✓ 找到瓦片: {checkPos} -> 世界坐标{worldPos:F2}, 距离{distance:F2}, 瓦片={tile.name}");
                        }
                    }
                }
            }
            
            if (foundTiles > 0)
            {
                Debug.Log($"  📍 这个Tilemap在玩家附近有 {foundTiles} 个瓦片！");
            }
            else
            {
                Debug.Log($"  ❌ 这个Tilemap在玩家附近没有瓦片");
            }
        }
        
        Debug.Log("=== 扫描完成 ===");
    }
    
    /// <summary>
    /// 绘制调试Gizmos
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;
        
        // 绘制攻击检测范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDetectionRange);
        
        // 如果有玩家，在玩家位置绘制攻击范围
        GameObject player = FindPlayer();
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.transform.position, attackDetectionRange);
            
            // 如果有Tilemap，绘制检查的瓦片网格
            if (collidableTilemap != null)
            {
                DrawTileGrid(player.transform.position);
            }
        }
    }
    
    /// <summary>
    /// 绘制瓦片网格用于调试
    /// </summary>
    private void DrawTileGrid(Vector3 centerPosition)
    {
        Vector3Int centerCell = collidableTilemap.WorldToCell(centerPosition);
        int checkRadius = Mathf.CeilToInt(attackDetectionRange);
        
        for (int x = -checkRadius; x <= checkRadius; x++)
        {
            for (int y = -checkRadius; y <= checkRadius; y++)
            {
                Vector3Int tilePosition = centerCell + new Vector3Int(x, y, 0);
                Vector3 worldPosition = collidableTilemap.CellToWorld(tilePosition) + collidableTilemap.cellSize * 0.5f;
                
                float distance = Vector3.Distance(centerPosition, worldPosition);
                if (distance <= attackDetectionRange)
                {
                    TileBase tile = collidableTilemap.GetTile(tilePosition);
                    
                    if (tile != null)
                    {
                        // 有瓦片的位置用绿色立方体表示
                        Gizmos.color = Color.green;
                        Gizmos.DrawCube(worldPosition, collidableTilemap.cellSize * 0.8f);
                    }
                    else
                    {
                        // 无瓦片的位置用灰色线框表示
                        Gizmos.color = Color.gray;
                        Gizmos.DrawWireCube(worldPosition, collidableTilemap.cellSize * 0.6f);
                    }
                }
            }
        }
    }
} 