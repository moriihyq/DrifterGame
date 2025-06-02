using UnityEngine;
using UnityEngine.UI; // 添加UI命名空间来使用RawImage

/// <summary>
/// 快速UI修复脚本 - 一键解决RawImage遮挡tilemaps问题
/// 只需将此脚本添加到场景中任意GameObject即可自动修复
/// </summary>
public class QuickUIFix : MonoBehaviour
{
    void Start()
    {
        FixRawImageOverlapping();
    }
    
    /// <summary>
    /// 修复RawImage遮挡问题
    /// </summary>
    private void FixRawImageOverlapping()
    {
        // 查找所有Canvas
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        int fixedCount = 0;
        
        foreach (Canvas canvas in allCanvases)
        {
            // 检查Canvas中是否有RawImage
            RawImage[] rawImages = canvas.GetComponentsInChildren<RawImage>();
            
            if (rawImages.Length > 0)
            {
                // 将包含RawImage的Canvas设置为背景层
                canvas.overrideSorting = true;
                canvas.sortingOrder = -10; // 负值确保在最底层
                
                Debug.Log($"<color=green>[QuickUIFix] 已修复Canvas: {canvas.name} (包含{rawImages.Length}个RawImage)</color>");
                fixedCount++;
            }
        }
        
        if (fixedCount > 0)
        {
            Debug.Log($"<color=green>[QuickUIFix] ✓ 修复完成！共修复了{fixedCount}个Canvas，RawImage不再遮挡tilemaps</color>");
        }
        else
        {
            Debug.Log($"<color=yellow>[QuickUIFix] 没有找到需要修复的Canvas（包含RawImage的Canvas）</color>");
        }
        
        // 修复完成后销毁自己
        Destroy(gameObject);
    }
} 