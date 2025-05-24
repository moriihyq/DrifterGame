using UnityEngine;
using UnityEngine.SceneManagement; // 必须引入场景管理命名空间

public class TitleScreenManager : MonoBehaviour
{
    // 在 Inspector 中指定要加载的游戏场景名称
    public string gameSceneName = "YourGameSceneName"; // !!! 修改为你实际的游戏场景名称 !!!
    public string mainMenuSceneName = "MainMenuScene"; // 主菜单场景名称
    public GameObject optionsPanel; // 选项菜单面板的引用
    public GameObject loadGamePanel; // 添加LoadGamePanel引用
    public GameObject mainUIPanel; // 添加主UI面板引用
    
    // 音频控制相关
    [Header("音频控制")]
    public AudioVolumeManager audioManager; // 音频控制管理器的引用

    private void Awake()
    {
        // 查找音频管理器
        if (audioManager == null)
        {
            audioManager = FindObjectOfType<AudioVolumeManager>();
            if (audioManager == null)
            {
                Debug.LogWarning("未找到音频管理器！");
            }
        }
    }

    void Start()
    {
        // 确保SaveManager存在
        GameInitializer.EnsureManagersExist();
        
        // 使用新的 API 替换弃用的 FindObjectOfType
        audioManager = Object.FindFirstObjectByType<AudioVolumeManager>();
        
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
        Debug.Log("开始新游戏，清除所有存档并加载游戏场景: " + gameSceneName);
        
        // 清除自动存档槽位（槽位0）以确保是新游戏
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSave(0);
        }
        
        // 异步加载场景可以防止卡顿，并可以显示加载进度条（如果需要）
        // SceneManager.LoadSceneAsync(gameSceneName);
        // 简单直接加载：
        SceneManager.LoadScene(gameSceneName);
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
        Debug.Log("尝试打开选项菜单...");
        if (optionsPanel != null)
        {
            Debug.Log("找到选项面板: " + optionsPanel.name + "，正在激活...");
            // 先确保面板存在并且是非激活状态
            if (!optionsPanel.activeInHierarchy)
            {
                optionsPanel.SetActive(true); // 显示选项菜单面板
                Debug.Log("选项面板已激活");
                
                // 尝试找到和重置音量滑动条
                var volumeManager = FindObjectOfType<AudioVolumeManager>();
                if (volumeManager != null)
                {
                    volumeManager.ResetSliderInteraction();
                }
            }
            else
            {
                Debug.LogWarning("选项面板已经处于激活状态，不需要再次激活");
            }
        }
        else
        {
            // 如果optionsPanel为空，尝试重新查找
            Debug.LogWarning("选项面板未分配！尝试重新查找...");
            
            // 尝试查找名为"OptionsMenPanel"的对象（注意拼写错误）
            optionsPanel = GameObject.Find("OptionsMenPanel");
            
            // 如果找到了，激活它
            if (optionsPanel != null)
            {
                Debug.Log("找到选项面板: " + optionsPanel.name + "，正在激活...");
                optionsPanel.SetActive(true);
            }
            else
            {
                Debug.LogError("无法找到选项面板！请在 Inspector 中设置引用或确保场景中存在名为'OptionsMenPanel'的对象。");
            }
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
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false); // 隐藏选项菜单面板
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
