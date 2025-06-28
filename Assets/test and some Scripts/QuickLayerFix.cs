using UnityEngine;

/// <summary>
/// 快速图层修复脚本 - 简单易用的渲染层级修复工具
/// 只需将此脚本添加到任何需要修复层级的对象上即可
/// </summary>
public class QuickLayerFix : MonoBehaviour
{
    [Header("快速设置")]
    [Tooltip("设置这个对象的排序顺序")]
    public int sortingOrder = 10;
    
    [Tooltip("启动时自动应用设置")]
    public bool applyOnStart = true;
    
    void Start()
    {
        if (applyOnStart)
        {
            ApplyLayerFix();
        }
    }
    
    /// <summary>
    /// 应用层级修复
    /// </summary>
    [ContextMenu("应用层级修复")]
    public void ApplyLayerFix()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        
        if (sr != null)
        {
            sr.sortingOrder = sortingOrder;
            Debug.Log($"<color=green>[QuickLayerFix] 已将 {gameObject.name} 的排序顺序设置为 {sortingOrder}</color>");
        }
        else
        {
            Debug.LogWarning($"[QuickLayerFix] {gameObject.name} 没有 SpriteRenderer 组件！");
        }
    }
    
    /// <summary>
    /// 设置为角色层级（排序顺序10）
    /// </summary>
    [ContextMenu("设置为角色层级")]
    public void SetAsCharacterLayer()
    {
        sortingOrder = 10;
        ApplyLayerFix();
    }
    
    /// <summary>
    /// 设置为UI层级（排序顺序20）
    /// </summary>
    [ContextMenu("设置为UI层级")]
    public void SetAsUILayer()
    {
        sortingOrder = 20;
        ApplyLayerFix();
    }
    
    /// <summary>
    /// 设置为背景层级（排序顺序-5）
    /// </summary>
    [ContextMenu("设置为背景层级")]
    public void SetAsBackgroundLayer()
    {
        sortingOrder = -5;
        ApplyLayerFix();
    }
} 