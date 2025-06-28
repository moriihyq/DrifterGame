using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 玩家图层修复脚本 - 解决角色在墙壁图层之下的问题
/// 此脚本确保玩家的SpriteRenderer显示在所有墙壁和背景元素之上
/// </summary>
public class PlayerLayerFix : MonoBehaviour
{
    [Header("排序层设置")]
    [Tooltip("玩家的排序层名称，留空则使用默认层")]
    public string playerSortingLayerName = "Player";
    
    [Tooltip("玩家的排序顺序，应该高于墙壁层（通常墙壁为0-5，玩家应设为10）")]
    public int playerSortingOrder = 10;
    
    [Header("自动检测设置")]
    [Tooltip("启动时自动修复排序层")]
    public bool autoFixOnStart = true;
    
    [Tooltip("实时监控并修复排序层")]
    public bool continouslyFix = false;
    
    private SpriteRenderer playerSpriteRenderer;
    
    void Start()
    {
        // 获取玩家的SpriteRenderer组件
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
        
        if (playerSpriteRenderer == null)
        {
            Debug.LogError($"[PlayerLayerFix] 在 {gameObject.name} 上未找到 SpriteRenderer 组件！");
            return;
        }
        
        if (autoFixOnStart)
        {
            FixPlayerLayer();
        }
        
        // 显示当前设置信息
        LogCurrentLayerInfo();
    }
    
    void Update()
    {
        if (continouslyFix && playerSpriteRenderer != null)
        {
            FixPlayerLayer();
        }
    }
    
    /// <summary>
    /// 修复玩家的渲染层级
    /// </summary>
    public void FixPlayerLayer()
    {
        if (playerSpriteRenderer == null) return;
        
        // 设置排序层名称（如果指定了的话）
        if (!string.IsNullOrEmpty(playerSortingLayerName))
        {
            playerSpriteRenderer.sortingLayerName = playerSortingLayerName;
        }
        
        // 设置排序顺序
        playerSpriteRenderer.sortingOrder = playerSortingOrder;
        
        Debug.Log($"<color=green>[PlayerLayerFix] 已修复 {gameObject.name} 的渲染层级:</color>");
        Debug.Log($"<color=green>- 排序层: {playerSpriteRenderer.sortingLayerName}</color>");
        Debug.Log($"<color=green>- 排序顺序: {playerSpriteRenderer.sortingOrder}</color>");
    }
    
    /// <summary>
    /// 检测并修复所有玩家对象的层级
    /// </summary>
    [ContextMenu("修复所有玩家层级")]
    public void FixAllPlayerLayers()
    {
        // 查找所有带有Player标签的对象
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        
        if (players.Length == 0)
        {
            Debug.LogWarning("[PlayerLayerFix] 未找到任何带有 'Player' 标签的对象");
            return;
        }
        
        foreach (GameObject player in players)
        {
            SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (!string.IsNullOrEmpty(playerSortingLayerName))
                {
                    sr.sortingLayerName = playerSortingLayerName;
                }
                sr.sortingOrder = playerSortingOrder;
                
                Debug.Log($"<color=green>[PlayerLayerFix] 已修复玩家 {player.name} 的层级</color>");
            }
        }
    }
    
    /// <summary>
    /// 分析场景中的渲染层级情况
    /// </summary>
    [ContextMenu("分析场景渲染层级")]
    public void AnalyzeSceneRenderingLayers()
    {
        Debug.Log("<color=cyan>========== 场景渲染层级分析 ==========</color>");
        
        // 分析TilemapRenderer
        TilemapRenderer[] tilemapRenderers = FindObjectsOfType<TilemapRenderer>();
        Debug.Log($"<color=yellow>发现 {tilemapRenderers.Length} 个 TilemapRenderer:</color>");
        
        foreach (TilemapRenderer tr in tilemapRenderers)
        {
            Debug.Log($"- {tr.gameObject.name}: 排序层={tr.sortingLayerName}, 排序顺序={tr.sortingOrder}");
        }
        
        // 分析SpriteRenderer
        SpriteRenderer[] spriteRenderers = FindObjectsOfType<SpriteRenderer>();
        Debug.Log($"<color=yellow>发现 {spriteRenderers.Length} 个 SpriteRenderer:</color>");
        
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            Debug.Log($"- {sr.gameObject.name}: 排序层={sr.sortingLayerName}, 排序顺序={sr.sortingOrder}");
        }
        
        Debug.Log("<color=cyan>========================================</color>");
    }
    
    /// <summary>
    /// 记录当前层级信息
    /// </summary>
    private void LogCurrentLayerInfo()
    {
        if (playerSpriteRenderer != null)
        {
            Debug.Log($"<color=cyan>[PlayerLayerFix] {gameObject.name} 当前渲染设置:</color>");
            Debug.Log($"<color=cyan>- 排序层: {playerSpriteRenderer.sortingLayerName}</color>");
            Debug.Log($"<color=cyan>- 排序顺序: {playerSpriteRenderer.sortingOrder}</color>");
        }
    }
    
    /// <summary>
    /// 设置玩家排序顺序
    /// </summary>
    /// <param name="order">新的排序顺序</param>
    public void SetPlayerSortingOrder(int order)
    {
        playerSortingOrder = order;
        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.sortingOrder = order;
            Debug.Log($"<color=green>[PlayerLayerFix] 已将 {gameObject.name} 的排序顺序设置为 {order}</color>");
        }
    }
} 