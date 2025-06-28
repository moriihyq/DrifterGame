using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerAttackTilemapIntegration : MonoBehaviour
{
    [Header("Tilemap攻击设置")]
    [SerializeField] private LayerMask tilemapLayers = -1; // Tilemap所在的图层
    [SerializeField] private float tilemapAttackRange = 2f; // 对Tilemap的攻击范围
    [SerializeField] private bool showAttackRange = true; // 显示攻击范围
    
    // 组件引用
    private PlayerAttackSystem playerAttackSystem;
    private BreakableTilemapManager breakableTilemapManager;
    
    private void Start()
    {
        InitializeComponents();
    }
    
    private void InitializeComponents()
    {
        // 获取PlayerAttackSystem
        playerAttackSystem = GetComponent<PlayerAttackSystem>();
        if (playerAttackSystem == null)
        {
            playerAttackSystem = FindObjectOfType<PlayerAttackSystem>();
            if (playerAttackSystem == null)
            {
                Debug.LogError("[PlayerAttackTilemapIntegration] 未找到PlayerAttackSystem组件！");
                return;
            }
        }
        
        // 获取BreakableTilemapManager
        breakableTilemapManager = FindObjectOfType<BreakableTilemapManager>();
        if (breakableTilemapManager == null)
        {
            Debug.LogWarning("[PlayerAttackTilemapIntegration] 未找到BreakableTilemapManager组件！将无法破坏Tilemap");
        }
        
        Debug.Log("[PlayerAttackTilemapIntegration] Tilemap攻击集成初始化完成");
    }
    
    private void Update()
    {
        CheckForTilemapAttack();
    }
    
    /// <summary>
    /// 检查对Tilemap的攻击
    /// </summary>
    private void CheckForTilemapAttack()
    {
        if (playerAttackSystem == null) return;
        
        // 监听攻击输入（与PlayerAttackSystem保持一致）
        if (Input.GetButtonDown("Fire1"))
        {
            PerformTilemapAttack();
        }
    }
    
    /// <summary>
    /// 执行对Tilemap的攻击
    /// </summary>
    private void PerformTilemapAttack()
    {
        Vector3 attackPosition = transform.position;
        
        // 获取攻击方向（基于玩家面向方向）
        Vector2 attackDirection = GetAttackDirection();
        Vector3 attackPoint = attackPosition + (Vector3)attackDirection * tilemapAttackRange * 0.5f;
        
        // 检测Tilemap碰撞体
        Collider2D[] tilemapColliders = Physics2D.OverlapCircleAll(attackPoint, tilemapAttackRange, tilemapLayers);
        
        foreach (Collider2D collider in tilemapColliders)
        {
            // 检查是否为Tilemap碰撞体
            TilemapCollider2D tilemapCollider = collider.GetComponent<TilemapCollider2D>();
            if (tilemapCollider != null)
            {
                Tilemap tilemap = tilemapCollider.GetComponent<Tilemap>();
                if (tilemap != null)
                {
                    AttackTilemap(tilemap, attackPoint);
                }
            }
        }
    }
    
    /// <summary>
    /// 攻击Tilemap
    /// </summary>
    /// <param name="tilemap">目标Tilemap</param>
    /// <param name="attackPoint">攻击点</param>
    private void AttackTilemap(Tilemap tilemap, Vector3 attackPoint)
    {
        // 将世界坐标转换为Tilemap坐标
        Vector3Int cellPosition = tilemap.WorldToCell(attackPoint);
        TileBase tile = tilemap.GetTile(cellPosition);
        
        if (tile != null)
        {
            Debug.Log($"[PlayerAttackTilemapIntegration] 攻击瓦片: {cellPosition}");
            
            // 如果有BreakableTilemapManager，使用它来处理破坏
            if (breakableTilemapManager != null)
            {
                breakableTilemapManager.TryBreakTile(cellPosition);
            }
            else
            {
                // 直接破坏瓦片（简单版本）
                tilemap.SetTile(cellPosition, null);
                CreateSimpleBreakEffect(tilemap.CellToWorld(cellPosition));
            }
        }
    }
    
    /// <summary>
    /// 获取攻击方向
    /// </summary>
    /// <returns>攻击方向向量</returns>
    private Vector2 GetAttackDirection()
    {
        // 根据玩家的面向方向或输入方向确定攻击方向
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector2 direction = new Vector2(horizontal, vertical).normalized;
        
        // 如果没有输入，默认向右攻击
        if (direction.magnitude < 0.1f)
        {
            direction = Vector2.right;
        }
        
        return direction;
    }
    
    /// <summary>
    /// 创建简单的破坏效果
    /// </summary>
    /// <param name="position">位置</param>
    private void CreateSimpleBreakEffect(Vector3 position)
    {
        // 创建简单的粒子效果
        GameObject effect = new GameObject("BreakEffect");
        effect.transform.position = position;
        
        // 创建几个小方块作为碎片
        for (int i = 0; i < 5; i++)
        {
            GameObject debris = new GameObject("Debris");
            debris.transform.parent = effect.transform;
            debris.transform.position = position + Random.insideUnitSphere * 0.3f;
            
            SpriteRenderer sr = debris.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSimpleSquareSprite();
            sr.color = new Color(0.8f, 0.6f, 0.4f);
            sr.sortingOrder = 10;
            
            Rigidbody2D rb = debris.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1f;
            Vector2 force = Random.insideUnitCircle * 3f + Vector2.up * 2f;
            rb.AddForce(force, ForceMode2D.Impulse);
            rb.AddTorque(Random.Range(-100f, 100f));
        }
        
        // 自动销毁效果
        Destroy(effect, 3f);
    }
    
    /// <summary>
    /// 创建简单的方形精灵
    /// </summary>
    /// <returns>方形精灵</returns>
    private Sprite CreateSimpleSquareSprite()
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
    
    /// <summary>
    /// 设置攻击范围
    /// </summary>
    /// <param name="range">新的攻击范围</param>
    public void SetAttackRange(float range)
    {
        tilemapAttackRange = range;
    }
    
    /// <summary>
    /// 设置Tilemap图层
    /// </summary>
    /// <param name="layers">图层遮罩</param>
    public void SetTilemapLayers(LayerMask layers)
    {
        tilemapLayers = layers;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (showAttackRange && playerAttackSystem != null)
        {
            Gizmos.color = Color.yellow;
            Vector2 attackDirection = GetAttackDirection();
            Vector3 attackPoint = transform.position + (Vector3)attackDirection * tilemapAttackRange * 0.5f;
            Gizmos.DrawWireSphere(attackPoint, tilemapAttackRange);
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, attackPoint);
        }
    }
} 