using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsMenuManager : MonoBehaviour
{
    [Header("界面引用")]
    [SerializeField] private GameObject optionsPanel;              // 选项界面面板
    [SerializeField] private Button closeButton;                   // 关闭按钮
    [SerializeField] private AudioVolumeManager audioManager;      // 音频管理器引用
    [SerializeField] private GameObject dimmer;                    // Dimmer背景遮罩
    [SerializeField] private GameObject mainUIPanel;               // 主UI面板
    
    [Header("配置")]
    [SerializeField] private bool resetSliderOnOpen = true;        // 打开面板时重置滑动条
    [SerializeField] private bool closeOnEscape = true;            // 是否可以按ESC关闭
    
    private void Start()
    {
        // 检查组件引用
        if (optionsPanel == null)
        {
            Debug.LogError("未分配选项面板！");
            return;
        }
        
        // 初始状态下隐藏选项面板
        optionsPanel.SetActive(false);
        
        // 设置关闭按钮事件
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseOptionsMenu);
        }
        
        // 确保有音频管理器引用
        if (audioManager == null)
        {
            audioManager = FindFirstObjectByType<AudioVolumeManager>();
            if (audioManager == null)
            {
                Debug.LogWarning("未找到AudioVolumeManager！音量设置可能无法正常工作。");
            }
        }
        
        // 自动查找MainUIPanel
        if (mainUIPanel == null)
        {
            mainUIPanel = GameObject.Find("MainUIPanel");
            if (mainUIPanel != null)
            {
                Debug.Log("OptionsMenuManager: 自动找到 MainUIPanel");
            }
            else
            {
                Debug.LogWarning("OptionsMenuManager: 未找到 MainUIPanel！请在Inspector中手动分配或确保场景中存在名为'MainUIPanel'的GameObject。");
            }
        }
    }
    
    // 外部调用：打开选项菜单
    public void OpenOptionsMenu()
    {
        // 确保面板存在
        if (optionsPanel == null) 
        {
            Debug.LogError("OptionsMenuManager: optionsPanel 为空！");
            return;
        }
        
        Debug.Log("OptionsMenuManager: 正在打开选项菜单...");
        
        // 关闭主UI面板
        if (mainUIPanel != null)
        {
            Debug.Log("OptionsMenuManager: 关闭 MainUIPanel");
            mainUIPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("OptionsMenuManager: MainUIPanel 未分配！请在Inspector中设置MainUIPanel引用。");
        }
        
        // 激活Dimmer背景遮罩
        if (dimmer != null)
        {
            Debug.Log("OptionsMenuManager: 激活 Dimmer");
            dimmer.SetActive(true);
        }
        else
        {
            Debug.LogWarning("OptionsMenuManager: Dimmer 未分配！请在Inspector中设置Dimmer引用。");
        }
        
        // 显示选项面板
        optionsPanel.SetActive(true);
        Debug.Log("OptionsMenuManager: 选项面板已激活");
        
        // 重置音量滑动条交互
        if (resetSliderOnOpen && audioManager != null)
        {
            audioManager.ResetSliderInteraction();
        }
        
        // 播放打开音效（如果有的话）
        // PlayOpenSound();
    }
    
    // 关闭选项菜单
    public void CloseOptionsMenu()
    {
        // 确保面板存在
        if (optionsPanel == null) 
        {
            Debug.LogError("OptionsMenuManager: optionsPanel 为空！");
            return;
        }
        
        Debug.Log("OptionsMenuManager: 正在关闭选项菜单...");
        
        // 重新激活主UI面板
        if (mainUIPanel != null)
        {
            Debug.Log("OptionsMenuManager: 重新激活 MainUIPanel");
            mainUIPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("OptionsMenuManager: MainUIPanel 未分配！");
        }
        
        // 关闭Dimmer背景遮罩
        if (dimmer != null)
        {
            Debug.Log("OptionsMenuManager: 关闭 Dimmer");
            dimmer.SetActive(false);
        }
        else
        {
            Debug.LogWarning("OptionsMenuManager: Dimmer 未分配！");
        }
        
        // 隐藏选项面板
        optionsPanel.SetActive(false);
        Debug.Log("OptionsMenuManager: 选项面板已关闭");
        
        // 播放关闭音效（如果有的话）
        // PlayCloseSound();
    }
    
    // 检查键盘输入
    private void Update()
    {
        // 如果启用了ESC关闭功能，并且按下了ESC键
        if (closeOnEscape && Input.GetKeyDown(KeyCode.Escape))
        {
            // 只有当选项面板处于激活状态时才关闭
            if (optionsPanel != null && optionsPanel.activeSelf)
            {
                CloseOptionsMenu();
            }
        }
    }
    
    // 切换选项菜单的显示状态（如果需要切换功能）
    public void ToggleOptionsMenu()
    {
        if (optionsPanel == null) return;
        
        if (optionsPanel.activeSelf)
        {
            CloseOptionsMenu();
        }
        else
        {
            OpenOptionsMenu();
        }
    }
} 