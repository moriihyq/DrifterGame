using UnityEngine;

/// <summary>
/// 简化版玩家图层修复脚本 - 专注于修复玩家角色的渲染层级
/// 不依赖TilemapRenderer，避免兼容性问题
/// </summary>
public class SimplePlayerLayerFix : MonoBehaviour
{
    [Header("玩家渲染设置")]
    [Tooltip("玩家的排序顺序，确保显示在墙壁之上")]
    public int playerSortingOrder = 10;
    
    [Tooltip("启动时自动修复")]
    public bool autoFixOnStart = true;
    
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer == null)
        {
            Debug.LogError($"[SimplePlayerLayerFix] {gameObject.name} 没有 SpriteRenderer 组件！");
            return;
        }
        
        if (autoFixOnStart)
        {
            FixPlayerLayer();
        }
    }
    
    /// <summary>
    /// 修复玩家的渲染层级
    /// </summary>
    [ContextMenu("修复玩家层级")]
    public void FixPlayerLayer()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = playerSortingOrder;
            Debug.Log($"<color=green>[SimplePlayerLayerFix] 已将 {gameObject.name} 的排序顺序设置为 {playerSortingOrder}</color>");
        }
        else
        {
            Debug.LogWarning($"[SimplePlayerLayerFix] {gameObject.name} 没有 SpriteRenderer 组件！");
        }
    }
    
    /// <summary>
    /// 设置新的排序顺序
    /// </summary>
    public void SetSortingOrder(int newOrder)
    {
        playerSortingOrder = newOrder;
        FixPlayerLayer();
    }
    
    /// <summary>
    /// 获取当前排序顺序
    /// </summary>
    public int GetCurrentSortingOrder()
    {
        if (spriteRenderer != null)
        {
            return spriteRenderer.sortingOrder;
        }
        return 0;
    }
} 