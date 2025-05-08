using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    [Header("菜单管理器")]
    [SerializeField] private OptionsMenuManager optionsManager;
    
    [Header("按钮引用")]
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button[] otherButtons;
    
    [Header("音频设置")]
    [SerializeField] private AudioSource buttonClickSource;
    [SerializeField] private AudioClip buttonClickSound;
    
    private void Start()
    {
        // 查找选项菜单管理器（如果未分配）
        if (optionsManager == null)
        {
            optionsManager = FindObjectOfType<OptionsMenuManager>();
            if (optionsManager == null)
            {
                Debug.LogError("未找到OptionsMenuManager！菜单功能可能无法正常工作。");
            }
        }
        
        // 设置Options按钮事件
        if (optionsButton != null)
        {
            // 移除任何现有的监听器，以避免重复调用
            optionsButton.onClick.RemoveAllListeners();
            
            // 添加新的监听器
            optionsButton.onClick.AddListener(() => {
                PlayButtonClickSound();
                HandleOptionsButtonClick();
            });
        }
        
        // 设置其他按钮的音效
        if (otherButtons != null)
        {
            foreach (Button btn in otherButtons)
            {
                if (btn != null)
                {
                    // 创建一个临时变量避免闭包问题
                    Button currentBtn = btn;
                    
                    // 添加播放点击音效的监听器（保留原有监听器）
                    var originalListeners = currentBtn.onClick.GetPersistentEventCount();
                    if (originalListeners == 0) // 如果没有现有监听器，添加一个空事件
                    {
                        currentBtn.onClick.AddListener(() => PlayButtonClickSound());
                    }
                    else // 如果有现有监听器，添加播放音效的额外监听
                    {
                        Button newBtn = currentBtn; // 再次创建临时变量
                        currentBtn.onClick.AddListener(() => PlayButtonClickSound());
                    }
                }
            }
        }
    }
    
    // 处理Options按钮点击
    private void HandleOptionsButtonClick()
    {
        if (optionsManager != null)
        {
            // 调用选项菜单管理器打开菜单
            optionsManager.OpenOptionsMenu();
            
            // 可以在这里添加其他逻辑，如禁用其他按钮等
        }
        else
        {
            Debug.LogWarning("点击Options按钮，但未找到OptionsMenuManager！");
        }
    }
    
    // 播放按钮点击音效
    private void PlayButtonClickSound()
    {
        if (buttonClickSource != null && buttonClickSound != null)
        {
            buttonClickSource.PlayOneShot(buttonClickSound);
        }
    }
} 