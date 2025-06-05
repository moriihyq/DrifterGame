using UnityEngine;
using UnityEngine.SceneManagement; // 必须引入场景管理命名空间

public class TitleScreenManager : MonoBehaviour
{
    // 在 Inspector 中指定要加载的游戏场景名称
    public string gameSceneName = "5.26地图"; // 默认游戏场景
    public string mainMenuSceneName = "MainMenuScene"; // 主菜单场景名称
    public GameObject optionsPanel; // 选项菜单面板的引用
    public GameObject loadGamePanel; // 添加LoadGamePanel引用
    public GameObject mainUIPanel; // 添加主UI面板引用
    
    // 音频控制相关
    [Header("音频控制")]
    public AudioVolumeManager audioManager; // 音频控制管理器的引用

    [Header("游戏启动设置")]
    public bool startGameLoadsRecentSave = false; // 是否让StartGame加载最近存档

    private void Awake()
    {
        // 查找音频管理器
        if (audioManager == null)
        {
            audioManager = FindFirstObjectByType<AudioVolumeManager>();
            if (audioManager == null)
            {
                Debug.LogWarning("未找到音频管理器！");
            }
        }
    }

    void Start()
    {
        Debug.Log("[TitleScreenManager] 脚本已启动 - 开始初始化");
        
        // 查找音频管理器
        if (audioManager == null)
        {
            audioManager = FindFirstObjectByType<AudioVolumeManager>();
            if (audioManager == null)
            {
                Debug.LogWarning("未找到音频管理器！");
            }
        }
        
        Debug.Log($"[TitleScreenManager] 音频管理器状态: {(audioManager != null ? "已找到" : "未找到")}");
        Debug.Log($"[TitleScreenManager] 当前场景: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        Debug.Log($"[TitleScreenManager] 游戏场景设置: {gameSceneName}");
        
        // 确保SaveManager存在
        // GameInitializer.EnsureManagersExist(); // 这行应该被注释或删除
        
        // 如果选项面板还没有被分配，尝试通过名称查找
        if (optionsPanel == null)
        {
            // 先尝试查找名为"OptionsMenPanel"的对象（注意拼写错误）
            optionsPanel = GameObject.Find("OptionsMenPanel");
            
            // 如果仍未找到，尝试查找名为"OptionsPanel"的对象
            if (optionsPanel == null)
            {
                optionsPanel = GameObject.Find("OptionsPanel");
            }
            
            // 如果还是没找到，输出错误日志
            if (optionsPanel == null)
            {
                Debug.LogError("无法找到选项面板！请确保场景中存在'OptionsMenPanel'或'OptionsPanel'对象。");
            }
            else
            {
                // 确保找到的面板初始状态为隐藏
                optionsPanel.SetActive(false);
                Debug.Log("已找到选项面板：" + optionsPanel.name);
            }
        }
        
        // 查找LoadGamePanel
        if (loadGamePanel == null)
        {
            loadGamePanel = GameObject.Find("LoadGamePanel");
            if (loadGamePanel != null)
            {
                loadGamePanel.SetActive(false);
                Debug.Log("已找到LoadGamePanel");
            }
        }
        
        // 查找MainUIPanel
        if (mainUIPanel == null)
        {
            mainUIPanel = GameObject.Find("MainUIPanel");
        }
    }

    // 公开方法，用于绑定到"开始游戏"按钮的 OnClick 事件
    public void StartGame()
    {
        Debug.Log("[TitleScreenManager] =================== StartGame() 开始 ===================");
        
        // 确保目标场景名正确
        if (string.IsNullOrEmpty(gameSceneName) || gameSceneName == "YourGameSceneName")
        {
            gameSceneName = "5.26地图"; // 强制使用已知的场景名
        }
        
        Debug.Log($"[TitleScreenManager] 直接加载游戏场景: {gameSceneName}");
        
        try
        {
            // 直接加载游戏场景，不进行任何存档操作
            SceneManager.LoadScene(gameSceneName);
            Debug.Log($"[TitleScreenManager] 场景加载调用完成: {gameSceneName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TitleScreenManager] 场景加载失败: {e.Message}");
            // 尝试备用场景
            try
            {
                SceneManager.LoadScene("5.26地图");
                Debug.Log("[TitleScreenManager] 备用场景加载成功");
            }
            catch (System.Exception e2)
            {
                Debug.LogError($"[TitleScreenManager] 备用场景也失败: {e2.Message}");
            }
        }
        
        Debug.Log("[TitleScreenManager] =================== StartGame() 完成 ===================");
    }
    
    // 公开方法，用于绑定到"读取游戏"按钮的 OnClick 事件
    public void OpenLoadGame()
    {
        Debug.Log("打开读档界面");
        if (loadGamePanel != null)
        {
            loadGamePanel.SetActive(true);
            if (mainUIPanel != null)
                mainUIPanel.SetActive(false);
                
            // 刷新存档列表
            var loadGameManager = loadGamePanel.GetComponent<LoadGamePanelManager>();
            if (loadGameManager != null)
            {
                loadGameManager.RefreshSaveSlots();
            }
        }
        else
        {
            Debug.LogError("LoadGamePanel未找到！");
        }
    }

    // 公开方法，用于绑定到"选项"按钮的 OnClick 事件
    public void OpenOptions()
    {
        Debug.Log("打开选项菜单");
        
        // 尝试使用OptionsMenuManager
        OptionsMenuManager optionsMenuManager = FindFirstObjectByType<OptionsMenuManager>();
        if (optionsMenuManager != null)
        {
            optionsMenuManager.OpenOptionsMenu();
        }
        else if (optionsPanel != null)
        {
            // 后备方案：直接控制面板（但不推荐）
            optionsPanel.SetActive(true); // 显示选项菜单面板
            Debug.LogWarning("未找到OptionsMenuManager，使用后备方案直接控制面板");
        }
        else
        {
            Debug.LogWarning("选项面板未分配！请在 Inspector 中设置引用。");
        }
    }

    // 公开方法，用于绑定到"返回"按钮的 OnClick 事件
    public void Back()
    {
        Debug.Log("返回主菜单");
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // 公开方法，用于关闭选项菜单
    public void CloseOptions()
    {
        Debug.Log("关闭选项菜单");
        
        // 尝试使用OptionsMenuManager
        OptionsMenuManager optionsMenuManager = FindFirstObjectByType<OptionsMenuManager>();
        if (optionsMenuManager != null)
        {
            optionsMenuManager.CloseOptionsMenu();
        }
        else if (optionsPanel != null)
        {
            // 后备方案：直接控制面板（但不推荐）
            optionsPanel.SetActive(false); // 隐藏选项菜单面板
            Debug.LogWarning("未找到OptionsMenuManager，使用后备方案直接控制面板");
        }
    }
    
    // 公开方法，用于设置音量
    public void SetVolume(float volume)
    {
        if (audioManager != null)
        {
            audioManager.SetVolume(volume);
        }
    }

    // 公开方法，用于绑定到"退出游戏"按钮的 OnClick 事件
    public void ExitGame()
    {
        Debug.Log("尝试退出游戏...");
        // 在编辑器模式下停止播放
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // 在构建好的游戏中退出应用程序
        Application.Quit();
        #endif
    }

    // 添加 Update 方法用于按键检测
    void Update()
    {
        // 检测返回按键（可以是 Escape 键，也可以根据你的需要修改）
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 如果选项面板打开，则关闭它
            if (optionsPanel != null && optionsPanel.activeSelf)
            {
                CloseOptions();
            }
            // 否则返回主菜单
            else
            {
                Back();
            }
        }

        // 检测 OPTIONS 按键（默认使用 O 键，可以根据需要调整）
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (optionsPanel != null && !optionsPanel.activeSelf)
            {
                OpenOptions();
            }
            else if (optionsPanel != null && optionsPanel.activeSelf)
            {
                CloseOptions();
            }
        }
    }
}
