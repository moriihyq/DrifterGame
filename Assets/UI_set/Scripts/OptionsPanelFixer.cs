using UnityEngine;

using UnityEngine.UI;
/// <summary>
/// 这个辅助脚本用于检测和修复Options面板的问题
/// </summary>
public class OptionsPanelFixer : MonoBehaviour
{
    // 在第一帧执行
    void Start()
    {
        Debug.Log("OptionsPanelFixer: 开始检测选项面板问题...");
        
        // 1. 检查场景中是否存在OptionsMenPanel
        GameObject optionsMenPanel = GameObject.Find("OptionsMenPanel");
        if (optionsMenPanel != null)
        {
            Debug.Log("OptionsPanelFixer: 找到OptionsMenPanel");
            
            // 检查OptionsMenPanel是否可见
            if (optionsMenPanel.activeInHierarchy)
            {
                Debug.Log("OptionsPanelFixer: OptionsMenPanel当前是可见的，将其隐藏");
                optionsMenPanel.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("OptionsPanelFixer: 场景中不存在OptionsMenPanel！");
        }
        
        // 2. 查找TitleScreenManager
        TitleScreenManager titleManager = FindFirstObjectByType<TitleScreenManager>();
        if (titleManager != null)
        {
            Debug.Log("OptionsPanelFixer: 找到TitleScreenManager");
            
            // 通过反射检查其optionsPanel属性是否已分配
            System.Reflection.FieldInfo panelField = typeof(TitleScreenManager).GetField("optionsPanel", 
                                                    System.Reflection.BindingFlags.Public | 
                                                    System.Reflection.BindingFlags.Instance);
            
            if (panelField != null)
            {
                GameObject currentPanel = panelField.GetValue(titleManager) as GameObject;
                
                if (currentPanel == null)
                {
                    Debug.LogWarning("OptionsPanelFixer: TitleScreenManager的optionsPanel未分配！尝试修复...");
                    
                    // 尝试将OptionsMenPanel分配给titleManager
                    if (optionsMenPanel != null)
                    {
                        panelField.SetValue(titleManager, optionsMenPanel);
                        Debug.Log("OptionsPanelFixer: 已将OptionsMenPanel分配给TitleScreenManager");
                    }
                }
                else
                {
                    Debug.Log("OptionsPanelFixer: TitleScreenManager的optionsPanel已分配为: " + currentPanel.name);
                }
            }
        }
        else
        {
            Debug.LogWarning("OptionsPanelFixer: 场景中不存在TitleScreenManager！");
        }
        
        // 3. 检查Options按钮的点击事件
        GameObject optionsButton = GameObject.Find("OptionsButton");
        if (optionsButton != null)
        {
            Debug.Log("OptionsPanelFixer: 找到OptionsButton");
            
            // 检查按钮组件
            UnityEngine.UI.Button button = optionsButton.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                // 清除现有的事件监听器并重新添加
                button.onClick.RemoveAllListeners();
                
                if (titleManager != null)
                {
                    button.onClick.AddListener(titleManager.OpenOptions);
                    Debug.Log("OptionsPanelFixer: 重新绑定了OptionsButton的点击事件到titleManager.OpenOptions");
                }
            }
            else
            {
                Debug.LogWarning("OptionsPanelFixer: OptionsButton上没有Button组件！");
            }
        }
        else
        {
            Debug.LogWarning("OptionsPanelFixer: 场景中不存在OptionsButton！");
        }
        
        Debug.Log("OptionsPanelFixer: 检测和修复完成");
    }
} 