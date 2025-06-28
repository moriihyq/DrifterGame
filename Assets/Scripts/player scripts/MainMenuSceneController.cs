using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 主菜单场景控制器 - 确保游戏相关组件不会在主菜单中运行
/// </summary>
public class MainMenuSceneController : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("[MainMenuSceneController] 主菜单场景控制器启动");
        
        // 重置Time.timeScale，确保游戏不会因为之前的胜利界面而停止
        Time.timeScale = 1f;
        
        // 清理可能残留的游戏相关对象
        CleanupGameObjects();
    }
    
    private void Start()
    {
        Debug.Log("[MainMenuSceneController] 主菜单场景初始化完成");
        
        // 确保音频和其他设置正确
        EnsureCorrectSettings();
    }
    
    /// <summary>
    /// 清理可能残留的游戏相关对象
    /// </summary>
    private void CleanupGameObjects()
    {
        // 清理GameVictoryManager实例
        GameVictoryManager victoryManager = FindFirstObjectByType<GameVictoryManager>();
        if (victoryManager != null)
        {
            Debug.Log("[MainMenuSceneController] 发现并销毁GameVictoryManager");
            Destroy(victoryManager.gameObject);
        }
        
        // 清理胜利界面Canvas
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            if (canvas.name.Contains("Victory") || canvas.sortingOrder >= 1000)
            {
                Debug.Log($"[MainMenuSceneController] 销毁胜利界面: {canvas.name}");
                Destroy(canvas.gameObject);
            }
        }
        
        // 清理任何名为"CrashDiagnostic"的对象（如果存在）
        GameObject diagnostic = GameObject.Find("CrashDiagnostic");
        if (diagnostic != null)
        {
            Debug.Log("[MainMenuSceneController] 发现并销毁CrashDiagnostic");
            Destroy(diagnostic);
        }
    }
    
    /// <summary>
    /// 确保正确的设置
    /// </summary>
    private void EnsureCorrectSettings()
    {
        // 确保Time.timeScale是1
        if (Time.timeScale != 1f)
        {
            Debug.Log($"[MainMenuSceneController] 重置Time.timeScale从 {Time.timeScale} 到 1");
            Time.timeScale = 1f;
        }
        
        // 确保光标显示
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
} 