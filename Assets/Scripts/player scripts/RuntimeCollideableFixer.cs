using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 运行时自动修复Collideable图层，使其可破坏
/// 将此脚本添加到场景中的任意对象即可自动运行
/// </summary>
public class RuntimeCollideableFixer : MonoBehaviour
{
    [Header("修复设置")]
    [SerializeField] private bool autoFixOnStart = true;
    [SerializeField] private bool showFixResults = true;
    [SerializeField] private KeyCode manualFixKey = KeyCode.F5;
    
    [Header("破坏参数")]
    [SerializeField] private float detectionRadius = 2f;
    [SerializeField] private int tileHealth = 2;
    [SerializeField] private bool createDebris = true;
    [SerializeField] private int debrisCount = 5;
    
    private bool hasFixed = false;
    
    void Start()
    {
        if (autoFixOnStart)
        {
            Invoke(nameof(FixCollideableLayers), 0.5f); // 延迟执行，确保所有对象都已加载
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(manualFixKey))
        {
            FixCollideableLayers();
        }
    }
    
    /// <summary>
    /// 修复所有Collideable图层
    /// </summary>
    public void FixCollideableLayers()
    {
        if (showFixResults)
        {
            Debug.Log("=== 开始修复Collideable图层 ===");
        }
        
        // 查找所有Tilemap
        Tilemap[] allTilemaps = FindObjectsOfType<Tilemap>();
        int fixedCount = 0;
        int alreadyFixedCount = 0;
        
        foreach (Tilemap tilemap in allTilemaps)
        {
            if (tilemap.name.Contains("Collideable"))
            {
                if (FixCollideableTilemap(tilemap))
                {
                    fixedCount++;
                }
                else
                {
                    alreadyFixedCount++;
                }
            }
        }
        
        // 确保玩家有必要的组件
        SetupPlayerComponents();
        
        if (showFixResults)
        {
            if (fixedCount > 0)
            {
                Debug.Log($"✅ 修复完成! 新修复了 {fixedCount} 个Collideable图层");
                Debug.Log("现在可以使用 左Ctrl键 或 鼠标左键 攻击墙壁!");
            }
            
            if (alreadyFixedCount > 0)
            {
                Debug.Log($"ℹ️ 有 {alreadyFixedCount} 个Collideable图层已经可破坏");
            }
            
            if (fixedCount == 0 && alreadyFixedCount == 0)
            {
                Debug.LogWarning("未找到任何Collideable图层!");
            }
        }
        
        hasFixed = true;
    }
    
    /// <summary>
    /// 修复单个Collideable图层
    /// </summary>
    /// <param name="tilemap">要修复的Tilemap</param>
    /// <returns>是否进行了修复</returns>
    private bool FixCollideableTilemap(Tilemap tilemap)
    {
        GameObject tilemapObj = tilemap.gameObject;
        bool wasFixed = false;
        
        // 检查是否已经有BreakableTilemapManager
        BreakableTilemapManager existingManager = tilemapObj.GetComponent<BreakableTilemapManager>();
        if (existingManager != null)
        {
            return false; // 已经修复过了
        }
        
        if (showFixResults)
        {
            Debug.Log($"修复Collideable图层: {tilemap.name}");
        }
        
        // 1. 确保有TilemapCollider2D
        TilemapCollider2D collider = tilemapObj.GetComponent<TilemapCollider2D>();
        if (collider == null)
        {
            collider = tilemapObj.AddComponent<TilemapCollider2D>();
            wasFixed = true;
        }
        
        // 2. 确保有CompositeCollider2D
        CompositeCollider2D compositeCollider = tilemapObj.GetComponent<CompositeCollider2D>();
        if (compositeCollider == null)
        {
            compositeCollider = tilemapObj.AddComponent<CompositeCollider2D>();
            wasFixed = true;
        }
        
        // 3. 确保有Rigidbody2D
        Rigidbody2D rb = tilemapObj.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = tilemapObj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            wasFixed = true;
        }
        
        // 4. 配置TilemapCollider2D
        collider.usedByComposite = true;
        
