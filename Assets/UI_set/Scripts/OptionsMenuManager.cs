using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsMenuManager : MonoBehaviour
{
    [Header("界面引用")]
    [SerializeField] private GameObject optionsPanel;              // 选项界面面板
    [SerializeField] private Button closeButton;                   // 关闭按钮
    [SerializeField] private AudioVolumeManager audioManager;      // 音频管理器引用
    
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
            audioManager = FindObjectOfType<AudioVolumeManager>();
            if (audioManager == null)
            {
                Debug.LogWarning("未找到AudioVolumeManager！音量设置可能无法正常工作。");
            }
        }
    }
    
    // 外部调用：打开选项菜单
    public void OpenOptionsMenu()
    {
        // 确保面板存在
        if (optionsPanel == null) return;
        
        // 显示选项面板
        optionsPanel.SetActive(true);
        
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
        if (optionsPanel == null) return;
        
        // 隐藏选项面板
        optionsPanel.SetActive(false);
        
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