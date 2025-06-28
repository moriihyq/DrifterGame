using UnityEngine;

/// <summary>
/// 简单Canvas修复脚本 - 将所有Canvas设置为背景层
/// 不依赖特定UI组件，直接修复所有Canvas的排序顺序
/// </summary>
public class SimpleCanvasFix : MonoBehaviour
{
    [Header("设置")]
    [Tooltip("Canvas的排序顺序")]
    public int canvasSortingOrder = -10;
    
    void Start()
    {
        FixAllCanvases();
    }
    
    /// <summary>
    /// 修复所有Canvas的排序顺序
    /// </summary>
    private void FixAllCanvases()
    {
        // 查找所有Canvas组件
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        int fixedCount = 0;
        
        foreach (Canvas canvas in allCanvases)
        {
            // 设置Canvas为背景层
            canvas.overrideSorting = true;
            canvas.sortingOrder = canvasSortingOrder;
            
            Debug.Log($"<color=green>[SimpleCanvasFix] 已修复Canvas: {canvas.name}</color>");
            fixedCount++;
        }
        
        if (fixedCount > 0)
        {
            Debug.Log($"<color=green>[SimpleCanvasFix] ✓ 修复完成！共修复了{fixedCount}个Canvas</color>");
            Debug.Log($"<color=cyan>所有Canvas现在的排序顺序为: {canvasSortingOrder}</color>");
        }
        else
        {
            Debug.Log($"<color=yellow>[SimpleCanvasFix] 没有找到Canvas组件</color>");
        }
        
        // 修复完成后销毁自己
        Destroy(gameObject);
    }
} 