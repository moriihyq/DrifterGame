using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleStartGameFix : MonoBehaviour
{
    [Header("场景设置")]
    public string targetSceneName = "5.26地图";
    
    public void StartGameSimple()
    {
        Debug.Log("[SimpleStartGameFix] 开始游戏 - 简化版本");
        
        try
        {
            // 直接加载游戏场景，不进行任何复杂操作
            Debug.Log($"[SimpleStartGameFix] 加载场景: {targetSceneName}");
            SceneManager.LoadScene(targetSceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SimpleStartGameFix] 加载场景失败: {e.Message}");
            
            // 尝试备用场景
            try
            {
                Debug.Log("[SimpleStartGameFix] 尝试加载备用场景: 可以运行的地图");
                SceneManager.LoadScene("可以运行的地图");
            }
            catch (System.Exception e2)
            {
                Debug.LogError($"[SimpleStartGameFix] 备用场景也加载失败: {e2.Message}");
            }
        }
    }
} 