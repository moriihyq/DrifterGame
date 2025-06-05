using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// RespawnManager修复器 - 解决场景切换时的卡死问题
/// </summary>
public class RespawnManagerFix : MonoBehaviour
{
    [Header("场景管理")]
    [Tooltip("主菜单场景名称")]
    public string mainMenuSceneName = "MainMenuScene";
    
    [Tooltip("游戏场景名称列表")]
    public string[] gameSceneNames = { "Example1", "5.26地图", "可以运行的地图" };
    
    [Tooltip("启用调试日志")]
    public bool enableDebugLog = true;
    
    private void Awake()
    {
        // 监听场景加载事件
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    
    private void OnDestroy()
    {
        // 取消监听场景事件
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
    
    /// <summary>
    /// 场景加载完成时调用
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (enableDebugLog)
            Debug.Log($"[RespawnManagerFix] 场景已加载: {scene.name}");
        
        // 检查是否为主菜单场景
        if (IsMainMenuScene(scene.name))
        {
            HandleMainMenuLoaded();
        }
        else if (IsGameScene(scene.name))
        {
            HandleGameSceneLoaded();
        }
    }
    
    /// <summary>
    /// 场景卸载时调用
    /// </summary>
    private void OnSceneUnloaded(Scene scene)
    {
        if (enableDebugLog)
            Debug.Log($"[RespawnManagerFix] 场景已卸载: {scene.name}");
    }
    
    /// <summary>
    /// 处理主菜单场景加载
    /// </summary>
    private void HandleMainMenuLoaded()
    {
        if (enableDebugLog)
            Debug.Log("[RespawnManagerFix] 检测到主菜单场景，禁用RespawnManager功能");
        
        // 查找并禁用RespawnManager
        RespawnManager respawnManager = FindFirstObjectByType<RespawnManager>();
        if (respawnManager != null)
        {
            // 禁用RespawnManager组件而不是销毁GameObject，这样回到游戏时还能恢复
            respawnManager.enabled = false;
            
            if (enableDebugLog)
                Debug.Log("[RespawnManagerFix] ✅ 已禁用RespawnManager组件");
        }
        
        // 清理可能存在的复活点
        RespawnPoint[] respawnPoints = FindObjectsByType<RespawnPoint>(FindObjectsSortMode.None);
        foreach (RespawnPoint point in respawnPoints)
        {
            point.gameObject.SetActive(false);
        }
        
        if (respawnPoints.Length > 0 && enableDebugLog)
            Debug.Log($"[RespawnManagerFix] 禁用了 {respawnPoints.Length} 个复活点");
    }
    
    /// <summary>
    /// 处理游戏场景加载
    /// </summary>
    private void HandleGameSceneLoaded()
    {
        if (enableDebugLog)
            Debug.Log("[RespawnManagerFix] 检测到游戏场景，启用RespawnManager功能");
        
        // 查找并启用RespawnManager
        RespawnManager respawnManager = FindFirstObjectByType<RespawnManager>();
        if (respawnManager != null)
        {
            // 启用RespawnManager组件
            respawnManager.enabled = true;
            
            // 延迟初始化，确保玩家对象已加载
            StartCoroutine(DelayedRespawnManagerInit(respawnManager));
            
            if (enableDebugLog)
                Debug.Log("[RespawnManagerFix] ✅ 已启用RespawnManager组件");
        }
    }
    
    /// <summary>
    /// 延迟初始化RespawnManager
    /// </summary>
    private System.Collections.IEnumerator DelayedRespawnManagerInit(RespawnManager respawnManager)
    {
        // 等待几帧确保场景完全加载
        yield return new WaitForEndOfFrame();
        yield return new WaitForFixedUpdate();
        yield return new WaitForSeconds(0.1f);
        
        // 强制重新查找玩家和复活点
        if (respawnManager != null && respawnManager.enabled)
        {
            // 使用反射或者公共方法重新初始化（如果有的话）
            var findPlayerMethod = respawnManager.GetType().GetMethod("FindPlayer", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var registerPointsMethod = respawnManager.GetType().GetMethod("RegisterAllRespawnPoints", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (findPlayerMethod != null)
            {
                findPlayerMethod.Invoke(respawnManager, null);
                if (enableDebugLog)
                    Debug.Log("[RespawnManagerFix] 重新查找玩家完成");
            }
            
            if (registerPointsMethod != null)
            {
                registerPointsMethod.Invoke(respawnManager, null);
                if (enableDebugLog)
                    Debug.Log("[RespawnManagerFix] 重新注册复活点完成");
            }
        }
    }
    
    /// <summary>
    /// 检查是否为主菜单场景
    /// </summary>
    private bool IsMainMenuScene(string sceneName)
    {
        return sceneName.Equals(mainMenuSceneName, System.StringComparison.OrdinalIgnoreCase) ||
               sceneName.Contains("Menu") ||
               sceneName.Contains("Title") ||
               sceneName.Contains("Start");
    }
    
    /// <summary>
    /// 检查是否为游戏场景
    /// </summary>
    private bool IsGameScene(string sceneName)
    {
        foreach (string gameScene in gameSceneNames)
        {
            if (sceneName.Equals(gameScene, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        
        // 如果不在已知游戏场景列表中，但也不是主菜单，假设为游戏场景
        return !IsMainMenuScene(sceneName);
    }
    
    /// <summary>
    /// 手动禁用RespawnManager（紧急使用）
    /// </summary>
    [ContextMenu("禁用RespawnManager")]
    public void DisableRespawnManager()
    {
        RespawnManager respawnManager = FindFirstObjectByType<RespawnManager>();
        if (respawnManager != null)
        {
            respawnManager.enabled = false;
            Debug.Log("[RespawnManagerFix] 手动禁用RespawnManager");
        }
    }
    
    /// <summary>
    /// 手动启用RespawnManager
    /// </summary>
    [ContextMenu("启用RespawnManager")]
    public void EnableRespawnManager()
    {
        RespawnManager respawnManager = FindFirstObjectByType<RespawnManager>();
        if (respawnManager != null)
        {
            respawnManager.enabled = true;
            StartCoroutine(DelayedRespawnManagerInit(respawnManager));
            Debug.Log("[RespawnManagerFix] 手动启用RespawnManager");
        }
    }
    
    /// <summary>
    /// 获取当前状态信息
    /// </summary>
    public string GetStatusInfo()
    {
        RespawnManager respawnManager = FindFirstObjectByType<RespawnManager>();
        string currentScene = SceneManager.GetActiveScene().name;
        bool isMainMenu = IsMainMenuScene(currentScene);
        bool isGameScene = IsGameScene(currentScene);
        
        string status = $"当前场景: {currentScene}\n";
        status += $"场景类型: {(isMainMenu ? "主菜单" : isGameScene ? "游戏场景" : "未知")}\n";
        status += $"RespawnManager存在: {(respawnManager != null ? "是" : "否")}\n";
        
        if (respawnManager != null)
        {
            status += $"RespawnManager启用: {(respawnManager.enabled ? "是" : "否")}\n";
        }
        
        return status;
    }
} 