        // 5. 添加BreakableTilemapManager
        BreakableTilemapManager manager = tilemapObj.AddComponent<BreakableTilemapManager>();
        ConfigureBreakableManager(manager, tilemap);
        wasFixed = true;
        
        return wasFixed;
    }
    
    /// <summary>
    /// 配置BreakableTilemapManager
    /// </summary>
    private void ConfigureBreakableManager(BreakableTilemapManager manager, Tilemap tilemap)
    {
        // 由于运行时无法使用SerializedProperty，我们使用反射或直接访问public字段
        // 这里假设BreakableTilemapManager有公共字段或属性可以直接设置
        
        // 如果BreakableTilemapManager的字段是private，需要添加公共设置方法
        // 或者我们可以通过反射来设置
        
        // 简单的配置方法（需要BreakableTilemapManager支持运行时配置）
        SetManagerField(manager, "detectionRadius", detectionRadius);
        SetManagerField(manager, "useHealthSystem", true);
        SetManagerField(manager, "tileHealth", tileHealth);
        SetManagerField(manager, "createDebris", createDebris);
        SetManagerField(manager, "debrisCount", debrisCount);
        SetManagerField(manager, "dropChance", 0.3f);
        SetManagerField(manager, "showDebugInfo", false);
        SetManagerField(manager, "visualizeDetection", false);
    }
    
    /// <summary>
    /// 使用反射设置字段值
    /// </summary>
    private void SetManagerField(BreakableTilemapManager manager, string fieldName, object value)
    {
        try
        {
            var field = manager.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
                
            if (field != null)
            {
                field.SetValue(manager, value);
            }
        }
        catch (System.Exception e)
        {
            if (showFixResults)
            {
                Debug.LogWarning($"无法设置字段 {fieldName}: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// 设置玩家组件
    /// </summary>
    private void SetupPlayerComponents()
    {
        PlayerAttackSystem playerAttackSystem = FindFirstObjectByType<PlayerAttackSystem>();
        if (playerAttackSystem == null)
        {
            if (showFixResults)
            {
                Debug.LogWarning("未找到PlayerAttackSystem, 无法自动设置玩家组件");
            }
            return;
        }
        
        GameObject playerObj = playerAttackSystem.gameObject;
        
        // 确保玩家有PlayerAttackTilemapIntegration
        PlayerAttackTilemapIntegration integration = playerObj.GetComponent<PlayerAttackTilemapIntegration>();
        if (integration == null)
        {
            integration = playerObj.AddComponent<PlayerAttackTilemapIntegration>();
            if (showFixResults)
            {
                Debug.Log("✓ 添加了PlayerAttackTilemapIntegration到玩家");
            }
        }
    }
    
    /// <summary>
    /// 检查当前修复状态
    /// </summary>
    [ContextMenu("检查修复状态")]
    public void CheckFixStatus()
    {
        Debug.Log("=== 检查Collideable图层修复状态 ===");
        
        Tilemap[] allTilemaps = FindObjectsOfType<Tilemap>();
        int collideableCount = 0;
        int fixedCount = 0;
        
        foreach (var tilemap in allTilemaps)
        {
            if (tilemap.name.Contains("Collideable"))
            {
                collideableCount++;
                BreakableTilemapManager manager = tilemap.GetComponent<BreakableTilemapManager>();
                if (manager != null)
                {
                    fixedCount++;
                    Debug.Log($"✓ {tilemap.name} - 已修复");
                }
                else
                {
                    Debug.Log($"✗ {tilemap.name} - 未修复");
                }
            }
        }
        
        Debug.Log($"统计: 共 {collideableCount} 个Collideable图层, 已修复 {fixedCount} 个");
        
        if (fixedCount < collideableCount)
        {
            Debug.Log($"按 {manualFixKey} 键手动修复剩余图层");
        }
    }
    
    void OnGUI()
    {
        if (!hasFixed && showFixResults)
        {
            GUI.Label(new Rect(10, 10, 400, 40), 
                $"Collideable图层修复器已激活\n按 {manualFixKey} 键手动修复");
        }
    }
} 